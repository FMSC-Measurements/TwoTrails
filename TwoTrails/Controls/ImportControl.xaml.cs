using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
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

        bool ignoreSelectionChange;
        bool? _AllPolysChecked;
        bool? AllPolysChecked
        {
            get => _AllPolysChecked;
            set {
                if (value != null)
                {
                    ignoreSelectionChange = true;
                    Polygons.ForEach(sp =>
                    {
                        sp.IsSelected = (bool)value;
                    });
                    ignoreSelectionChange = false;
                    PolygonSelectionChanged?.Invoke(this, new EventArgs());
                }
                _AllPolysChecked = value;
            }
        }

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

        public ImportControl(IReadOnlyTtDataLayer dal, bool sortByName, bool hasMetadata = false, bool hasGroups = false, bool hasNmea = false)
        {
            DAL = dal;
            Polygons = dal.GetPolygons().OrderBy(p => sortByName ? (object)p.Name : p.TimeCreated).Select(p => new SelectablePolygon(p)).ToList();

            foreach (SelectablePolygon sp in Polygons)
            {
                sp.PropertyChanged += (Object sender, PropertyChangedEventArgs e) => {
                    if (e.PropertyName == nameof(SelectablePolygon.IsSelected))
                    {
                        if (!ignoreSelectionChange)
                            PolygonSelectionChanged?.Invoke(this, new EventArgs());
                        else
                        {

                        }
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
                IsSelected = true;
            }
        }
    }
}
