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
    /// <summary>
    /// Abstract description for feature markers in start and end image.
    /// Every MarkerSet also keeps it current set of interpolated markers.
    /// </summary>
    public abstract class MarkerSet
    {
        /// <summary>
        /// Marker interface for basic functions.
        /// </summary>
        public abstract class IMarker
        {
            public abstract void UpdateInterpolatedMarker(float interp);
        }

        /// <summary>
        /// Abstract Marker Template. Provides unified access functions
        /// </summary>
        /// <typeparam name="T">Type of Marker per Image.</typeparam>
        public abstract class Marker<T> : IMarker
        {
            public T StartMarker;
            public T EndMarker;
            public T InterpolatedMarker;

            /// <summary>
            /// Access by index.
            /// </summary>
            /// <param name="element">0: StartImagePoint
            /// 1: EndImagePoint
            /// 2: InterpolatedImageVector</param>
            /// <returns>Relative image position value.</returns>
            public T this[Location element]
            {
                get
                {
                    switch (element)
                    {
                        case Location.START_IMAGE:
                            return StartMarker;
                        case Location.END_IMAGE:
                            return EndMarker;
                        case Location.OUTPUT_IMAGE:
                            return InterpolatedMarker;
                    }
                    throw new Exception("PointMarker has only 3 Elements!");
                }
                set
                {
                    switch (element)
                    {
                        case Location.START_IMAGE:
                            StartMarker = value;
                            break;
                        case Location.END_IMAGE:
                            EndMarker = value;
                            break;
                        case Location.OUTPUT_IMAGE:
                            InterpolatedMarker = value;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Intern list of all markers.
        /// </summary>
        protected List<IMarker> _markerList = new List<IMarker>();

        /// <summary>
        /// Last used interpolation factor. Will be kept to determine the interpolated position of new Markers.
        /// </summary>
        protected float _lastInterpolationFactor;

        /// <summary>
        /// Possible Location for mouse actions
        /// </summary>
        public enum Location
        {
            START_IMAGE = 0,
            END_IMAGE = 1,
            OUTPUT_IMAGE = 2,
            NONE = -1
        }

        /// <summary>
        /// Removes all markers
        /// </summary>
        public virtual void ClearMarkers()
        {
            _markerList.Clear();
        }
        
        /// <summary>
        /// Performs action for left mouse button down on one of the 3 image areas. Usually sets a Marker.
        /// </summary>
        /// <param name="clickLocation">Clicked image type.</param>
        /// <param name="imageCor">Relative coordinates on the given image.</param>
        /// <param name="imageSizePixel">Size of the clicked image in pixel</param>
        public abstract void OnLeftMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel);

        /// <summary>
        /// Performs action for left mouse button up on one of the 3 image areas. Usually releases a Marker.
        /// </summary>
        public abstract void OnLeftMouseButtonUp();

        /// <summary>
        /// Performs action for right mouse click on one of the 3 image areas. Removes a marker
        /// </summary>
        /// <param name="clickLocation">Clicked image type.</param>
        /// <param name="imageCor">Relative coordinates on the given image.</param>
        /// <param name="imageSizePixel">Size of the clicked image in pixel</param>
        public abstract void OnRightMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel);

        /// <summary>
        /// Performs action for mouse movements on one of the 3 image areas. Moves a marker or applies hovering effects.
        /// </summary>
        /// <param name="clickLocation">Clicked image type.</param>
        /// <param name="imageCor">Relative coordinates on the given image.</param>
        /// <param name="imageSizePixel">Size of the clicked image in pixel</param>
        /// <returns>true if any marker has changed its position</returns>
        public abstract bool OnMouseMove(Location clickLocation, Vector imageCor, Vector imageSizePixel);

        /// <summary>
        /// Updates the coordinates/representation of the interpolated marker
        /// </summary>
        /// <param name="interpolation">Interpolation from 0 (startimage) to 1 (endimage)</param>
        public virtual void UpdateInterpolation(float interpolation)
        {
            _lastInterpolationFactor = interpolation;
            foreach (var marker in _markerList)
                marker.UpdateInterpolatedMarker(interpolation);
        }

        /// <summary>
        /// Updates objects on given canvas. This means, this function will update the rendering of the marker.
        /// Attention: All objects on the Canvas will be deleted!
        /// </summary>
        public abstract void UpdateMarkerCanvas(Location location, Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel);

        /// <summary>
        /// Basic pixel size value for marker rendering.
        /// </summary>
        protected const int MARKER_RENDER_SIZE = 10;

        /// <summary>
        /// Adds point marker renderings to a canvas.
        /// </summary>
        protected void AddPointsToCanvases(IEnumerable<Vector> pointsPerCanvas, int selectedPoint, int hoveredPoint,
                                            Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel)
        {
            int markerIdx = 0;
            foreach(Vector point in pointsPerCanvas)
            {
                var markerRect = new Rectangle();
                markerRect.Width = MARKER_RENDER_SIZE;
                markerRect.Height = MARKER_RENDER_SIZE;
                markerRect.StrokeThickness = 2;

                if (selectedPoint == markerIdx)
                {
                    markerRect.Stroke = new SolidColorBrush(Colors.Red);
                    markerRect.Fill = new SolidColorBrush(Colors.Wheat);
                }
                else if (hoveredPoint == markerIdx)
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

                Canvas.SetLeft(markerRect, point.X * imageSizePixel.X + imageOffsetPixel.X - MARKER_RENDER_SIZE / 2);
                Canvas.SetTop(markerRect, point.Y * imageSizePixel.Y + imageOffsetPixel.Y - MARKER_RENDER_SIZE / 2);

                imageCanvas.Children.Add(markerRect);

                ++markerIdx;
            }
        }

        /// <summary>
        /// Performs hit test with a set of given marker renderings.
        /// </summary>
        protected int PointHitTest(IEnumerable<Vector> points, Vector imageCor, Vector imageSizePixel)
        {
            Vector halfMarkerSize = new Vector(MARKER_RENDER_SIZE / imageSizePixel.X, MARKER_RENDER_SIZE / imageSizePixel.Y) * 0.5f;

            // delete Vectors!
            // find corresonding
            int i = 0;
            foreach(Vector point in points)
            {
                if (imageCor.IsInRectangle(point - halfMarkerSize, point + halfMarkerSize))
                    return i;
                ++i;
            }

            return -1;
        }
    }
}
