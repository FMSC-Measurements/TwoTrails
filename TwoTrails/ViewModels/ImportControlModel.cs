using FMSC.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Units;
using TwoTrails.DAL;

namespace TwoTrails.ViewModels
{
    public class ImportControlModel : BaseModel
    {
        public event EventHandler UnitSelectionChanged;

        private bool ignoreSelectionChange;
        private bool _AllUnitsChecked = true;
        public bool AllUnitsChecked
        {
            get => _AllUnitsChecked;
            set
            {
                ignoreSelectionChange = true;
                Units.ForEach(sp =>
                {
                    sp.IsSelected = value;
                });
                ignoreSelectionChange = false;
                UnitSelectionChanged?.Invoke(this, new EventArgs());

                _AllUnitsChecked = value;
            }
        }

        public int TotalPoints => Units.Sum(p => p.IsSelected ? p.PointCount : 0);

        public List<SelectableUnit> Units { get; }

        public IEnumerable<string> SelectedUnits { get { return Units.Where(p => p.IsSelected).Select(p => p.Unit.CN); } }

        public IReadOnlyTtDataLayer DAL { get; }

        public bool HasMetadata { get; }
        public bool IncludeMetadata { get; set; } = true;

        public bool HasGroups { get; }
        public bool IncludeGroups { get; set; } = true;

        public bool HasNmea { get; }
        public bool IncludeNmea { get; set; } = true;


        public bool HasSelectedUnits { get { return Units.Where(p => p.IsSelected).Any(); } }

        public ImportControlModel(IReadOnlyTtDataLayer dal, bool sortByName, bool hasMetadata = false, bool hasGroups = false, bool hasNmea = false)
        {
            DAL = dal;
            Units = dal.GetUnits()
                .OrderBy(p => sortByName ? (object)p.Name : p.TimeCreated)
                .Select(p => new SelectableUnit(p, dal.GetPointCount(p.CN)))
                .ToList();

            foreach (SelectableUnit sp in Units)
            {
                sp.PropertyChanged += (Object sender, PropertyChangedEventArgs e) =>
                {
                    if (e.PropertyName == nameof(SelectableUnit.IsSelected))
                    {
                        if (!ignoreSelectionChange)
                        {
                            _AllUnitsChecked = Units.All(p => p.IsSelected);
                            OnPropertyChanged(nameof(AllUnitsChecked), nameof(HasSelectedUnits));
                            UnitSelectionChanged?.Invoke(this, new EventArgs());
                        }

                        OnPropertyChanged(nameof(TotalPoints));
                    }
                };
            }

            HasMetadata = hasMetadata;
            HasGroups = hasGroups;
            HasNmea = hasNmea;
        }

        public class SelectableUnit : BaseModel
        {
            public TtUnit Unit { get; }
            public bool IsSelected { get { return Get<bool>(); } set { Set(value); } }
            public int PointCount { get; private set; }

            public SelectableUnit(TtUnit unit, int pointCount)
            {
                Unit = unit;
                IsSelected = true;
                PointCount = pointCount;
            }
        }
    }
}
