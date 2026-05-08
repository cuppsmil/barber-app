using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Models;
using BarberApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BarberApp.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly SecureStorageService _storage;

    [ObservableProperty]
    private ObservableCollection<AppointmentItem> _appointments = new();

    [ObservableProperty]
    private bool _isRefreshing = false;

    [ObservableProperty]
    private string _emptyMessage = "История пуста";

    public HistoryViewModel()
    {
        _dbService = new DatabaseService();
        _storage = new SecureStorageService();
        MessagingCenter.Subscribe<HomeViewModel>(this, "RefreshProfile", async (sender) =>
        {
            Debug.WriteLine("📨 HistoryViewModel получил RefreshProfile -> обновляем");
            await RefreshCommand.ExecuteAsync(null);
        });
        Task.Run(async () => await LoadHistoryAsync());
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadHistoryAsync();
        IsRefreshing = false;
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            var clientId = await _storage.GetClientIdAsync();
            if (!clientId.HasValue)
            {
                EmptyMessage = "Сначала войдите в аккаунт";
                return;
            }

            var history = await _dbService.GetClientHistoryAsync(clientId.Value);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Appointments.Clear();

                if (history.Count == 0)
                {
                    EmptyMessage = "У вас пока нет записей\nЗапишитесь к мастеру!";
                }
                else
                {
                    foreach (var item in history)
                    {
                        // ✅ ЛОГИКА СТАТУСА ПО ДАТЕ
                        if (item.Date < DateTime.Now)
                        {
                            item.Status = "✓ Завершено";
                            item.StatusTextColor = Color.FromArgb("#065F46"); // Тёмно-зелёный текст
                            item.StatusBgColor = Color.FromArgb("#D1FAE5");   // Светло-зелёный фон
                        }
                        else
                        {
                            item.Status = "⏳ Запланировано";
                            item.StatusTextColor = Color.FromArgb("#4C1D95"); // Тёмно-фиолетовый текст
                            item.StatusBgColor = Color.FromArgb("#E9D5FF");   // Светло-фиолетовый фон
                        }
                        Appointments.Add(item);
                    }
                }
            });

            Debug.WriteLine($">>> История загружена: {history.Count} записей");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Ошибка загрузки истории: {ex.Message}");
            EmptyMessage = "Ошибка загрузки данных";
        }
    }
}