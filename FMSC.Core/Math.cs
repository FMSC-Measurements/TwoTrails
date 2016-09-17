using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FMSC.Core
{
    public static class MathEx
    {
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
    }
}
