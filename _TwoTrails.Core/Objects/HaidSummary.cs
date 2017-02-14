using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class PolygonSummary
    {
        private TtPoint _LastTtPoint, _LastTtBndPt, _LastTravPoint, _LastGpsPoint;

        private List<TtLeg> _Legs = new List<TtLeg>();

        public ReadOnlyCollection<TtLeg> Legs { get; }

        private bool traversing = false;

        public TtPolygon Polygon { get; }
        public HaidResult Result { get; }

        public double TotalGpsError { get; private set; } = 0;
        public double TotalTraverseError { get; private set; } = 0;
        public double TotalTraverseLength { get; private set; } = 0;
        public int TotalTraverseSegments { get; private set; } = 0;
        public double GpsAreaError { get; private set; } = 0;
        public double TraverseAreaError { get; private set; } = 0;

        private double travLength = 0;
        private int travSegments = 0;

        public String SummaryText { get; }


        public PolygonSummary(ITtManager manager, TtPolygon polygon, bool showPoints = false)
        {
            Polygon = polygon;
            Legs = new ReadOnlyCollection<TtLeg>(_Legs);

            List<TtPoint> allPoints = manager.GetPoints(polygon.CN);

            if (allPoints.Count > 2)
            {
                IEnumerable<TtPoint> points = allPoints;//.Where(p => p.OnBoundary);
                if (points.Count() > 2)
                {
                    StringBuilder sbPoints = new StringBuilder();

                    foreach (TtPoint point in points)
                        ProcessPoint(sbPoints, point, false, showPoints);

                    TtPoint sp = points.First();
                    foreach (TtPoint p in points)
                    {
                        if (p.OnBoundary)
                        {
                            sp = p;
                            break;
                        }
                    }

                    if (!sp.HasSameAdjLocation(_LastTtBndPt))
                        _Legs.Add(new TtLeg(_LastTtBndPt, sp));

                    double perim = 0;
                    foreach (TtLeg leg in _Legs.Where(l => l is TtLeg))
                    {
                        TotalGpsError += leg.Error;
                        perim += leg.LegLength;
                    }

                    StringBuilder sb = new StringBuilder();
                    
                    sb.AppendFormat("The polygon area is: {0:0.000} Ha ({1:0.00} ac).{2}",
                        Math.Round(polygon.AreaHectaAcres, 2),
                        Math.Round(polygon.AreaAcres, 2),
                        Environment.NewLine);

                    sb.AppendFormat("The polygon exterior perimeter is: {0:0.00} M ({1:0} ft).{2}",
                        Math.Round(polygon.Perimeter, 2),
                        Math.Round(polygon.PerimeterFt, 0),
                        Environment.NewLine);

                    sb.AppendFormat("The polyline perimeter is: {0:0.00} M ({1:0} ft).{2}{2}",
                        Math.Round(polygon.PerimeterLine, 2),
                        Math.Round(polygon.PerimeterLineFt, 0),
                        Environment.NewLine);

                    if (TotalGpsError > Consts.MINIMUM_POINT_DIGIT_ACCURACY)
                    {
                        sb.AppendFormat("GPS Contribution: {0:0.000} Ha ({1:0.00} ac){2}",
                            Math.Round(FMSC.Core.Convert.MetersSquaredToHa(TotalGpsError), 2),
                            Math.Round(FMSC.Core.Convert.MetersSquaredToAcres(TotalGpsError), 2),
                            Environment.NewLine);

                        GpsAreaError = TotalGpsError / polygon.Area * 100.0;
                        sb.AppendFormat("GPS Contribution Ratio of area-error-area to area is: {0:0.00}%.{1}{1}",
                            Math.Round(GpsAreaError, 2),
                            Environment.NewLine);
                    }

                    if (TotalTraverseError > Consts.MINIMUM_POINT_DIGIT_ACCURACY)
                    {
                        sb.AppendFormat("Traverse Contribution: {0:0.000} Ha ({1:0.00} ac){2}",
                            Math.Round(FMSC.Core.Convert.MetersSquaredToHa(TotalTraverseError), 2),
                            Math.Round(FMSC.Core.Convert.MetersSquaredToAcres(TotalTraverseError), 2),
                            Environment.NewLine);

                        TraverseAreaError = TotalTraverseError / polygon.Area * 100.0;
                        sb.AppendFormat("Traverse Contribution Ratio of area-error-area to area is: {0:0.00}%.{1}{1}",
                            Math.Round(TraverseAreaError, 2),
                            Environment.NewLine);
                    }

                    sb.Append(sbPoints.ToString());

                    SummaryText = sb.ToString();

                    sbPoints.Clear();
                    sb.Clear();
                    _LastTtPoint = _LastTtBndPt = _LastTravPoint = _LastGpsPoint = null;
                }
                else
                {
                    Result = HaidResult.InsufficientPoints;
                    SummaryText = "Polygon has Insufficient Points\n\n";
                }
            }
            else
            {
                Result = HaidResult.Empty;
                SummaryText = "Polygon has Insufficient Points\n\n";
            }
        }
        

        private void ProcessPoint(StringBuilder sbPoints, TtPoint point, bool fromQndm = false, bool showPoints = false)
        {
            switch (point.OpType)
            {
                case OpType.GPS:
                case OpType.Take5:
                case OpType.Walk:
                case OpType.WayPoint:
                    {
                        if (point.OnBoundary)
                        {
                            if (traversing)
                            {
                                CloseTraverse(sbPoints, point);
                            }
                            else if (_LastTtBndPt != null)
                                _Legs.Add(new TtLeg(_LastTtBndPt, point));

                            _LastTtBndPt = point;
                        }

                        if (!fromQndm && showPoints)
                        {
                            sbPoints.AppendFormat("Point {0}: {1} {2}- ", point.PID, point.OnBoundary ? " " : "*", point.OpType);
                            sbPoints.AppendFormat("Accuracy is {0:0.00#} meters.{1}", point.Accuracy, Environment.NewLine);
                        }

                        _LastGpsPoint = point;
                    }
                    break;
                case OpType.Traverse:
                    {
                        if (_LastTtPoint != null)
                        {
                            if (point.OnBoundary)
                            {
                                if (traversing)
                                {
                                    travLength += MathEx.Distance(_LastTtPoint.UnAdjX, _LastTtPoint.UnAdjY, point.UnAdjX, point.UnAdjY);

                                    if (_LastTtBndPt != null)
                                    {
                                        _Legs.Add(new TtLeg(_LastTravPoint, point));
                                    }
                                }
                                else
                                {
                                    travSegments = 0;
                                    travLength = MathEx.Distance(_LastTtPoint.UnAdjX, _LastTtPoint.UnAdjY, point.UnAdjX, point.UnAdjY);
                                    traversing = true;

                                    if (showPoints && !fromQndm)
                                    {
                                        sbPoints.AppendFormat("Traverse Start:{0}", Environment.NewLine);
                                    }

                                    if (_LastTtBndPt != null)
                                    {
                                        _Legs.Add(new TtLeg(_LastTtBndPt, point));
                                    }
                                }
                                
                                _LastTtBndPt = point;
                            }

                            travSegments++;
                        }

                        _LastTravPoint = point;
                    }
                    break;
                case OpType.SideShot:
                    {
                        if (showPoints && !fromQndm)
                        {
                            sbPoints.AppendFormat("Point {0}: {1} SideShot off Point {2}.{3}",
                                    point.PID, point.OnBoundary ? " " : "*", _LastGpsPoint.PID, Environment.NewLine);
                        }

                        if (_LastTtBndPt != null && point.OnBoundary || fromQndm)
                        {
                            _Legs.Add(new TtLeg(_LastTtBndPt, point));
                        }
                        
                        if (point.OnBoundary)
                        {
                            _LastTtBndPt = point;
                        }
                    }
                    break;
                case OpType.Quondam:
                    {
                        QuondamPoint qp = (QuondamPoint)point;

                        if (qp.ParentPoint.OpType == OpType.Traverse && _LastTtPoint != null)
                        {
                            if (traversing)
                            {
                                CloseTraverse(sbPoints, point);
                            }
                            else
                            {
                                if (_LastTtBndPt != null)
                                {
                                    _Legs.Add(new TtLeg(_LastTtBndPt, qp.ParentPoint));
                                }
                            }
                        }
                        else
                        {
                            ProcessPoint(sbPoints, qp.ParentPoint, true, showPoints);
                        }

                        if (showPoints)
                        {
                            sbPoints.AppendFormat("Point {0}: {1} Quondam to Point {2} ({3}){4}.{5}", point.PID,
                                point.OnBoundary ? " " : "*", qp.ParentPoint.PID, qp.ParentPoint.OpType,
                                qp.Polygon.Name != qp.ParentPoint.Polygon.Name ?
                                    $" in {qp.ParentPoint.Polygon.Name}" : String.Empty,
                                Environment.NewLine);
                        }

                        if (point.OnBoundary)
                        {
                            _LastTtBndPt = point;
                        }
                    }
                    break;
            }

            _LastTtPoint = point;
        }

        private void CloseTraverse(StringBuilder sbPoints, TtPoint point)
        {
            double closeError = MathEx.Distance(_LastTtPoint.UnAdjX, _LastTtPoint.UnAdjY, point.UnAdjX, point.UnAdjY);

            double travError = closeError < Consts.MINIMUM_POINT_DIGIT_ACCURACY ? Double.PositiveInfinity : travLength / closeError;

            sbPoints.AppendFormat("   Traverse Total Segments: {0}{1}", travSegments, Environment.NewLine);
            sbPoints.AppendFormat("   Traverse Total Distance: {0:0.00} feet.{1}", Math.Round(FMSC.Core.Convert.ToFeetTenths(travLength, Distance.Meters), 2), Environment.NewLine);
            sbPoints.AppendFormat("   Traverse Closing Distance: {0:0.00} feet.{1}", Math.Round(FMSC.Core.Convert.ToFeetTenths(closeError, Distance.Meters), 2), Environment.NewLine);
            sbPoints.AppendFormat("   Traverse Close Error: 1 part in {0:0.00}.{1}",
                    Double.IsPositiveInfinity(travError) ? "∞" : Math.Round(travError, 2).ToString(),
                    Environment.NewLine);
            
            TotalTraverseError += (travLength * closeError / 2);

            traversing = false;
        }
    }
    public class TtLeg
    {
        public TtPoint Point1 { get; }
        public TtPoint Point2 { get; }

        public double LegLength { get; private set; }
        public double Error { get; }
        
        public TtLeg(TtPoint p1, TtPoint p2)
        {
            Point1 = p1;
            Point2 = p2;

            LegLength = MathEx.Distance(p1.AdjX, p1.AdjY, p2.AdjX, p2.AdjY);
            Error = LegLength * (p1.Accuracy + p2.Accuracy) / 2d; ;
        }
    }
    

    public enum HaidResult
    {
        Valid,
        Invalid,
        Empty,
        InsufficientPoints
    }
}
