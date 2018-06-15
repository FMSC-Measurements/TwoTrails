using FMSC.GeoSpatial.Types;
using System;
using System.Collections.Generic;

namespace FMSC.GeoSpatial
{
    public static class GeoTools
    {
        public static double ConvertElevation(double elevation, UomElevation to, UomElevation from)
        {
            if (to != from)
                return (to == UomElevation.Meters) ? 1200d / 3937d : 3937d / 1200d;
            return elevation;
        }

        public static Position getGeoMidPoint(Position position1, Position position2)
        {
            List<Position> arr = new List<Position>();

            arr.Add(position1);
            arr.Add(position2);

            return getGeoMidPoint(arr);
        }

        public static Position getGeoMidPoint(List<Position> positions)
        {
            Double x, y, q;
            x = y = q = 0.0;

            int size = positions.Count;

            Double lat, lon;
            foreach (Position p in positions)
            {
                lat = p.Latitude.toSignedDecimal() * Math.PI / 180.0;
                lon = p.Longitude.toSignedDecimal() * Math.PI / 180.0;

                x += Math.Cos(lat) * Math.Cos(lon);
                y += Math.Cos(lat) * Math.Sin(lon);
                q += Math.Sin(lat);
            }

            x /= size;
            y /= size;
            q /= size;

            lon = Math.Atan2(y, x);
            Double hyp = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            lat = Math.Atan2(q, hyp);

            return new Position(new Latitude(lat * 180.0 / Math.PI), new Longitude(lon * 180.0 / Math.PI));
        }

        public static Position getGeoMidPoint(List<Double> lats, List<Double> lons)
        {
            if (lats.Count != lons.Count)
            {
                throw new Exception("List size Mismatch");
            }

            Double x, y, q;
            x = y = q = 0.0;

            int size = lats.Count;

            Double lat, lon;
            for (int i = 0; i < size; i++)
            {
                lat = lats[i] * Math.PI / 180.0;
                lon = lons[i] * Math.PI / 180.0;

                x += Math.Cos(lat) * Math.Cos(lon);
                y += Math.Cos(lat) * Math.Sin(lon);
                q += Math.Sin(lat);
            }

            x /= size;
            y /= size;
            q /= size;

            lon = Math.Atan2(y, x);
            Double hyp = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            lat = Math.Atan2(q, hyp);

            return new Position(new Latitude(lat * 180.0 / Math.PI), new Longitude(lon * 180.0 / Math.PI));
        }


        public static GeoPosition getMidPioint(List<GeoPosition> positions)
        {
            if (positions == null || positions.Count < 1)
            {
                throw new Exception("No positions to average.");
            }

            Double x, y, z, q;
            x = y = z = q = 0.0;

            int size = positions.Count;

            Double lat, lon;
            foreach (GeoPosition p in positions)
            {
                lat = p.Latitude.toSignedDecimal() * Math.PI / 180.0;
                lon = p.Longitude.toSignedDecimal() * Math.PI / 180.0;

                x += Math.Cos(lat) * Math.Cos(lon);
                y += Math.Cos(lat) * Math.Sin(lon);
                q += Math.Sin(lat);

                z += ConvertElevation(p.Elevation, UomElevation.Meters, p.UomElevation);
            }

            x /= size;
            y /= size;
            z /= size;
            q /= size;

            lon = Math.Atan2(y, x);
            Double hyp = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            lat = Math.Atan2(q, hyp);

            return new GeoPosition(lat * 180.0 / Math.PI, lon * 180.0 / Math.PI,
                ConvertElevation(z, positions[0].UomElevation, UomElevation.Meters), positions[0].UomElevation);
        }

        public static GeoPosition getMidPioint(List<Double> lats, List<Double> lons, List<Double> elevations, UomElevation uomElevation)
        {
            if (lats == null || lats.Count < 1)
            {
                throw new Exception("No positions to average.");
            }

            if (lats.Count != lons.Count && lats.Count != elevations.Count)
            {
                throw new Exception("Array size Mismatch");
            }

            Double x, y, z, q;
            x = y = z = q = 0.0;

            int size = lats.Count;

            Double lat, lon;
            for (int i = 0; i < size; i++)
            {
                lat = lats[i] * Math.PI / 180.0;
                lon = lons[i] * Math.PI / 180.0;

                x += Math.Cos(lat) * Math.Cos(lon);
                y += Math.Cos(lat) * Math.Sin(lon);
                q += Math.Sin(lat);
                z += elevations[i];
            }

            x /= size;
            y /= size;
            z /= size;
            q /= size;

            lon = Math.Atan2(y, x);
            Double hyp = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            lat = Math.Atan2(q, hyp);

            return new GeoPosition(lat * 180.0 / Math.PI, lon * 180.0 / Math.PI, z, uomElevation);
        }
    }
}
