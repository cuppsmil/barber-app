using BarberApp.ViewModels;
using System.Diagnostics;

namespace BarberApp.Views;

public partial class HistoryPage : ContentPage
{
    public HistoryPage()
    {
        InitializeComponent();
        Debug.WriteLine("✅ HistoryPage создан");
    }

    // ✅ ОБНОВЛЯЕМ ПРИ КАЖДОМ ПОЯВЛЕНИИ СТРАНИЦЫ
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("🔄 HistoryPage: OnAppearing -> обновляем историю");

        if (BindingContext is HistoryViewModel vm)
        {
            await vm.RefreshCommand.ExecuteAsync(null);
        }
    }
}