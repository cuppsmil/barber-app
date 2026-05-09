using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Models;
using BarberApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BarberApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly SecureStorageService _storage;

    [ObservableProperty] private ObservableCollection<AppointmentItem> _history = new();
    [ObservableProperty] private ObservableCollection<Master> _favorites = new();

    [ObservableProperty] private string _userName = "Загрузка...";
    [ObservableProperty] private string _userPhone = "";
    [ObservableProperty] private string _userLogin = "";

    // ✅ ИЗМЕНЯЕМЫЕ ПОЛЯ
    [ObservableProperty] private bool _isEditFormVisible = false;
    [ObservableProperty] private string _editName = "";
    [ObservableProperty] private string _editPassword = "";  // ⚠️ Передаётся как есть, хэшируется в DatabaseService
    [ObservableProperty] private string _confirmPassword = "";

    public ProfileViewModel()
    {
        _dbService = new DatabaseService();
        _storage = new SecureStorageService();

        Debug.WriteLine("✅ ProfileViewModel создан");

        MessagingCenter.Subscribe<HomeViewModel>(this, "RefreshProfile", async (sender) =>
        {
            Debug.WriteLine("📨 ProfileViewModel получил RefreshProfile");
            await RefreshAsync();
        });

        LoadData();
    }

    private async void LoadData()
    {
        try
        {
            Debug.WriteLine("🔵 LoadData: начало");
            UserName = "Загрузка...";

            var clientId = await _storage.GetClientIdAsync();
            Debug.WriteLine($"🔵 ClientId: {clientId?.ToString() ?? "null"}");

            if (!clientId.HasValue || clientId.Value == 0)
            {
                UserName = "❌ Не авторизован";
                return;
            }

            var info = await _dbService.GetClientInfoAsync(clientId.Value);
            UserName = info.Name;
            UserPhone = info.Phone;
            UserLogin = info.Login;
            EditName = info.Name;

            var hist = await _dbService.GetClientHistoryAsync(clientId.Value);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                History.Clear();
                foreach (var h in hist) History.Add(h);
                Debug.WriteLine($" Записей: {History.Count}");
            });

            var favs = await _dbService.GetFavoritesAsync(clientId.Value);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Favorites.Clear();
                foreach (var f in favs) Favorites.Add(f);
                Debug.WriteLine($"🔵 Избранное: {Favorites.Count}");
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"🔴 Ошибка LoadData: {ex.Message}");
            UserName = $"Ошибка: {ex.Message}";
        }
    }

    public async Task RefreshAsync()
    {
        Debug.WriteLine("🟡 RefreshAsync вызван");
        LoadData();
    }

    [RelayCommand]
    private void ToggleEditForm()
    {
        IsEditFormVisible = !IsEditFormVisible;
        if (!IsEditFormVisible)
        {
            EditPassword = "";
            ConfirmPassword = "";
        }
    }

    [RelayCommand]
    private async Task SaveProfileChangesAsync()
    {
        Debug.WriteLine("💾 SaveProfileChangesAsync вызван");

        // ✅ ПРОВЕРКА ИМЕНИ
        if (string.IsNullOrWhiteSpace(EditName))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите имя", "OK");
            return;
        }

        // ✅ ПРОВЕРКА ПАРОЛЯ (если вводится)
        if (!string.IsNullOrWhiteSpace(EditPassword))
        {
            if (EditPassword.Length < 6)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Пароль должен содержать минимум 6 символов", "OK");
                return;
            }

            if (EditPassword != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Пароли не совпадают", "OK");
                return;
            }
            Debug.WriteLine(">> Пароль прошёл валидацию (будет захэширован в DatabaseService)");
        }

        var clientId = await _storage.GetClientIdAsync();
        if (clientId == null) return;

        try
        {
            // ✅ ПАРОЛЬ ПЕРЕДАЁТСЯ "КАК ЕСТЬ" — DatabaseService сам захэширует
            string? newPassword = string.IsNullOrWhiteSpace(EditPassword) ? null : EditPassword;

            Debug.WriteLine($">> Вызов UpdateClientProfileAsync: clientId={clientId.Value}, newName={EditName}, newPassword={(newPassword == null ? "null" : "есть")}");

            await _dbService.UpdateClientProfileAsync(clientId.Value, EditName, newPassword);

            // ✅ В SECURE STORAGE НЕ ХРАНИМ ПАРОЛЬ! Только имя
            await _storage.UpdateCredentialsAsync(name: EditName);

            UserName = EditName;

            IsEditFormVisible = false;
            EditPassword = "";
            ConfirmPassword = "";

            Debug.WriteLine("✅ Профиль успешно обновлён");
            await Application.Current.MainPage.DisplayAlert("Успех", "Данные сохранены", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Ошибка обновления профиля: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task RemoveFromFavoritesAsync(Master master)
    {
        var clientId = await _storage.GetClientIdAsync();
        if (clientId == null) return;

        await _dbService.ToggleFavoriteAsync(clientId.Value, master.Id, false);
        await RefreshAsync();
    }
}