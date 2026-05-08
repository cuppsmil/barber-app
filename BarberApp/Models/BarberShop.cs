namespace BarberApp.Models;

public class BarberShop
{
    public Salon Salon { get; set; } = new();
    public List<Master> Masters { get; set; } = new();
    public List<ServiceItem> AllServices { get; set; } = new();

    public string Name => Salon.Name;
    public string Address => Salon.Address;
    public string Phone => Salon.Phone;
    public double Rating => 4.8 + (new Random().NextDouble() * 0.2);
    public string Description => "Профессиональный барбершоп с опытными мастерами";
    public string ImageUrl => $"https://images.unsplash.com/photo-1503951914875-452162b0f3f1?auto=format&fit=crop&w=1200&q=80&random={Salon.Id}";
}