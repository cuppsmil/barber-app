using CommunityToolkit.Mvvm.ComponentModel;

namespace BarberApp.Models;

public partial class Master : ObservableObject
{
    public int Id { get; set; }
    public string Fio { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Passport { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;

    public string Initial => !string.IsNullOrEmpty(Fio) ? Fio[0].ToString().ToUpper() : "M";

    [ObservableProperty]
    private bool _isSelected;
}