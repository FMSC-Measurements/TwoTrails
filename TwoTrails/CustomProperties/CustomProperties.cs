using System.Windows;
using System.Windows.Controls;

namespace TwoTrails.CustomProperties
{
    public static class CustomProperties
    {
        public static readonly DependencyProperty BackgroundEnabledProperty =
        DependencyProperty.RegisterAttached("BackgroundEnabled", typeof(bool), typeof(CustomProperties), new PropertyMetadata(default(bool)));

        public static void SetBackgroundEnabled(Control element, bool value)
        {
            element.SetValue(BackgroundEnabledProperty, value);
        }

        public static double GetBackgroundEnabled(Control element)
        {
            return (double)element.GetValue(BackgroundEnabledProperty);
        }
    }
}
