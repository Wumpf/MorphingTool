using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MorphingTool
{
    public class RadialFunctionsWarping : WarpingAlgorithm
    {
        // http://www.rbf-morph.com/index.php/product/background

        const float POINT_WEIGHT = 0.1f;

        unsafe void WarpingAlgorithm.WarpImage(MarkerSet markerSet, Morphing.ImageData inputImage, Morphing.ImageData outputImage)
        {
            System.Diagnostics.Debug.Assert(markerSet != null && inputImage != null && outputImage != null);
            
            PointMarkerSet pointMarkerSet = markerSet as PointMarkerSet;
            System.Diagnostics.Debug.Assert(pointMarkerSet != null);

            float xStep = 1.0f / outputImage.Width;

            Parallel.For(0, outputImage.Height, yi =>
            {
                Color* outputDataPixel = outputImage.Data + yi * outputImage.Width;
                Color* lastOutputDataPixel = outputDataPixel + outputImage.Width;
                double y = (double)yi / outputImage.Height;

                for (double x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
                {
                    Vector position = new Vector(x,y);
                    foreach(PointMarkerSet.PointMarker marker in pointMarkerSet.Points)
                    {
                        double distSq = (position - marker.InterpolatedImageVector).LengthSquared;

                        // inverse quadratic!
                        double influence = 1.0f / (1.0f + distSq / POINT_WEIGHT);
                        position += (marker.StartImagePoint - marker.InterpolatedImageVector) * influence;
                    }
                    position = position.ClampToImageArea();

                    *outputDataPixel = inputImage.Sample(position.X, position.Y);
                }
            });
        }
    }
}
