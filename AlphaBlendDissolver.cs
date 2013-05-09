using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace MorphingTool
{
    /// <summary>
    /// Crossdissolving of two images using AlphaBlending. The most common CrossDissolver
    /// </summary>
    public class AlphaBlendDissolver : CrossDissolver
    {
        /// <summary>
        /// Dissolves two images using alphablending
        /// </summary>
        /// <param name="startImage">Image for percentage=0</param>
        /// <param name="endImage">Image for percentage=1</param>
        /// <param name="morphingProgress">Dissolve percentage from 0 to 1</param>
        /// <param name="outputImage">Target for image output data</param>
        public void DissolveImages(Morphing.ImageData startImage, Morphing.ImageData endImage, float percentage, WriteableBitmap outputImage)
        {
            System.Diagnostics.Debug.Assert(percentage >= 0.0f && percentage <= 1.0f);
            System.Diagnostics.Debug.Assert(startImage != null && endImage != null && outputImage != null);

            outputImage.Lock();
            
            unsafe
            {
                UInt32* outputData = (UInt32*)outputImage.BackBuffer;
                int width = outputImage.PixelWidth;
                Parallel.For(0, outputImage.PixelHeight, y =>
                {
                    UInt32* outputDataPixel = outputData + y * width;
                    for (int x = 0; x < width; ++x)
                    {
                        byte grayness = (byte)(255 * percentage);
                        *outputDataPixel = ImageUtilities.ColorToUInt(Color.FromRgb(grayness, grayness, grayness));
                        ++outputDataPixel;
                    }
                });
            }

            outputImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, outputImage.PixelWidth, outputImage.PixelHeight));
            outputImage.Unlock();
        }
    }
}
