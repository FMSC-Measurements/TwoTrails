using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails.Mapping
{
    public class PolygonVisibilityControl : NotifyPropertyChangedEx
    {
        private readonly ObservableCollection<TtMapPolygonManager> PolygonManagers;

        private readonly PropertyInfo VisibileProperty;
        private readonly PropertyInfo AdjBndVisibleProperty;
        private readonly PropertyInfo AdjBndPointsVisibleProperty;
        private readonly PropertyInfo UnAdjBndVisibleProperty;
        private readonly PropertyInfo UnAdjBndPointsVisibleProperty;
        private readonly PropertyInfo AdjNavVisibleProperty;
        private readonly PropertyInfo AdjNavPointsVisibleProperty;
        private readonly PropertyInfo UnAdjNavVisibleProperty;
        private readonly PropertyInfo UnAdjNavPointsVisibleProperty;
        private readonly PropertyInfo AdjMiscPointsVisibleProperty;
        private readonly PropertyInfo UnAdjMiscPointsVisibleProperty;
        private readonly PropertyInfo WayPointsVisibleProperty;

        private bool ignoreChanges;


        public PolygonVisibilityControl()
        {
            Type type = typeof(TtMapPolygonManager);

            VisibileProperty = type.GetProperty(nameof(TtMapPolygonManager.Visible));
            AdjBndVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.AdjBndVisible));
            AdjBndPointsVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.AdjBndPointsVisible));
            UnAdjBndVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.UnAdjBndVisible));
            UnAdjBndPointsVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.UnAdjBndPointsVisible));
            AdjNavVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.AdjNavVisible));
            AdjNavPointsVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.AdjNavPointsVisible));
            UnAdjNavVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.UnAdjNavVisible));
            UnAdjNavPointsVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.UnAdjNavPointsVisible));
            AdjMiscPointsVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.AdjMiscPointsVisible));
            UnAdjMiscPointsVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.UnAdjMiscPointsVisible));
            WayPointsVisibleProperty = type.GetProperty(nameof(TtMapPolygonManager.WayPointsVisible));
        }


        public void AddManagers(ObservableCollection<TtMapPolygonManager> polyManagers)
        {
            foreach (TtMapPolygonManager pm in PolygonManagers)
            {
                pm.PropertyChanged += PolyManager_PropertyChanged;
            }

            PolygonManagers.CollectionChanged += PolygonManagers_CollectionChanged;
        }


        private void PolygonManagers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TtMapPolygonManager pm in e.NewItems)
                    {
                        pm.PropertyChanged += PolyManager_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtMapPolygonManager pm in e.OldItems)
                    {
                        pm.PropertyChanged -= PolyManager_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (TtMapPolygonManager pm in e.NewItems)
                    {
                        pm.PropertyChanged += PolyManager_PropertyChanged;
                    }
                    foreach (TtMapPolygonManager pm in e.OldItems)
                    {
                        pm.PropertyChanged -= PolyManager_PropertyChanged;
                    }
                    break;
            }
        }

        private void PolyManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TtMapPolygonManager pm)
            {
                switch (e.PropertyName)
                {
                    case nameof(Visible): UpdateVisibilityField(ref _Visible, pm, VisibileProperty); break;
                    case nameof(AdjBndVisible): UpdateVisibilityField(ref _AdjBndVisible, pm, AdjBndVisibleProperty); break;
                    case nameof(AdjBndPointsVisible): UpdateVisibilityField(ref _AdjBndPointsVisible, pm, AdjBndPointsVisibleProperty); break;
                    case nameof(UnAdjBndVisible): UpdateVisibilityField(ref _UnAdjBndVisible, pm, UnAdjBndVisibleProperty); break;
                    case nameof(UnAdjBndPointsVisible): UpdateVisibilityField(ref _UnAdjBndPointsVisible, pm, UnAdjBndPointsVisibleProperty); break;
                    case nameof(AdjNavVisible): UpdateVisibilityField(ref _AdjNavVisible, pm, AdjNavVisibleProperty); break;
                    case nameof(AdjNavPointsVisible): UpdateVisibilityField(ref _AdjNavPointsVisible, pm, AdjNavPointsVisibleProperty); break;
                    case nameof(UnAdjNavVisible): UpdateVisibilityField(ref _UnAdjNavVisible, pm, UnAdjNavVisibleProperty); break;
                    case nameof(UnAdjNavPointsVisible): UpdateVisibilityField(ref _UnAdjNavPointsVisible, pm, UnAdjNavPointsVisibleProperty); break;
                    case nameof(AdjMiscPointsVisible): UpdateVisibilityField(ref _AdjMiscPointsVisible, pm, AdjMiscPointsVisibleProperty); break;
                    case nameof(UnAdjMiscPointsVisible): UpdateVisibilityField(ref _UnAdjMiscPointsVisible, pm, UnAdjMiscPointsVisibleProperty); break;
                    case nameof(WayPointsVisible): UpdateVisibilityField(ref _WayPointsVisible, pm, WayPointsVisibleProperty); break;
                }
            }
        }


        private void UpdateVisibilityField(ref bool? field, TtMapPolygonManager polyMapManager, PropertyInfo propertyInfo)
        {
            ignoreChanges = true;

            bool value = (bool)propertyInfo.GetValue(polyMapManager);

            if ((field == true && !value) || (field == false && value))
                field = null;
            else if (Visible == null)
            {
                bool areVis = PolygonManagers.All(pm => (bool)propertyInfo.GetValue(pm));
                field = areVis || PolygonManagers.All(pm => !(bool)propertyInfo.GetValue(pm)) ? (bool?)areVis : null;
            }

            OnPropertyChanged(propertyInfo.Name);

            ignoreChanges = false;
        }


        private bool SetVisibilityField(ref bool? field, bool? value, PropertyInfo propertyInfo, [CallerMemberName] string propertyName = null)
        {
            if (field != value && !ignoreChanges)
            {
                field = value;

                if (field is bool val)
                {
                    foreach (TtMapPolygonManager pm in PolygonManagers)
                    {
                        propertyInfo.SetValue(pm, val);
                    }
                }

                OnPropertyChanged(propertyName);

                return true;
            }

            return false;
        }



        private object locker = new object();
        
        private bool? _Visible;
        public bool? Visible
        {
            get { return _Visible; }
            set { SetVisibilityField(ref _Visible, value, VisibileProperty); }
        }


        private bool? _AdjBndVisible;
        public bool? AdjBndVisible
        {
            get { return _AdjBndVisible; }
            set { SetVisibilityField(ref _AdjBndVisible, value, AdjBndVisibleProperty); }
        }

        private bool? _AdjBndPointsVisible;
        public bool? AdjBndPointsVisible
        {
            get { return _AdjBndPointsVisible; }
            set { SetVisibilityField(ref _AdjBndPointsVisible, value, AdjBndPointsVisibleProperty); }
        }


        private bool? _UnAdjBndVisible;
        public bool? UnAdjBndVisible
        {
            get { return _UnAdjBndVisible; }
            set { SetVisibilityField(ref _UnAdjBndVisible, value, UnAdjBndVisibleProperty); }
        }

        private bool? _UnAdjBndPointsVisible;
        public bool? UnAdjBndPointsVisible
        {
            get { return _UnAdjBndPointsVisible; }
            set { SetVisibilityField(ref _UnAdjBndPointsVisible, value, UnAdjBndPointsVisibleProperty); }
        }


        private bool? _AdjNavVisible;
        public bool? AdjNavVisible
        {
            get { return _AdjNavVisible; }
            set { SetVisibilityField(ref _AdjNavVisible, value, AdjNavVisibleProperty); }
        }

        private bool? _AdjNavPointsVisible;
        public bool? AdjNavPointsVisible
        {
            get { return _AdjNavPointsVisible; }
            set { SetVisibilityField(ref _AdjNavPointsVisible, value, AdjNavPointsVisibleProperty); }
        }


        private bool? _UnAdjNavVisible;
        public bool? UnAdjNavVisible
        {
            get { return _UnAdjNavVisible; }
            set { SetVisibilityField(ref _UnAdjNavVisible, value, UnAdjNavVisibleProperty); }
        }

        private bool? _UnAdjNavPointsVisible;
        public bool? UnAdjNavPointsVisible
        {
            get { return _UnAdjNavPointsVisible; }
            set { SetVisibilityField(ref _UnAdjNavPointsVisible, value, UnAdjNavPointsVisibleProperty); }
        }



        private bool? _AdjMiscPointsVisible;
        public bool? AdjMiscPointsVisible
        {
            get { return _AdjMiscPointsVisible; }
            set { SetVisibilityField(ref _AdjMiscPointsVisible, value, AdjMiscPointsVisibleProperty); }
        }

        private bool? _UnAdjMiscPointsVisible;
        public bool? UnAdjMiscPointsVisible
        {
            get { return _UnAdjMiscPointsVisible; }
            set { SetVisibilityField(ref _UnAdjMiscPointsVisible, value, UnAdjMiscPointsVisibleProperty); }
        }


        private bool? _WayPointsVisible;
        public bool? WayPointsVisible
        {
            get { return _WayPointsVisible; }
            set { SetVisibilityField(ref _WayPointsVisible, value, WayPointsVisibleProperty); }
        }
    }
}
