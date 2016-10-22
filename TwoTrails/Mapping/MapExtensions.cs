using FMSC.Core.Utilities;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TwoTrails.Mapping
{
    public static class MapExtensions
    {
        private static Dictionary<Map, DelayActionHandler> _MapRefreshHandlers = new Dictionary<Map, DelayActionHandler>();

        private static object locker = new object();


        public static void Refresh(this Map map)
        {
            lock (locker)
            {
                if (_MapRefreshHandlers.ContainsKey(map))
                {
                    _MapRefreshHandlers[map].DelayInvoke();
                }
                else
                {
                    DelayActionHandler dah = new DelayActionHandler(
                        () =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Pushpin p = new Pushpin();
                                p.Visibility = Visibility.Hidden;
                                p.Location = new Location(map.Center.Latitude, map.Center.Longitude);

                                map.Children.Add(p);
                                map.Children.Remove(p);

                                _MapRefreshHandlers.Remove(map);
                            });
                        },
                        50);
                    _MapRefreshHandlers.Add(map, dah);
                    dah.DelayInvoke();
                } 
            }
        }
    }
}
