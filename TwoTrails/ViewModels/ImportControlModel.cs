using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails.ViewModels
{
    public class ImportControlModel : NotifyPropertyChangedEx
    {
        public event EventHandler PolygonSelectionChanged;

        private bool ignoreSelectionChange;
        private bool _AllPolysChecked = true;
        public bool AllPolysChecked
        {
            get => _AllPolysChecked;
            set
            {
                ignoreSelectionChange = true;
                Polygons.ForEach(sp =>
                {
                    sp.IsSelected = value;
                });
                ignoreSelectionChange = false;
                PolygonSelectionChanged?.Invoke(this, new EventArgs());

                _AllPolysChecked = value;
            }
        }

        public int TotalPoints => Polygons.Sum(p => p.IsSelected ? p.PointCount : 0);

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

        public ImportControlModel(IReadOnlyTtDataLayer dal, bool sortByName, bool hasMetadata = false, bool hasGroups = false, bool hasNmea = false)
        {
            DAL = dal;
            Polygons = dal.GetPolygons()
                .OrderBy(p => sortByName ? (object)p.Name : p.TimeCreated)
                .Select(p => new SelectablePolygon(p, dal.GetPointCount(p.CN)))
                .ToList();

            foreach (SelectablePolygon sp in Polygons)
            {
                sp.PropertyChanged += (Object sender, PropertyChangedEventArgs e) =>
                {
                    if (e.PropertyName == nameof(SelectablePolygon.IsSelected))
                    {
                        if (ignoreSelectionChange)
                        {
                            PolygonSelectionChanged?.Invoke(this, new EventArgs());
                        }
                        else
                        {
                            _AllPolysChecked = Polygons.All(p => p.IsSelected);
                            OnPropertyChanged(nameof(AllPolysChecked));
                        }

                        OnPropertyChanged(nameof(TotalPoints));
                    }
                };
            }

            HasMetadata = hasMetadata;
            HasGroups = hasGroups;
            HasNmea = hasNmea;
        }

        public class SelectablePolygon : NotifyPropertyChangedEx
        {
            public TtPolygon Polygon { get; }
            public bool IsSelected { get { return Get<bool>(); } set { Set(value); } }
            public int PointCount { get; private set; }

            public SelectablePolygon(TtPolygon polygon, int pointCount)
            {
                Polygon = polygon;
                IsSelected = true;
                PointCount = pointCount;
            }
        }
    }
}
