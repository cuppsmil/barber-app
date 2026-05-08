using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class TimeSlotHighlightConverter : IValueConverter
{
    public static TimeSlotHighlightConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TimeSlot currentTimeSlot)
            return Color.FromArgb("#E5E7EB");

        if (!currentTimeSlot.IsAvailable)
            return Color.FromArgb("#E5E7EB");

        if (parameter is TimeSlot selectedSlot && selectedSlot.Time == currentTimeSlot.Time)
            return Color.FromArgb("#5B21B6"); // Темно-фиолетовый (выбран)

        return Color.FromArgb("#9333EA"); // Светло-фиолетовый (свободен)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}