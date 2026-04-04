using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;

namespace desktop.app
{
    public class UriToBitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string uriString && !string.IsNullOrEmpty(uriString))
            {
                try
                {
                    var uri = new Uri(uriString);
                    var asset = AssetLoader.Open(uri);
                    return new Bitmap(asset);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}