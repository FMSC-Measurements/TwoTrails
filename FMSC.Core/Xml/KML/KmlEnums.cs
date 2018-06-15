namespace FMSC.Core.Xml.KML
{
    public enum ColorMode
    {
        Normal,
        Random
    }

    public enum DisplayMode
    {
        Default,
        Hide
    }

    public enum ListItemType
    {
        Check,
        CheckOffOnly,
        CheckHideChildern,
        RadioFolder
    }

    public enum State
    {
        Open,
        Closed,
        Error,
        Fetching0,
        Fetching1,
        Fetching2
    }

    public enum XYUnitType
    {
        Fraction,
        Pixels,
        InsetPixels
    }

    public struct HotSpot
    {
        public double X;
        public double Y;
        public XYUnitType XUnits;
        public XYUnitType YUnits;

        public HotSpot(double x, double y, XYUnitType xu, XYUnitType yu)
        {
            X = x;
            Y = y;
            XUnits = xu;
            YUnits = yu;
        }
    }

    public enum AltitudeMode
    {
        ClampToGround,
        ClampToSeaFloor,
        RelativeToGround,
        RealitiveToSeaFloor,
        Absolute
    }
}
