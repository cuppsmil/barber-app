namespace BarberApp.Models;

public class AppointmentItem
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string MasterName { get; set; } = "";
    public string SalonName { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Status { get; set; } = "Запланировано";
}