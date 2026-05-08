using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Models;
using BarberApp.Services;
using System.Collections.ObjectModel;

namespace BarberApp.ViewModels;

public partial class AdminMastersViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SecureStorageService _storage;

    [ObservableProperty]
    private ObservableCollection<Master> _masters = new();

    [ObservableProperty]
    private string _salonName = "Загрузка...";

    public AdminMastersViewModel()
    {
        _db = new DatabaseService();
        _storage = new SecureStorageService();
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Application.Current!.MainPage!.Navigation.PopAsync();
    }

    private async Task LoadDataAsync()
    {
        var salonId = await _storage.GetAdminSalonIdAsync();
        if (salonId.HasValue)
        {
            // Получаем название салона
            var salon = await _db.GetSalonAsync(salonId.Value);
            SalonName = salon?.Name ?? "Салон";

            // Получаем мастеров
            var list = await _db.GetSalonMastersAsync(salonId.Value);
            Masters.Clear();
            foreach (var m in list) Masters.Add(m);
        }
    }
}