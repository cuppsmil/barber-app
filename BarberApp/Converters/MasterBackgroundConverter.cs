using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class MasterBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Master selected && parameter is Master current)
        {
            return selected.Id == current.Id
                ? Color.FromArgb("#F3E8FF")
                : Colors.White;
        }
        return Colors.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}