using FMSC.Core;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class RezonePointsCommand : ITtPointsCommand
    {
        private List<Tuple<GpsPoint, double, double>> NewValues = new List<Tuple<GpsPoint, double, double>>();
        private List<Tuple<GpsPoint, double, double>> OldValues = new List<Tuple<GpsPoint, double, double>>();
        private readonly Guid _ID = Guid.NewGuid();

        public RezonePointsCommand(TtManager manager, IEnumerable<GpsPoint> points) : base(manager, points)
        {
            foreach (GpsPoint point in Points)
            {
                //get real lat and lon
                Point latLon = point.HasLatLon ? new Point((double)point.Longitude, (double)point.Latitude) : UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);
                //get real utm
                UTMCoords realCoords = UTMTools.ConvertLatLonSignedDecToUTM(latLon.Y, latLon.X);

                if (realCoords.Zone != point.Metadata.Zone && !point.HasSameUnAdjLocation(realCoords.X, realCoords.Y))
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

            Manager.AddAction(DataActionType.RezonedPoints, $"{NewValues.Count} points rezoned.", _ID);
        }

        public override void Undo()
        {
            foreach (var tup in OldValues)
            {
                PointProperties.UNADJX.SetValue(tup.Item1, tup.Item2);
                PointProperties.UNADJY.SetValue(tup.Item1, tup.Item3);
            }

            Manager.RemoveAction(_ID);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Rezone {NewValues.Count} points";
    }
}
