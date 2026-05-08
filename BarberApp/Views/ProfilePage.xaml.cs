using BarberApp.ViewModels;
using System.Diagnostics;

namespace BarberApp.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = new ProfileViewModel();
        Debug.WriteLine("✅ ProfilePage создан");
    }

    // ✅ ОБНОВЛЯЕМ ПРИ КАЖДОМ ПОЯВЛЕНИИ СТРАНИЦЫ
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("🔄 ProfilePage: OnAppearing -> обновляем данные");

        if (BindingContext is ProfileViewModel vm)
        {
            await vm.RefreshAsync();
        }
    }
}