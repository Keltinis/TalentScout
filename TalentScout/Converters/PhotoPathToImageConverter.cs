using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TalentScout.Converters;

public sealed class PhotoPathToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var relativePath = value as string;
        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(path)) return null;
        var image = new BitmapImage();
        image.BeginInit(); image.UriSource = new Uri(path, UriKind.Absolute); image.CacheOption = BitmapCacheOption.OnLoad; image.EndInit(); image.Freeze();
        return image;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
