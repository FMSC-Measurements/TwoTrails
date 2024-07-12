using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;
using FSConvert = FMSC.Core.Convert;


namespace TwoTrails.Core
{
    public class PolygonSummary
    {
        private TtPoint _LastTtPoint, _LastTtBndPt, _LastTravPoint, _LastGpsPoint;

        private List<TtLeg> _Legs = new List<TtLeg>();

        private bool traversing = false;

        public TtPolygon Polygon { get; }
        public HaidResult Result { get; }
        public UnitAreaType UnitAreaType { get; }

        public bool HasTraverse { get; }


        public GeometricErrorReductionResult GERResult { get; }
        public bool GERAvailable => UnitAreaType == UnitAreaType.General;

        public ExclusionSummary Exclusions { get; }
        public int ExclusionsCount { get; }
        public bool HasExclusions { get; }

        public double TotalGpsError { get; private set; } = 0;
        public double TotalTraverseError { get; private set; } = 0;
        public double TotalTraverseLength { get; private set; } = 0;
        public int TotalTraverseSegments { get; private set; } = 0;
        public double GpsAreaError { get; private set; } = 0;
        public double TraverseAreaError { get; private set; } = 0;


        public double TotalAreaWExclusions { get; private set; } = 0;
        public double TotalPerimWExclusions { get; private set; } = 0;
        public double ExcludedAreaPercent { get; private set; } = 0;
        public double ExcludedPerimeterPercent { get; private set; } = 0;
        public double TotalAreaErrorAreaWExclusions { get; private set; } = 0;
        public double TotalGERAreaErrorAreaWExclusions { get; private set; } = 0;
        public double TotalAreaErrorWExclusions { get; private set; } = 0;
        public double TotalGERAreaErrorWExclusions { get; private set; } = 0;


        public double WorstTravSegmentError { get; private set; } = double.PositiveInfinity;

        private double travLength = 0;
        private int travSegments = 0;

        public String SummaryText { get; }


        public PolygonSummary(ITtManager manager, TtPolygon polygon, bool generateSummaryText = false, bool showPoints = false)
        {
            Polygon = polygon;

            List<TtPoint> points = manager.GetPoints(polygon.CN);

            UnitAreaType = UnitAnalyzer.GetUnitAreaType(points);

            if (GERAvailable)
            {
                GERResult = new GeometricErrorReductionResult(points.OnBndPointsList(), polygon.Area);
            }

            if (points.Count > 2)
            {
                StringBuilder sbPoints = new StringBuilder();

                foreach (TtPoint point in points)
                    ProcessPoint(sbPoints, point, null, showPoints);

                TtPoint sp = null;
                foreach (TtPoint p in points)
                {
                    if (p.OnBoundary)
                    {
                        sp = p;
                        break;
                    }
                }

                if (sp != null && !sp.HasSameAdjLocation(_LastTtBndPt))
                    _Legs.Add(new TtLeg(_LastTtBndPt, sp));

                double perim = 0;
                foreach (TtLeg leg in _Legs)
                {
                    TotalGpsError += leg.Error;
                    perim += leg.LegLength;
                }

                if (TotalGpsError > 0)
                {
                    GpsAreaError = TotalGpsError / polygon.Area * 100.0;

                    Exclusions = new ExclusionSummary(manager, manager.GetPolygons().Where(p => p.ParentUnitCN == polygon.CN), generateSummaryText);

                    ExclusionsCount = Exclusions.Count();
                    HasExclusions = ExclusionsCount > 0;


                    if (HasExclusions)
                    {
                        TotalAreaWExclusions = polygon.Area + Exclusions.TotalArea;
                        TotalPerimWExclusions = polygon.Perimeter + Exclusions.TotalPerimeter;

                        TotalAreaErrorAreaWExclusions = TotalGpsError + Exclusions.TotalAreaErrorArea;

                        ExcludedAreaPercent = Exclusions.TotalArea / Polygon.Area * 100d;
                        ExcludedPerimeterPercent = Exclusions.TotalPerimeter / Polygon.Perimeter * 100d;

                        TotalAreaErrorWExclusions = TotalAreaErrorAreaWExclusions / TotalAreaWExclusions * 100d;

                        if (GERAvailable && Exclusions.GERAvailable)
                        {
                            TotalGERAreaErrorAreaWExclusions = GERResult.TotalErrorArea + Exclusions.TotalGERAreaErrorArea;
                            TotalGERAreaErrorWExclusions = TotalGERAreaErrorAreaWExclusions / TotalAreaWExclusions * 100d;
                        }
                    }
                }

                if (_LastTravPoint != null)
                    HasTraverse = true;

                if (generateSummaryText)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("Polygon Area: {0:F3} Ac ({1:F2} Ha).{2}",
                        Math.Round(polygon.AreaAcres, 3),
                        Math.Round(polygon.AreaHectaAcres, 2),
                        Environment.NewLine);

                    sb.AppendFormat("Polygon Exterior Perimeter: {0:F2} Ft ({1:F1} M).{2}",
                        Math.Round(polygon.PerimeterFt, 0),
                        Math.Round(polygon.Perimeter, 2),
                        Environment.NewLine);

                    sb.AppendFormat("Polyline Perimeter: {0:F2} Ft ({1:F1} M).{2}{2}",
                        Math.Round(polygon.PerimeterLineFt, 0),
                        Math.Round(polygon.PerimeterLine, 2),
                        Environment.NewLine);

                    if (TotalGpsError > 0)
                    {
                        sb.AppendFormat("GPS Area-Error Contribution: {0:F3} Ac ({1:F2} Ha){2}",
                            Math.Round(FSConvert.ToAcre(TotalGpsError, Area.MeterSq), 3),
                            Math.Round(FSConvert.ToHectare(TotalGpsError, Area.MeterSq), 2),
                            Environment.NewLine);

                        sb.AppendFormat("GPS Contribution Ratio of Area-Error-Area to Area: {0:F2}%.{1}{1}",
                            Math.Round(GpsAreaError, 2),
                            Environment.NewLine);

                        if (GERAvailable && GpsAreaError >= Consts.HB_STANDARD_ACC &&
                            GERResult.AreaError > 0 && GERResult.AreaError < Consts.HB_STANDARD_ACC)
                        {
                            sb.AppendFormat("Geometric Area-Error Contribution: {0:F3} Ac ({1:F2} Ha){2}",
                                Math.Round(FSConvert.ToAcre(GERResult.TotalGpsErrorArea, Area.MeterSq), 3),
                                Math.Round(FSConvert.ToHectare(GERResult.TotalGpsErrorArea, Area.MeterSq), 2),
                                Environment.NewLine);

                            sb.AppendFormat("Geometric Error Reduction Ratio of Area-Error-Area to Area: {0:F2}%.{1}{1}",
                                Math.Round(GERResult.AreaError, 2),
                                Environment.NewLine);
                        }
                    }

                    if (HasTraverse)
                    {
                        sb.AppendFormat("Traverse Contribution: {0:F3} Ac ({1:F2} Ha){2}",
                            Math.Round(FSConvert.ToAcre(TotalTraverseError, Area.MeterSq), 2),
                            Math.Round(FSConvert.ToHectare(TotalTraverseError, Area.MeterSq), 2),
                            Environment.NewLine);

                        TraverseAreaError = TotalTraverseError / polygon.Area * 100.0;
                        sb.AppendFormat("Traverse Contribution Ratio of Area-Error-Area to Area: {0:F2}%.{1}{1}",
                            Math.Round(TraverseAreaError, 2),
                            Environment.NewLine);
                    }

                    if (TotalAreaWExclusions > 0)
                    {
                        sb.Append($"Total Exclusions: {ExclusionsCount}\n");
                            
                        sb.AppendFormat("Polygon Area with Exclusions: {0:F3} Ac ({1:F2} Ha).{2}",
                        Math.Round(FSConvert.ToAcre(TotalAreaWExclusions, Area.MeterSq), 3),
                        Math.Round(FSConvert.ToHectare(TotalAreaWExclusions, Area.MeterSq), 2),
                        Environment.NewLine);

                        sb.AppendFormat("Polygon Perimeter with Exclusions: {0:F2} Ft ({1:F1} M).{2}{2}",
                            Math.Round(FSConvert.ToFeetTenths(TotalPerimWExclusions, Distance.Meters), 0),
                            Math.Round(TotalPerimWExclusions, 2),
                            Environment.NewLine);

                        sb.AppendFormat("GPS Area-Error Contribution with Exclusions: {0:F3} Ac ({1:F2} Ha){2}",
                            Math.Round(FSConvert.ToAcre(TotalAreaErrorAreaWExclusions, Area.MeterSq), 2),
                            Math.Round(FSConvert.ToHectare(TotalAreaErrorAreaWExclusions, Area.MeterSq), 2),
                            Environment.NewLine);

                        sb.AppendFormat("GPS Contribution Ratio of Area-Error-Area to Area: {0:F2}%.{1}{1}",
                            Math.Round(TotalAreaErrorWExclusions, 2),
                            Environment.NewLine);

                        if (GERAvailable && Exclusions.GERAvailable && TotalAreaErrorWExclusions >= Consts.HB_STANDARD_ACC &&
                            TotalGERAreaErrorWExclusions < Consts.HB_STANDARD_ACC)
                        {
                            sb.AppendFormat("Geometric Area-Error Contribution with Exclusions: {0:F3} Ac ({1:F2} Ha){2}",
                                Math.Round(FSConvert.ToAcre(TotalGERAreaErrorAreaWExclusions, Area.MeterSq), 3),
                                Math.Round(FSConvert.ToHectare(TotalGERAreaErrorAreaWExclusions, Area.MeterSq), 2),
                                Environment.NewLine);

                            sb.AppendFormat("Geometric Error Reduction Ratio of Area-Error-Area to Area with Exclusions: {0:F2}%.{1}{1}",
                                Math.Round(TotalGERAreaErrorWExclusions, 2),
                                Environment.NewLine);
                        }

                        sb.Append(Exclusions.SummaryText);
                    }

                    if (showPoints)
                        sb.Append($"Points:\n{sbPoints}");


                    SummaryText = sb.ToString();
                    sb.Clear();
                }

                sbPoints.Clear();
                _Legs.Clear();
                _LastTtPoint = _LastTtBndPt = _LastTravPoint = _LastGpsPoint = null;
            }
            else
            {
                Result = HaidResult.Empty;
                SummaryText = "Polygon has Insufficient Points\n\n";
            }
        }
        

        private void ProcessPoint(StringBuilder sbPoints, TtPoint point, QuondamPoint parent = null, bool showPoints = false)
        {
            bool isOnBound = (parent != null && parent.OnBoundary) || (parent == null && point.OnBoundary);

            switch (point.OpType)
            {
                case OpType.GPS:
                case OpType.Take5:
                case OpType.Walk:
                case OpType.WayPoint:
                    {
                        if (traversing)
                            CloseTraverse(sbPoints, point);

                        if (isOnBound)
                        {
                            if (_LastTtBndPt != null)
                                _Legs.Add(new TtLeg(_LastTtBndPt, point));

                            _LastTtBndPt = point;
                        }

                        if (parent == null && showPoints)
                        {
                            sbPoints.Append($"Point {point.PID}: {(point.OnBoundary ? " " : "*")} {point.OpType}- ");
                            sbPoints.Append($"Accuracy is {point.Accuracy:0.00#} meters.{Environment.NewLine}");
                        }

                        _LastGpsPoint = point;
                    }
                    break;
                case OpType.Traverse:
                    {
                        if (_LastTtPoint != null)
                        {
                            if (isOnBound)
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

                                    if (showPoints && parent == null)
                                    {
                                        sbPoints.Append($"Traverse Start:{Environment.NewLine}");
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
                        if (showPoints && parent == null)
                        {
                            sbPoints.Append($"Point {point.PID}: {(point.OnBoundary ? " " : "*")} SideShot off Point {_LastGpsPoint?.PID}.{Environment.NewLine}");
                        }

                        if (_LastTtBndPt != null && isOnBound)
                        {
                            _Legs.Add(new TtLeg(_LastTtBndPt, point));
                        }
                        
                        if (isOnBound)
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
                            ProcessPoint(sbPoints, qp.ParentPoint, qp, showPoints);
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

            sbPoints.Append($"   Traverse Total Segments: {travSegments}{Environment.NewLine}");
            sbPoints.Append($"   Traverse Total Distance: {Math.Round(FSConvert.ToFeetTenths(travLength, Distance.Meters), 2):0.00} feet.{Environment.NewLine}");
            sbPoints.Append($"   Traverse Closing Distance: {Math.Round(FSConvert.ToFeetTenths(closeError, Distance.Meters), 2):0.00} feet.{Environment.NewLine}");
            sbPoints.Append($"   Traverse Closing Error: 1 part in {(Double.IsPositiveInfinity(travError) ? "∞" : Math.Round(travError, 2).ToString("0.00"))}.{Environment.NewLine}");
            
            TotalTraverseError += (travLength * closeError / 2);

            if (travError < WorstTravSegmentError)
                WorstTravSegmentError = travError;

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
