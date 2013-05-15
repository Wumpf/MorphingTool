using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MorphingTool
{
    /// <summary>
    /// Warping using triangle meshes
    /// </summary>
    public class MeshWarping : WarpingAlgorithm
    {
        const float LINE_WEIGHT = 0.03f;

        unsafe void WarpingAlgorithm.WarpImage(MarkerSet markerSet, Morphing.ImageData inputImage, Morphing.ImageData outputImage, bool startImage)
        {
            System.Diagnostics.Debug.Assert(markerSet != null && inputImage != null && outputImage != null);

    /*         LineMarkerSet lineMarkerSet = markerSet as LineMarkerSet;
            System.Diagnostics.Debug.Assert(lineMarkerSet != null);

            
            
           WarpMarker[] markers;
            if (startImage)
            {
                markers = lineMarkerSet.Lines.Select(x => new WarpMarker
                {
                    target_start = x.InterpolatedMarker.Start,
                    target_dirNorm = x.InterpolatedMarker.End - x.InterpolatedMarker.Start,
                    target_perpendicNorm = (x.InterpolatedMarker.End - x.InterpolatedMarker.Start).Perpendicular(),
                    target_lineLength = (x.InterpolatedMarker.End - x.InterpolatedMarker.Start).Length,

                    dest_start = x.StartMarker.Start,
                    dest_dirNorm = x.StartMarker.End - x.StartMarker.Start,
                    dest_perpendicNorm = (x.StartMarker.End - x.StartMarker.Start).Perpendicular(),
                }).ToArray();
            }
            else
            {
                markers =  lineMarkerSet.Lines.Select(x => new WarpMarker
                {
                    target_start = x.InterpolatedMarker.Start,
                    target_dirNorm = x.InterpolatedMarker.End - x.InterpolatedMarker.Start,
                    target_perpendicNorm = (x.InterpolatedMarker.End - x.InterpolatedMarker.Start).Perpendicular(),
                    target_lineLength = (x.InterpolatedMarker.End - x.InterpolatedMarker.Start).Length,

                    dest_start = x.EndMarker.Start,
                    dest_dirNorm = x.EndMarker.End - x.EndMarker.Start,
                    dest_perpendicNorm = (x.EndMarker.End - x.EndMarker.Start).Perpendicular(),
                }).ToArray();
            }
            for (int markerIdx = 0; markerIdx < markers.Length; ++markerIdx)
            {
             //   markers[markerIdx].target_dirLength = markers[markerIdx].target_dir.Length;
                markers[markerIdx].target_perpendicNorm.Normalize();
                markers[markerIdx].target_dirNorm.Normalize();
                markers[markerIdx].dest_perpendicNorm.Normalize();
                markers[markerIdx].dest_dirNorm.Normalize();
            }

            if (markers.Length == 0)
                return;
            */

            double xStep = 1.0 / outputImage.Width;

            Parallel.For(0, outputImage.Height, yi =>
            {
                Color* outputDataPixel = outputImage.Data + yi * outputImage.Width;
                Color* lastOutputDataPixel = outputDataPixel + outputImage.Width;
                double y = (double)yi / outputImage.Height;

                for (double x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
                {
                    Vector position = new Vector(x, y);
                    Vector displacement = new Vector(0, 0);
                    double weightSum = 0.0f;

                  //  for (int markerIdx = 0; markerIdx < markers.Length; ++markerIdx)
                    {
                    }

                    //displacement /= weightSum;
                    position += displacement;
                    position = position.ClampToImageArea();

                    *outputDataPixel = inputImage.Sample(position.X, position.Y);
                }
            });
        }
    }
}
