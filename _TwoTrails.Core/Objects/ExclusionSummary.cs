using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TwoTrails.Core
{
    public class ExclusionSummary : IEnumerable<PolygonSummary>
    {
        public List<PolygonSummary> _Exclusions { get; }

        public double TotalArea { get; }
        public double TotalPerimeter { get; }

        public double TotalAreaErrorArea { get; private set; }
        public double TotalGERAreaErrorArea { get; private set; }


        public ExclusionSummary(ITtManager manager, IEnumerable<TtPolygon> polygons) 
        {
            _Exclusions = polygons.Select(p => new PolygonSummary(manager, p)).ToList();

            foreach (var ps in _Exclusions)
            {
                TotalArea += ps.Polygon.Area;
                TotalPerimeter += ps.Polygon.Perimeter;
                TotalAreaErrorArea += ps.TotalGpsError;
                TotalGERAreaErrorArea += ps.GERResult.TotalErrorArea;
            }
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
