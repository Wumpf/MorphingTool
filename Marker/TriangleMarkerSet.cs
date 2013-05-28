using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

/// Defines a a Triangle Marker
using TriangleMarker = MIConvexHull.DefaultTriangulationCell<MorphingTool.TriangleMarkerSet.Vertex>;

namespace MorphingTool
{
    class TriangleMarkerSet : MarkerSet
    {
        /// <summary>
        /// A triangle vertex
        /// </summary>
        public class Vertex : Marker<Vector>, MIConvexHull.IVertex
        {
            public override void UpdateInterpolatedMarker(float interp)
            {
                InterpolatedMarker = StartMarker.Lerp(EndMarker, interp);
                Vector pos = StartMarker.Lerp(EndMarker, 0.5f);
                _triangulationPosition[0] = pos.X;
                _triangulationPosition[1] = pos.Y;
            }

            /// <summary>
            /// position used for triangulation
            /// </summary>
            public double[] _triangulationPosition = new double[2];

            /// <summary>
            /// position method for MIConvexHull.IVertex
            /// </summary>
            public double[] Position
            {
                get { return _triangulationPosition; }
                set { _triangulationPosition = value; }
            }
        };

        /// <summary>
        /// Markerlist of this markerset
        /// </summary>
        public IEnumerable<TriangleMarker> Triangles
        { get { return _triangles; } }
        private IEnumerable<TriangleMarker> _triangles = new List<TriangleMarker>();


        private IEnumerable<Vertex> Vertices
        { get { return _markerList.Cast<Vertex>(); } }

        private const int NUM_NONEDITABLE_MARKER = 4;

        private int _selectedMarker = -1;
        private int _hoveredMarker = -1;

        public TriangleMarkerSet()
        {
            // fixed corner points with random offset to avaid triangulation border cases
            Random rand = new Random();
            double offset = rand.NextDouble() * 0.001;
            _markerList.Add(new Vertex() { StartMarker = new Vector(0, -offset), EndMarker = new Vector(0, -offset) });
            offset = rand.NextDouble() * 0.001;
            _markerList.Add(new Vertex() { StartMarker = new Vector(1.0 + offset, 0), EndMarker = new Vector(1 + offset, 0) });
            offset = rand.NextDouble() * 0.001;
            _markerList.Add(new Vertex() { StartMarker = new Vector(1, 1 + offset), EndMarker = new Vector(1, 1 + offset) });
            offset = rand.NextDouble() * 0.001;
            _markerList.Add(new Vertex() { StartMarker = new Vector(0 - offset, 1), EndMarker = new Vector(0 - offset, 1) });

            foreach (var marker in _markerList)
                marker.UpdateInterpolatedMarker(0.0f);

            // delauny triangulation!
            _triangles = MIConvexHull.DelaunayTriangulation<Vertex, TriangleMarker>.Create(Vertices).Cells;
        }


        /// <summary>
        /// Removes all markers
        /// </summary>
        override public void ClearMarkers()
        {
            _markerList.RemoveRange(NUM_NONEDITABLE_MARKER, _markerList.Count - NUM_NONEDITABLE_MARKER);
            _triangles = MIConvexHull.DelaunayTriangulation<Vertex, TriangleMarker>.Create(Vertices).Cells;
        }

        /// <summary>
        /// Checks if a click is on a marker-point
        /// </summary>
        /// <returns>-1 if none, otherwise markerindex</returns>
        private int MarkerHitTest(Location clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (clickLocation == Location.START_IMAGE)
                return PointHitTest(Vertices.Select(x => x.StartMarker), imageCor, imageSizePixel);
            else if (clickLocation == Location.END_IMAGE)
                return PointHitTest(Vertices.Select(x => x.EndMarker), imageCor, imageSizePixel);
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
            };
            newMarker.UpdateInterpolatedMarker(_lastInterpolationFactor);

            // add marker
            _selectedMarker = _markerList.Count;
            _markerList.Add(newMarker);

            // recompute delauny triangulation!
            _triangles = MIConvexHull.DelaunayTriangulation<Vertex, TriangleMarker>.Create(Vertices).Cells;
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

                // recompute delauny triangulation!
                _triangles = MIConvexHull.DelaunayTriangulation<Vertex, TriangleMarker>.Create(Vertices).Cells;

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

                // recompute delauny triangulation!
                _triangles = MIConvexHull.DelaunayTriangulation<Vertex, TriangleMarker>.Create(Vertices).Cells;
            }
        }

        public override void UpdateMarkerCanvas(Location location, Canvas imageCanvas, Vector imageOffsetPixel, Vector imageSizePixel)
        {
            // brute force way - todo: move exiting elements (identifing by name), delete obsolte ones and create new ones
            imageCanvas.Children.Clear();

            // triangle lines
            foreach(var triangle in _triangles)
            {
                Polygon canvasTriangle = new Polygon();
                Point posA = new Point(triangle.Vertices[0][location].X * imageSizePixel.X + imageOffsetPixel.X, triangle.Vertices[0][location].Y * imageSizePixel.Y + imageOffsetPixel.Y);
                Point posB = new Point(triangle.Vertices[1][location].X * imageSizePixel.X + imageOffsetPixel.X, triangle.Vertices[1][location].Y * imageSizePixel.Y + imageOffsetPixel.Y);
                Point posC = new Point(triangle.Vertices[2][location].X * imageSizePixel.X + imageOffsetPixel.X, triangle.Vertices[2][location].Y * imageSizePixel.Y + imageOffsetPixel.Y);
                canvasTriangle.Points.Add(posA);
                canvasTriangle.Points.Add(posB);
                canvasTriangle.Points.Add(posC);
                canvasTriangle.Fill = Brushes.Transparent;
                canvasTriangle.Stretch = Stretch.Fill;
                canvasTriangle.Stroke = Brushes.White;
                canvasTriangle.StrokeThickness = 2;

                Canvas.SetLeft(canvasTriangle, Math.Min(Math.Min(posA.X, posB.X), posC.X));
                Canvas.SetTop(canvasTriangle, Math.Min(Math.Min(posA.Y, posB.Y), posC.Y));
                Canvas.SetRight(canvasTriangle, Math.Max(Math.Max(posA.X, posB.X), posC.X));
                Canvas.SetBottom(canvasTriangle, Math.Max(Math.Max(posA.Y, posB.Y), posC.Y));

                imageCanvas.Children.Add(canvasTriangle);
            }

            AddPointsToCanvases(Vertices.Select(x => x[location]), _selectedMarker, _hoveredMarker, imageCanvas, imageOffsetPixel, imageSizePixel);
        }
    }
}