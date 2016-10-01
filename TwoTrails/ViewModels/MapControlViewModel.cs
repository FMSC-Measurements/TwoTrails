using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Mapping;

namespace TwoTrails.ViewModels
{
    public class MapControlViewModel
    {

        public MapControlViewModel(Map map, TtManager manager)
        {
            MapManager mm = new MapManager(map, manager.Points, manager.Polygons);
        }

    }
}
