namespace BarberApp.Models;

public class ServiceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Description { get; set; } = string.Empty;

    public string DisplayText => $"{Name} • {Duration} мин";
}