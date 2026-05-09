using System.Globalization;

namespace BarberApp.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public static readonly InvertedBoolConverter Default = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}