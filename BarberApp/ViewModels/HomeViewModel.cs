using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Models;
using BarberApp.Services;
using System.Collections.ObjectModel;

namespace BarberApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;

    [ObservableProperty]
    private ObservableCollection<BarberShop> _shops = new();

    [ObservableProperty]
    private BarberShop? _selectedShop;

    [ObservableProperty]
    private Master? _selectedMaster;

    [ObservableProperty]
    private ServiceItem? _selectedService;

    [ObservableProperty]
    private ObservableCollection<TimeSlot> _timeSlots = new();

    [ObservableProperty]
    private TimeSlot? _selectedTimeSlot;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Now;

    
    private readonly SecureStorageService _storage;
    public HomeViewModel()
    {
        _dbService = new DatabaseService();
        _storage = new SecureStorageService();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var shops = await _dbService.GetBarberShopsAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Shops.Clear();
                foreach (var shop in shops)
                {
                    Shops.Add(shop);
                }

                if (Shops.Count > 0)
                {
                    SelectedShop = Shops[0];
                }

                IsLoading = false;
            });
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
                IsLoading = false;
            });
        }
    }

    partial void OnSelectedShopChanged(BarberShop? oldValue, BarberShop? newValue)
    {
        if (newValue == null) return;
        SelectedMaster = null;
        SelectedService = null;
        SelectedTimeSlot = null;
        System.Diagnostics.Debug.WriteLine($"🏪 САЛОН ИЗМЕНЁН: {newValue.Name}");
    }

    partial void OnSelectedMasterChanged(Master? oldValue, Master? newValue)
    {
        if (newValue != null)
            _ = LoadTimeSlotsAsync();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        if (SelectedMaster != null)
            _ = LoadTimeSlotsAsync();
    }

    private async Task LoadTimeSlotsAsync()
    {
        if (SelectedMaster == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Мастер не выбран, слоты не грузятся.");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"🔄 Загрузка слотов для: {SelectedMaster.Fio}, дата: {SelectedDate:d}");

        TimeSlots.Clear();

        var startHour = 8;
        var endHour = 19;

        var busySlots = await _dbService.GetBusySlotsAsync(SelectedMaster.Id, SelectedDate);

        for (int h = startHour; h < endHour; h++)
        {
            var time = new TimeSpan(h, 0, 0);
            var slotDateTime = SelectedDate.Date.Add(time);

            bool isBusyInDb = busySlots.Any(t => t == time);
            bool isPast = slotDateTime < DateTime.Now;

            TimeSlots.Add(new TimeSlot
            {
                Time = time,
                IsAvailable = !isBusyInDb && !isPast
            });
        }

        System.Diagnostics.Debug.WriteLine($"✅ Слотов загружено: {TimeSlots.Count}");
        SelectedTimeSlot = null;
    }

    [RelayCommand]
    private void SelectMaster(Master master)
    {
        SelectedMaster = master;
        System.Diagnostics.Debug.WriteLine($"👨‍🔧 ВЫБРАН МАСТЕР: {master.Fio}");
    }

    [RelayCommand]
    private void SelectServiceItem(ServiceItem service)
    {
        SelectedService = service;
        System.Diagnostics.Debug.WriteLine($"🛠️ ВЫБРАНА УСЛУГА: {service.Name}");
    }

    [RelayCommand]
    private void SelectTimeSlot(TimeSlot timeSlot)
    {
        SelectedTimeSlot = timeSlot;
        System.Diagnostics.Debug.WriteLine($"⏰ ВЫБРАНО ВРЕМЯ: {timeSlot.DisplayTime}");
    }

    [RelayCommand]
    private async Task BookAppointmentAsync()
    {
        if (SelectedShop == null || SelectedMaster == null || SelectedService == null || SelectedTimeSlot == null)
        {
            await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Выберите барбершоп, мастера, услугу и время", "OK");
            return;
        }

        var appointmentDateTime = SelectedDate.Date.Add(SelectedTimeSlot.Time);

        if (appointmentDateTime < DateTime.Now)
        {
            await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Выбранное время уже прошло", "OK");
            return;
        }

        var confirmed = await Application.Current!.MainPage!.DisplayAlert(
            "Подтверждение записи",
            $"Записаться к {SelectedMaster.Fio} на {appointmentDateTime:dd.MM.yyyy HH:mm}?",
            "Да", "Отмена");

        if (confirmed)
        {
            try
            {
                IsLoading = true;

                // ✅ ПОЛУЧАЕМ ID ТЕКУЩЕГО ПОЛЬЗОВАТЕЛЯ
                var clientIdNullable = await _storage.GetClientIdAsync();
                if (!clientIdNullable.HasValue || clientIdNullable.Value == 0)
                {
                    IsLoading = false;
                    await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Сначала войдите в аккаунт!", "OK");
                    return;
                }

                int clientId = clientIdNullable.Value;
                System.Diagnostics.Debug.WriteLine($">>> ЗАПИСЬ: clientId={clientId}, master={SelectedMaster.Fio}");

                var appointmentId = await _dbService.CreateAppointmentAsync(
                    SelectedMaster.Id,
                    clientId,  // ✅ ТЕПЕРЬ ТУТ ПРАВИЛЬНЫЙ ID (9)
                    SelectedService.Id,
                    appointmentDateTime);

                IsLoading = false;

                await Application.Current!.MainPage!.DisplayAlert("✅ Успешно!", $"Запись создана!", "OK");

                MessagingCenter.Send(this, "RefreshProfile");
                System.Diagnostics.Debug.WriteLine("📨 Отправлено сообщение RefreshProfile");

            }
            catch (Exception ex)
            {
                IsLoading = false;
                await Application.Current!.MainPage!.DisplayAlert("❌ Ошибка", ex.Message, "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadDataAsync();
    }
    // === ИЗБРАННОЕ ===
  
    [RelayCommand]
    private async Task ToggleFavoriteAsync(Master master)
    {
        if (master == null) return;

        try
        {
            var clientIdNullable = await _storage.GetClientIdAsync();
            if (!clientIdNullable.HasValue) return;

            int clientId = clientIdNullable.Value;

            // Проверяем, есть ли уже в избранном
            var favs = await _dbService.GetFavoritesAsync(clientId);
            bool isAlreadyFav = favs.Any(f => f.Id == master.Id);

            await _dbService.ToggleFavoriteAsync(clientId, master.Id, !isAlreadyFav);

            var msg = isAlreadyFav
                ? $"❌ {master.Fio} удалён из избранного"
                : $"❤️ {master.Fio} добавлен в избранное";

            await Application.Current.MainPage.DisplayAlert("Избранное", msg, "OK");

            // ✅ ОТПРАВЛЯЕМ СИГНАЛ ПРОФИЛЮ: ОБНОВИ ДАННЫЕ!
            MessagingCenter.Send(this, "RefreshProfile");
            System.Diagnostics.Debug.WriteLine($"📨 Отправлено RefreshProfile после изменения избранного");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}