using BarberApp.ViewModels;
using BarberApp.Services;
namespace BarberApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = new ProfileViewModel();
        System.Diagnostics.Debug.WriteLine("✅ ProfilePage инициализирован");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        System.Diagnostics.Debug.WriteLine("🟢 OnAppearing сработал -> принудительно обновляем");
        if (BindingContext is ProfileViewModel vm)
        {
            vm.RefreshAsync();
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var secureStorage = new SecureStorageService();
        await secureStorage.ClearCredentialsAsync();

        if (Application.Current != null && Application.Current.MainPage != null)
        {
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}