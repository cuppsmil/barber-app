using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BarberApp.Services;
using System.Collections.ObjectModel;
using BarberApp.Models;

namespace BarberApp.ViewModels;

public partial class AdminClientsViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly SecureStorageService _storage;

    [ObservableProperty]
    private ObservableCollection<SalonClient> _clients = new();

    [ObservableProperty]
    private string _salonName = "Загрузка...";

    public AdminClientsViewModel()
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
            var salon = await _db.GetSalonAsync(salonId.Value);
            SalonName = salon?.Name ?? "Салон";

            var list = await _db.GetSalonClientsAsync(salonId.Value);
            Clients.Clear();
            foreach (var c in list) Clients.Add(c);
        }
    }
}