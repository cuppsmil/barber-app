using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class ServiceBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ServiceItem selected && parameter is ServiceItem current)
        {
            return selected.Id == current.Id
                ? Color.FromArgb("#6B46C1")
                : Colors.White;
        }
        return Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}