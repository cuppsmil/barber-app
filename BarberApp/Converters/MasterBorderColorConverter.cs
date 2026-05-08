using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class MasterBorderColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Master selected && parameter is Master current)
        {
            return selected.Id == current.Id
                ? Color.FromArgb("#6B46C1")
                : Color.FromArgb("#E5E7EB");
        }
        return Color.FromArgb("#E5E7EB");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}