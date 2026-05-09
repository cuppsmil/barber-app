using System.Text.RegularExpressions;
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

    // ✅ НОВЫЕ: Сообщения об ошибках для каждого поля
    [ObservableProperty] private string _nameError = string.Empty;
    [ObservableProperty] private string _phoneError = string.Empty;
    [ObservableProperty] private string _loginError = string.Empty;
    [ObservableProperty] private string _passwordError = string.Empty;
    [ObservableProperty] private string _confirmPasswordError = string.Empty;

    public ICommand PerformLoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand ToggleModeCommand { get; }

    public LoginViewModel()
    {
        _authService = new AuthService();
        _secureStorage = new SecureStorageService();
        PerformLoginCommand = new Command(async () => await PerformLoginAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
        ToggleModeCommand = new Command(ToggleMode);
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ВАЛИДАЦИИ ===

    private void ClearErrors()
    {
        NameError = PhoneError = LoginError = PasswordError = ConfirmPasswordError = ErrorMessage = string.Empty;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Введите имя";
            return false;
        }
        if (Name.Length < 2)
        {
            NameError = "Имя должно содержать минимум 2 символа";
            return false;
        }
        if (!Regex.IsMatch(Name, @"^[а-яА-ЯёЁa-zA-Z\s\-']+$"))
        {
            NameError = "Имя может содержать только буквы, пробелы, дефис и апостроф";
            return false;
        }
        NameError = string.Empty;
        return true;
    }

    private bool ValidatePhone()
    {
        if (string.IsNullOrWhiteSpace(Phone))
        {
            PhoneError = "Введите телефон";
            return false;
        }
        // Убираем всё кроме цифр и +
        var cleanPhone = Regex.Replace(Phone, @"[^\d+]", "");

        // Проверка российского формата: +7 или 8, потом 10 цифр
        if (!Regex.IsMatch(cleanPhone, @"^(\+7|8)\d{10}$"))
        {
            PhoneError = "Введите телефон в формате +7 (999) 123-45-67";
            return false;
        }
        PhoneError = string.Empty;
        return true;
    }

    private bool ValidateLogin()
    {
        if (string.IsNullOrWhiteSpace(Login))
        {
            LoginError = "Введите логин";
            return false;
        }
        if (Login.Length < 3)
        {
            LoginError = "Логин должен содержать минимум 3 символа";
            return false;
        }
        if (Login.Length > 20)
        {
            LoginError = "Логин не должен превышать 20 символов";
            return false;
        }
        if (!Regex.IsMatch(Login, @"^[a-zA-Z0-9_]+$"))
        {
            LoginError = "Логин может содержать только латинские буквы, цифры и _";
            return false;
        }
        LoginError = string.Empty;
        return true;
    }

    private bool ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Введите пароль";
            return false;
        }
        if (Password.Length < 8)
        {
            PasswordError = "Пароль должен содержать минимум 8 символов";
            return false;
        }
        if (!Regex.IsMatch(Password, @"[A-Z]"))
        {
            PasswordError = "Пароль должен содержать хотя бы одну заглавную букву";
            return false;
        }
        if (!Regex.IsMatch(Password, @"[a-z]"))
        {
            PasswordError = "Пароль должен содержать хотя бы одну строчную букву";
            return false;
        }
        if (!Regex.IsMatch(Password, @"[0-9]"))
        {
            PasswordError = "Пароль должен содержать хотя бы одну цифру";
            return false;
        }
        PasswordError = string.Empty;
        return true;
    }

    private bool ValidateConfirmPassword()
    {
        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordError = "Подтвердите пароль";
            return false;
        }
        if (ConfirmPassword != Password)
        {
            ConfirmPasswordError = "Пароли не совпадают";
            return false;
        }
        ConfirmPasswordError = string.Empty;
        return true;
    }

    // === ОСНОВНЫЕ МЕТОДЫ ===

    public async Task PerformLoginAsync()
    {
        if (IsBusy) return;
        ClearErrors();

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
                    clientId.Value, Name, Login, Phone, Password);

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

    public async Task RegisterAsync()
    {
        if (IsBusy) return;
        ClearErrors();

        try
        {
            // ✅ ЗАПУСКАЕМ ВАЛИДАЦИЮ ВСЕХ ПОЛЕЙ
            bool isValid = true;
            isValid &= ValidateName();
            isValid &= ValidatePhone();
            isValid &= ValidateLogin();
            isValid &= ValidatePassword();
            isValid &= ValidateConfirmPassword();

            if (!isValid)
            {
                ErrorMessage = "Ошибка";
                return; // Не показываем MessageBox, ошибки уже в полях
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine($">> 📝 Регистрация: {Name}, {Login}");

            // Нормализуем телефон перед отправкой (убираем пробелы, скобки, тире)
            var cleanPhone = Regex.Replace(Phone, @"[^\d+]", "");

            int? clientId = await _authService.RegisterAsync(Name, cleanPhone, Login, Password);

            if (clientId.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($">> ✅ Регистрация успешна! ClientId: {clientId.Value}");

                await _secureStorage.SaveCredentialsAsync(
                    clientId.Value, Name, Login, cleanPhone, Password);

                Application.Current!.MainPage = new NavigationPage(new RootPage());
            }
            else
            {
                ErrorMessage = "Пользователь с таким логином уже существует";
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
        ClearErrors();
    }
}