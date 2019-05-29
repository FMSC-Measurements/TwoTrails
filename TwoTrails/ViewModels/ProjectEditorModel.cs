using CSUtil.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
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
            NewPolygonCommand = new RelayCommand(x => NewPolygon(x as ListBox));
            DeletePolygonCommand = new RelayCommand(x => DeletePolygon());

            PolygonAccuracyChangedCommand = new RelayCommand(x => PolygonAccuracyChanged(x as string));

            PolygonUpdateAccCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => UpdatePolygonAcc(),
                x => CurrentPolygon != null && CurrentPolygon.Accuracy != PolygonAccuracy,
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

            MetadataZoneChangedCommand = new RelayCommand(x => MetadataZoneChanged(x as string));

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
                }
            });

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

        #region Polygon
        public double TotalPolygonArea { get { return Get<double>(); } set { Set(value); } }
        public double TotalPolygonPerimeter { get { return Get<double>(); } set { Set(value); } }

        public double PolygonAccuracy { get { return Get<double>(); } set { Set(value); } }

        private TtPolygon _BackupPoly;
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
                        old.PropertyChanged -= Polygon_PropertyChanged;
                        old.PolygonChanged -= GeneratePolygonSummary;
                    }

                    if (_CurrentPolygon != null)
                    {
                        _BackupPoly = new TtPolygon(_CurrentPolygon);
                        PolygonAccuracy = _CurrentPolygon.Accuracy;

                        _CurrentPolygon.PropertyChanged += Polygon_PropertyChanged;
                        _CurrentPolygon.PolygonChanged += GeneratePolygonSummary;

                        GeneratePolygonSummary(_CurrentPolygon);
                    }
                });

            }
        }

        private void GeneratePolygonSummary(TtPolygon polygon)
        {
            PolygonSummary = HaidLogic.GenerateSummary(Manager, polygon, true);
        }

        public PolygonSummary PolygonSummary { get { return Get<PolygonSummary>(); } set { Set(value); } }


        private void Polygon_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender as TtPolygon).Equals(_BackupPoly))
            {
                _Project.ProjectUpdated();
            }
            
            if (e.PropertyName == nameof(TtPolygon.Area))
            {
                if (Manager.GetPoints(CurrentPolygon.CN).GroupBy(p => p.Metadata.Zone).Count() > 1)
                {
                    MessageBox.Show($"Polygon '{CurrentPolygon.Name}' has points associated with more than one Metadata Zone." +
                        "This may cause issues with area calculations. Please make sure all the points use the same zone.",
                        "Polygon Zone Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

        }

        public ICommand PolygonChangedCommand { get; }
        public ICommand NewPolygonCommand { get; }
        public ICommand DeletePolygonCommand { get; }
        public ICommand PolygonUpdateAccCommand { get; }
        public ICommand PolygonAccuracyLookupCommand { get; }
        public ICommand PolygonAccuracyChangedCommand { get; }
        public ICommand SavePolygonSummary { get; }

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
        }

        private void PolygonChanged(TtPolygon poly)
        {
            CurrentPolygon = poly;
            PolygonAccuracy = poly != null ? poly.Accuracy : Consts.DEFAULT_POINT_ACCURACY;
        }

        private bool PolygonAccuracyChanged(string accStr)
        {
            if (accStr != null)
            {
                if (double.TryParse(accStr, out double acc))
                {
                    PolygonAccuracy = acc;
                    return true;
                }
            }

            return false;
        }

        private void UpdatePolygonAcc()
        {
            CurrentPolygon.Accuracy = PolygonAccuracy;
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

        private void NewPolygon(ListBox listBox)
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
                PointStartIndex = pointStartIndex > 0 ? pointStartIndex : num * 1000 + 10
            };
        }

        private void DeletePolygon()
        {
            if (CurrentPolygon != null)
            {
                if (MessageBox.Show($"Confirm Delete Polygon '{CurrentPolygon.Name}'", "Delete Polygon",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    Manager.DeletePolygon(CurrentPolygon);

                    _Project.ProjectUpdated();
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
        #endregion

        #region Metadata
        public int MetadataZone { get { return Get<int>(); } set { Set(value); } }

        private TtMetadata _BackupMeta;
        private TtMetadata _CurrentMetadata;
        public TtMetadata CurrentMetadata
        {
            get { return _CurrentMetadata; }
            private set
            {
                TtMetadata old = _CurrentMetadata;

                SetField(ref _CurrentMetadata, value, () => {
                    if (old != null)
                    {
                        old.PropertyChanged -= Metadata_PropertyChanged;
                    }

                    if (_CurrentMetadata != null)
                    {
                        _BackupMeta = new TtMetadata(_CurrentMetadata);
                        MetadataZone = _CurrentMetadata.Zone;
                        _CurrentMetadata.PropertyChanged += Metadata_PropertyChanged;
                    }
                });

            }
        }

        private void Metadata_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender as TtMetadata).Equals(_BackupMeta))
            {
                _Project.ProjectUpdated();
            }
        }

        public ICommand MetadataChangedCommand { get; }
        public ICommand NewMetadataCommand { get; }
        public ICommand DeleteMetadataCommand { get; }
        public ICommand MetadataZoneChangedCommand { get; }
        public ICommand MetadataUpdateZoneCommand { get; }
        public ICommand SetDefaultMetadataCommand { get; }

        private void MetadataChanged(TtMetadata meta)
        {
            MetadataZone = meta != null ? meta.Zone : 13;
            CurrentMetadata = meta;
        }

        private bool MetadataZoneChanged(string zoneStr)
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
                CurrentMetadata.Zone = MetadataZone;
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
                    ITtManager manager = Manager;

                    IEnumerable<TtPoint> points = manager.GetPoints().Where(p => p.MetadataCN == CurrentMetadata.CN);

                    if (CurrentMetadata.Zone != manager.DefaultMetadata.Zone && points.Any())
                    {
                        if (MessageBox.Show(
                            String.Format("Metdata '{0}' does not have the same zone as the Default Metadata. All the points that use {0} will have their zone converted.", CurrentMetadata),
                            "Convert Zones", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                            return;
                    }

                    manager.DeleteMetadata(CurrentMetadata);

                    _Project.ProjectUpdated();
                }
            }
        }
        #endregion

        #region Group
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

        public bool GroupFieldIsEditable { get { return Get<bool>(); } set { Set(value); } }

        private void Group_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender as TtGroup).Equals(_BackupGroup))
            {
                _Project.ProjectUpdated();
            }
        }

        public ICommand GroupChangedCommand { get; }
        public ICommand NewGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }

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
