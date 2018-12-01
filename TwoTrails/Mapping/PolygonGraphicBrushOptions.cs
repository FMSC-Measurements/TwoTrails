using System.Windows.Media;
using TwoTrails.Core;

namespace TwoTrails.Mapping
{
    public class PolygonGraphicBrushOptions : PolygonGraphicOptions
    {
        public PolygonGraphicBrushOptions(string cn, PolygonGraphicOptions options) : base(cn, options)
        {
        }

        public PolygonGraphicBrushOptions(string cn, int adjBndColor, int unAdjBndColor, int adjNavColor, int unAdjNavColor, int adjPtsColor, int unAdjPtsColor, int wayPtsColor, float adjWidth, float unAdjWidth) : base(cn, adjBndColor, unAdjBndColor, adjNavColor, unAdjNavColor, adjPtsColor, unAdjPtsColor, wayPtsColor, adjWidth, unAdjWidth)
        {
        }



        public SolidColorBrush AdjBndBrush
        {
            get { return new SolidColorBrush(GetColor(_AdjBndColor)); }
        }

        public SolidColorBrush UnAdjBndBrush
        {
            get { return new SolidColorBrush(GetColor(_UnAdjBndColor)); }
        }

        public SolidColorBrush AdjNavBrush
        {
            get  { return new SolidColorBrush(GetColor(_AdjNavColor)); }
        }

        public SolidColorBrush UnAdjNavBrush
        {
            get { return new SolidColorBrush(GetColor(_UnAdjNavColor)); }
        }

        public SolidColorBrush AdjPtsBrush
        {
            get { return new SolidColorBrush(GetColor(_AdjPtsColor)); }
        }

        public SolidColorBrush UnAdjPtsBrush
        {
            get { return new SolidColorBrush(GetColor(_UnAdjPtsColor)); }
        }

        public SolidColorBrush WayPtsBrush
        {
            get { return new SolidColorBrush(GetColor(_WayPtsColor)); }
        }


        public static Color GetColor(int argb)
        {
            return Color.FromArgb(GetAlpha(argb), GetRed(argb), GetGreen(argb), GetBlue(argb));
        }
    }
}
