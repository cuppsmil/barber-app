using System.Globalization;

namespace BarberApp.Converters;

public class TimeSlotColorConverter : IValueConverter
{
    public static TimeSlotColorConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // value - это IsAvailable (bool)
        if (value is bool isAvailable)
        {
            return isAvailable
                ? Color.FromArgb("#9333EA") // Фиолетовый (доступен)
                : Color.FromArgb("#E5E7EB"); // Серый (занят)
        }
        return Color.FromArgb("#E5E7EB");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}