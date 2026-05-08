using BarberApp.Services;
using BarberApp.ViewModels;

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
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            LoginButton.IsEnabled = false;
            ErrorLabel.IsVisible = false;

            // Передаем данные из полей ввода в ViewModel
            _viewModel.Login = LoginEntry.Text;
            _viewModel.Password = PasswordEntry.Text;

            // ✅ ВЫЗЫВАЕМ МЕТОД НАПРЯМУЮ (он Public)
            // Это позволит await корректно ждать завершения входа
            await _viewModel.PerformLoginAsync();
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoginButton.IsEnabled = true;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            RegisterButton.IsEnabled = false;
            ErrorLabel.IsVisible = false;

            _viewModel.Name = NameEntry.Text;
            _viewModel.Phone = PhoneEntry.Text;
            _viewModel.Login = RegisterLoginEntry.Text;
            _viewModel.Password = RegisterPasswordEntry.Text;
            _viewModel.ConfirmPassword = ConfirmPasswordEntry.Text;

            // ✅ ВЫЗЫВАЕМ МЕТОД РЕГИСТРАЦИИ НАПРЯМУЮ
            await _viewModel.RegisterAsync();

            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                ErrorLabel.Text = _viewModel.ErrorMessage;
                ErrorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка регистрации: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            RegisterButton.IsEnabled = true;
        }
    }

    private void OnToggleModeClicked(object sender, EventArgs e)
    {
        _isLoginMode = !_isLoginMode;
        LoginMode.IsVisible = _isLoginMode;
        RegisterMode.IsVisible = !_isLoginMode;
        ErrorLabel.IsVisible = false;
    }
}