using System;
using System.Collections.Generic;

namespace FMSC.Core.Xml.KML
{
    public class Polygon
    {
        public string CN { get; }

        public string Name { get; set; }
        public List<Coordinates> OuterBoundary { get; set; }
        public List<Coordinates> InnerBoundary { get; set; }
        public bool IsPath { get; set; }

        public bool HasOuterBoundary { get { return OuterBoundary != null && OuterBoundary.Count > 0; } }
        public bool HasInnerBoundary { get { return InnerBoundary != null && InnerBoundary.Count > 0; } }

        private AltitudeMode? _AltMode;
        public AltitudeMode? AltMode
        {
            get { return _AltMode; }
            set
            {
                _AltMode = value;

                if (_AltMode != null &&
                    (_AltMode == AltitudeMode.ClampToSeaFloor || _AltMode == AltitudeMode.ClampToGround))
                {
                    Tessellate = 1;
                    Extrude = 0;
                }
                else
                {
                    Tessellate = 0;
                    Extrude = 1;
                }
            }
        }

        public int Tessellate { get; set; } = 0;
        public int Extrude { get; set; } = 0;
        

        public Polygon(string name, List<Coordinates> outerBoundary = null)
        {
            Name = name;
            OuterBoundary = outerBoundary;
            CN = Guid.NewGuid().ToString();
            IsPath = false;
        }


        public Coordinates GetOBAveragedCoords()
        {
            Coordinates c = new Coordinates();
            double lat = 0, lon = 0;
            int count = 0;

            foreach (Coordinates coord in OuterBoundary)
            {
                lat += coord.Latitude;
                lon += coord.Longitude;
                count++;
            }

            if (count > 0)
            {
                c.Longitude = lon / count;
                c.Latitude = lat / count;
            }

            c.Altitude = 1000;

            return c;
        }

        public Coordinates GetIBAveragedCoords()
        {
            Coordinates c = new Coordinates();
            double lat = 0, lon = 0;
            int count = 0;

            foreach (Coordinates coord in InnerBoundary)
            {
                lat += coord.Latitude;
                lon += coord.Longitude;
                count++;
            }

            if (count > 0)
            {
                c.Longitude = lon / count;
                c.Latitude = lat / count;
            }

            c.Altitude = 1000;

            return c;
        }

        public CoordinalExtent GetOuterDimensions()
        {
            if (OuterBoundary.Count < 1)
                return null;

            CoordinalExtent.Builder b = new CoordinalExtent.Builder();

            foreach (Coordinates c in OuterBoundary)
                b.Include(c);

            return b.Build();
        }

        public CoordinalExtent GetInnerDimensions()
        {
            if (InnerBoundary.Count < 1)
                return null;

            CoordinalExtent.Builder b = new CoordinalExtent.Builder();

            foreach (Coordinates c in InnerBoundary)
                b.Include(c);

            return b.Build();
        }
    }
}
