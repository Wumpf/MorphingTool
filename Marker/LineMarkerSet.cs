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
    class LineMarkerSet : MarkerSet
    {
        public class Line
        {
            public Vector Start;
            public Vector End;

            static public Line Lerp(Line a, Line b, float interp)
            {
                return new Line()
                {
                    Start = a.Start.Lerp(b.Start, interp),
                    End = a.End.Lerp(b.End, interp)
                };
            }
        }

        public class LineMarker : Marker<Line>
        {
            public override void UpdateInterpolatedMarker(float interp)
            {
                InterpolatedMarker = Line.Lerp(StartMarker, EndMarker, interp);
            }
        }

        public IEnumerable<LineMarker> Lines
        { get { return _markerList.Cast<LineMarker>(); } }

        private int _dragedEndPoint = -1;
        private int _dragedStartPoint = -1;
        private int _hoveredStartPoint = -1;
        private int _hoveredEndPoint = -1;
        private bool _dragBoth = false;

        public override void OnLeftMouseButtonDown(MarkerSet.MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            // selection?
            if (_hoveredStartPoint >= 0 || _hoveredEndPoint >= 0)
            {
                _dragedEndPoint = _hoveredEndPoint;
                _dragedStartPoint = _hoveredStartPoint;
                return;
            }


            // add new
            _dragBoth = true;
            _dragedEndPoint = _markerList.Count;
            LineMarker newLine = new LineMarker()
            {
                StartMarker = new Line() { Start = imageCor, End = imageCor },
                EndMarker = new Line() { Start = imageCor, End = imageCor },
                InterpolatedMarker = new Line() { Start = imageCor, End = imageCor }
            };
            _markerList.Add(newLine);
        }

        public override void OnLeftMouseButtonUp()
        {
            _dragedStartPoint = -1;
            _dragedEndPoint = -1;
            _dragBoth = false;
        }

        public override void OnRightMouseButtonDown(MarkerSet.MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_dragedStartPoint >= 0)
                _markerList.RemoveAt(_dragedStartPoint);
            else if (_dragedEndPoint >= 0)
                _markerList.RemoveAt(_dragedEndPoint);
            else
            {
                int hit = PointHitTest(Lines.Select(x => x[clickLocation].End), imageCor, imageSizePixel);
                if (hit >= 0) _markerList.RemoveAt(hit);
                else
                {
                    hit = PointHitTest(Lines.Select(x => x[clickLocation].Start), imageCor, imageSizePixel);
                    if (hit >= 0)  _markerList.RemoveAt(hit);
                }
            }

            _dragedStartPoint = -1;
            _dragedEndPoint = -1;
        }

        public override void OnMouseMove(MarkerSet.MouseLocation clickLocation, Vector imageCor, Vector imageSizePixel)
        {
            if (_dragedStartPoint >= 0)
            {
                ((LineMarker)_markerList[_dragedStartPoint])[clickLocation].Start = imageCor;
                if(_dragBoth)
                    ((LineMarker)_markerList[_dragedStartPoint])[clickLocation == MouseLocation.START_IMAGE ? MouseLocation.END_IMAGE : MouseLocation.START_IMAGE].Start = imageCor;
                _markerList[_dragedStartPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
            }
            else if (_dragedEndPoint >= 0)
            {
                ((LineMarker)_markerList[_dragedEndPoint])[clickLocation].End = imageCor;
                if (_dragBoth)
                    ((LineMarker)_markerList[_dragedEndPoint])[clickLocation == MouseLocation.START_IMAGE ? MouseLocation.END_IMAGE : MouseLocation.START_IMAGE].End = imageCor;
                _markerList[_dragedEndPoint].UpdateInterpolatedMarker(_lastInterpolationFactor);
            }
            else
            {
                _hoveredEndPoint = PointHitTest(Lines.Select(x => x[clickLocation].End), imageCor, imageSizePixel); ;
                if (_hoveredEndPoint < 0)
                    _hoveredStartPoint = PointHitTest(Lines.Select(x => x[clickLocation].Start), imageCor, imageSizePixel);
                else
                    _hoveredStartPoint = -1;
            }
        }

        public override void UpdateMarkerCanvas(Canvas[] imageCanvas, Vector[] imageOffsetPixel, Vector[] imageSizePixel)
        {
            System.Diagnostics.Debug.Assert(imageCanvas.Length == imageOffsetPixel.Length && imageOffsetPixel.Length == imageSizePixel.Length);

            for (int i = 0; i < imageCanvas.Length; ++i)
            {
                MouseLocation location = (MouseLocation)i;

                // brute force way - todo: move exiting elements (identifing by name), delete obsolte ones and create new ones
                imageCanvas[i].Children.Clear();

                // arrows
                for(int markerIdx = 0; markerIdx<_markerList.Count; ++markerIdx)
                {
                    LineMarker marker = (LineMarker)_markerList[markerIdx];

                    PathGeometry pathGeometry = new PathGeometry();
                    pathGeometry.FillRule = FillRule.Nonzero;
                    PathFigure pathFigure = new PathFigure();
                    pathFigure.StartPoint = new Point(marker[location].Start.X * imageSizePixel[i].X + imageOffsetPixel[i].X,
                                                      marker[location].Start.Y * imageSizePixel[i].Y + imageOffsetPixel[i].Y);
                    pathFigure.IsClosed = true;
                    pathGeometry.Figures.Add(pathFigure);
                    LineSegment lineSegment1 = new LineSegment();
                    lineSegment1.Point = new Point(marker[location].End.X * imageSizePixel[i].X + imageOffsetPixel[i].X,
                                                   marker[location].End.Y * imageSizePixel[i].Y + imageOffsetPixel[i].Y);
                    pathFigure.Segments.Add(lineSegment1);

                    Path arrow = new Path();
                    arrow.Stretch = Stretch.Fill;
                    arrow.StrokeLineJoin = PenLineJoin.Round;
                    if (markerIdx == _dragedEndPoint || markerIdx == _dragedStartPoint)
                    {
                        arrow.Stroke = new SolidColorBrush(Colors.Red);
                        arrow.Fill = new SolidColorBrush(Colors.Wheat);
                    }
                    else if (markerIdx == _hoveredStartPoint || markerIdx == _hoveredEndPoint)
                    {
                        arrow.Stroke = new SolidColorBrush(Colors.DarkRed);
                        arrow.Fill = new SolidColorBrush(Colors.Wheat);
                    }
                    else
                    {
                        arrow.Stroke = new SolidColorBrush(Colors.Black);
                        arrow.Fill = new SolidColorBrush(Colors.White);
                    }
                    arrow.StrokeThickness = 2;
                    arrow.Data = pathGeometry;

                    Canvas.SetLeft(arrow, Math.Min(marker[location].Start.X, marker[location].End.X) * imageSizePixel[i].X + imageOffsetPixel[i].X);
                    Canvas.SetTop(arrow, Math.Min(marker[location].Start.Y, marker[location].End.Y) * imageSizePixel[i].Y + imageOffsetPixel[i].Y);
                    Canvas.SetRight(arrow, Math.Max(marker[location].Start.X, marker[location].End.X) * imageSizePixel[i].X + imageOffsetPixel[i].X);
                    Canvas.SetBottom(arrow, Math.Max(marker[location].Start.Y, marker[location].End.Y) * imageSizePixel[i].Y + imageOffsetPixel[i].Y);

                    imageCanvas[i].Children.Add(arrow);
                }

                // points
                AddPointsToCanvases(Lines.Select(x => x[location].Start), _dragedStartPoint, _hoveredStartPoint, imageCanvas[i], imageOffsetPixel[i], imageSizePixel[i]);
                AddPointsToCanvases(Lines.Select(x => x[location].End), _dragedEndPoint, _hoveredEndPoint, imageCanvas[i], imageOffsetPixel[i], imageSizePixel[i]);
            }
        }
    }
}
