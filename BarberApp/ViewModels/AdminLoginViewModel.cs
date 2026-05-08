using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Services;
using BarberApp.Views;

namespace BarberApp.ViewModels;

public partial class AdminLoginViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SecureStorageService _storage;

    [ObservableProperty] private string _login = "";
    [ObservableProperty] private string _password = "";

    public AdminLoginViewModel()
    {
        _db = new DatabaseService();
        _storage = new SecureStorageService();
    }

    [RelayCommand]
    private async Task AdminLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password)) return;

        var admin = await _db.GetAdminAsync(Login, Password);
        if (admin.HasValue)
        {
            await _storage.SaveAdminCredentialsAsync(admin.Value.Id, admin.Value.SalonId, Login);
            Application.Current!.MainPage = new NavigationPage(new AdminDashboardPage());
        }
        else
        {
            await Application.Current!.MainPage!.DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
        }
    }
}