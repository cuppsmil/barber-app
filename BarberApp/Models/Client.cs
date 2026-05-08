namespace BarberApp.Models;

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }
    public int? DiscId { get; set; }
}