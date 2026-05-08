using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using BarberApp.Services;
using BarberApp.Views;

namespace BarberApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly SecureStorageService _secureStorage;

    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private bool _isLoginMode = true;
    [ObservableProperty] private bool _isBusy = false;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Команды для XAML (если нужно)
    public ICommand PerformLoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand ToggleModeCommand { get; }

    public LoginViewModel()
    {
        _authService = new AuthService();
        _secureStorage = new SecureStorageService();

        // Команды просто вызывают методы
        PerformLoginCommand = new Command(async () => await PerformLoginAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
        ToggleModeCommand = new Command(ToggleMode);
    }

    // ✅ СДЕЛАЛ PUBLIC, ЧТОБЫ ВЫЗЫВАТЬ ИЗ CODE-BEHIND
    public async Task PerformLoginAsync()
    {
        if (IsBusy) return;

        try
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль";
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ErrorMessage, "OK");
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine($">> 🔑 Попытка входа: {Login}");

            int? clientId = await _authService.LoginAsync(Login, Password);

            if (clientId.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($">> ✅ Вход успешен! ClientId: {clientId.Value}");

                await _secureStorage.SaveCredentialsAsync(
                    clientId.Value,
                    Name,
                    Login,
                    Phone, // ✅ Передаем телефон
                    Password
                );

                System.Diagnostics.Debug.WriteLine(">> 🚀 Переход на главную...");
                Application.Current!.MainPage = new NavigationPage(new RootPage());
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль";
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("Исключение", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ✅ СДЕЛАЛ PUBLIC
    public async Task RegisterAsync()
    {
        if (IsBusy) return;

        try
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Заполните все поля";
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ErrorMessage, "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ErrorMessage, "OK");
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine($">> 📝 Регистрация: {Name}, {Login}");

            int? clientId = await _authService.RegisterAsync(Name, Phone, Login, Password);

            if (clientId.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($">> ✅ Регистрация успешна! ClientId: {clientId.Value}");

                await _secureStorage.SaveCredentialsAsync(
                    clientId.Value,
                    Name,
                    Login,
                    Phone, // ✅ Передаем телефон
                    Password
                );

                System.Diagnostics.Debug.WriteLine(">> 🚀 Переход на главную...");
                Application.Current!.MainPage = new NavigationPage(new RootPage());
            }
            else
            {
                ErrorMessage = "Пользователь уже существует";
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
            await Application.Current!.MainPage!.DisplayAlert("Исключение", ErrorMessage, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ToggleMode()
    {
        IsLoginMode = !IsLoginMode;
        ErrorMessage = string.Empty;
    }
}