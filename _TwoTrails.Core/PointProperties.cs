using System;
using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public static class PointProperties
    {
        public static readonly PropertyInfo INDEX;
        public static readonly PropertyInfo PID;
        public static readonly PropertyInfo TIME_CREATED;
        public static readonly PropertyInfo POLY;
        public static readonly PropertyInfo GROUP;
        public static readonly PropertyInfo META;
        public static readonly PropertyInfo COMMENT;
        public static readonly PropertyInfo BOUNDARY;
        public static readonly PropertyInfo ADJX;
        public static readonly PropertyInfo ADJY;
        public static readonly PropertyInfo ADJZ;
        public static readonly PropertyInfo UNADJX;
        public static readonly PropertyInfo UNADJY;
        public static readonly PropertyInfo UNADJZ;
        public static readonly PropertyInfo ACCURACY;
        public static readonly PropertyInfo QLINKS;
        public static readonly PropertyInfo EXTENDED_DATA;

        public static readonly PropertyInfo LAT;
        public static readonly PropertyInfo LON;
        public static readonly PropertyInfo ELEVATION;
        public static readonly PropertyInfo MAN_ACC_GPS;

        public static readonly PropertyInfo FWD_AZ;
        public static readonly PropertyInfo BK_AZ;
        public static readonly PropertyInfo SLP_DIST;
        public static readonly PropertyInfo SLP_ANG;

        public static readonly PropertyInfo PARENT_POINT;
        public static readonly PropertyInfo MAN_ACC_QP;

        static PointProperties()
        {
            Type tt = typeof(TtPoint);
            Type gps = typeof(GpsPoint);
            Type trav = typeof(TravPoint);
            Type qp = typeof(QuondamPoint);
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            INDEX = tt.GetProperty(nameof(TtPoint.Index), bf);
            PID = tt.GetProperty(nameof(TtPoint.PID), bf);
            TIME_CREATED = tt.GetProperty(nameof(TtPoint.TimeCreated), bf);
            POLY = tt.GetProperty(nameof(TtPoint.Polygon), bf);
            GROUP = tt.GetProperty(nameof(TtPoint.Group), bf);
            META = tt.GetProperty(nameof(TtPoint.Metadata), bf);
            COMMENT = tt.GetProperty(nameof(TtPoint.Comment), bf);
            BOUNDARY = tt.GetProperty(nameof(TtPoint.OnBoundary), bf);
            ADJX = tt.GetProperty(nameof(TtPoint.AdjX), bf);
            ADJY = tt.GetProperty(nameof(TtPoint.AdjY), bf);
            ADJZ = tt.GetProperty(nameof(TtPoint.AdjZ), bf);
            UNADJX = tt.GetProperty(nameof(TtPoint.UnAdjX), bf);
            UNADJY = tt.GetProperty(nameof(TtPoint.UnAdjY), bf);
            UNADJZ = tt.GetProperty(nameof(TtPoint.UnAdjZ), bf);
            ACCURACY = tt.GetProperty(nameof(TtPoint.Accuracy), bf);
            QLINKS = tt.GetProperty(nameof(TtPoint.LinkedPoints), bf);
            EXTENDED_DATA = tt.GetProperty(nameof(TtPoint.ExtendedData), bf);

            LAT = gps.GetProperty(nameof(GpsPoint.Latitude), bf);
            LON = gps.GetProperty(nameof(GpsPoint.Longitude), bf);
            ELEVATION = gps.GetProperty(nameof(GpsPoint.Elevation), bf);
            MAN_ACC_GPS = gps.GetProperty(nameof(GpsPoint.ManualAccuracy), bf);

            FWD_AZ = trav.GetProperty(nameof(TravPoint.FwdAzimuth), bf);
            BK_AZ = trav.GetProperty(nameof(TravPoint.BkAzimuth), bf);
            SLP_DIST = trav.GetProperty(nameof(TravPoint.SlopeDistance), bf);
            SLP_ANG = trav.GetProperty(nameof(TravPoint.SlopeAngle), bf);

            PARENT_POINT = qp.GetProperty(nameof(QuondamPoint.ParentPoint), bf);
            MAN_ACC_QP = qp.GetProperty(nameof(QuondamPoint.ManualAccuracy), bf);
        }

        public static PropertyInfo GetPropertyByName(string name)
        {
            switch (name)
            {
                case nameof(TtPoint.Index): return INDEX;
                case nameof(TtPoint.PID): return PID;
                case nameof(TtPoint.TimeCreated): return TIME_CREATED;
                case nameof(TtPoint.Polygon): return POLY;
                case nameof(TtPoint.Group): return GROUP;
                case nameof(TtPoint.Metadata): return META;
                case nameof(TtPoint.Comment): return COMMENT;
                case nameof(TtPoint.OnBoundary): return BOUNDARY;
                case nameof(TtPoint.AdjX): return ADJX;
                case nameof(TtPoint.AdjY): return ADJY;
                case nameof(TtPoint.AdjZ): return ADJZ;
                case nameof(TtPoint.UnAdjX): return UNADJX;
                case nameof(TtPoint.UnAdjY): return UNADJY;
                case nameof(TtPoint.UnAdjZ): return UNADJZ;
                case nameof(TtPoint.Accuracy): return ACCURACY;
                case nameof(TtPoint.LinkedPoints): return QLINKS;

                case nameof(GpsPoint.Latitude): return LAT;
                case nameof(GpsPoint.Longitude): return LON;
                case nameof(GpsPoint.Elevation): return ELEVATION;
                //case nameof(GpsPoint.ManualAccuracy): return MAN_ACC_GPS;

                case nameof(TravPoint.FwdAzimuth): return FWD_AZ;
                case nameof(TravPoint.BkAzimuth): return BK_AZ;
                case nameof(TravPoint.SlopeDistance): return SLP_DIST;
                case nameof(TravPoint.SlopeAngle): return SLP_ANG;

                case nameof(QuondamPoint.ParentPoint): return PARENT_POINT;
                //case nameof(QuondamPoint.ManualAccuracy): return MAN_ACC_QP;
            }

            return null;
        }

        public static Type GetPointTypeByPropertyName(string name)
        {
            switch (name)
            {
                case nameof(TtPoint.Index):
                case nameof(TtPoint.PID):
                case nameof(TtPoint.TimeCreated):
                case nameof(TtPoint.Polygon):
                case nameof(TtPoint.Group):
                case nameof(TtPoint.Metadata):
                case nameof(TtPoint.Comment):
                case nameof(TtPoint.OnBoundary):
                case nameof(TtPoint.AdjX):
                case nameof(TtPoint.AdjY):
                case nameof(TtPoint.AdjZ):
                case nameof(TtPoint.UnAdjX):
                case nameof(TtPoint.UnAdjY):
                case nameof(TtPoint.UnAdjZ):
                case nameof(TtPoint.Accuracy):
                case nameof(TtPoint.LinkedPoints): return typeof(TtPoint);

                case nameof(GpsPoint.Latitude):
                case nameof(GpsPoint.Longitude):
                case nameof(GpsPoint.Elevation): return typeof(GpsPoint);
                //case nameof(GpsPoint.ManualAccuracy): return MAN_ACC_GPS;

                case nameof(TravPoint.FwdAzimuth):
                case nameof(TravPoint.BkAzimuth):
                case nameof(TravPoint.SlopeDistance):
                case nameof(TravPoint.SlopeAngle): return typeof(TravPoint);

                case nameof(QuondamPoint.ParentPoint): return typeof(QuondamPoint);
                //case nameof(QuondamPoint.ManualAccuracy): return MAN_ACC_QP;
            }

            return null;
        }
    }
}
