using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MorphingTool
{
    /// <summary>
    /// Miscellaneous utils for image manipulation
    /// </summary>
    public static class ImageUtilities
    {
        /// <summary>
        /// Converts a Color-struct to a bgra uint32.
        /// </summary>
        static public UInt32 ColorToUInt(Color color)
        {
            return (UInt32)((color.A << 24) | (color.R << 16) |
                            (color.G << 8) | (color.B << 0));
        }

        /// <summary>
        /// Converts a bgra uint32 to a Color-struct
        /// </summary>
        static public Color UIntToColor(UInt32 color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        public static RenderTargetBitmap CreateResizedImage(ImageSource source, int width, int height)
        {
            // source: http://xiu.shoeke.com/2010/07/15/resizing-images-with-wpf-4-0/

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, new Rect(0, 0, width, height)));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return resizedImage;
        }
    }
}