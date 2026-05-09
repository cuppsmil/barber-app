using CommunityToolkit.Mvvm.ComponentModel;

namespace BarberApp.Models;

public partial class ServiceItem: ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Description { get; set; } = string.Empty;

    public string DisplayText => $"{Name} • {Duration} мин";
    [ObservableProperty]
    private bool _isSelected;
}