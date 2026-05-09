using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Models;
using BarberApp.Services;
using System.Collections.ObjectModel;

namespace BarberApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly SecureStorageService _storage;

    [ObservableProperty] private ObservableCollection<BarberShop> _shops = new();
    [ObservableProperty] private BarberShop? _selectedShop;
    [ObservableProperty] private Master? _selectedMaster;
    [ObservableProperty] private ServiceItem? _selectedService;
    [ObservableProperty] private ObservableCollection<TimeSlot> _timeSlots = new();
    [ObservableProperty] private TimeSlot? _selectedTimeSlot;
    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private DateTime _selectedDate = DateTime.Now;
    [ObservableProperty] private decimal _currentServicePrice = 0;

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
                foreach (var shop in shops) Shops.Add(shop);
                if (Shops.Count > 0) SelectedShop = Shops[0];
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
        ClearMasterSelection();
        SelectedMaster = null;
        SelectedService = null;
        SelectedTimeSlot = null;
        CurrentServicePrice = 0;
    }

    partial void OnSelectedMasterChanged(Master? oldValue, Master? newValue)
    {
        if (newValue != null)
        {
            _ = LoadTimeSlotsAsync();
            _ = UpdatePriceAsync();
        }
    }

    partial void OnSelectedServiceChanged(ServiceItem? oldValue, ServiceItem? newValue)
    {
        if (newValue != null) _ = UpdatePriceAsync();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        if (SelectedMaster != null) _ = LoadTimeSlotsAsync();
    }

    // === ЛОГИКА ВЫДЕЛЕНИЯ ===

    private void ClearMasterSelection()
    {
        if (SelectedShop != null)
            foreach (var m in SelectedShop.Masters) m.IsSelected = false;
    }

    private void ClearServiceSelection()
    {
        if (SelectedShop != null)
            foreach (var s in SelectedShop.AllServices) s.IsSelected = false;
    }

    private void ClearTimeSlotSelection()
    {
        foreach (var t in TimeSlots) t.IsSelected = false;
    }

    [RelayCommand]
    private void SelectMaster(Master master)
    {
        ClearMasterSelection();
        master.IsSelected = true;
        SelectedMaster = master;
    }

    [RelayCommand]
    private void SelectServiceItem(ServiceItem service)
    {
        ClearServiceSelection();
        service.IsSelected = true;
        SelectedService = service;
    }

    [RelayCommand]
    private void SelectTimeSlot(TimeSlot timeSlot)
    {
        if (!timeSlot.IsAvailable) return;
        ClearTimeSlotSelection();
        timeSlot.IsSelected = true;
        SelectedTimeSlot = timeSlot;
    }

    private async Task UpdatePriceAsync()
    {
        if (SelectedMaster != null && SelectedService != null)
        {
            try
            {
                CurrentServicePrice = await _dbService.GetServicePriceAsync(SelectedMaster.Id, SelectedService.Id);
            }
            catch { CurrentServicePrice = 0; }
        }
        else CurrentServicePrice = 0;
    }

    private async Task LoadTimeSlotsAsync()
    {
        if (SelectedMaster == null) return;
        TimeSlots.Clear();
        ClearTimeSlotSelection();

        var busySlots = await _dbService.GetBusySlotsAsync(SelectedMaster.Id, SelectedDate);
        for (int h = 8; h < 19; h++)
        {
            var time = new TimeSpan(h, 0, 0);
            var slotDateTime = SelectedDate.Date.Add(time);
            TimeSlots.Add(new TimeSlot
            {
                Time = time,
                IsAvailable = !busySlots.Any(t => t == time) && slotDateTime >= DateTime.Now
            });
        }
        SelectedTimeSlot = null;
    }

    [RelayCommand]
    private async Task BookAppointmentAsync()
    {
        if (SelectedShop == null || SelectedMaster == null || SelectedService == null || SelectedTimeSlot == null)
        {
            await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Выберите все параметры", "OK");
            return;
        }

        var appointmentDateTime = SelectedDate.Date.Add(SelectedTimeSlot.Time);
        if (appointmentDateTime < DateTime.Now)
        {
            await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Время уже прошло", "OK");
            return;
        }

        var confirmed = await Application.Current!.MainPage!.DisplayAlert(
            "Подтверждение",
            $"Записаться к {SelectedMaster.Fio} на {appointmentDateTime:dd.MM.yyyy HH:mm}?\n\nСтоимость: {CurrentServicePrice:N0} ₽",
            "Да", "Отмена");

        if (confirmed)
        {
            try
            {
                IsLoading = true;
                var clientId = await _storage.GetClientIdAsync();
                if (!clientId.HasValue || clientId.Value == 0)
                {
                    IsLoading = false;
                    await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Сначала войдите!", "OK");
                    return;
                }

                await _dbService.CreateAppointmentAsync(
                    SelectedMaster.Id, clientId.Value, SelectedService.Id, appointmentDateTime, CurrentServicePrice);

                IsLoading = false;
                await Application.Current!.MainPage!.DisplayAlert("✅ Успешно!", $"Запись создана!\n{CurrentServicePrice:N0} ₽", "OK");
                MessagingCenter.Send(this, "RefreshProfile");
            }
            catch (Exception ex)
            {
                IsLoading = false;
                await Application.Current!.MainPage!.DisplayAlert("❌ Ошибка", ex.Message, "OK");
            }
        }
    }

    [RelayCommand] private async Task ReloadAsync() => await LoadDataAsync();

    [RelayCommand]
    private async Task ToggleFavoriteAsync(Master master)
    {
        if (master == null) return;
        var clientId = await _storage.GetClientIdAsync();
        if (!clientId.HasValue) return;

        var favs = await _dbService.GetFavoritesAsync(clientId.Value);
        bool isAlreadyFav = favs.Any(f => f.Id == master.Id);
        await _dbService.ToggleFavoriteAsync(clientId.Value, master.Id, !isAlreadyFav);

        await Application.Current.MainPage.DisplayAlert("Избранное",
            isAlreadyFav ? $"❌ {master.Fio} удалён" : $"❤️ {master.Fio} добавлен", "OK");
        MessagingCenter.Send(this, "RefreshProfile");
    }
}