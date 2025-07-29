using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SkiaSharp;
using Svg.Skia;

namespace WpfRecorder.Converters;

public class SvgToImageSourceConverter : IValueConverter
{
    public int Width { get; set; } = 24;
    public int Height { get; set; } = 24;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string path)
        {
            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            var resourceStream = Application.GetResourceStream(uri);
            if (resourceStream == null)
                return null!;

            using var stream = resourceStream.Stream;
            var svg = new SKSvg();
            svg.Load(stream);

            var bitmap = new SKBitmap(Width, Height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawPicture(svg.Picture);

            return BitmapSource.Create(
                bitmap.Width, bitmap.Height, 96, 96,
                System.Windows.Media.PixelFormats.Bgra32,
                null, bitmap.Bytes, bitmap.RowBytes);
        }

        return null!;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}