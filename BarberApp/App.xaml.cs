namespace BarberApp;
using BarberApp.Views;
public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            // Показываем ошибку
            MainPage = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    Padding = 30,
                    Spacing = 20,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label { Text = "❌ Ошибка запуска", FontSize = 24, FontAttributes = FontAttributes.Bold },
                        new Label { Text = ex.Message, TextColor = Colors.Red },
                        new Label { Text = ex.InnerException?.Message ?? "", TextColor = Colors.Orange },
                        new Button
                        {
                            Text = "Повторить",
                            Command = new Command(() =>
                            {
                                MainPage = new NavigationPage(new LoginPage());
                            })
                        }
                    }
                }
            };
        }
    }
}