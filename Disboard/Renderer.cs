using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Disboard
{
    static class Renderer
    {
        internal static Stream Render(this Control control)
        {
            var stream = new MemoryStream();
            var png = new PngBitmapEncoder();
            control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            control.Arrange(new Rect(0, 0, control.DesiredSize.Width, control.DesiredSize.Height));
            var target = new RenderTargetBitmap(Math.Max(1, (int)control.ActualWidth), Math.Max(1, (int)control.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            target.Render(control);
            png.Frames.Add(BitmapFrame.Create(target));
            png.Save(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
