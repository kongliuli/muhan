using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernBoxes.Presentation.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is String hex && !String.IsNullOrEmpty(hex))
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
