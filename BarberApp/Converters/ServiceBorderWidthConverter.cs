using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class ServiceBorderWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ServiceItem selected && parameter is ServiceItem current)
        {
            return selected.Id == current.Id ? 3.0 : 1.0;
        }
        return 1.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}