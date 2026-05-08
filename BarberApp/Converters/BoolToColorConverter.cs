using System.Globalization;

namespace BarberApp.Converters;

public class BoolToColorConverter : IValueConverter
{
    public static BoolToColorConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAvailable)
        {
            return isAvailable ? Color.FromArgb("#6B46C1") : Color.FromArgb("#E5E7EB");
        }
        return Color.FromArgb("#E5E7EB");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}