using CSUtil.ComponentModel;
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
using TwoTrails.DAL;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for ImportControl.xaml
    /// </summary>
    public partial class ImportControl : UserControl
    {
        public event EventHandler PolygonSelectionChanged;

        public List<SelectablePolygon> Polygons { get; }

        public IEnumerable<string> SelectedPolygons { get { return Polygons.Where(p => p.IsSelected).Select(p => p.Polygon.CN); } }

        public IReadOnlyTtDataLayer DAL { get; }

        public bool HasMetadata { get; }
        public bool IncludeMetadata { get; set; } = true;

        public bool HasGroups { get; }
        public bool IncludeGroups { get; set; } = true;

        public bool HasNmea { get; }
        public bool IncludeNmea { get; set; } = true;


        public bool HasSelectedPolygons { get { return Polygons.Where(p => p.IsSelected).Any(); } }

        public ImportControl(IReadOnlyTtDataLayer dal, bool hasMetadata = false, bool hasGroups = false, bool hasNmea = false)
        {
            DAL = dal;
            Polygons = dal.GetPolygons().Select(p => new SelectablePolygon(p)).ToList();

            foreach (SelectablePolygon sp in Polygons)
            {
                sp.PropertyChanged += (Object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                    if (e.PropertyName == nameof(SelectablePolygon.IsSelected))
                    {
                        PolygonSelectionChanged?.Invoke(this, new EventArgs());
                    }
                };
            }

            HasMetadata = hasMetadata;
            HasGroups = hasGroups;
            HasNmea = hasNmea;

            InitializeComponent();

            this.DataContext = this;
        }

        public class SelectablePolygon : NotifyPropertyChangedEx
        {
            public TtPolygon Polygon { get; }
            public bool IsSelected { get { return Get<bool>(); } set { Set(value); } }

            public SelectablePolygon(TtPolygon polygon)
            {
                Polygon = polygon;
                IsSelected = false;
            }
        }
    }
}
