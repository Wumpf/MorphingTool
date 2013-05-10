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

        const float POINT_WEIGHT = 0.05f;

        private struct WarpMarker
        {
            public Vector CurrentPos;
            public Vector MoveVec;
        }

        unsafe void WarpingAlgorithm.WarpImage(MarkerSet markerSet, Morphing.ImageData inputImage, Morphing.ImageData outputImage, bool startImage)
        {
            System.Diagnostics.Debug.Assert(markerSet != null && inputImage != null && outputImage != null);
          
            PointMarkerSet pointMarkerSet = markerSet as PointMarkerSet;
            System.Diagnostics.Debug.Assert(pointMarkerSet != null);

            // minor precomputation - movevector & interppos (for more mem consistency)
            WarpMarker[] markers;
            if (startImage)
            {
                markers = pointMarkerSet.Points.Select(x => new WarpMarker
                {
                    CurrentPos = x.InterpolatedImageVector,
                    MoveVec = x.StartImagePoint - x.InterpolatedImageVector
                }).ToArray();
            }
            else
            {
                markers = pointMarkerSet.Points.Select(x => new WarpMarker
                {
                    CurrentPos = x.InterpolatedImageVector,
                    MoveVec = x.EndImagePoint - x.InterpolatedImageVector
                }).ToArray();
            }
            float xStep = 1.0f / outputImage.Width;

            Parallel.For(0, outputImage.Height, yi =>
            {
                Color* outputDataPixel = outputImage.Data + yi * outputImage.Width;
                Color* lastOutputDataPixel = outputDataPixel + outputImage.Width;
                double y = (double)yi / outputImage.Height;

                for (double x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
                {
                    Vector ownPosition = new Vector(x, y);
                    Vector position = ownPosition;
                    
                    // fixed ptr won't work inside loop! // for(WarpMarker* pMarker = pMarkerFirst; pMarker != pMarkerEnd; ++pMarker)
                    foreach (var marker in markers)
                    {
                        double distSq = (ownPosition - marker.CurrentPos).LengthSquared;
                        double influence = Math.Exp(-distSq / POINT_WEIGHT);//1.0f / (1.0f + distSq / POINT_WEIGHT);        // inverse quadratic!
                        position += marker.MoveVec * influence;
                    }
                    position = position.ClampToImageArea();

                    *outputDataPixel = inputImage.Sample(position.X, position.Y);
                }
            });
        }
    }
}
