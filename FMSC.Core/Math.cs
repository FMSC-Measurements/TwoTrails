using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FMSC.Core
{
    public static class MathEx
    {
        public static double Distance(Point p1, Point p2)
        {
            return Distance(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }


        public static Point RotatePoint(Point point, double angle, Point rPoint)
        {
            return RotatePoint(point.X, point.Y, angle, rPoint.X, rPoint.Y);
        }

        public static Point RotatePoint(double x, double y, double angle, double rX, double rY)
        {
            return new Point(
                    (Math.Cos(angle) * (x - rX) - Math.Sin(angle) * (y - rY) + rX),
                    (Math.Sin(angle) * (x - rX) + Math.Cos(angle) * (y - rY) + rY));
        }


        //Compute the dot product AB . AC
        private static double DotProduct(Point pointA, Point pointB, Point pointC)
        {
            Point AB = new Point(), BC = new Point();
            AB.X = pointB.X - pointA.X;
            AB.Y = pointB.Y - pointA.Y;
            BC.X = pointC.X - pointB.X;
            BC.Y = pointC.Y - pointB.Y;
            return AB.X * BC.X + AB.Y * BC.Y;
        }

        //Compute the cross product AB x AC
        private static double CrossProduct(Point pointA, Point pointB, Point pointC)
        {
            Point AB = new Point(), AC = new Point();
            AB.X = pointB.X - pointA.X;
            AB.Y = pointB.Y - pointA.Y;
            AC.X = pointC.X - pointA.X;
            AC.Y = pointC.Y - pointA.Y;
            return AB.X * AC.Y - AB.Y * AC.X;
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        public static double LineToPointDistance2D(Point pointA, Point pointB, Point pointC, bool isSegment = true)
        {
            double dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                double dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                double dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }
            return Math.Abs(dist);
        }
    }
}
