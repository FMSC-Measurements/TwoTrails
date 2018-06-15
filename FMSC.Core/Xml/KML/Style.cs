using System;

namespace FMSC.Core.Xml.KML
{
    public class Style
    {
        #region Properties
        public string ID { get; }
        public string StyleUrl { get { return $"#{ID}"; } }

        public string IconID { get; set; }
        public string IconUrl { get; set; }
        public KmlColor IconColor { get; set; }
        public double? IconScale { get; set; }
        public ColorMode? IconColorMode { get; set; }
        public double? IconHeading { get; set; }
        public HotSpot? IconHotSpot { get; set; }

        public string LabelID { get; set; }
        public KmlColor LabelColor { get; set; }
        public double? LabelScale { get; set; }
        public ColorMode? LabelColorMode { get; set; }

        public string LineID { get; set; }
        public KmlColor LineColor { get; set; }
        public double? LineWidth { get; set; }
        public ColorMode? LineColorMode { get; set; }
        public KmlColor LineOuterColor { get; set; }
        public double? LineOuterWidth { get; set; }
        public double? LinePhysicalWidth { get; set; }
        public bool? LineLabelVisibility { get; set; }

        public string PolygonID { get; set; }
        public KmlColor PolygonColor { get; set; }
        public ColorMode? PolygonColorMode { get; set; }
        public bool? PolygonFill { get; set; }
        public bool? PolygonOutline { get; set; }

        public string BalloonID { get; set; }
        public KmlColor BalloonBgColor { get; set; }
        public KmlColor BalloonTextColor { get; set; }
        public string BalloonText { get; set; }
        public DisplayMode? BalloonDisplayMode { get; set; }

        public string ListID { get; set; }
        public ListItemType? ListListItemType { get; set; }
        public KmlColor ListBgColor { get; set; }
        public State? ListItemState { get; set; }
        public string ListItemIconUrl { get; set; }
        #endregion


        public Style() : this(null, null) { }

        public Style(string id) : this(id, null) { }

        public Style(Style style) : this(null, style) { }

        public Style(string id, Style style)
        {
            ID = id??Guid.NewGuid().ToString();

            if (style != null)
            {
                PolygonID = style.PolygonID;
                PolygonColorMode = style.PolygonColorMode;
                PolygonColor = style.PolygonColor;
                PolygonFill = style.PolygonFill;
                PolygonOutline = style.PolygonOutline;

                IconID = style.IconID;
                IconUrl = style.IconUrl;
                IconColor = style.IconColor;
                IconScale = style.IconScale;
                IconHeading = style.IconHeading;
                IconHotSpot = style.IconHotSpot;

                LabelID = style.LabelID;
                LabelColor = style.LabelColor;
                LabelScale = style.LabelScale;
                LabelColorMode = style.LabelColorMode;

                LineID = style.LineID;
                LineColor = style.LineColor;
                LineWidth = style.LineWidth;
                LineColorMode = style.LineColorMode;
                LineOuterColor = style.LineOuterColor;
                LineOuterWidth = style.LineOuterWidth;
                LinePhysicalWidth = style.LinePhysicalWidth;
                LineLabelVisibility = style.LineLabelVisibility;

                BalloonID = style.BalloonID;
                BalloonBgColor = style.BalloonBgColor;
                BalloonTextColor = style.BalloonTextColor;
                BalloonText = style.BalloonText;
                BalloonDisplayMode = style.BalloonDisplayMode;

                ListID = style.ListID;
                ListListItemType = style.ListListItemType;
                ListBgColor = style.ListBgColor;
                ListItemState = style.ListItemState;
                ListItemIconUrl = style.ListItemIconUrl;
            }
        }

        public void SetColorsILP(KmlColor c)
        {
            IconColor = c;
            LineColor = c;
            PolygonColor = new KmlColor(c);
            PolygonColor.A = 50;

            IconColorMode = ColorMode.Normal;
            LineColorMode = ColorMode.Normal;
            PolygonColorMode = ColorMode.Normal;
        }
    }
}
