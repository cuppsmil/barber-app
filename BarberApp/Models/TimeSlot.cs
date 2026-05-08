namespace BarberApp.Models;

public class TimeSlot
{
    public TimeSpan Time { get; set; }
    public bool IsAvailable { get; set; }
    public string DisplayTime => Time.ToString(@"hh\:mm");
}