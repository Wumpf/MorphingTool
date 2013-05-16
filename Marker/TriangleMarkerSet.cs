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
        public class Vertex : Marker<Vector>
        {
            public override void UpdateInterpolatedMarker(float interp)
            {
                InterpolatedMarker = StartMarker.Lerp(EndMarker, interp);
                TriangulationPosition = StartMarker.Lerp(EndMarker, 0.5f);
            }

            /// <summary>
            /// position used for triangulation
            /// </summary>
            public Vector TriangulationPosition;

            /// <summary>
            /// triangles that use this vertex
            /// </summary>
            public List<Triangle> AssociatedTriangles = new List<Triangle>();
        };

        public class Triangle
        {
            public Vertex vertexA;
            public Vertex vertexB;
            public Vertex vertexC;
        }

        private List<Triangle> _triangles = new List<Triangle>();

        public IEnumerable<Vertex> Points
        { get { return _markerList.Cast<Vertex>(); } }

        private const int NUM_NONEDITABLE_MARKER = 4;

        private int _selectedMarker = -1;
        private int _hoveredMarker = -1;

        public TriangleMarkerSet()
        {
            _markerList.Add(new Vertex() { StartMarker = new Vector(0, 0), EndMarker = new Vector(0, 0) });
            _markerList.Add(new Vertex() { StartMarker = new Vector(1, 0), EndMarker = new Vector(1, 0) });
            _markerList.Add(new Vertex() { StartMarker = new Vector(1, 1), EndMarker = new Vector(1, 1) });
            _markerList.Add(new Vertex() { StartMarker = new Vector(0, 1), EndMarker = new Vector(0, 1) });

            _triangles.Add(new Triangle() { vertexA = (Vertex)_markerList[0], vertexB = (Vertex)_markerList[1], vertexC = (Vertex)_markerList[3] });
            _triangles.Add(new Triangle() { vertexA = (Vertex)_markerList[1], vertexB = (Vertex)_markerList[2], vertexC = (Vertex)_markerList[3] });

            foreach (var triangle in _triangles)
            {
                triangle.vertexA.AssociatedTriangles.Add(triangle);
                triangle.vertexB.AssociatedTriangles.Add(triangle);
                triangle.vertexC.AssociatedTriangles.Add(triangle);
            }
            foreach (var marker in _markerList)
                marker.UpdateInterpolatedMarker(0.0f);
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

            var newMarker = new Vertex()
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
                ((Vertex)_markerList[_selectedMarker])[clickLocation] = imageCor;
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

            // triangle lines
            for (int triangleIndex = 0; triangleIndex < _triangles.Count; ++triangleIndex)
            {
                var triangle = new Polygon();
                triangle.Points.Add(new Point(_triangles[triangleIndex].vertexA[location].X, _triangles[triangleIndex].vertexA[location].Y));
                triangle.Points.Add(new Point(_triangles[triangleIndex].vertexB[location].X, _triangles[triangleIndex].vertexB[location].Y));
                triangle.Points.Add(new Point(_triangles[triangleIndex].vertexC[location].X, _triangles[triangleIndex].vertexC[location].Y));
                triangle.Fill = Brushes.Transparent;
                triangle.Width = imageSizePixel.X;
                triangle.Height = imageSizePixel.Y;
                triangle.Stretch = Stretch.Fill;
                triangle.Stroke = Brushes.White;
                triangle.StrokeThickness = 2;

                Canvas.SetLeft(triangle, imageOffsetPixel.X);
                Canvas.SetTop(triangle, imageOffsetPixel.Y);

                imageCanvas.Children.Add(triangle);
            }

            AddPointsToCanvases(Points.Select(x => x[location]), _selectedMarker, _hoveredMarker, imageCanvas, imageOffsetPixel, imageSizePixel);
        }
    }
}