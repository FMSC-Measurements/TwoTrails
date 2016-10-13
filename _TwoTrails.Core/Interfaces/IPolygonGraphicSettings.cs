using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core
{
    public interface IPolygonGraphicSettings
    {
        int AdjBndColor { get; }
        int UnAdjBndColor { get; }

        int AdjNavColor { get; }
        int UnAdjNavColor { get; }

        int AdjPtsColor { get; }
        int UnAdjPtsColor { get; }

        int WayPtsColor { get; }

        float AdjWidth { get; }
        float UnAdjWidth { get; }

        PolygonGraphicOptions CreatePolygonGraphicOptions(string polyCN);
    }
}
