using FMSC.Core;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class RezonePointsCommand : ITtPointsCommand
    {
        private List<Tuple<GpsPoint, double, double>> NewValues = new List<Tuple<GpsPoint, double, double>>();
        private List<Tuple<GpsPoint, double, double>> OldValues = new List<Tuple<GpsPoint, double, double>>();

        public RezonePointsCommand(IEnumerable<GpsPoint> points) : base(points)
        {
            foreach (GpsPoint point in Points)
            {
                //get real lat and lon
                Point latLon = point.HasLatLon ? new Point((double)point.Longitude, (double)point.Latitude) : UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);
                //get real utm
                UTMCoords realCoords = UTMTools.ConvertLatLonSignedDecToUTM(latLon.Y, latLon.X);

                if (realCoords.Zone != point.Metadata.Zone)
                {
                    OldValues.Add(Tuple.Create(point, point.UnAdjX, point.UnAdjY));
                    NewValues.Add(Tuple.Create(point, realCoords.X, realCoords.Y));
                }
            }
        }

        public override void Redo()
        {
            foreach (var tup in NewValues)
            {
                PointProperties.UNADJX.SetValue(tup.Item1, tup.Item2);
                PointProperties.UNADJY.SetValue(tup.Item1, tup.Item3);
            }
        }

        public override void Undo()
        {
            foreach (var tup in OldValues)
            {
                PointProperties.UNADJX.SetValue(tup.Item1, tup.Item2);
                PointProperties.UNADJY.SetValue(tup.Item1, tup.Item3);
            }
        }
    }
}
