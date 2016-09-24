using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core.Xml.KML
{
    public class Placemark
    {
        #region Properties

        public string CN { get; }
        public string Name { get; set; }
        public string Desctription { get; set; }
        public string StyleUrl { get; set; }
        public View View { get; set; }
        public Properties Properties { get; set; }
        public bool? Visibility { get; set; }
        public bool? Open { get; set; }
        
        public List<Polygon> Polygons { get; } = new List<Polygon>();
        public List<Point> Points = new List<Point>();
        #endregion
        

        public Placemark(string name, string desc = null, View view = null)
        {
            Name = name;
            View = view;

            Desctription = desc??String.Empty;

            CN = Guid.NewGuid().ToString();
        }

        public Placemark(Placemark pm) : this(pm.Name, pm.Desctription, new View(pm.View))
        {
            CN = pm.CN;
            StyleUrl = pm.StyleUrl; ;
            Properties = new Properties(pm.Properties);
            Visibility = pm.Visibility;
            Open = pm.Open;

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


        public Point GetPoint(string cn) => Points.FirstOrDefault(p => p.CN == cn);

        public Point GetPointByName(string name) => Points.FirstOrDefault(p => p.Name == name);


        public Polygon GetPolygon(string cn) => Polygons.FirstOrDefault(p => p.CN == cn);

        public Polygon GetPolygonByName(string name) => Polygons.FirstOrDefault(p => p.Name == name);
        #endregion
    }
}
