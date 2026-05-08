namespace BarberApp.Views;
public partial class AdminLoginPage : ContentPage
{
    public AdminLoginPage() => InitializeComponent();
    private void OnBackClicked(object s, EventArgs e) =>
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
}