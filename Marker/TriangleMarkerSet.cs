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
    class TriangleMarkerSet : MarkerSet
    {
        public class PointMarker : Marker<Vector>
        {
            public override void UpdateInterpolatedMarker(float interp)
            {
                InterpolatedMarker = StartMarker.Lerp(EndMarker, interp);
            }
        };

        public IEnumerable<PointMarker> Points
        { get { return _markerList.Cast<PointMarker>(); } }

        private const int NUM_NONEDITABLE_MARKER = 4;

        private int _selectedMarker = -1;
        private int _hoveredMarker = -1;

        public TriangleMarkerSet()
        {
            _markerList.Add(new PointMarker() { StartMarker = new Vector(0, 0), EndMarker = new Vector(0, 0), InterpolatedMarker = new Vector(0, 0) });
            _markerList.Add(new PointMarker() { StartMarker = new Vector(1, 0), EndMarker = new Vector(1, 0), InterpolatedMarker = new Vector(1, 0) });
            _markerList.Add(new PointMarker() { StartMarker = new Vector(1, 1), EndMarker = new Vector(1, 1), InterpolatedMarker = new Vector(1, 1) });
            _markerList.Add(new PointMarker() { StartMarker = new Vector(0, 1), EndMarker = new Vector(0, 1), InterpolatedMarker = new Vector(0, 1) });
        }

        /// <summary>
        /// Checks if a click is on a marker-point
        /// </summary>
        /// <returns>-1 if none, otherwise markerindex</returns>
        private int MarkerHitTest(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == Location.START_IMAGE)
                return PointHitTest(Points.Select(x => x.StartMarker), imageCor, imageSizePixel);
            else if (clickLocation == Location.END_IMAGE)
                return PointHitTest(Points.Select(x => x.EndMarker), imageCor, imageSizePixel);
            else
                return -1;
        }

        public override void OnLeftMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == Location.NONE)
                return;

            // hit an existing?
            _selectedMarker = MarkerHitTest(clickLocation, imageCor, imageSizePixel);
            if (_selectedMarker >= 0)
                return;

            var newMarker = new PointMarker()
            {
                StartMarker = imageCor,
                EndMarker = imageCor,
                InterpolatedMarker = imageCor
            };
            _selectedMarker = _markerList.Count;
            _markerList.Add(newMarker);
        }

        public override bool OnMouseMove(MarkerSet.Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == Location.NONE)
                return false;

            _hoveredMarker = MarkerHitTest(clickLocation, imageCor, imageSizePixel);

            if (_selectedMarker >= NUM_NONEDITABLE_MARKER)
            {
                ((PointMarker)_markerList[_selectedMarker])[clickLocation] = imageCor;
                _markerList[_selectedMarker].UpdateInterpolatedMarker(_lastInterpolationFactor);
                return true;
            }
            return false;
        }

        public override void OnLeftMouseButtonUp()
        {
            _selectedMarker = -1;
        }

        public override void OnRightMouseButtonDown(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == Location.NONE)
                return;

            int markerIndex = MarkerHitTest(clickLocation, imageCor, imageSizePixel);
            if (markerIndex >= NUM_NONEDITABLE_MARKER)
            {
                _markerList.RemoveAt(markerIndex);
                _selectedMarker = -1;
            }
        }

        public override void UpdateMarkerCanvas(Location location, Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel)
        {
            // brute force way - todo: move exiting elements (identifing by name), delete obsolte ones and create new ones
            imageCanvas.Children.Clear();

            AddPointsToCanvases(Points.Select(x => x[location]), _selectedMarker, _hoveredMarker, imageCanvas, imageOffsetPixel, imageSizePixel);
        }
    }
}