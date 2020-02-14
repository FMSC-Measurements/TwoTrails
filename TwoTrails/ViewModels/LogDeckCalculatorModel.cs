using CSUtil.ComponentModel;
using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using Convert = FMSC.Core.Convert;

namespace TwoTrails.ViewModels
{
    public class LogDeckCalculatorModel : NotifyPropertyChangedEx
    {
        private TtProject _Project;

        public double LogLength {
            get => _Project.Settings.DeviceSettings.LogDeckLength;
            set => Set(value, () => {
                if (_Project.Settings.DeviceSettings is DeviceSettings ds) { ds.LogDeckLength = value; }
                UpdateVolume();
            });
        }
        public double CollarWidth {
            get => _Project.Settings.DeviceSettings.LogDeckCollarWidth;
            set => Set(value, () => {
                if (_Project.Settings.DeviceSettings is DeviceSettings ds) { ds.LogDeckCollarWidth = value; }
                UpdateVolume();
            });
        }
        public double Defect {
            get => _Project.Settings.DeviceSettings.LogDeckDefect;
            set => Set(value, () => {
                if (_Project.Settings.DeviceSettings is DeviceSettings ds) { ds.LogDeckDefect = value; }
                UpdateVolume();
            });
        }
        public double Void {
            get => _Project.Settings.DeviceSettings.LogDeckVoid;
            set => Set(value, () => {
                if (_Project.Settings.DeviceSettings is DeviceSettings ds) { ds.LogDeckVoid = value; }
                UpdateVolume();
            });
        }

        private double areaMt;
        public double Area { get => Get<double>(); set => Set(value); }
        public double Perimeter { get => Get<double>(); set => Set(value); }
        public double FaceArea { get => Get<double>(); set => Set(value); } // = area + collar area
        public double NetVolume { get => Get<double>(); set => Set(value); } // face area * log length
        public double GrossVolume { get => Get<double>(); set => Set(value); } // = net * void * defect

        public Distance Distance { get => _Project.Settings.DeviceSettings.LogDeckDistance;
            set => Set(value, () => {
                if (_Project.Settings.DeviceSettings is DeviceSettings ds) { ds.LogDeckDistance = value;}
                UpdateVolume();
            });
        }
        public Volume Volume { get => _Project.Settings.DeviceSettings.LogDeckVolume;
            set => Set(value, () => {
                if (_Project.Settings.DeviceSettings is DeviceSettings ds) { ds.LogDeckVolume = value; }
                UpdateVolume();
            });
        }

        public ReadOnlyCollection<TtPolygon> Polygons { get; }

        public TtPolygon Polygon { get => Get<TtPolygon>(); set => Set(value, () => CalculateDeck(value)); }
        private TtPolygon DeckPolygon = null;


        public LogDeckCalculatorModel(TtProject project)
        {
            _Project = project;

            Polygons = new ReadOnlyCollection<TtPolygon>(
                _Project.Manager.GetPolygons().Where(p => _Project.Manager.IsPolygonValid(p.CN)).ToList());

            if (Polygons.Count > 0)
                Polygon = Polygons[0];
        }


        private void CalculateDeck(TtPolygon polygon)
        {
            DeckPolygon = polygon;
            List<TtPoint> points = _Project.Manager.GetPoints(DeckPolygon.CN).Where(pt => pt.IsBndPoint()).ToList();

            List<Point> LzPoints = new List<Point>();
            var fpt = points[0];

            foreach (var pt in points)
                LzPoints.Add(new Point(MathEx.Distance(fpt.AdjX, fpt.AdjY, pt.AdjX, pt.AdjY), pt.AdjZ));

            LzPoints.Add(LzPoints[0]);

            double perim = 0, area = 0;
            Point p1, p2;

            for (int i = 0; i < LzPoints.Count - 1; i++)
            {
                p1 = LzPoints[i];
                p2 = LzPoints[i + 1];

                perim += MathEx.Distance(p1.X, p1.Y, p2.X, p2.Y);
                area += (p2.X - p1.X) * (p2.Y + p1.Y);
            }

            areaMt = Math.Abs(area) / 2d;
            Perimeter = perim;

            UpdateVolume();
        }

        private void UpdateVolume()
        {
            if (DeckPolygon != null)
            {
                Area areaType = (Distance == Distance.Meters) ? FMSC.Core.Area.MeterSq : FMSC.Core.Area.FeetSq;

                double faceAreaMt = areaMt + Convert.Distance(CollarWidth, Distance.Meters, Distance) * Perimeter;
                Area = Convert.Area(areaMt, areaType, FMSC.Core.Area.MeterSq);
                FaceArea = Convert.Area(faceAreaMt, areaType, FMSC.Core.Area.MeterSq);
                GrossVolume = Convert.Volume(faceAreaMt * Convert.Distance(LogLength, Distance.Meters, Distance), Volume, Volume.CubicMeter);
                NetVolume = GrossVolume * (1 - (Defect + Void) / 100d);
            }
        }
    }
}
