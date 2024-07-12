using FMSC.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSConvert = FMSC.Core.Convert;

namespace TwoTrails.Core
{
    public class ExclusionSummary : IEnumerable<PolygonSummary>
    {
        public List<PolygonSummary> _Exclusions { get; }

        public double TotalArea { get; }
        public double TotalPerimeter { get; }

        public double TotalAreaErrorArea { get; private set; }
        public double TotalGERAreaErrorArea { get; private set; }

        public bool GERAvailable { get; private set; } = true;

        public bool HasTies { get; private set; }

        public string SummaryText { get; private set; }


        public ExclusionSummary(ITtManager manager, IEnumerable<TtPolygon> polygons, bool generateSummaryText = false, bool advancedProcessing = false) 
        {
            _Exclusions = polygons.Select(p => new PolygonSummary(manager, p, generateSummaryText, false, advancedProcessing)).ToList();

            StringBuilder sbExc = new StringBuilder();

            if (generateSummaryText)
            {
                sbExc.AppendLine($"Excluded Units:");
            }

            foreach (var ps in _Exclusions)
            {
                TotalArea += ps.Polygon.Area;
                TotalPerimeter += ps.Polygon.Perimeter;
                TotalAreaErrorArea += ps.TotalGpsError;

                if (ps.HasTies)
                    HasTies = true;

                if (ps.GERAvailable)
                    TotalGERAreaErrorArea += ps.GERResult.TotalErrorArea;
                else
                    GERAvailable = false;

                if (generateSummaryText)
                {
                    if (ps.HasTies)
                        sbExc.AppendLine($" Warning: Unit has ties.");

                    sbExc.AppendLine($" {ps.Polygon.Name}");
                    sbExc.AppendFormat("  Area: {0:F3} Ac ({1:F2} Ha){2}",
                        Math.Round(FSConvert.ToAcre(ps.Polygon.Area, Area.MeterSq), 3),
                        Math.Round(FSConvert.ToHectare(ps.Polygon.Area, Area.MeterSq), 2),
                        Environment.NewLine);

                    sbExc.AppendFormat("  Perimeter: {0:F2} Ft ({1:F2} M){2}",
                        Math.Round(ps.Polygon.PerimeterFt, 2),
                        Math.Round(ps.Polygon.Perimeter),
                        Environment.NewLine);
                    
                    sbExc.AppendFormat("  Area-Error: {0:F3} Ac ({1:F2} Ha){2}",
                        Math.Round(FSConvert.ToAcre(ps.TotalGpsError, Area.MeterSq), 3),
                        Math.Round(FSConvert.ToHectare(ps.TotalGpsError, Area.MeterSq), 2),
                        Environment.NewLine);

                    if (ps.GERAvailable)
                        sbExc.AppendFormat("  GER Area-Error: {0:F3} Ac ({1:F2} Ha){2}",
                            Math.Round(FSConvert.ToAcre(ps.GERResult.TotalErrorArea, Area.MeterSq), 3),
                            Math.Round(FSConvert.ToHectare(ps.GERResult.TotalErrorArea, Area.MeterSq), 2),
                            Environment.NewLine);
                    sbExc.AppendLine();
                }
            }

            SummaryText = sbExc.ToString();
        }


        public IEnumerator<PolygonSummary> GetEnumerator()
        {
            return _Exclusions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Exclusions.GetEnumerator();
        }
    }
}
