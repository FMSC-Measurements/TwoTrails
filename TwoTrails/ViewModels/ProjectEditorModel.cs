using FMSC.Core;
using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;
using TwoTrails.Dialogs;
using static TwoTrails.Core.MediaTools;

namespace TwoTrails.ViewModels
{
    public class ProjectEditorModel : BaseModel, IDisposable
    {
        #region Commands
        public ICommand OpenMapWindowCommand { get; }
        public BindedRelayCommand<ProjectEditorModel> HistoryCommand { get; set; }

        public ICommand RecalculateAllPolygonsCommand { get; }
        public ICommand CalculateLogDeckCommand { get; }

        public ICommand DiscardChangesCommand { get; }

        public ICommand ViewUserActivityCommand { get; }

        #region Polygons
        public ICommand PolygonChangedCommand { get; }
        public ICommand CreatePolygonCommand { get; }
        public ICommand DeletePolygonCommand { get; }
        public BindedRelayCommand<ProjectEditorModel> PolygonUpdateAccCommand { get; }
        public BindedRelayCommand<ProjectEditorModel> PolygonAccuracyLookupCommand { get; }
        public BindedRelayCommand<ProjectEditorModel> SavePolygonSummary { get; }
        #endregion

        #region Metadata
        public ICommand MetadataChangedCommand { get; }
        public ICommand NewMetadataCommand { get; }
        public BindedRelayCommand<ProjectEditorModel> DeleteMetadataCommand { get; }
        public BindedRelayCommand<ProjectEditorModel> MetadataUpdateZoneCommand { get; }
        public ICommand SetDefaultMetadataCommand { get; }
        #endregion

        #region Groups
        public ICommand GroupChangedCommand { get; }
        public ICommand NewGroupCommand { get; }
        public BindedRelayCommand<ProjectEditorModel> DeleteGroupCommand { get; }
        #endregion

        #region Cleanup
        public RelayCommand RemoveDuplicateMetadataCommand { get; }
        public RelayCommand RemoveUnusedPolygonsCommand { get; }
        public RelayCommand RemoveUnusedMetadataCommand { get; }
        public RelayCommand RemoveUnusedGroupsCommand { get; }
        #endregion

        #region Advanced
        public RelayCommand AnalyzeProjectCommand { get; }
        #endregion
        #endregion

        public MainWindowModel MainModel { get; private set; }

        public TtProject Project { get; }
        public ProjectEditorControl ProjectEditorControl { get; set; }
        public UserActivityControl UserActivityControl { get; set; }

        public PointEditorModel PointEditor { get; }
        public PointEditorControl PointEditorControl { get; }

        private DataStyleModel DataStyle { get; }

        public TtProjectInfo ProjectInfo => Project.ProjectInfo;
        public TtHistoryManager Manager => Project.HistoryManager;
        public TtSettings Settings => Project.Settings;

        public MapControl MapControl { get; private set; }
        public MapWindow MapWindow { get; private set; }
        public bool IsMapWindowOpen => MapWindow != null;


        private readonly KeyEventHandler KeyDownHandler, KeyUpHandler;
        public bool CtrlKeyPressed { get; private set; }


        public ReadOnlyObservableCollection<TtPolygon> Polygons => Manager.Polygons;
        public ListCollectionView PolygonsLVC { get; }

        public ReadOnlyObservableCollection<TtMetadata> Metadata => Manager.Metadata;
        public ReadOnlyObservableCollection<TtGroup> Groups => Manager.Groups;
        public ReadOnlyObservableCollection<TtMediaInfo> MediaInfo => Manager.MediaInfo;


        public ProjectEditorModel(TtProject project, MainWindowModel mainWindowModel)
        {
            Project = project;
            MainModel = mainWindowModel;

            DataStyle = new DataStyleModel(project.HistoryManager.Polygons);
            PointEditor = new PointEditorModel(project, this);
            PointEditorControl = new PointEditorControl(PointEditor, DataStyle);
            UserActivityControl = new UserActivityControl(Project.HistoryManager);

            MapControl = new MapControl(project);

            #region Commands
            OpenMapWindowCommand = new RelayCommand(x =>
            {
                if (ProjectEditorControl != null)
                {
                    if (MapControl != null)
                    {
                        MapWindow = new MapWindow(Project.ProjectName, MapControl);
                        ProjectEditorControl.MapContainer.Children.Remove(MapControl);
                    }
                    else
                        throw new Exception("MapControl Not Found");

                    MapWindow.Closing += MapWindow_Closing;

                    MapWindow.Show();

                    OnPropertyChanged(nameof(IsMapWindowOpen), nameof(MapWindow));

                    ProjectEditorControl.SwitchToTab(ProjectTabSection.Points);

                    MapWindow.Show();
                }
                else
                    throw new Exception("ProjectEditorControl Not Loaded");
            });

            HistoryCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => OpenHistoryManager(),
                x => Manager != null ? Manager.CanRedo || Manager.CanRedo : false,
                this, m => new { m.Manager.CanRedo, m.Manager.CanUndo });

            DiscardChangesCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => Manager.BaseManager.Reset(),
                x => Manager.CanUndo,
                this, m => m.Manager.CanUndo);

            RecalculateAllPolygonsCommand = new RelayCommand(x => Manager.RecalculatePolygons());
            CalculateLogDeckCommand = new RelayCommand(x => CalculateLogDeck());

            #region Polygons
            PolygonChangedCommand = new RelayCommand(x => PolygonChanged(x as TtPolygon));
            CreatePolygonCommand = new RelayCommand(x => CreatePolygon(x as ListBox));
            DeletePolygonCommand = new RelayCommand(x => DeletePolygon(x as ListBox));

            PolygonUpdateAccCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => UpdatePolygonAcc(),
                x => CurrentPolygon != null && CurrentPolygon.Accuracy != _PolygonAccuracy,
                this,
                m => new { m.PolygonAccuracy, m.CurrentPolygon.Accuracy });

            PolygonAccuracyLookupCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => AccuracyLookup(),
                x => CurrentPolygon != null,
                this,
                m => m.CurrentPolygon);

            SavePolygonSummary = new BindedRelayCommand<ProjectEditorModel>(
                x => SavePolygonsummary(), x => CurrentPolygon != null,
                this, m => m.CurrentPolygon);
            #endregion

            #region Metadata
            MetadataChangedCommand = new RelayCommand(x => MetadataChanged(x as TtMetadata));
            NewMetadataCommand = new RelayCommand(x => NewMetadata(x as ListBox));
            SetDefaultMetadataCommand = new RelayCommand(x => SetMetadataDefault());
            DeleteMetadataCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => DeleteMetadata(),
                x => x != null && (x as TtMetadata).CN != Consts.EmptyGuid,
                this,
                m => m.CurrentMetadata);

            MetadataUpdateZoneCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => UpdateMetadataZone(),
                x => CurrentMetadata != null && CurrentMetadata.Zone != MetadataZone,
                this,
                m => new { m.MetadataZone, m.CurrentMetadata.Zone });


            //DeleteMetadataCommand = new BindedRelayCommand<ProjectEditorModel>(
            //    (x, m) => m.DeleteMetadata(),
            //    (x, m) => x != null && (x as TtMetadata).CN != Consts.EmptyGuid,
            //    this,
            //    m => m.CurrentMetadata);

            //MetadataUpdateZoneCommand = new BindedRelayCommand<ProjectEditorModel>(
            //    (x, m) => m.UpdateMetadataZone(),
            //    (x, m) => m.CurrentMetadata != null && m.CurrentMetadata.Zone != m.MetadataZone,
            //    this,
            #endregion

            #region Groups
            GroupChangedCommand = new RelayCommand(x => GroupChanged(x as TtGroup));
            NewGroupCommand = new RelayCommand(x => NewGroup(x as ListBox));
            DeleteGroupCommand = new BindedRelayCommand<ProjectEditorModel>(
                x => DeleteGroup(),
                x => x != null && (x as TtGroup).CN != Consts.EmptyGuid,
                this,
                m => m.CurrentGroup);


            //DeleteGroupCommand = new BindedRelayCommand<ProjectEditorModel>(
            //    (x, m) => m.DeleteGroup(),
            //    (x, m) => x != null && (x as TtGroup).CN != Consts.EmptyGuid,
            //    this,
            //    m => m.CurrentGroup);
            #endregion

            #region Media
            Tiles = new ObservableCollection<ImageTile>();
            MediaInfoChangedCommand = new RelayCommand(x => MediaInfoChanged(x as TtMediaInfo));
            MediaSelectedCommand = new RelayCommand(x => MediaSelected(x as TtMediaInfo));
            HideMediaViewerCommand = new RelayCommand(x => MediaViewerVisible = false);
            #endregion

            #region Cleanup
            RemoveDuplicateMetadataCommand = new RelayCommand(x => RemoveDuplicateMetadata());
            RemoveUnusedPolygonsCommand = new RelayCommand(x => RemoveUnusedPolygons());
            RemoveUnusedMetadataCommand = new RelayCommand(x => RemoveUnusedMetadata());
            RemoveUnusedGroupsCommand = new RelayCommand(x => RemoveUnusedGroups());
            #endregion

            #region Advanced
            AnalyzeProjectCommand = new RelayCommand(x => AnalyzeProjectDialog.ShowDialog(Project, MainModel.MainWindow));
            #endregion

            #endregion


            PolygonShapeChanged(null);

            foreach (TtPolygon poly in Manager.Polygons)
                poly.PolygonChanged += PolygonShapeChanged;

            ((INotifyCollectionChanged)Manager.Polygons).CollectionChanged += PolygonCollectionChanged;

            Manager.HistoryChanged += Manager_HistoryChanged;

            CurrentMetadata = Metadata[0];
            CurrentGroup = Groups[0];

            if (Polygons != null && Polygons.Count > 0)
                CurrentPolygon = Polygons[0];

            if (MediaInfo != null && MediaInfo.Count > 0)
                CurrentMediaInfo = MediaInfo[0];

            PolygonsLVC = CollectionViewSource.GetDefaultView(Polygons) as ListCollectionView;
            PolygonsLVC.CustomSort = new PolygonSorter(project.Settings.SortPolysByName);


            KeyDownHandler = new KeyEventHandler(OnKeyDown);
            KeyUpHandler = new KeyEventHandler(OnKeyUp);

            PointEditorControl.AddHandler(PointEditorControl.KeyDownEvent, KeyDownHandler);
            PointEditorControl.AddHandler(PointEditorControl.KeyUpEvent, KeyUpHandler);

            Project.Settings.PropertyChanged += (s, pce) =>
            {
                if (pce.PropertyName == nameof(TtSettings.SortPolysByName))
                {
                    PolygonsLVC.CustomSort = new PolygonSorter(Project.Settings.SortPolysByName);
                }
            };
        }

        protected override void Dispose(bool disposing)
        {
            PolygonSummary = null;

            HistoryCommand.Dispose();
            PolygonUpdateAccCommand.Dispose();
            PolygonAccuracyLookupCommand.Dispose();
            SavePolygonSummary.Dispose();

            DeleteMetadataCommand.Dispose();
            MetadataUpdateZoneCommand.Dispose();

            DeleteGroupCommand.Dispose();


            CloseWindows();

            ((INotifyCollectionChanged)Manager.Polygons).CollectionChanged -= PolygonCollectionChanged;

            foreach (TtPolygon poly in Manager.Polygons)
                poly.PolygonChanged -= PolygonShapeChanged;

            Manager.HistoryChanged -= Manager_HistoryChanged;

            if (_CurrentPolygon != null)
                _CurrentPolygon.PolygonChanged -= GeneratePolygonSummaryAndStats;

            PointEditorControl.RemoveHandler(PointEditorControl.KeyDownEvent, KeyDownHandler);
            PointEditorControl.RemoveHandler(PointEditorControl.KeyUpEvent, KeyUpHandler);
        }


        private void MapWindow_Closing(object sender, EventArgs e)
        {
            ProjectEditorControl.MapContainer.Children.Add(MapWindow.MapControl);

            MapWindow.Closing -= MapWindow_Closing;
            MapWindow = null;

            OnPropertyChanged(nameof(IsMapWindowOpen), nameof(MapWindow));
        }

        private void Manager_HistoryChanged(object sender, HistoryEventArgs e)
        {
            if (e.DataType != null && e.HistoryEventType == HistoryEventType.Undone || e.HistoryEventType == HistoryEventType.Redone)
            {
                if (e.DataType == PolygonProperties.DataType)
                {
                    BindPolygonValues(CurrentPolygon);
                }
                else if (e.DataType == GroupProperties.DataType)
                {
                    BindGroupValues(CurrentGroup);
                }
                else if (e.DataType == MetadataProperties.DataType)
                {
                    BindMetadataValues(CurrentMetadata);
                }
            }
        }


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                CtrlKeyPressed = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                CtrlKeyPressed = false;
        }


        private void CloseWindows()
        {
            MapWindow?.Close();
        }


        private void OpenHistoryManager()
        {
            MainModel.MainWindow.IsEnabled = false;
            HistoryDialog.ShowDialog(Manager, MainModel.MainWindow, (onClose) =>
            {
                MainModel.MainWindow.IsEnabled = true;
                MainModel.MainWindow.Activate();
            });
        }

        private void CalculateLogDeck()
        {
            if (Manager.BaseManager.PolygonCount > 0)
            {
                MainModel.MainWindow.IsEnabled = false;
                LogDeckCalculatorDialog.Show(Project, this.MainModel.MainWindow, () =>
                {
                    MainModel.MainWindow.IsEnabled = true;
                });
            }
            else
                MessageBox.Show("No Polygons in Project.", String.Empty, MessageBoxButton.OK, MessageBoxImage.Stop);
        }


        #region Polygons

        #region Properties
        private TtPolygon _CurrentPolygon;
        public TtPolygon CurrentPolygon
        {
            get => _CurrentPolygon;
            private set
            {
                TtPolygon old = _CurrentPolygon;

                SetField(ref _CurrentPolygon, value, () => {
                    if (old != null)
                    {
                        old.PolygonChanged -= GeneratePolygonSummaryAndStats;
                    }

                    BindPolygonValues(value);

                    if (_CurrentPolygon != null)
                    {
                        _CurrentPolygon.PolygonChanged += GeneratePolygonSummaryAndStats;

                        ValidatePolygon(value);

                        GeneratePolygonSummaryAndStats(_CurrentPolygon);
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


        //private AnglePointResult _PolygonAnglePointResult;
        //public AnglePointResult PolygonAnglePointResult
        //{
        //    get => _PolygonAnglePointResult;
        //    private set {
        //        SetField(ref _PolygonAnglePointResult, value,
        //            () => OnPropertyChanged(
        //                nameof(IsAnglePoint), nameof(AnglePointText),
        //                nameof(AnglePointToolTip), nameof(AnglePointTextDec)
        //            ));
        //    }
        //}

        //public bool IsAnglePoint => PolygonAnglePointResult == AnglePointResult.Qualifies;

        //public String AnglePointText => IsAnglePoint ? "Qualifies" : "Invalid";

        //public String AnglePointToolTip => IsAnglePoint ? "This unit meets the angle point method requirements." : 
        //    String.Join("\n", _PolygonAnglePointResult.GetErrorMessages());

        //public String AnglePointTextDec => IsAnglePoint ? "None" : "Underline";

        public GeometricErrorReductionResult _GERResult;
        public GeometricErrorReductionResult GERResult
        {
            get => _GERResult;
            private set
            {
                SetField(ref _GERResult, value);//,
                    //() => OnPropertyChanged(
                    //    nameof(IsAnglePoint), nameof(AnglePointText),
                    //    nameof(AnglePointToolTip), nameof(AnglePointTextDec)
                    //));
            }
        }

        public PolygonSummary PolygonSummary { get { return Get<PolygonSummary>(); } set { Set(value); } }

        public double TotalReduction => (_GERResult != null && PolygonSummary != null && PolygonSummary.TotalGpsError > 0) ?
                                            (1 - (GERResult.TotalError / PolygonSummary.TotalGpsError)) * 100: 0;


        private void EditPolygonValue<T>(ref T? origValue, T? newValue, PropertyInfo property, bool allowNull = false) where T : struct, IEquatable<T>
        {
            if (origValue == null ^ newValue == null || !origValue.Equals(newValue))
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
            if (origValue == null ^ newValue == null || !origValue.Equals(newValue))
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


        private void GeneratePolygonSummaryAndStats(TtPolygon polygon)
        {
            PolygonSummary = HaidLogic.GenerateSummary(Manager, polygon, true);
            GERResult = AnglePointLogic.GetGeometricErrorReduction(Manager, polygon);
            OnPropertyChanged(nameof(TotalReduction));

            //PolygonAnglePointResult = AnglePointLogic.VerifyGeometry(Manager, polygon.CN);
        }


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
                    break;
            }
        }

        private void PolygonShapeChanged(TtPolygon polygon)
        {
            //double area = 0, perim = 0;
            //foreach (TtPolygon poly in Manager.Polygons)
            //{
            //    area += poly.Area;
            //    perim += poly.Perimeter;
            //}

            //TotalPolygonArea = area;
            //TotalPolygonPerimeter = perim;

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
                Owner = MainModel.MainWindow
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
            TtPolygon polygon = CreatePolygon();
            Manager.AddPolygon(polygon);
            listBox.SelectedItem = polygon;
            CurrentPolygon = polygon;
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
                            listBox.SelectedIndex = Polygons.Count - 1;
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
                        sw.Write(HaidLogic.GenerateSummaryHeader(ProjectInfo, Project.FilePath));
                        sw.WriteLine($"\n-- {CurrentPolygon.Name} --");
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

        #region Properties
        private TtMetadata _CurrentMetadata;
        public TtMetadata CurrentMetadata
        {
            get => _CurrentMetadata;
            private set => SetField(ref _CurrentMetadata, value, () => {
                BindMetadataValues(value);
                MetaFieldIsEditable = value != null && value.CN != Consts.EmptyGuid;
            });
        }

        public bool MetaFieldIsEditable { get { return Get<bool>(); } set { Set(value); } }

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

        private double _MetadataMagDec;
        public double MetadataMagDec
        {
            get => _MetadataMagDec;
            set => EditMetadataValue(ref _MetadataMagDec, value, MetadataProperties.MAG_DEC);
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
                SetField(ref _MetadataZone, value, nameof(MetadataZone));
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


        private void EditMetadataValue<T>(ref T origValue, T newValue, PropertyInfo property) where T : struct, IEquatable<T>
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                Manager.EditMetadata(CurrentMetadata, property, newValue);
            }
        }

        private void EditMetadataValue<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : class
        {
            if (origValue == null ^ newValue == null || !origValue.Equals(newValue))
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

        private void BindMetadataValues(TtMetadata metadata)
        {
            if (CurrentMetadata != null)
            {
                _MetadataName = metadata.Name;
                _MetadataComment = metadata.Comment;
                _MetadataMagDec = metadata.MagDec;
                _MetadataDecType = metadata.DecType;
                _MetadataDatum = metadata.Datum;
                _MetadataZone = metadata.Zone;
                _MetadataDistance = metadata.Distance;
                _MetadataElevation = metadata.Elevation;
                _MetadataSlope = metadata.Slope;
                _MetadataGpsReceiver = metadata.GpsReceiver;
                _MetadataRangeFinder = metadata.RangeFinder;
                _MetadataCompass = metadata.Compass;
                _MetadataCrew = metadata.Crew;
            }
            else
            {
                _MetadataName = String.Empty;
                _MetadataComment = String.Empty;
                _MetadataMagDec = Settings.MetadataSettings.MagDec;
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
                nameof(MetadataMagDec),
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
            CurrentMetadata = meta;
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
            Project.Settings.MetadataSettings.SetMetadataSettings(CurrentMetadata);
            MessageBox.Show("Metadata defaults set.");
        }

        private void NewMetadata(ListBox listBox)
        {
            TtMetadata metadata = CreateMetadata();
            Manager.AddMetadata(metadata);
            listBox.SelectedItem = metadata;
            CurrentMetadata = metadata;
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
                }
            }
        }
        #endregion


        #region Group

        #region Properties
        public bool GroupFieldIsEditable { get { return Get<bool>(); } set { Set(value); } }

        private TtGroup _CurrentGroup;
        public TtGroup CurrentGroup
        {
            get => _CurrentGroup;
            private set
            {
                SetField(ref _CurrentGroup, value, () => {
                    BindGroupValues(value);
                    GroupFieldIsEditable = value != null && value.CN != Consts.EmptyGuid;
                });
            }
        }

        private String _GroupName = String.Empty;
        public String GroupName
        {
            get => _GroupName;
            set => EditGroupValue(ref _GroupName, value, GroupProperties.NAME);
        }

        private String _GroupDescription = String.Empty;
        public String GroupDescription
        {
            get => _GroupDescription;
            set => EditGroupValue(ref _GroupDescription, value, GroupProperties.DESCRIPTION);
        }

        private GroupType _GroupType;
        public GroupType GroupType
        {
            get => _GroupType;
            set => EditGroupEnum(ref _GroupType, value, GroupProperties.GROUP_TYPE);
        }

        
        private void EditGroupValue<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : class
        {
            if (origValue == null ^ newValue == null || !origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditGroup(CurrentGroup, property, newValue);
                }
            }
        }

        private void EditGroupEnum<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : Enum
        {
            if (origValue == null ^ newValue == null || !origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    Manager.EditGroup(CurrentGroup, property, newValue);
                }
            }
        }
        
        private void BindGroupValues(TtGroup group)
        {
            if (group != null)
            {
                _GroupName = group.Name;
                _GroupDescription = group.Description;
                _GroupType = group.GroupType;
            }
            else
            {
                _GroupName = String.Empty;
                _GroupDescription = String.Empty;
                _GroupType = GroupType.General;
            }

            OnPropertyChanged(
                nameof(GroupName),
                nameof(GroupDescription),
                nameof(GroupType));
        }
        #endregion


        private void GroupChanged(TtGroup group)
        {
            CurrentGroup = group;
        }

        private void NewGroup(ListBox listBox)
        {
            TtGroup group = CreateGroup();
            Manager.AddGroup(group);
            listBox.SelectedItem = group;
            CurrentGroup = group;
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
                    MediaTools.LoadImageAsync(Project.MAL, image, new AsyncCallback(ImageLoaded));
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
                MainModel.MainWindow.Dispatcher.Invoke(
                    () => Tiles.Add(
                        new ImageTile(iar.ImageInfo, iar.Image, (x) => MediaViewerVisible = true)
                    )
                );
        }
        #endregion


        #region Cleanup
        private void RemoveDuplicateMetadata()
        {
            List<Tuple<TtMetadata,List<TtMetadata>>> dupMetaList = Manager.GetMetadata()
                .GroupBy(meta => new {
                    meta.Zone, meta.Comment, meta.Compass, meta.Crew, meta.Datum, meta.DecType,
                    meta.Distance, meta.Elevation, meta.GpsReceiver, meta.MagDec, meta.RangeFinder, meta.Slope })
                .Select(g => Tuple.Create(g.First(), g.Skip(1).ToList()))
                .Where(ml => ml.Item2.Count > 0)
                .ToList();

            if (dupMetaList.Count > 0)
            {
                int removedMetaCount = dupMetaList.Sum(ml => ml.Item2.Count);

                if (MessageBox.Show($"{removedMetaCount} metadata will be deleted and {(removedMetaCount > 1 ? "their" : "its")} associated points merged into existing identical metadata.",
                    "Remove Duplicate Metadata", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    Manager.StartMultiCommand();

                    foreach (var ml in dupMetaList)
                    {
                        //set all points who have a duplicate meta to be set to the first meta that will be kept
                        Manager.EditPoints(Manager.Points.Where(p => ml.Item2.Any(m => p.MetadataCN == m.CN)), PointProperties.META, ml.Item1);

                        foreach (var delMeta in ml.Item2.Where(m => m.CN != Consts.EmptyGuid))
                            Manager.DeleteMetadata(delMeta);
                    }
                    Manager.CommitMultiCommand(new AddDataActionCommand(DataActionType.None, Manager.BaseManager, $"Removed {removedMetaCount} duplicate metadata"));
                }
            }
            else
            {
                MessageBox.Show("There is no duplicate Metadata.", "");
            }
        }

        private void RemoveUnusedPolygons()
        {
            List<TtPolygon> delPolys = Manager.Polygons.Where(poly => poly.CN != Consts.EmptyGuid && !Manager.GetPoints(poly.CN).Any()).ToList();

            if (delPolys.Count > 0)
            {
                if (MessageBox.Show($"{delPolys.Count} polygon{(delPolys.Count > 1 ? "s" : String.Empty)} will be deleted for containing no points.",
                    "Remove Empty Polygons", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    Manager.StartMultiCommand();

                    foreach (var poly in delPolys)
                    {
                        Manager.DeletePolygon(poly);
                    }

                    Manager.CommitMultiCommand(new AddDataActionCommand(DataActionType.None, Manager.BaseManager, $"Removed {delPolys.Count} empty polygons"));
                }
            }
            else
            {
                MessageBox.Show("There are 0 empty poylgons.");
            }
        }

        private void RemoveUnusedMetadata()
        {
            List<TtMetadata> delMeta = Manager.Metadata.Where(meta => meta.CN != Consts.EmptyGuid && !Manager.Points.Any(p => p.MetadataCN == meta.CN)).ToList();

            if (delMeta.Count > 0)
            {
                if (MessageBox.Show($"{delMeta.Count} metadata will be deleted for having no associated points.",
                        "Remove Unassocaited Metadata", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    Manager.StartMultiCommand();

                    foreach (var meta in delMeta)
                    {
                        Manager.DeleteMetadata(meta);
                    }

                    Manager.CommitMultiCommand(new AddDataActionCommand(DataActionType.None, Manager.BaseManager, $"Removed {delMeta.Count} unused metadata"));
                }
            }
            else
            {
                MessageBox.Show("There are 0 metadata with unassociated points.");
            }
        }

        private void RemoveUnusedGroups()
        {
            List<TtGroup> delGroups = Manager.Groups.Where(group => group.CN != Consts.EmptyGuid && !Manager.Points.Any(p => p.GroupCN == group.CN)).ToList();

            if (delGroups.Count > 0)
            {
                if (MessageBox.Show($"{delGroups.Count} group{(delGroups.Count > 1 ? "s" : String.Empty)} will be deleted for having no associated points.",
                    "Remove Empty Groups", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    Manager.StartMultiCommand();

                    foreach (var group in delGroups)
                    {
                        Manager.DeleteGroup(group);
                    }

                    Manager.CommitMultiCommand(new AddDataActionCommand(DataActionType.None, Manager.BaseManager, $"Removed {delGroups.Count} unused groups"));
                }
            }
            else
            {
                MessageBox.Show("There are 0 groups with unassociated points.");
            }
        }
        #endregion
    }
}
