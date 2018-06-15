namespace TwoTrails.DAL
{

    public enum ParseMode
    {
        Basic,
        Advanced,
        LatLon
    }

    public enum PointTextFieldType
    {
        NO_FIELD = 0,
        CN = 1,
        OPTYPE = 2,
        INDEX = 3,
        PID = 4,
        TIME = 5,
        POLY_NAME = 6,
        GROUP_NAME = 7,
        COMMENT = 8,
        META_CN = 9,
        ONBND = 10,
        UNADJX = 11,
        UNADJY = 12,
        UNADJZ = 13,
        ACCURACY = 14,
        MAN_ACC = 15,
        RMSER = 16,
        LATITUDE = 17,
        LONGITUDE = 18,
        ELEVATION = 19,
        FWD_AZ = 20,
        BK_AZ = 21,
        SLOPE_DIST = 22,
        SLOPE_DIST_TYPE = 23,
        SLOPE_ANG = 24,
        SLOPE_ANG_TYPE = 25,
        PARENT_CN = 26,
        POLY_CN = 27,
        GROUP_CN = 28
    }
}
