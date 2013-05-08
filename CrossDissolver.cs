using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MorphingTool
{
    /// <summary>
    /// Performs dissolving between two images
    /// </summary>
    public interface CrossDissolver
    {
        /// <summary>
        /// Dissolves two images.
        /// </summary>
        /// <param name="startImage">Image for percentage=0</param>
        /// <param name="endImage">Image for percentage=1</param>
        /// <param name="morphingProgress">Dissolve percentage from 0 to 1</param>
        /// <param name="outputImage">Target for image output data</param>
        void DissolveImages(ImageSource startImage, ImageSource endImage, float percentage, WriteableBitmap outputImage);
    }
}
