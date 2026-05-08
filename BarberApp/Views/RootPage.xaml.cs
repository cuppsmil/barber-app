namespace BarberApp.Views;

public partial class RootPage : ContentPage
{
    private int _currentIndex = 0;

    public RootPage()
    {
        InitializeComponent();
    }

    // Обработчик нажатия на вкладку "Барбершопы"
    private void OnBarbershopsTabSelected(object sender, EventArgs e)
    {
        SwitchTab(0);
    }

    // Обработчик нажатия на вкладку "Профиль"
    private void OnProfileTabSelected(object sender, EventArgs e)
    {
        SwitchTab(1);
    }

    // Обработчик нажатия на вкладку "История"
    private void OnHistoryTabSelected(object sender, EventArgs e)
    {
        SwitchTab(2);
    }

    private void SwitchTab(int index)
    {
        _currentIndex = index;

        // Скрываем все страницы
        HomePageView.IsVisible = false;
        ProfilePageView.IsVisible = false;
        HistoryPageView.IsVisible = false;

        // Сбрасываем цвета иконок
        BarbershopsLabel.TextColor = Color.FromArgb("#666666");
        ProfileLabel.TextColor = Color.FromArgb("#666666");
        HistoryLabel.TextColor = Color.FromArgb("#666666");

        // Показываем нужную страницу и подсвечиваем иконку
        switch (index)
        {
            case 0:
                HomePageView.IsVisible = true;
                BarbershopsLabel.TextColor = Color.FromArgb("#6B46C1");
                break;
            case 1:
                ProfilePageView.IsVisible = true;
                ProfileLabel.TextColor = Color.FromArgb("#6B46C1");
                break;
            case 2:
                HistoryPageView.IsVisible = true;
                HistoryLabel.TextColor = Color.FromArgb("#6B46C1");
                break;
        }
    }
}