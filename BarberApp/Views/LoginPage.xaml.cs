using BarberApp.ViewModels;
using System.Diagnostics;

namespace BarberApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;
    private bool _isLoginMode = true;

    public LoginPage()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        BindingContext = _viewModel;

        Debug.WriteLine("✅ LoginPage создана");
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("🔵 OnLoginClicked нажата");

        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            ErrorLabel.IsVisible = false;

            // Передаём данные из полей в ViewModel
            _viewModel.Login = LoginEntry.Text;
            _viewModel.Password = PasswordEntry.Text;

            await _viewModel.PerformLoginAsync();

            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                ErrorLabel.Text = _viewModel.ErrorMessage;
                ErrorLabel.IsVisible = true;
                await DisplayAlert("Ошибка", _viewModel.ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("🔵 OnRegisterClicked нажата");
        Debug.WriteLine($">> Имя: {NameEntry.Text}");
        Debug.WriteLine($">> Телефон: {PhoneEntry.Text}");
        Debug.WriteLine($">> Логин: {RegisterLoginEntry.Text}");

        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            ErrorLabel.IsVisible = false;

            // ✅ ПЕРЕДАЁМ ДАННЫЕ ИЗ ПОЛЕЙ
            _viewModel.Name = NameEntry.Text;
            _viewModel.Phone = PhoneEntry.Text;
            _viewModel.Login = RegisterLoginEntry.Text;
            _viewModel.Password = RegisterPasswordEntry.Text;
            _viewModel.ConfirmPassword = ConfirmPasswordEntry.Text;

            Debug.WriteLine(">> Вызов RegisterAsync...");
            await _viewModel.RegisterAsync();

            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                Debug.WriteLine($">> Ошибка: {_viewModel.ErrorMessage}");
                ErrorLabel.Text = _viewModel.ErrorMessage;
                ErrorLabel.IsVisible = true;

                // Показываем ошибки под полями
                NameErrorLabel.Text = _viewModel.NameError;
                NameErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.NameError);

                PhoneErrorLabel.Text = _viewModel.PhoneError;
                PhoneErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.PhoneError);

                LoginErrorLabel.Text = _viewModel.LoginError;
                LoginErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.LoginError);

                PasswordErrorLabel.Text = _viewModel.PasswordError;
                PasswordErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.PasswordError);

                ConfirmPasswordErrorLabel.Text = _viewModel.ConfirmPasswordError;
                ConfirmPasswordErrorLabel.IsVisible = !string.IsNullOrEmpty(_viewModel.ConfirmPasswordError);

                await DisplayAlert("Ошибка регистрации", _viewModel.ErrorMessage, "OK");
            }
            else
            {
                Debug.WriteLine(">> ✅ Успешно!");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($">> 🔴 Исключение: {ex.Message}");
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void OnToggleModeClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("🔄 Переключение режима");
        _isLoginMode = !_isLoginMode;
        LoginMode.IsVisible = _isLoginMode;
        RegisterMode.IsVisible = !_isLoginMode;
        ErrorLabel.IsVisible = false;
    }

    private void OnAdminLoginClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("👑 Переход в админку");
        Application.Current!.MainPage = new NavigationPage(new AdminLoginPage());
    }
}