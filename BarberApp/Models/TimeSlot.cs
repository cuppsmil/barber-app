using CommunityToolkit.Mvvm.ComponentModel;

namespace BarberApp.Models;

public partial class TimeSlot: ObservableObject
{
    public TimeSpan Time { get; set; }
    public bool IsAvailable { get; set; }
    public string DisplayTime => Time.ToString(@"hh\:mm");
    [ObservableProperty]
    private bool _isSelected;
}