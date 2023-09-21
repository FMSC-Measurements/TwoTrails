namespace TwoTrails.Core
{
    public interface IUnitGraphicSettings
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

        UnitGraphicOptions CreateUnitGraphicOptions(string unitCN);
    }
}
