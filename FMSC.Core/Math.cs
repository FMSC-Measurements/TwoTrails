using System;
using System.Windows;

namespace FMSC.Core
{
    public static class MathEx
    {
        public static double Distance(Point p1, Point p2)
        {
            return Distance(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static double Distance(double aX, double aY, double bX, double bY)
        {
            return Math.Sqrt(Math.Pow(bX - aX, 2) + Math.Pow(bY - aY, 2));
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
            return DotProduct(pointA.X, pointA.Y, pointB.X, pointB.Y, pointC.X, pointC.Y);
        }
        
        //Compute the dot product AB . AC
        private static double DotProduct(double aX, double aY, double bX, double bY, double cX, double cY)
        {
            return (bX - aX) * (cX - bX) + (bY - aY) * (cY - bY);
        }


        //Compute the cross product AB x AC
        private static double CrossProduct(Point pointA, Point pointB, Point pointC)
        {
            return CrossProduct(pointA.X, pointA.Y, pointB.X, pointB.Y, pointC.X, pointC.Y);
        }

        //Compute the cross product AB x AC
        private static double CrossProduct(double aX, double aY, double bX, double bY, double cX, double cY)
        {
            return (bX - aX) * (cY - aY) - bY - aY * (cX - aX);
        }
        

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        public static double LineToPointDistance2D(Point pointA, Point pointB, Point pointC, bool isSegment = true)
        {
            return LineToPointDistance2D(pointA.X, pointA.Y, pointB.X, pointB.Y, pointC.X, pointC.Y, isSegment);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        public static double LineToPointDistance2D(double aX, double aY, double bX, double bY, double cX, double cY, bool isSegment = true)
        {
            double dist = CrossProduct(aX, aY, bX, bY, cX, cY) / Distance(aX, aY, bX, bY);
            if (isSegment)
            {
                double dot1 = DotProduct(aX, aY, bX, bY, cX, cY);
                if (dot1 > 0)
                    return Distance(bX, bY, cX, cY);

                double dot2 = DotProduct(aX, aY, bX, bY, cX, cY);
                if (dot2 > 0)
                    return Distance(aX, aY, cX, cY);
            }
            return Math.Abs(dist);
        }
    }
}
