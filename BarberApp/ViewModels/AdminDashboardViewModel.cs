using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Services;
using BarberApp.Views;

namespace BarberApp.ViewModels;

public partial class AdminDashboardViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SecureStorageService _storage;

    [ObservableProperty] private string _salonName = "Загрузка...";

    public AdminDashboardViewModel()
    {
        _db = new DatabaseService();
        _storage = new SecureStorageService();
        _ = LoadSalonNameAsync();
    }

    private async Task LoadSalonNameAsync()
    {
        var salonId = await _storage.GetAdminSalonIdAsync();
        if (salonId.HasValue)
        {
            var row = await _db.GetSalonAsync(salonId.Value);
            SalonName = row?.Name ?? "Салон";
        }
    }

    [RelayCommand]
    private async Task OpenAppointmentsAsync() =>
    await Application.Current!.MainPage!.Navigation.PushAsync(new AdminAppointmentsPage());

    [RelayCommand]
    private async Task OpenMastersAsync() =>
        await Application.Current!.MainPage!.Navigation.PushAsync(new AdminMastersPage());

    [RelayCommand]
    private async Task OpenClientsAsync() =>
        await Application.Current!.MainPage!.Navigation.PushAsync(new AdminClientsPage());

    [RelayCommand]
    private async Task OpenServicesAsync() =>
        await Application.Current!.MainPage!.Navigation.PushAsync(new AdminServicesPage());

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _storage.ClearAdminCredentialsAsync();
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}