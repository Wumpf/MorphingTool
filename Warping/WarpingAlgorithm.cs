using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MorphingTool
{
    interface WarpingAlgorithm
    {
        unsafe void WarpImage(MarkerSet markerSet, Morphing.ImageData inputImage, Morphing.ImageData outputImage, bool startImage);
    }
}
