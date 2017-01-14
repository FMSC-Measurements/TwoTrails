using CSUtil;
using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TwoTrails.Core
{
    public class PolygonCalculator
    {
        private int polyCorners;
        private double[] polyX, polyY, constant, multiple;

        public PolygonCalculator(IEnumerable<Point> points)
        {
            if (points == null || !points.HasAtLeast(3))
            {
                throw new Exception("Insufficent number of points.");
            }

            Point current = points.First(), temp = new Point(double.PositiveInfinity, double.PositiveInfinity);
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


            polyCorners = nPoints.Count;
            polyX = new double[polyCorners];
            polyY = new double[polyCorners];
            constant = new double[polyCorners];
            multiple = new double[polyCorners];

            for (int k = 0; k < polyCorners; k++)
            {
                temp = nPoints[k];
                polyX[k] = temp.X;
                polyY[k] = temp.Y;
            }

            int i, j = polyCorners - 1;

            for (i = 0; i < polyCorners; i++)
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
            int i, j = polyCorners - 1;
            bool oddNodes = false;

            for (i = 0; i < polyCorners; i++)
            {
                if ((polyY[i] < y && polyY[j] >= y || polyY[j] < y && polyY[i] >= y))
                {
                    oddNodes ^= (y * multiple[i] + constant[i] < x);
                }
                j = i;
            }

            return oddNodes;
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

                    for (int i = 1; i < polyCorners; i++)
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
