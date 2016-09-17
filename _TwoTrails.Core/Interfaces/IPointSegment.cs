using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public interface IPointSegment
    {
        void Add(TtPoint point);
        void Adjust();
        int Count { get; }
        bool IsValid { get; }
    }
}
