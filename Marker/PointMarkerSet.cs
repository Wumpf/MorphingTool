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
            public Vector StartImagePoint;
            public Vector EndImagePoint;
            public Vector InterpolatedImageVector;

            /// <summary>
            /// Access by index.
            /// </summary>
            /// <param name="element">0: StartImagePoint
            /// 1: EndImagePoint
            /// 2: InterpolatedImageVector</param>
            /// <returns>Relative image position value.</returns>
            public Vector this[MouseLocation element]
            {
                get
                {
                    switch ((int)element)
                    {
                        case 0:
                            return StartImagePoint;
                        case 1:
                            return EndImagePoint;
                        case 2:
                            return InterpolatedImageVector;
                    }
                    throw new Exception("PointMarker has only 3 Elements!");
                }
                set
                {
                    switch ((int)element)
                    {
                        case 0:
                            StartImagePoint = value;
                            break;
                        case 1:
                            EndImagePoint = value;
                            break;
                        case 2:
                            InterpolatedImageVector = value;
                            break;
                    }
                }
            }
        };

        public List<PointMarker> Points
        { get { return _points; } }

        private List<PointMarker> _points = new List<PointMarker>();
        private int _selectedMarker = -1;
        private int _hoveredMarker = -1;

        /// <summary>
        /// Checks if a click is on a marker-point
        /// </summary>
        /// <returns>-1 if none, otherwise markerindex</returns>
        private int MarkerHitTest(MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == MouseLocation.NONE)
                return -1;

            Vector halfMarkerSize = new Vector(MARKER_RENDER_SIZE / imageSizePixel.X, MARKER_RENDER_SIZE / imageSizePixel.Y) * 0.5f;

            // delete Vectors!
            // find corresonding
            for (int i = 0; i < _points.Count; ++i)
            {
                bool result = false;
                if (clickLocation == MouseLocation.START_IMAGE)
                    result = imageCor.IsInRectangle(_points[i].StartImagePoint - halfMarkerSize, _points[i].StartImagePoint + halfMarkerSize);
                else if (clickLocation == MouseLocation.END_IMAGE)
                    result = imageCor.IsInRectangle(_points[i].EndImagePoint - halfMarkerSize, _points[i].EndImagePoint + halfMarkerSize);

                if (result)
                    return i;
            }

            return -1;
        }

        public override void OnLeftMouseButtonDown(MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == MouseLocation.NONE)
                return;

            // hit an existing?
            _selectedMarker = MarkerHitTest(clickLocation, imageCor, imageSizePixel);
            if (_selectedMarker >= 0)
                return;

            var newMarker = new PointMarker()
            {
                StartImagePoint = imageCor,
                EndImagePoint = imageCor,
                InterpolatedImageVector = imageCor
            };
            _points.Add(newMarker);
            _selectedMarker = _points.Count - 1;
        }

        public override void OnMouseMove(MarkerSet.MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == MouseLocation.NONE)
                return;

            _hoveredMarker = MarkerHitTest(clickLocation, imageCor, imageSizePixel);

            if (_selectedMarker >= 0)
            {
                _points[_selectedMarker][clickLocation] = imageCor;
                _points[_selectedMarker].InterpolatedImageVector = _points[_selectedMarker].StartImagePoint.Lerp(_points[_selectedMarker].EndImagePoint, _lastInterpolationFactor);
            }
        }

        public override void OnLeftMouseButtonUp()
        {
            _selectedMarker = -1;
        }
     
        public override void OnRightMouseButtonDown(MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == MouseLocation.NONE)
                return;

            int markerIndex = MarkerHitTest(clickLocation, imageCor, imageSizePixel);
            if (markerIndex >= 0)
            {
                _points.RemoveAt(markerIndex);
                _selectedMarker = -1;
            }
        }

        public override void UpdateInterpolation(float interpolation)
        {
            base.UpdateInterpolation(interpolation);

            foreach (var marker in _points)
                marker.InterpolatedImageVector = marker.StartImagePoint.Lerp(marker.EndImagePoint, interpolation);
        }

        public override void UpdateMarkerCanvas(Canvas[] imageCanvas, Vector[] imageOffsetPixel, Vector[] imageSizePixel)
        {
            System.Diagnostics.Debug.Assert(imageCanvas.Length == imageOffsetPixel.Length && imageOffsetPixel.Length == imageSizePixel.Length);

            for (int i = 0; i < imageCanvas.Length; ++i)
            {
                // brute force way - todo: move exiting elements (identifing by name), delete obsolte ones and create new ones
                imageCanvas[i].Children.Clear();

                for (int markerIdx = 0; markerIdx < _points.Count; ++markerIdx)
                {
                    var markerRect = new Rectangle();
                    markerRect.Width = MARKER_RENDER_SIZE;
                    markerRect.Height = MARKER_RENDER_SIZE;
                    markerRect.StrokeThickness = 2;

                    if (_selectedMarker == markerIdx)
                    {
                        markerRect.Stroke = new SolidColorBrush( Colors.Red);
                        markerRect.Fill = new SolidColorBrush(Colors.Wheat);
                    }
                    else if (_hoveredMarker == markerIdx)
                    {
                         markerRect.Stroke = new SolidColorBrush(Colors.DarkRed);
                         markerRect.Fill = new SolidColorBrush(Colors.Wheat);
                    }
                    else
                    {
                        markerRect.Stroke = new SolidColorBrush(Colors.Black);
                        markerRect.Fill = new SolidColorBrush(Colors.White);
                    }

                    markerRect.RadiusX = MARKER_RENDER_SIZE / 4;
                    markerRect.RadiusY = MARKER_RENDER_SIZE / 4;

                    Canvas.SetLeft(markerRect, _points[markerIdx][(MouseLocation)i].X * imageSizePixel[i].X + imageOffsetPixel[i].X - MARKER_RENDER_SIZE / 2);
                    Canvas.SetTop(markerRect, _points[markerIdx][(MouseLocation)i].Y * imageSizePixel[i].Y + imageOffsetPixel[i].Y - MARKER_RENDER_SIZE / 2);

                    imageCanvas[i].Children.Add(markerRect);
                }
            }
        }
    }
}
