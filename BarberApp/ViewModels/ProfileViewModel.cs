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
    [ObservableProperty] private bool _isEditFormVisible = false;
    [ObservableProperty] private string _editLogin = "";
    [ObservableProperty] private string _editPassword = "";
    [ObservableProperty] private string _confirmPassword = "";

    public ProfileViewModel()
    {
        _dbService = new DatabaseService();
        _storage = new SecureStorageService();

        Debug.WriteLine("🟢 ProfileViewModel создан");

        // ✅ АВТО-ОБНОВЛЕНИЕ ПОСЛЕ ЗАПИСИ
        MessagingCenter.Subscribe<HomeViewModel>(this, "RefreshProfile", async (sender) =>
        {
            Debug.WriteLine("📨 Получено сообщение RefreshProfile -> обновляем данные");
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
            Debug.WriteLine($"🔵 ClientId из хранилища: {clientId?.ToString() ?? "null"}");

            if (!clientId.HasValue || clientId.Value == 0)
            {
                UserName = "❌ Не авторизован";
                return;
            }

            var info = await _dbService.GetClientInfoAsync(clientId.Value);
            UserName = info.Name;
            UserPhone = info.Phone;
            UserLogin = info.Login;
            EditLogin = info.Login;

            // История
            var hist = await _dbService.GetClientHistoryAsync(clientId.Value);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                History.Clear();
                foreach (var h in hist) History.Add(h);
                Debug.WriteLine($" Записей в истории: {History.Count}");
            });

            // Избранное
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
            Debug.WriteLine($"🔴 ОШИБКА LoadData: {ex.Message}");
            UserName = $"Ошибка: {ex.Message}";
        }
    }

    public async Task RefreshAsync()
    {
        Debug.WriteLine("🟡 RefreshAsync вызван");
        LoadData();
    }

    [RelayCommand] private void ToggleEditForm() => IsEditFormVisible = !IsEditFormVisible;

    [RelayCommand]
    private async Task SaveProfileChangesAsync()
    {
        if (string.IsNullOrWhiteSpace(EditLogin)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите логин", "OK"); return; }
        if (!string.IsNullOrWhiteSpace(EditPassword) && EditPassword != ConfirmPassword) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Пароли не совпадают", "OK"); return; }

        var clientId = await _storage.GetClientIdAsync();
        if (clientId == null) return;

        string? newPassword = string.IsNullOrWhiteSpace(EditPassword) ? null : EditPassword;
        await _dbService.UpdateCredentialsAsync(clientId.Value, EditLogin, newPassword);
        await _storage.UpdateCredentialsAsync(login: EditLogin, password: newPassword);

        UserLogin = EditLogin;
        IsEditFormVisible = false;
        EditPassword = "";
        ConfirmPassword = "";
        await Application.Current.MainPage.DisplayAlert("Успех", "Данные сохранены", "OK");
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