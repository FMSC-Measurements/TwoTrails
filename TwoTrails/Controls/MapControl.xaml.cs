using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoTrails.Core;
using TwoTrails.Mapping;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        public static readonly DependencyProperty ManagerProperty =
                DependencyProperty.Register(nameof(Manager), typeof(TtManager), typeof(MapControl));

        public TtManager Manager { get; set; }

        public MapControl()
        {
            InitializeComponent();

            this.Loaded += MapControl_Loaded;
        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            Manager = (TtManager)this.GetValue(ManagerProperty);

            MapManager mm = new MapManager(map, Manager.Points, Manager.Polygons);
        }
    }
}
