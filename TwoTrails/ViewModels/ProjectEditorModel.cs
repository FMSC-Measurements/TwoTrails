using CSUtil.ComponentModel;
using FMSC.Core;
using FMSC.Core.Windows.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;
using TwoTrails.Dialogs;
using static TwoTrails.Core.MediaTools;

namespace TwoTrails.ViewModels
{
    public class ProjectEditorModel : NotifyPropertyChangedEx
    {
        public ICommand OpenMapWindowCommand { get; }
        public ICommand UndoCommand { get; set; }
        public ICommand RedoCommand { get; set; }


        private TtProject _Project;

        public TtProjectInfo ProjectInfo { get { return _Project.ProjectInfo; } }
        public TtHistoryManager Manager { get { return _Project.Manager; } }
        public TtSettings Settings { get { return _Project.Settings; } }
        
        public MapControl MapControl { get; set; }
        public MapWindow MapWindow { get; set; }
        public bool IsMapWindowOpen { get { return MapWindow != null; } }
        
        public PointEditorControl DataController { get; }

        public ReadOnlyObservableCollection<TtPolygon> Polygons { get { return Manager.Polygons; } }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get { return Manager.Metadata; } }
        public ReadOnlyObservableCollection<TtGroup> Groups { get { return Manager.Groups; } }
        public ReadOnlyObservableCollection<TtMediaInfo> MediaInfo { get { return Manager.MediaInfo; } }
        

        public ProjectEditorModel(TtProject project)
        {
            _Project = project;

            DataController = new PointEditorControl(project.DataEditor, new DataStyleModel(project));

            PolygonChangedCommand = new RelayCommand(x => PolygonChanged(x as TtPolygon));
            CreatePolygonCommand = new RelayCommand(x => CreatePolygon(x as ListBox));
            DeletePolygonCommand = new RelayCommand(x => DeletePolygon(x as ListBox));

            PolygonUpdateAccCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => UpdatePolygonAcc(),
                x => CurrentPolygon != null && CurrentPolygon.Accuracy != _PolygonAccuracy,
                this,
                x => new { x.PolygonAccuracy, CurrentPolygon.Accuracy });

            PolygonAccuracyLookupCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => AccuracyLookup(),
                x => CurrentPolygon != null,
                this,
                x => x.CurrentPolygon);

            SavePolygonSummary = new BindedRelayCommand<ProjectEditorModel>(
                x => SavePolygonsummary(),
                x => CurrentPolygon != null, this, x => x.CurrentPolygon);

            MetadataChangedCommand = new RelayCommand(x => MetadataChanged(x as TtMetadata));
            NewMetadataCommand = new RelayCommand(x => NewMetadata(x as ListBox));
            SetDefaultMetadataCommand = new RelayCommand(x => SetMetadataDefault());
            DeleteMetadataCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => DeleteMetadata(),
                x => x != null && (x as TtMetadata).CN != Consts.EmptyGuid,
                this,
                x => x.CurrentMetadata);

            MetadataZoneTextboxEditedCommand = new RelayCommand(x => MetadataZoneTextboxEdited(x as string));

            MetadataUpdateZoneCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => UpdateMetadataZone(),
                x => CurrentMetadata != null && CurrentMetadata.Zone != MetadataZone,
                this,
                x => x.MetadataZone);


            GroupChangedCommand = new RelayCommand(x => GroupChanged(x as TtGroup));
            NewGroupCommand = new RelayCommand(x => NewGroup(x as ListBox));
            DeleteGroupCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => DeleteGroup(),
                x => x != null && (x as TtGroup).CN != Consts.EmptyGuid,
                this,
                x => x.CurrentGroup);

            PolygonShapeChanged(null);

            foreach (TtPolygon poly in Manager.Polygons)
                poly.PolygonChanged += PolygonShapeChanged;

            ((INotifyCollectionChanged)Manager.Polygons).CollectionChanged += PolygonCollectionChanged;


            Tiles = new ObservableCollection<ImageTile>();
            MediaInfoChangedCommand = new RelayCommand(x => MediaInfoChanged(x as TtMediaInfo));
            MediaSelectedCommand = new RelayCommand(x => MediaSelected(x as TtMediaInfo));
            HideMediaViewerCommand = new RelayCommand(x => MediaViewerVisible = false);


            Func<ProjectTabSection, Type, bool> doesTabAndDataMatch = (tab, type) =>
            {
                if (type == null)
                    return true;
                switch (tab)
                {
                    case ProjectTabSection.Project: return type == ProjectProperties.DataType;
                    case ProjectTabSection.Points: return type.IsAssignableFrom(PointProperties.DataType);
                    case ProjectTabSection.Polygons: return type == PolygonProperties.DataType;
                    case ProjectTabSection.Metadata: return type == MetadataProperties.DataType;
                    case ProjectTabSection.Groups: return type == GroupProperties.DataType;
                    case ProjectTabSection.Media: return type == PointProperties.DataType;
                    case ProjectTabSection.Map:
                    default: return false;
                }
            };

            UndoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Manager.Undo(),
                x => Manager.CanUndo && doesTabAndDataMatch(_Project.ProjectTab.CurrentTabSection, Manager.UndoCommandType),
                Manager,
                x => x.CanUndo);

            RedoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Manager.Redo(),
                x => Manager.CanRedo && doesTabAndDataMatch(_Project.ProjectTab.CurrentTabSection, Manager.RedoCommandType),
                Manager,
                x => x.CanRedo);

            OpenMapWindowCommand = new RelayCommand(x =>
            {
                if (x is Grid grid)
                {
                    MapControl = grid.Children[0] as MapControl;

                    grid.Children.Remove(MapControl);

                    MapWindow = MapControl != null ? new MapWindow(_Project.ProjectName, MapControl) : new MapWindow(_Project);
                    OnPropertyChanged(nameof(IsMapWindowOpen), nameof(MapWindow));

                    MapWindow.Closed += (s, e) =>
                    {
                        MapWindow = null;

                        grid.Dispatcher.Invoke(() =>
                        {
                            grid.Children.Add(MapControl);
                            OnPropertyChanged(nameof(IsMapWindowOpen), nameof(MapWindow));
                        });
                    };

                    MapWindow.Show();

                    _Project.ProjectTab.SwitchToTabSection(ProjectTabSection.Points);
                }
            });

            Manager.HistoryChanged += (s, e) =>
            {
                if (e.DataType != null && e.HistoryEventType == HistoryEventType.Undone || e.HistoryEventType == HistoryEventType.Redone)
                {
                    if (e.DataType == PolygonProperties.DataType)
                    {
                        BindPolygonValues(CurrentPolygon);
                    }
                    else if (e.DataType == GroupProperties.DataType)
                    {
                        
                    }
                    else if (e.DataType == MetadataProperties.DataType)
                    {
                        
                    }
                }
            };

            CurrentMetadata = Metadata[0];
            CurrentGroup = Groups[0];

            if (Polygons != null && Polygons.Count > 0)
                CurrentPolygon = Polygons[0];

            if (MediaInfo != null && MediaInfo.Count > 0)
                CurrentMediaInfo = MediaInfo[0];
        }

        public void CloseWindows()
        {
            MapWindow?.Close();
        }


        #region Polygons
        #region Commands
        public ICommand PolygonChangedCommand { get; }
        public ICommand CreatePolygonCommand { get; }
        public ICommand DeletePolygonCommand { get; }
        public ICommand PolygonUpdateAccCommand { get; }
        public ICommand PolygonAccuracyLookupCommand { get; }
        public ICommand SavePolygonSummary { get; }
        #endregion

        #region Properties
        private TtPolygon _CurrentPolygon;
        public TtPolygon CurrentPolygon
        {
            get { return _CurrentPolygon; }
            private set
            {
                TtPolygon old = _CurrentPolygon;

                SetField(ref _CurrentPolygon, value, () => {
                    if (old != null)
                    {
                        old.PolygonChanged -= GeneratePolygonSummary;
                    }

                    BindPolygonValues(value);

                    if (_CurrentPolygon != null)
                    {
                        _CurrentPolygon.PolygonChanged += GeneratePolygonSummary;

                        ValidatePolygon(value);

                        GeneratePolygonSummary(_CurrentPolygon);
                    }
                });
            }
        }

        private string _PolygonName = String.Empty;
        public string PolygonName
        {
            get => _PolygonName;
            set => EditPolygonValue(ref _PolygonName, value, PolygonProperties.NAME);
        }

        private double _PolygonAccuracy;
        public double PolygonAccuracy
        {
            get => _PolygonAccuracy;
            set {
                _PolygonAccuracy = value;
                OnPropertyChanged(nameof(PolygonAccuracy));
            }
        }

        private string _PolygonDescription = String.Empty;
        public string PolygonDescription
        {
            get => _PolygonDescription;
            set => EditPolygonValue(ref _PolygonDescription, value, PolygonProperties.DESCRIPTION);
        }

        private int? _PolygonPointStartIndex;
        public int? PolygonPointStartIndex
        {
            get => _PolygonPointStartIndex;
            set => EditPolygonValue(ref _PolygonPointStartIndex, value, PolygonProperties.POINT_START_INDEX);
        }

        private int? _PolygonIncrement;
        public int? PolygonIncrement
        {
            get => _PolygonIncrement;
            set => EditPolygonValue(ref _PolygonIncrement, value, PolygonProperties.INCREMENT);
        }

        public double TotalPolygonArea { get { return Get<double>(); } set { Set(value); } }
        public double TotalPolygonPerimeter { get { return Get<double>(); } set { Set(value); } }


        private void EditPolygonValue<T>(ref T? origValue, T? newValue, PropertyInfo property, bool allowNull = false) where T : struct, IEquatable<T>
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditPolygon(CurrentPolygon, property, newValue);
                }
            }
        }

        private void EditPolygonValue<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : class
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditPolygon(CurrentPolygon, property, newValue);
                }
            }
        }
        #endregion


        private void BindPolygonValues(TtPolygon polygon)
        {
            if (polygon != null)
            {
                _PolygonName = polygon.Name;
                _PolygonAccuracy = polygon.Accuracy;
                _PolygonPointStartIndex = polygon.PointStartIndex;
                _PolygonIncrement = polygon.Increment;
                _PolygonDescription = polygon.Description;
            }
            else
            {
                _PolygonName = String.Empty;
                _PolygonAccuracy = Consts.DEFAULT_POINT_ACCURACY;
                _PolygonPointStartIndex = Consts.DEFAULT_POINT_START_INDEX;
                _PolygonIncrement = Consts.DEFAULT_POINT_INCREMENT;
                _PolygonDescription = String.Empty;
            }

            OnPropertyChanged(
                nameof(PolygonName),
                nameof(PolygonAccuracy),
                nameof(PolygonPointStartIndex),
                nameof(PolygonIncrement),
                nameof(PolygonDescription));
        }


        private void GeneratePolygonSummary(TtPolygon polygon)
        {
            PolygonSummary = HaidLogic.GenerateSummary(Manager, polygon, true);
        }

        public PolygonSummary PolygonSummary { get { return Get<PolygonSummary>(); } set { Set(value); } }


        private void PolygonCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TtPolygon poly in e.NewItems)
                        poly.PolygonChanged += PolygonShapeChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPolygon poly in e.OldItems)
                        poly.PolygonChanged -= PolygonShapeChanged;
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    foreach (TtPolygon poly in e.NewItems)
                        poly.PolygonChanged += PolygonShapeChanged;
                    foreach (TtPolygon poly in e.OldItems)
                        poly.PolygonChanged -= PolygonShapeChanged;
                    break;
            }
        }

        private void PolygonShapeChanged(TtPolygon polygon)
        {
            double area = 0, perim = 0;
            foreach (TtPolygon poly in Manager.Polygons)
            {
                area += poly.Area;
                perim += poly.Perimeter;
            }

            TotalPolygonArea = area;
            TotalPolygonPerimeter = perim;

            ValidatePolygon(polygon);
        }

        private void PolygonChanged(TtPolygon poly)
        {
            CurrentPolygon = poly;
        }


        private void UpdatePolygonAcc()
        {
            if (!_PolygonAccuracy.Equals(CurrentPolygon.Accuracy))
            {
                Manager.EditPolygon(CurrentPolygon, PolygonProperties.ACCURACY, _PolygonAccuracy);
            }

            OnPropertyChanged(nameof(PolygonAccuracy));
        }

        private void AccuracyLookup()
        {
            SessionData.HasGpsAccReport();

            AccuracyDataDialog dialog = new AccuracyDataDialog(SessionData.GpsAccuracyReport, _CurrentPolygon.Accuracy, SessionData.MakeID, SessionData.ModelID)
            {
                Owner = _Project.MainModel.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                CurrentPolygon.Accuracy = dialog.Accuracy;
                PolygonAccuracy = dialog.Accuracy;
                SessionData.MakeID = dialog.MakeID;
                SessionData.ModelID = dialog.ModelID;
                OnPropertyChanged(nameof(PolygonAccuracy));
            }
        }

        private void CreatePolygon(ListBox listBox)
        {
            Manager.AddPolygon(CreatePolygon());
            listBox.SelectedIndex = listBox.Items.Count - 1;
        }

        /// <summary>
        /// Create a Polygon
        /// </summary>
        /// <param name="name">Name of Polygon</param>
        /// <param name="pointStartIndex">Point starting index for points in the polygon</param>
        /// <returns>New Polygon</returns>
        public TtPolygon CreatePolygon(String name = null, int pointStartIndex = 0)
        {
            int num = Manager.PolygonCount + 1;
            return new TtPolygon()
            {
                Name = name ?? $"Poly {num}",
                PointStartIndex = pointStartIndex > 0 ? pointStartIndex : num * 1000 + Consts.DEFAULT_POINT_INCREMENT
            };
        }

        private void DeletePolygon(ListBox listBox)
        {
            if (CurrentPolygon != null)
            {
                string delSupMsg = "";

                List<TtPoint> points = Manager.GetPoints(CurrentPolygon.CN);
                if (points.Count > 0)
                {
                    int qlpts = points.Count(p => p.HasQuondamLinks);
                    int rpts = points.Count - qlpts;


                    delSupMsg = $"\n{rpts} point{(rpts > 1 ? "s" : String.Empty)} will be deleted" +
                        $"{(qlpts > 0 ? $" and {qlpts} will be moved to replace quondams" : String.Empty)}.";
                }

                if (MessageBox.Show($"Confirm Delete Polygon '{CurrentPolygon.Name}'.{delSupMsg}", "Delete Polygon",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    int oldIndex = listBox.SelectedIndex;

                    Manager.DeletePolygon(CurrentPolygon);

                    if (Polygons.Count > 0 && listBox.SelectedIndex < 0)
                    {
                        if (oldIndex >= Polygons.Count)
                        {
                            listBox.SelectedItem = Polygons.Last();
                        }
                        else
                        {
                            listBox.SelectedIndex = oldIndex;
                        }
                    }
                }
            }
        }
        
        private void SavePolygonsummary()
        {
            if (CurrentPolygon != null)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    DefaultExt = "(Text File *.txt)|*.txt",
                    FileName = $"{CurrentPolygon.Name}_Summary"
                };
                sfd.DefaultExt = ".txt";
                sfd.OverwritePrompt = true;

                if (sfd.ShowDialog() == true)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.Write(HaidLogic.GenerateSummaryHeader(ProjectInfo, _Project.FilePath));
                        sw.WriteLine(PolygonSummary.SummaryText);
                    }
                }
            }
        }

        private void ValidatePolygon(TtPolygon polygon)
        {
            if (polygon != null)
            {
                int zone = 0;
                foreach (TtPoint point in Manager.GetPoints(polygon.CN))
                {
                    if (point is QuondamPoint qp)
                    {
                        if (zone == 0)
                        {
                            zone = qp.ParentPoint.Metadata.Zone;
                        }
                        else if (zone != qp.ParentPoint.Metadata.Zone)
                        {
                            MessageBox.Show($"Polygon '{polygon.Name}' has Quondam Points associated with more than one Metadata " +
                                "Zone or a Zone that is not the same with other points in the polygon. " +
                                "This may cause issues with area calculations. Please make sure all the points use the same zone.",
                                "Polygon Zone Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                    }
                    else
                    {
                        if (zone == 0)
                        {
                            zone = point.Metadata.Zone;
                        }
                        else if (zone != point.Metadata.Zone)
                        {
                            MessageBox.Show($"Polygon '{polygon.Name}' has Points associated with more than one Metadata Zone." +
                                "This may cause issues with area calculations. Please make sure all the points use the same zone.",
                                "Polygon Zone Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                    }
                } 
            }
        }
        #endregion


        #region Metadata
        #region Commands
        public ICommand MetadataChangedCommand { get; }
        public ICommand NewMetadataCommand { get; }
        public ICommand DeleteMetadataCommand { get; }
        public ICommand MetadataZoneTextboxEditedCommand { get; }
        public ICommand MetadataUpdateZoneCommand { get; }
        public ICommand SetDefaultMetadataCommand { get; }
        #endregion

        #region Properties
        private TtMetadata _CurrentMetadata;
        public TtMetadata CurrentMetadata
        {
            get => _CurrentMetadata;
            private set => SetField(ref _CurrentMetadata, value, BindMetadataValues);
        }


        private string _MetadataName = String.Empty;
        public string MetadataName
        {
            get => _MetadataName;
            set => EditMetadataValue(ref _MetadataName, value, MetadataProperties.NAME);
        }

        private string _MetadataComment = String.Empty;
        public string MetadataComment
        {
            get => _MetadataComment;
            set => EditMetadataValue(ref _MetadataComment, value, MetadataProperties.COMMENT);
        }

        private DeclinationType _MetadataDecType;
        public DeclinationType MetadataDecType
        {
            get => _MetadataDecType;
            set => EditMetadataEnum(ref _MetadataDecType, value, MetadataProperties.DEC_TYPE);
        }

        private Datum _MetadataDatum;
        public Datum MetadataDatum
        {
            get => _MetadataDatum;
            set => EditMetadataEnum(ref _MetadataDatum, value, MetadataProperties.DATUM);
        }

        private int _MetadataZone;
        public int MetadataZone
        {
            get => _MetadataZone;
            set
            {
                _MetadataZone = value;
                OnPropertyChanged(nameof(MetadataZone));
            }
        }

        private Distance _MetadataDistance;
        public Distance MetadataDistance
        {
            get => _MetadataDistance;
            set => EditMetadataEnum(ref _MetadataDistance, value, MetadataProperties.DISTANCE);
        }

        private Distance _MetadataElevation;
        public Distance MetadataElevation
        {
            get => _MetadataElevation;
            set => EditMetadataEnum(ref _MetadataElevation, value, MetadataProperties.ELEVATION);
        }
        
        private Slope _MetadataSlope;
        public Slope MetadataSlope
        {
            get => _MetadataSlope;
            set => EditMetadataEnum(ref _MetadataSlope, value, MetadataProperties.SLOPE);
        }

        private string _MetadataGpsReceiver = String.Empty;
        public string MetadataGpsReceiver
        {
            get => _MetadataGpsReceiver;
            set => EditMetadataValue(ref _MetadataGpsReceiver, value, MetadataProperties.GPS_RECEIVER);
        }

        private string _MetadataRangeFinder = String.Empty;
        public string MetadataRangeFinder
        {
            get => _MetadataRangeFinder;
            set => EditMetadataValue(ref _MetadataRangeFinder, value, MetadataProperties.RANGE_FINDER);
        }

        private string _MetadataCompass = String.Empty;
        public string MetadataCompass
        {
            get => _MetadataCompass;
            set => EditMetadataValue(ref _MetadataCompass, value, MetadataProperties.COMPASS);
        }

        private string _MetadataCrew = String.Empty;
        public string MetadataCrew
        {
            get => _MetadataCrew;
            set => EditMetadataValue(ref _MetadataCrew, value, MetadataProperties.CREW);
        }


        private void EditMetadataValue<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : class
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditMetadata(CurrentMetadata, property, newValue);
                }
            }
        }

        private void EditMetadataEnum<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : Enum
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditMetadata(CurrentMetadata, property, newValue);
                }
            }
        }

        private void BindMetadataValues()
        {
            if (CurrentMetadata != null)
            {
                _MetadataName = CurrentMetadata.Name;
                _MetadataComment = CurrentMetadata.Comment;
                _MetadataDecType = CurrentMetadata.DecType;
                _MetadataDatum = CurrentMetadata.Datum;
                _MetadataZone = CurrentMetadata.Zone;
                _MetadataDistance = CurrentMetadata.Distance;
                _MetadataElevation = CurrentMetadata.Elevation;
                _MetadataSlope = CurrentMetadata.Slope;
                _MetadataGpsReceiver = CurrentMetadata.GpsReceiver;
                _MetadataRangeFinder = CurrentMetadata.RangeFinder;
                _MetadataCompass = CurrentMetadata.Compass;
                _MetadataCrew = CurrentMetadata.Crew;
            }
            else
            {
                _MetadataName = String.Empty;
                _MetadataComment = String.Empty;
                _MetadataDecType = Settings.MetadataSettings.DecType;
                _MetadataDatum = Settings.MetadataSettings.Datum;
                _MetadataZone = Settings.MetadataSettings.Zone;
                _MetadataDistance = Settings.MetadataSettings.Distance;
                _MetadataElevation = Settings.MetadataSettings.Elevation;
                _MetadataSlope = Settings.MetadataSettings.Slope;
                _MetadataGpsReceiver = String.Empty;
                _MetadataRangeFinder = String.Empty;
                _MetadataCompass = String.Empty;
                _MetadataCrew = String.Empty;
            }

            OnPropertyChanged(
                nameof(MetadataName),
                nameof(MetadataComment),
                nameof(MetadataDecType),
                nameof(MetadataDatum),
                nameof(MetadataZone),
                nameof(MetadataDistance),
                nameof(MetadataElevation),
                nameof(MetadataSlope),
                nameof(MetadataGpsReceiver),
                nameof(MetadataRangeFinder),
                nameof(MetadataCompass),
                nameof(MetadataCrew));
        }
        #endregion


        private void MetadataChanged(TtMetadata meta)
        {
            MetadataZone = meta != null ? meta.Zone : 13;
            CurrentMetadata = meta;
        }

        private bool MetadataZoneTextboxEdited(string zoneStr)
        {
            if (zoneStr != null)
            {
                if (int.TryParse(zoneStr, out int zone))
                {
                    MetadataZone = zone;
                    return true;
                }
            }

            return false;
        }

        private void UpdateMetadataZone()
        {
            if (MessageBox.Show("Are you sure you want to change the metadata's zone?", "Confirm zone change.", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (!_MetadataZone.Equals(CurrentMetadata.Zone))
                {
                    Manager.EditMetadata(CurrentMetadata, MetadataProperties.ZONE, _MetadataZone);
                }

                OnPropertyChanged(nameof(MetadataZone));
            }
        }
        
        private void SetMetadataDefault()
        {
            _Project.Settings.MetadataSettings.SetMetadataSettings(CurrentMetadata);
            MessageBox.Show("Metadata defaults set.");
        }

        private void NewMetadata(ListBox listBox)
        {
            Manager.AddMetadata(CreateMetadata());
            listBox.SelectedIndex = listBox.Items.Count - 1;
        }

        /// <summary>
        /// Create a new Metadata
        /// </summary>
        /// <param name="name">Name of metadata</param>
        /// <returns>New metadata</returns>
        public TtMetadata CreateMetadata(String name = null)
        {
            return new TtMetadata(Settings.MetadataSettings.CreateDefaultMetadata())
            {
                CN = Guid.NewGuid().ToString(),
                Name = name ?? $"Meta {Manager.Metadata.Count + 1}"
            };
        }

        private void DeleteMetadata()
        {
            if (CurrentMetadata != null && CurrentMetadata.CN != Consts.EmptyGuid)
            {
                if (MessageBox.Show($"Confirm Delete Metadata '{CurrentMetadata.Name}'", "Delete Metadata",
                MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    if (CurrentMetadata.Zone != Manager.DefaultMetadata.Zone &&
                        Manager.GetPoints().Where(p => p.MetadataCN == CurrentMetadata.CN).Any())
                    {
                        if (MessageBox.Show(
                            String.Format("Metdata '{0}' does not have the same zone as the Default Metadata. All the points that use {0} will have their zone converted.", CurrentMetadata),
                            "Convert Zones", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                            return;
                    }

                    Manager.DeleteMetadata(CurrentMetadata);

                    _Project.ProjectUpdated();
                }
            }
        }
        #endregion


        #region Group
        #region Commands
        public ICommand GroupChangedCommand { get; }
        public ICommand NewGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        #endregion

        #region Properties
        public bool GroupFieldIsEditable { get { return Get<bool>(); } set { Set(value); } }

        private TtGroup _BackupGroup;
        private TtGroup _CurrentGroup;
        public TtGroup CurrentGroup
        {
            get { return _CurrentGroup; }
            private set
            {
                TtGroup old = _CurrentGroup;

                SetField(ref _CurrentGroup, value, () => {
                    if (old != null)
                    {
                        old.PropertyChanged -= Group_PropertyChanged;
                    }

                    if (_CurrentGroup != null)
                    {
                        _BackupGroup = new TtGroup(value);
                        GroupFieldIsEditable = value.CN != Consts.EmptyGuid;
                        _CurrentGroup.PropertyChanged += Group_PropertyChanged;
                    }
                    else
                        GroupFieldIsEditable = false;
                });

            }
        }



        private void EditGroupValue<T>(ref T? origValue, T? newValue, PropertyInfo property, bool allowNull = false) where T : struct, IEquatable<T>
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditGroup(CurrentGroup, property, newValue);
                }
            }
        }

        private void EditGroupValue<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : class
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditGroup(CurrentGroup, property, newValue);
                }
            }
        }
        #endregion

        private void Group_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender as TtGroup).Equals(_BackupGroup))
            {
                _Project.ProjectUpdated();
            }
        }


        private void GroupChanged(TtGroup group)
        {
            CurrentGroup = group;
        }

        private void NewGroup(ListBox listBox)
        {
            Manager.AddGroup(CreateGroup());
            listBox.SelectedIndex = listBox.Items.Count - 1;
        }

        /// <summary>
        /// Creates a new Group
        /// </summary>
        /// <param name="groupType">Type of Group to create</param>
        /// <returns>New Group</returns>
        public TtGroup CreateGroup(GroupType groupType = GroupType.General)
        {
            return new TtGroup($"{(groupType != GroupType.General ? $"{groupType}" : "Group")}_{Guid.NewGuid().ToString().Substring(0, 8)}");
        }

        private void DeleteGroup()
        {
            if (CurrentGroup != null && CurrentGroup.CN != Consts.EmptyGuid)
            {
                if (MessageBox.Show($"Confirm Delete Group '{CurrentGroup.Name}'", "Delete Group",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    Manager.DeleteGroup(CurrentGroup);

                    _Project.ProjectUpdated();
                }
            }
        }
        #endregion


        #region Media
        public ObservableCollection<ImageTile> Tiles { get; }

        private TtMediaInfo _CurrentMediaInfo;
        public TtMediaInfo CurrentMediaInfo
        {
            get { return _CurrentMediaInfo; }
            private set { SetField(ref _CurrentMediaInfo, value); }
        }

        public ICommand MediaInfoChangedCommand { get; }
        public ICommand MediaSelectedCommand { get; }
        public ICommand HideMediaViewerCommand { get; }

        public bool MediaViewerVisible { get { return Get<bool>(); } set { Set(value); } }

        private void MediaInfoChanged(TtMediaInfo mediaInfo)
        {
            CurrentMediaInfo = mediaInfo;

            Tiles.Clear();

            foreach (TtImage image in CurrentMediaInfo.Images)
            {
                try
                {
                    MediaTools.LoadImageAsync(_Project.MAL, image, new AsyncCallback(ImageLoaded));
                }
                catch // (FileNotFoundException ex)
                {
                    //
                }
            }

            MediaViewerVisible = false;
        }

        private void MediaSelected(TtMediaInfo mediaInfo)
        {
            MessageBox.Show(mediaInfo.Title);
        }

        private void ImageLoaded(IAsyncResult res)
        {
            if (res is ImageAsyncResult iar)
                _Project.MainModel.MainWindow.Dispatcher.Invoke(
                    () => Tiles.Add(
                        new ImageTile(iar.ImageInfo, iar.Image, (x) => MediaViewerVisible = true)
                    )
                );
        }
        #endregion
    }
}
