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
           
            int width = outputImage.PixelWidth;
            int height = outputImage.PixelHeight;
            float xStep = 1.0f / width;
            unsafe
            {
                Morphing.Color* outputData = (Morphing.Color*)outputImage.BackBuffer;
                Parallel.For(0, outputImage.PixelHeight, yi =>
                {
                    Morphing.Color* outputDataPixel = outputData + yi * width;
                    Morphing.Color* lastOutputDataPixel = outputDataPixel + width;
                    float y = (float)yi / height;
                    for (float x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
                    {
                        *outputDataPixel = Morphing.Color.Lerp(startImage.SampleLinear(x, y), endImage.SampleLinear(x, y), percentage);
                    }
                });
            }

            outputImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, outputImage.PixelWidth, outputImage.PixelHeight));
            outputImage.Unlock();
        }
    }
}
