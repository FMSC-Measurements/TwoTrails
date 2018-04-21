using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public interface IObservableTtManager : ITtManager
    {
        ReadOnlyObservableCollection<TtPoint> Points { get; }
        ReadOnlyObservableCollection<TtPolygon> Polygons { get; }
        ReadOnlyObservableCollection<TtMetadata> Metadata { get; }
        ReadOnlyObservableCollection<TtGroup> Groups { get; }
        ReadOnlyObservableCollection<TtMediaInfo> MediaInfo { get; }
    }
}
