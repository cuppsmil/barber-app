using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class ServiceHighlightConverter : IValueConverter
{
    public static ServiceHighlightConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ServiceItem selectedService && parameter is ServiceItem currentService)
        {
            if (selectedService.Id == currentService.Id)
            {
                if (targetType == typeof(Color))
                    return Color.FromArgb("#6B46C1"); // Фиолетовая рамка
                if (targetType == typeof(double))
                    return 3.0; // Толстая рамка
                return Color.FromArgb("#6B46C1"); // Фиолетовый фон
            }
        }

        if (targetType == typeof(Color))
            return Color.FromArgb("#E5E7EB"); // Серая рамка
        if (targetType == typeof(double))
            return 1.0;
        return Color.FromArgb("#FFFFFF"); // Белый фон
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}