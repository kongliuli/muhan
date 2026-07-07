using ModernBoxes.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ModernBoxes.Desktop.Presentation
{
    public static class CardViewResolver
    {
        private static readonly Dictionary<string, Func<FrameworkElement>> Builtins = new(StringComparer.Ordinal)
        {
            ["一言"] = () => new UCOneWord(),
            ["应用"] = () => new UCusedApplications(),
            ["文件夹"] = () => new UCtempDirectory(),
            ["文件"] = () => new UcTempFile(),
            ["便签"] = () => new UCnotes(),
        };

        public static FrameworkElement Resolve(string cardName, Type? pluginViewType = null, object? dataContext = null)
        {
            FrameworkElement view;
            if (pluginViewType != null && typeof(FrameworkElement).IsAssignableFrom(pluginViewType))
                view = (FrameworkElement)Activator.CreateInstance(pluginViewType)!;
            else if (Builtins.TryGetValue(cardName, out var factory))
                view = factory();
            else
                view = new System.Windows.Controls.TextBlock
                {
                    Text = $"未找到卡片视图: {cardName}",
                    Margin = new Thickness(12),
                };

            if (dataContext != null)
                view.DataContext = dataContext;
            return view;
        }
    }
}
