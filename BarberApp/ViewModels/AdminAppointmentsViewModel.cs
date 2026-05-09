using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Models;
using BarberApp.Services;
using System.Collections.ObjectModel;

namespace BarberApp.ViewModels;

public partial class AdminAppointmentsViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SecureStorageService _storage;

    [ObservableProperty]
    private ObservableCollection<AppointmentItem> _items = new();

    public AdminAppointmentsViewModel()
    {
        _db = new DatabaseService();
        _storage = new SecureStorageService();
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Application.Current!.MainPage!.Navigation.PopAsync();
    }

    private async Task LoadDataAsync()
    {
        var salonId = await _storage.GetAdminSalonIdAsync();
        System.Diagnostics.Debug.WriteLine($">>> AdminAppointments: salonId = {salonId?.ToString() ?? "null"}");

        if (salonId.HasValue)
        {
            var list = await _db.GetSalonAppointmentsAsync(salonId.Value);
            Items.Clear();
            foreach (var item in list)
            {
                Items.Add(item);
                System.Diagnostics.Debug.WriteLine($">>> Добавлено: {item.Date} | Цена: {item.Price} ₽");
            }
        }
    }
}