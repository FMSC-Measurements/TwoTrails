using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TwoTrails.Core
{
    public class PolygonCalculator
    {
        private int polyEdges;
        private double[] polyX, polyY, constant, multiple;


        public PolygonCalculator(IEnumerable<Point> points)
        {
            if (points == null || !points.HasAtLeast(3))
            {
                throw new Exception("Insufficent number of points.");
            }

            Point temp = new Point(double.PositiveInfinity, double.PositiveInfinity);
            List<Point> nPoints = new List<Point>();
            
            foreach (Point p in points)
            {
                if (temp != p)
                {
                    nPoints.Add(p);
                }
                temp = p;
            }

            if (nPoints.Count < 3)
            {
                throw new Exception("Input points are not a polygon.");
            }
            
            polyEdges = nPoints.Count;
            polyX = new double[polyEdges];
            polyY = new double[polyEdges];
            constant = new double[polyEdges];
            multiple = new double[polyEdges];

            for (int k = 0; k < polyEdges; k++)
            {
                temp = nPoints[k];
                polyX[k] = temp.X;
                polyY[k] = temp.Y;
            }

            int i, j = polyEdges - 1;

            for (i = 0; i < polyEdges; i++)
            {
                if (polyY[j] == polyY[i])
                {
                    constant[i] = polyX[i];
                    multiple[i] = 0;
                }
                else
                {
                    constant[i] = polyX[i] -
                            (polyY[i] * polyX[j]) / (polyY[j] - polyY[i]) +
                            (polyY[i] * polyX[i]) / (polyY[j] - polyY[i]);

                    multiple[i] = (polyX[j] - polyX[i]) / (polyY[j] - polyY[i]);
                }

                j = i;
            }
        }


        public bool IsPointInPolygon(Point point)
        {
            return IsPointInPolygon(point.X, point.Y);
        }

        public bool IsPointInPolygon(double x, double y)
        {
            int i, j = polyEdges - 1;
            bool oddNodes = false;

            for (i = 0; i < polyEdges; i++)
            {
                if ((polyY[i] < y && polyY[j] >= y || polyY[j] < y && polyY[i] >= y))
                {
                    oddNodes ^= (y * multiple[i] + constant[i] < x);
                }
                j = i;
            }

            return oddNodes;
        }


        public double ShortestDistanceFromPolygonEdge(Point point)
        {
            return ShortestDistanceToPolygonEdge(point.X, point.Y);
        }

        public double ShortestDistanceToPolygonEdge(double x, double y)
        {
            double shortestDist = double.MaxValue, temp;

            for (int i = 0; i < polyX.Length - 1; i++)
            {
                temp = MathEx.DistanceToLine(x, y, polyX[i], polyY[i], polyX[i + 1], polyY[i + 1]);
                if (temp < shortestDist)
                {
                    shortestDist = temp;
                }
            }

            return shortestDist;
        }


        private Boundaries _PointBoundaries = null;
        public Boundaries PointBoundaries
        {
            get
            {
                if (_PointBoundaries == null)
                {
                    double top, bottom, left, right, x, y;

                    left = right = polyX[0];
                    top = bottom = polyY[0];

                    for (int i = 1; i < polyEdges; i++)
                    {
                        x = polyX[i];
                        y = polyY[i];

                        if (y > top)
                            top = y;

                        if (y < bottom)
                            bottom = y;

                        if (x < left)
                            left = x;

                        if (x > right)
                            right = x;
                    }

                    _PointBoundaries = new Boundaries(top, left, bottom, right); 
                }

                return _PointBoundaries;
            }
        }

        public class Boundaries
        {
            public Point TopLeft { get; }
            public Point BottomRight { get; }

            public Boundaries(Point topLeft, Point bottomRight)
            {
                TopLeft = topLeft;
                BottomRight = bottomRight;
            }

            public Boundaries(double top, double left, double bottom, double right)
            {
                TopLeft = new Point(left, top);
                BottomRight = new Point(right, bottom);
            }
        }
    }
}
