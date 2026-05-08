namespace BarberApp.Models;

public class AppointmentItem
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string MasterName { get; set; } = "";
    public string SalonName { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Status { get; set; } = "Запланировано";
    public Color StatusTextColor { get; set; } = Color.FromArgb("#4C1D95");
    public Color StatusBgColor { get; set; } = Color.FromArgb("#E9D5FF");
}