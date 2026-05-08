using System.Globalization;
using BarberApp.Models;

namespace BarberApp.Converters;

public class MasterHighlightConverter : IValueConverter
{
    public static MasterHighlightConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Master selectedMaster && parameter is Master currentMaster)
        {
            if (selectedMaster.Id == currentMaster.Id)
            {
                // Возвращаем разные значения в зависимости от targetType
                if (targetType == typeof(Color))
                    return Color.FromArgb("#6B46C1"); // BorderColor (фиолетовый)
                if (targetType == typeof(double))
                    return 3.0; // BorderWidth
                if (targetType == typeof(Thickness))
                    return new Thickness(3);
                return Color.FromArgb("#F3E8FF"); // BackgroundColor (светло-фиолетовый)
            }
        }

        // Не выбран
        if (targetType == typeof(Color))
            return Color.FromArgb("#E5E7EB"); // Серая рамка
        if (targetType == typeof(double))
            return 1.0;
        if (targetType == typeof(Thickness))
            return new Thickness(1);
        return Color.FromArgb("#FFFFFF"); // Белый фон
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}