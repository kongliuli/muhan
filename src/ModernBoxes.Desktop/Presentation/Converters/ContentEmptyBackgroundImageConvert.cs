using ModernBoxes.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModernBoxes.Presentation.Converters
{
    public class ContentEmptyBackgroundImageConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == AppConstants.ComponentAppMenuName)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}