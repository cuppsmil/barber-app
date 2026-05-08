using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class MasterBorderWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Master selected && parameter is Master current)
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