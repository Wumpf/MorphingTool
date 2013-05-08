using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public void DissolveImages(ImageSource startImage, ImageSource endImage, float percentage, WriteableBitmap outputImage)
        {
            System.Diagnostics.Debug.Assert(percentage >= 0.0f && percentage <= 1.0f);
            System.Diagnostics.Debug.Assert(startImage != null && endImage != null && outputImage != null);

            outputImage.Lock();
            
            unsafe
            {
                UInt32* outputData = (UInt32*)outputImage.BackBuffer;
                int outputDataOffset = 0;
                for (int y = 0; y < outputImage.PixelHeight; ++y)
                {
                    for (int x = 0; x < outputImage.PixelWidth; ++x)
                    {
                        outputData[outputDataOffset] = ImageUtilities.ColorToUInt(Color.FromRgb(255, 0, 255));
                        ++outputDataOffset;
                    }
                }
            }

            outputImage.AddDirtyRect(new System.Windows.Int32Rect(0, 0, outputImage.PixelWidth, outputImage.PixelHeight));
            outputImage.Unlock();
        }
    }
}
