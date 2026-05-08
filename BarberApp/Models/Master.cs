namespace BarberApp.Models;

public class Master
{
    public int Id { get; set; }
    public string Fio { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Passport { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;

    public string Initial => !string.IsNullOrEmpty(Fio) ? Fio[0].ToString().ToUpper() : "M";
}