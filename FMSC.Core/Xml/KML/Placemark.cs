using System;
using System.Collections.Generic;
using System.Linq;

namespace FMSC.Core.Xml.KML
{
    public class Placemark : KmlProperties
    {
        #region Properties
        public string CN { get; }
        public View View { get; set; }
        
        public List<Polygon> Polygons { get; } = new List<Polygon>();
        public List<KmlPoint> Points { get; } = new List<KmlPoint>();
        #endregion
        

        public Placemark(string name, string desc = null, View view = null) : base(name, desc)
        {
            View = view;

            CN = Guid.NewGuid().ToString();
        }

        public Placemark(Placemark pm) : base(pm)
        {
            CN = pm.CN;
            View = new View(pm.View);

            Polygons = pm.Polygons.ToList();
            Points = pm.Points.ToList();
        }


        #region Methods
        
        public void RemovePoint(string cn)
        {
            for (int i = Points.Count - 1; i > -1; i--)
            {
                if (Points[i].CN == cn)
                {
                    Points.RemoveAt(i);
                    return;
                }
            }
        }

        public void RemovePolygon(string cn)
        {
            for (int i = Polygons.Count - 1; i > -1; i--)
            {
                if (Polygons[i].CN == cn)
                {
                    Polygons.RemoveAt(i);
                    return;
                }
            }
        }


        public KmlPoint GetPoint(string cn) => Points.FirstOrDefault(p => p.CN == cn);

        public KmlPoint GetPointByName(string name) => Points.FirstOrDefault(p => p.Name == name);


        public Polygon GetPolygon(string cn) => Polygons.FirstOrDefault(p => p.CN == cn);

        public Polygon GetPolygonByName(string name) => Polygons.FirstOrDefault(p => p.Name == name);
        #endregion
    }
}
