using ModernBoxes.Core.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModernBoxes.Presentation.Views
{
    public partial class AsyncResultIcon : UserControl
    {
        public static readonly DependencyProperty IconPathProperty =
            DependencyProperty.Register(nameof(IconPath), typeof(string), typeof(AsyncResultIcon),
                new PropertyMetadata(string.Empty, OnIconSourceChanged));

        public static readonly DependencyProperty FallbackPathProperty =
            DependencyProperty.Register(nameof(FallbackPath), typeof(string), typeof(AsyncResultIcon),
                new PropertyMetadata(string.Empty, OnIconSourceChanged));

        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register(nameof(IconText), typeof(string), typeof(AsyncResultIcon),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconImageProperty =
            DependencyProperty.Register(nameof(IconImage), typeof(ImageSource), typeof(AsyncResultIcon),
                new PropertyMetadata(null));

        public string IconPath
        {
            get => (string)GetValue(IconPathProperty);
            set => SetValue(IconPathProperty, value);
        }

        public string FallbackPath
        {
            get => (string)GetValue(FallbackPathProperty);
            set => SetValue(FallbackPathProperty, value);
        }

        public string IconText
        {
            get => (string)GetValue(IconTextProperty);
            set => SetValue(IconTextProperty, value);
        }

        public ImageSource? IconImage
        {
            get => (ImageSource?)GetValue(IconImageProperty);
            private set => SetValue(IconImageProperty, value);
        }

        public AsyncResultIcon()
        {
            InitializeComponent();
        }

        private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AsyncResultIcon icon)
                icon.LoadIconAsync();
        }

        private async void LoadIconAsync()
        {
            IconImage = null;
            var path = !string.IsNullOrWhiteSpace(IconPath) ? IconPath : FallbackPath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            var loaded = await Task.Run(() => TryLoadImage(path)).ConfigureAwait(true);
            if (loaded != null)
                IconImage = loaded;
        }

        private static ImageSource? TryLoadImage(string path)
        {
            try
            {
                if (path.EndsWith(".exe", System.StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".lnk", System.StringComparison.OrdinalIgnoreCase))
                {
                    using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                    if (icon == null)
                        return null;
                    using var bitmap = icon.ToBitmap();
                    using var stream = new MemoryStream();
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }

                var fileImage = new BitmapImage();
                fileImage.BeginInit();
                fileImage.CacheOption = BitmapCacheOption.OnLoad;
                fileImage.UriSource = new Uri(path, UriKind.Absolute);
                fileImage.EndInit();
                fileImage.Freeze();
                return fileImage;
            }
            catch
            {
                return null;
            }
        }
    }
}
