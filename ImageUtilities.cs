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