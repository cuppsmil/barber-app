using BarberApp.Services;
using BarberApp.ViewModels;
using System.Diagnostics;

namespace BarberApp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;
    private readonly SecureStorageService _secureStorage;

    public ProfilePage()
    {
        InitializeComponent();
        _viewModel = new ProfileViewModel();
        _secureStorage = new SecureStorageService();
        BindingContext = _viewModel;

        Debug.WriteLine("ProfilePage создан");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("ProfilePage: OnAppearing -> обновляем данные");

        if (_viewModel != null)
        {
            await _viewModel.RefreshAsync();
        }
    }

    // ✅ ОБРАБОТЧИК КНОПКИ ВЫХОДА
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Кнопка выхода нажата");

        var confirmed = await DisplayAlert(
            "Выход",
            "Вы действительно хотите выйти из аккаунта?",
            "Да", "Отмена");

        if (confirmed)
        {
            try
            {
                // Очищаем данные клиента
                await _secureStorage.ClearCredentialsAsync();
                Debug.WriteLine("Данные клиента очищены");

                // Возвращаем на страницу входа
                Application.Current!.MainPage = new NavigationPage(new LoginPage());
                Debug.WriteLine("Переход на LoginPage выполнен");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при выходе: {ex.Message}");
                await DisplayAlert("Ошибка", $"Не удалось выйти: {ex.Message}", "OK");
            }
        }
    }
}