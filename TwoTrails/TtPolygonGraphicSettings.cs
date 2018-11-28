using TwoTrails.Core;

namespace TwoTrails
{
    public class TtPolygonGraphicSettings : IPolygonGraphicSettings
    {
        //All Colors are in the ARGB format

        //FFF44336
        public int AdjBndColor { get; } = -65536;//- 769226;
        //FFB71C1C
        public int UnAdjBndColor { get; } = -4776932;

        //FF3F51B5
        public int AdjNavColor { get; } = -12627531;
        //FF1A237E
        public int UnAdjNavColor { get; } = -15064194;

        //FF9C27B0
        public int AdjPtsColor { get; } = -6543440;
        //FF4A148C
        public int UnAdjPtsColor { get; } = -11922292;

        //FFE65100
        public int WayPtsColor { get; } = -1683200;
        
        public float AdjWidth { get; } = 2;
        public float UnAdjWidth { get; } = 5;


        public PolygonGraphicOptions CreatePolygonGraphicOptions(string polyCN)
        {
            return new PolygonGraphicOptions(polyCN, AdjBndColor, UnAdjBndColor,
                AdjNavColor, UnAdjNavColor, AdjPtsColor, UnAdjPtsColor, WayPtsColor, AdjWidth, UnAdjWidth);
        }
    }
}
