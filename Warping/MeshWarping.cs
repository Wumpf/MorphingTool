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

        struct SourceTriangle
        {
            public Vector A;
            public Vector V0;
            public Vector V1;
        };

        struct DestTriangle
        {
            public Vector A;
            public Vector V0;
            public Vector V1;
            public double V0DotV0_divDenom;
            public double V1DotV1_divDenom;
            public double V0DotV1_divDenom;
        };

        unsafe void WarpingAlgorithm.WarpImage(MarkerSet markerSet, Morphing.ImageData inputImage, Morphing.ImageData outputImage, bool startImage)
        {
            System.Diagnostics.Debug.Assert(markerSet != null && inputImage != null && outputImage != null);

            TriangleMarkerSet triangleMarkerSet = markerSet as TriangleMarkerSet;
            System.Diagnostics.Debug.Assert(triangleMarkerSet != null);

            if (triangleMarkerSet.Triangles.Count() == 0)
                return;

            // prepare data
                // dest
            DestTriangle[] destTriangles = triangleMarkerSet.Triangles.Select(x => new DestTriangle() 
                    {
                        A = x.Vertices[0].InterpolatedMarker,
                        V0 = x.Vertices[2].InterpolatedMarker - x.Vertices[0].InterpolatedMarker,
                        V1 = x.Vertices[1].InterpolatedMarker - x.Vertices[0].InterpolatedMarker
                    }).ToArray();
            for (int i = 0; i < destTriangles.Length; ++i )
            {
                destTriangles[i].V0DotV0_divDenom = destTriangles[i].V0.LengthSquared;
                destTriangles[i].V1DotV1_divDenom = destTriangles[i].V1.LengthSquared;
                destTriangles[i].V0DotV1_divDenom = destTriangles[i].V0.Dot(destTriangles[i].V1);
                double invDenom = 1.0 / (destTriangles[i].V0DotV0_divDenom * destTriangles[i].V1DotV1_divDenom - destTriangles[i].V0DotV1_divDenom * destTriangles[i].V0DotV1_divDenom);
                destTriangles[i].V0DotV0_divDenom *= invDenom;
                destTriangles[i].V1DotV1_divDenom *= invDenom;
                destTriangles[i].V0DotV1_divDenom *= invDenom;
            }
                // src
            SourceTriangle[] sourceTriangles;
            if (startImage)
            {
                sourceTriangles = triangleMarkerSet.Triangles.Select(x => new SourceTriangle() 
                    {
                        A = x.Vertices[0].StartMarker,
                        V0 = x.Vertices[2].StartMarker - x.Vertices[0].StartMarker,
                        V1 = x.Vertices[1].StartMarker - x.Vertices[0].StartMarker 
                    }).ToArray();
            }
            else
            {
                sourceTriangles = triangleMarkerSet.Triangles.Select(x => new SourceTriangle() 
                    {
                        A = x.Vertices[0].EndMarker,
                        V0 = x.Vertices[2].EndMarker - x.Vertices[0].EndMarker,
                        V1 = x.Vertices[1].EndMarker - x.Vertices[0].EndMarker 
                    }).ToArray();
            }
          
            // process pixel wise
            double xStep = 1.0 / outputImage.Width;
            Parallel.For(0, outputImage.Height, yi =>
            {
                Color* outputDataPixel = outputImage.Data + yi * outputImage.Width;
                Color* lastOutputDataPixel = outputDataPixel + outputImage.Width;
                double y = (double)yi / outputImage.Height;

                double u,v;
                double dot00, dot01, dot02, dot11, dot12;
                double invDenom;
                Vector v2;

                for (double x = 0; outputDataPixel != lastOutputDataPixel; x += xStep, ++outputDataPixel)
                {
                    Vector position = new Vector(x, y);

                    for (int triangleIdx = 0; triangleIdx < destTriangles.Length; ++triangleIdx)
                    {
                        // compute barycentric - http://www.blackpawn.com/texts/pointinpoly/
                        v2 = position - destTriangles[triangleIdx].A;

                        // Compute dot products
                        dot02 = VectorExtension.Dot(destTriangles[triangleIdx].V0, v2);
                        dot12 = VectorExtension.Dot(destTriangles[triangleIdx].V1, v2);

                        // Compute barycentric coordinates
                        u = (destTriangles[triangleIdx].V1DotV1_divDenom * dot02 - destTriangles[triangleIdx].V0DotV1_divDenom * dot12);
                        v = (destTriangles[triangleIdx].V0DotV0_divDenom * dot12 - destTriangles[triangleIdx].V0DotV1_divDenom * dot02);

                        // Check if point is in triangle
                        if ((u >= 0) && (v >= 0) && (u + v < 1))
                        {
                            position = sourceTriangles[triangleIdx].A + sourceTriangles[triangleIdx].V0 * u + sourceTriangles[triangleIdx].V1 * v;
                            break;
                        }
                    }
                    position = position.ClampToImageArea();
                    *outputDataPixel = inputImage.Sample(position.X, position.Y);
                }
            });
        }
    }
}
