using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Models;
using BarberApp.Services;
using System.Collections.ObjectModel;

namespace BarberApp.ViewModels;

public partial class AdminServicesViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SecureStorageService _storage;

    [ObservableProperty]
    private ObservableCollection<ServiceItem> _services = new();

    [ObservableProperty]
    private string _salonName = "Загрузка...";

    public AdminServicesViewModel()
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
        if (salonId.HasValue)
        {
            var salon = await _db.GetSalonAsync(salonId.Value);
            SalonName = salon?.Name ?? "Салон";

            var list = await _db.GetSalonServicesAsync(salonId.Value);
            Services.Clear();
            foreach (var s in list) Services.Add(s);
        }
    }
}