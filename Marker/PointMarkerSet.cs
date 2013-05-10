using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MorphingTool
{
    public class PointMarkerSet : MarkerSet
    {
        private const int MARKER_RENDER_SIZE = 10;

        public class PointMarker
        {
            public ImagePosition StartImagePoint;
            public ImagePosition EndImagePoint;
            public ImagePosition InterpolatedImagePoint;
        };

        private List<PointMarker> points = new List<PointMarker>();

        public override void OnLeftClick(MouseLocation clickLocation, ImagePosition imageCor)
        {
            if (clickLocation == MouseLocation.NONE)
                return;

            var newMarker = new PointMarker()
            {
                StartImagePoint = imageCor,
                EndImagePoint = imageCor,
                InterpolatedImagePoint = imageCor
            };
            points.Add(newMarker);
        }

        public override void OnRightClick(MouseLocation clickLocation, ImagePosition imageCor)
        {
        }

        public override void OnMouseMove(MouseLocation clickLocation, ImagePosition imageCor)
        {
        }

        public override void UpdateInterpolation(float interpolation)
        {
            base.UpdateInterpolation(interpolation);

            foreach (var marker in points)
                marker.InterpolatedImagePoint = ImagePosition.Lerp(marker.StartImagePoint, marker.EndImagePoint, interpolation);
        }

        public override void UpdateMarkerCanvas(Canvas[] imageCanvas, Point[] imageOffsetPixel, Point[] imageSizePixel)
        {
            System.Diagnostics.Debug.Assert(imageCanvas.Length == imageOffsetPixel.Length && imageOffsetPixel.Length == imageSizePixel.Length);

            for (int i = 0; i < imageCanvas.Length; ++i)
            {
                // brute force way - todo: move exiting elements (identifing by name), delete obsolte ones and create new ones
                imageCanvas[i].Children.Clear();

                foreach (var marker in points)
                {
                    var markerRect = new Rectangle();
                    markerRect.Width = MARKER_RENDER_SIZE;
                    markerRect.Height = MARKER_RENDER_SIZE;
                    markerRect.Stroke = new SolidColorBrush(Colors.Black);
                    markerRect.StrokeThickness = 2;
                    markerRect.Fill = new SolidColorBrush(Colors.White);
                    markerRect.RadiusX = MARKER_RENDER_SIZE / 4;
                    markerRect.RadiusY = MARKER_RENDER_SIZE / 4;

                    Canvas.SetLeft(markerRect, marker.StartImagePoint.X * imageSizePixel[i].X + imageOffsetPixel[i].X - MARKER_RENDER_SIZE / 2);
                    Canvas.SetTop(markerRect, marker.StartImagePoint.Y * imageSizePixel[i].Y + imageOffsetPixel[i].Y - MARKER_RENDER_SIZE / 2);

                    imageCanvas[i].Children.Add(markerRect);
                }
            }
        }
    }
}
