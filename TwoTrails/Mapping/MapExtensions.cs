using FMSC.Core.Utilities;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
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

                    map.Unloaded += Map_Unloaded;

                    _MapRefreshHandlers.Add(map, dah);
                    dah.DelayInvoke();

                } 
            }
        }

        private static void Map_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Map map && _MapRefreshHandlers.ContainsKey(map))
            {
                map.Unloaded -= Map_Unloaded;

                DelayActionHandler dah = _MapRefreshHandlers[map];
                _MapRefreshHandlers.Remove(map);
                dah.Dispose();
            }
        }



        public static void UnhookAnimationDrivers(this Map map)
        {
            Type type = typeof(MapCore);

            object zoomAndPanAnimationDriver = type.GetField("_ZoomAndPanAnimationDriver", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(map);
            object modeSwitchAnationDriver = type.GetField("_ModeSwitchAnationDriver", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(map);

            UnhookAnimationDriver(zoomAndPanAnimationDriver);
            UnhookAnimationDriver(modeSwitchAnationDriver);
        }

        private static void UnhookAnimationDriver(object animationDriver)
        {
            Type type = animationDriver.GetType();

            var f = type.GetField("AnimationProgressProperty", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField).GetValue(animationDriver);

            DependencyProperty dp = (DependencyProperty)f;

            var m = type.GetMethod("OnAnimationProgressChanged", BindingFlags.Instance | BindingFlags.NonPublic);

            EventHandler eh = (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), animationDriver, m);

            DependencyPropertyDescriptor.FromProperty(dp, type).RemoveValueChanged(animationDriver, eh);
        }
    }
}
