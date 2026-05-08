using System.Globalization;

namespace BarberApp.Converters;

public class NotNullConverter : IValueConverter
{
    public static NotNullConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}