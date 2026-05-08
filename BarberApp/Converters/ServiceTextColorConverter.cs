using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class ServiceTextColorConverter : IValueConverter
{
    public static ServiceTextColorConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ServiceItem selectedService && parameter is ServiceItem currentService)
        {
            if (selectedService.Id == currentService.Id)
            {
                return Colors.White;
            }
        }
        return Color.FromArgb("#111827");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}