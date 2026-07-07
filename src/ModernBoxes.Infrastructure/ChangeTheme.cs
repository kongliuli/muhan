using ModernBoxes.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Infrastructure
{
    public class ChangeTheme
    {
        public static event Action? ThemeChanged;

        public static void SetTheme(Theme theme)
        {
            ResourceDictionary resourceDictionary = null;
            if (theme != Theme.light)
            {
                resourceDictionary = new ResourceDictionary() { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml") };
                theme = Theme.dark;
            }
            else
            {
                resourceDictionary = new ResourceDictionary() { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml") };
                theme = Theme.light;
            }
            ConfigHelper.setConfig("theme", theme);
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml") });
            ThemeChanged?.Invoke();
        }
    }
}
