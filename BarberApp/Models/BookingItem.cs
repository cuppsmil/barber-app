namespace BarberApp.Models;

public class BookingItem
{
    public int Id { get; set; }
    public string BarberShop { get; set; } = string.Empty;
    public string Master { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}