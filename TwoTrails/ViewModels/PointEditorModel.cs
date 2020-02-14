using CSUtil;
using CSUtil.ComponentModel;
using FMSC.Core;
using FMSC.Core.Windows.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using FMSC.GeoSpatial.UTM;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TwoTrails.Converters;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Dialogs;
using TwoTrails.Utils;
using Point = FMSC.Core.Point;

namespace TwoTrails.ViewModels
{
    public class PointEditorModel : NotifyPropertyChangedEx
    {
        #region Vars
        private readonly string[] SelectionChangedProperties = new string[]
        {
            nameof(HasSelection),
            nameof(SelectedPoint),
            nameof(SelectedPoints),
            nameof(MultipleSelections),
            nameof(OnlyGpsTypes),
            nameof(OnlyTravTypes),
            nameof(OnlyManAccTypes),
            nameof(OnlyQuondams),
            nameof(HasPossibleCorridor),
            nameof(ConvertTypeHeader),
            nameof(PID),
            nameof(SamePID),
            nameof(Index),
            nameof(SameIndex),
            nameof(Polygon),
            nameof(SamePolygon),
            nameof(Metadata),
            nameof(SameMetadata),
            nameof(Group),
            nameof(SameGroup),
            nameof(OnBoundary),
            nameof(SameOnBound),
            nameof(UnAdjX),
            nameof(SameUnAdjX),
            nameof(UnAdjY),
            nameof(SameUnAdjY),
            nameof(UnAdjZ),
            nameof(SameUnAdjZ),
            nameof(ManAcc),
            nameof(SameManAcc),
            nameof(FwdAz),
            nameof(SameFwdAz),
            nameof(BkAz),
            nameof(SameBkAz),
            nameof(SlpAng),
            nameof(SameSlpAng),
            nameof(SlpDist),
            nameof(SameSlpDist),
            nameof(ParentPoint),
            nameof(SameParentPoint),
            nameof(Comment),
            nameof(SameComment)
        };
        
        private bool CtrlKeyPressed { get; set; }
        #endregion

        #region Commands
        public ICommand RefreshPoints { get; }

        public ICommand ChangeQuondamParentCommand { get; }
        public ICommand RenamePointsCommand { get; }
        public ICommand ReverseSelectedCommand { get; }
        public ICommand ResetPointCommand { get; }
        public ICommand ResetPointFieldCommand { get; }
        public ICommand DeleteCommand { get; }

        public RelayCommand CreatePointCommand { get; }
        public RelayCommand CreateQuondamsCommand { get; }
        public RelayCommand ConvertPointsCommand { get; }
        public RelayCommand MovePointsCommand { get; }
        public RelayCommand RetraceCommand { get; }
        public RelayCommand ReindexCommand { get; }
        public RelayCommand CreatePlotsCommand { get; }
        public RelayCommand CreateSubsampleCommand { get; }
        public RelayCommand CreateCorridorCommand { get; }
        public RelayCommand ModifyDataDictionaryCommand { get; }
        
        public ICommand SelectAlternateCommand { get; }
        public ICommand SelectGpsCommand { get; }
        public ICommand SelectTravCommand { get; }
        public ICommand SelectInverseCommand { get; }
        public ICommand DeselectCommand { get; }

        public ICommand CopyCellValueCommand { get; }
        public ICommand ExportValuesCommand { get; }
        public ICommand ViewPointDetailsCommand { get; }
        public ICommand ViewSatInfoCommand { get; }
        public ICommand UpdateAdvInfo { get; }
        #endregion

        #region Properties
        private ListCollectionView _Points;
        public ListCollectionView Points
        {
            get { return _Points; }
            set { SetField(ref _Points, value); }
        }

        public TtProject Project { get; }
        public TtHistoryManager Manager { get; }
        
        public bool HasSelection
        {
            get { return SelectedPoints != null && SelectedPoints.Count > 0; }
        }

        public bool MultipleSelections
        {
            get { return SelectedPoints != null && SelectedPoints.Count > 1; }
        }


        public TtPoint SelectedPoint { get; private set; }

        private IList _SelectedPoints = new ArrayList();
        public IList SelectedPoints
        {
            get { return _SelectedPoints; }
            set { SetField(ref _SelectedPoints, value);  OnSelectionChanged(); }
        }

        public int SelectedColumnIndex { get; set; }

        private bool _SelectionChanged = true;
        private List<TtPoint> _SortedSelectedPoints;
        public List<TtPoint> GetSortedSelectedPoints(Func<TtPoint, bool> where = null)
        {
            if (_SelectionChanged)
            {
                _SortedSelectedPoints = _SelectedPoints.Cast<TtPoint>().ToList();
                _SortedSelectedPoints.Sort();
                _SelectionChanged = false;
            }

            return new List<TtPoint>(where != null ? _SortedSelectedPoints.Where(where) : _SortedSelectedPoints);
        }

        public IList VisiblePoints { get; set; } = new ArrayList();

        public bool HasGpsTypes { get; private set; }
        public bool HasTravTypes { get; private set; }
        public bool HasQndms { get; private set; }

        public bool OnlyTravTypes { get; private set; }
        public bool OnlyManAccTypes { get; private set; }
        public bool OnlyGpsTypes { get; private set; }
        public bool OnlyQuondams { get; private set; }
        public bool HasPossibleCorridor { get; private set; }

        private bool ignoreSelectionChange = false, settingFields = false;
        
        public String ConvertTypeHeader
        {
            get
            {
                if (HasSelection)
                {
                    if (OnlyQuondams)
                        return "Convert to GPS";
                    else
                    {
                        OpType op = GetSortedSelectedPoints().First().OpType;

                        if (op == OpType.SideShot)
                            return "Convert to Traverse";
                        else if (op == OpType.Traverse)
                            return "Convert to SideShot";
                        else
                            return "Convert";
                    }
                }
                else
                    return "Convert";
            }
        }
        public String ReverseTypeHeader => SelectedPoints.Count > 2 ? "Reverse Points" : "Swap Points";

        private ObservableCollection<DataGridColumn> _DataColumns;
        public ObservableCollection<DataGridColumn> DataColumns
        {
            get { return _DataColumns; }
            private set { SetField(ref _DataColumns, value); }
        }

        private ObservableCollection<DataDictionaryField> _ExtendedDataFields;
        public ObservableCollection<DataDictionaryField> ExtendedDataFields
        {
            get { return _ExtendedDataFields; }
            private set { SetField(ref _ExtendedDataFields, value); }
        }

        private ObservableCollection<Control> _VisibleFields;
        public ObservableCollection<Control> VisibleFields
        {
            get { return _VisibleFields; }
            private set { SetField(ref _VisibleFields, value); }
        }

        public ObservableCollection<MenuItem> AdvInfoItems { get; }

        #region ToolsProperties
        public bool PlotToolInUse
        {
            get => Get<bool>();
            set => Set(value, () => {
                CreatePlotsCommand.RaiseCanExecuteChanged();
                CreateSubsampleCommand.RaiseCanExecuteChanged();
            });
        }
        #endregion

        #region AdvInfoItems
        public double? AI_DistFt { get { return Get<double?>(); } set { Set(value); } }
        public double? AI_DistMt { get { return Get<double?>(); } set { Set(value); } }

        public double? AI_Az { get { return Get<double?>(); } set { Set(value); } }
        public double? AI_AzTrue { get { return Get<double?>(); } set { Set(value); } }

        public string AI_CenterLL { get { return Get<string>(); } set { Set(value); } }
        public string AI_CenterUTM { get { return Get<string>(); } set { Set(value); } }

        public string AI_AverageLL { get { return Get<string>(); } set { Set(value); } }
        public string AI_AverageUTM { get { return Get<string>(); } set { Set(value); } }
        #endregion


        private void UpdateAdvInfoItems()
        {
            if (HasSelection && _SelectedPoints.Count > 1)
            {
                UTMCoords GetCoords(TtPoint p, int tzone)
                {
                    if (p.Metadata.Zone == tzone)
                        return new UTMCoords(p.AdjX, p.AdjY, tzone);
                    else
                        return UTMTools.ShiftZones(p.AdjX, p.AdjY, tzone, p.Metadata.Zone);
                }

                List<TtPoint> points = GetSortedSelectedPoints();

                double n, s, e, w, ax, ay, cx, cy, mine, maxe, dist = 0;
                double? az = null;

                int zone = Manager.DefaultMetadata.Zone;

                TtPoint currPoint = points[0];
                UTMCoords coords = GetCoords(currPoint, zone);

                n = s = ay = cy = coords.Y;
                e = w = ax = cx = coords.X;
                mine = maxe = currPoint.AdjZ;
                
                TtPoint lastPoint = currPoint;
                UTMCoords lastCoords = coords;

                bool isPoly = points.Count > 2;
                for (int i = 1; i < points.Count; i++)
                {
                    currPoint = points[i];
                    coords = GetCoords(currPoint, zone);

                    if (coords.Y > n)
                        n = coords.Y;

                    if (coords.Y < s)
                        s = coords.Y;

                    if (coords.X > e)
                        e = coords.X;

                    if (coords.X < w)
                        w = coords.X;

                    if (currPoint.AdjZ > maxe)
                        maxe = currPoint.AdjZ;

                    if (currPoint.AdjZ < mine)
                        mine = currPoint.AdjZ;

                    ax += coords.X;
                    ay += coords.Y;

                    dist += MathEx.Distance(coords.X, coords.Y, lastCoords.X, lastCoords.Y);

                    if (!isPoly)
                        az = TtUtils.AzimuthOfPoint(lastCoords.X, lastCoords.Y, coords.X, coords.Y);

                    lastCoords = coords;
                }

                AI_DistMt = dist;
                AI_DistFt = FMSC.Core.Convert.Distance(dist, Distance.FeetTenths, Distance.Meters);

                cx = (e + w) / 2;
                cy = (n + s) / 2;

                Point point = UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(cx, cy, zone);

                AI_CenterUTM = $"X: {cx.ToString("F3")}, Y: {cy.ToString("F3")}";
                AI_CenterLL = $"Lat: {point.Y.ToString("F3")}, Lon: {point.X.ToString("F3")}";
                
                if (isPoly)
                {
                    ax /= points.Count;
                    ay /= points.Count;

                    point = UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(ax, ay, zone);

                    AI_AverageUTM = $"X: {ax.ToString("F3")}, Y: {ay.ToString("F3")}";
                    AI_AverageLL = $"Lat: {point.Y.ToString("F3")}, Lon: {point.X.ToString("F3")}";

                    AI_Az = AI_AzTrue = null;
                }
                else
                {
                    AI_AverageUTM = AI_AverageLL = null;

                    AI_Az = az;
                    AI_AzTrue = az + Manager.DefaultMetadata.MagDec;
                }
            }
            else
            {
                AI_DistFt = AI_DistMt = null;
                AI_Az = AI_AzTrue = null;
                AI_AverageUTM = AI_AverageLL = AI_CenterUTM = AI_CenterLL = null;
            }
        }

        private MenuItem[] GenerateAdvInfoItems()
        {
            MenuItem GenerateAdvInfoMenuItem(string headerStringFormat, string propertyName, Action action)
            {
                MenuItem mi = new MenuItem()
                {
                    HeaderStringFormat = headerStringFormat,
                    Command = new RelayCommand(x => action()),
                };

                BindingOperations.SetBinding(mi, MenuItem.HeaderProperty, new Binding()
                {
                    Path = new PropertyPath(propertyName),
                    Mode = BindingMode.OneWay,
                    Source = this
                });

                BindingOperations.SetBinding(mi, MenuItem.VisibilityProperty, new Binding()
                {
                    Path = new PropertyPath(propertyName),
                    Mode = BindingMode.OneWay,
                    Source = this,
                    Converter = new NotNullVisibilityConverter()
                });

                return mi;
            }

            return new MenuItem[]
            {
                GenerateAdvInfoMenuItem("Dist (Ft):      {0:#,0.0000}", nameof(AI_DistFt), () => Clipboard.SetText(AI_DistFt.ToString())),
                GenerateAdvInfoMenuItem("Dist (Mt):      {0:#,0.0000}", nameof(AI_DistMt), () => Clipboard.SetText(AI_DistMt.ToString())),
                GenerateAdvInfoMenuItem("Azimuth (Mag):  {0:#,0.00}", nameof(AI_Az), () => Clipboard.SetText(AI_Az.ToString())),
                GenerateAdvInfoMenuItem("Azimuth (True): {0:#,0.00}", nameof(AI_AzTrue), () => Clipboard.SetText(AI_AzTrue.ToString())),
                GenerateAdvInfoMenuItem("Center (L/L):   {0}", nameof(AI_CenterLL), () => Clipboard.SetText(AI_CenterLL)),
                GenerateAdvInfoMenuItem("Center (UTM):   {0}", nameof(AI_CenterUTM), () => Clipboard.SetText(AI_CenterUTM)),
                GenerateAdvInfoMenuItem("Average (L/L):  {0}", nameof(AI_AverageLL), () => Clipboard.SetText(AI_AverageLL)),
                GenerateAdvInfoMenuItem("Average (UTM):  {0}", nameof(AI_AverageUTM), () => Clipboard.SetText(AI_AverageUTM))
            };
        }
        

        private void CompareAndSet<T>(ref bool same, ref T oldVal, T newVal)
        {
            if (oldVal != null && !EqualityComparer<T>.Default.Equals(oldVal, newVal))
            {
                same = false;
            }
            else
            {
                oldVal = newVal;
            }
        }

        private void CompareAndSet(ref bool same, ref string oldVal, string newVal)
        {
            if (String.IsNullOrWhiteSpace(oldVal) ^ String.IsNullOrWhiteSpace(newVal))
            {
                same = (oldVal != null && newVal != null) ?
                    oldVal == newVal :
                    String.IsNullOrWhiteSpace(oldVal) == String.IsNullOrWhiteSpace(newVal);

                if (!same)
                {
                    oldVal = null;
                }
            }
            else
            {
                oldVal = newVal;
            }
        }
        #endregion

        #region Point Properties
        private bool _SamePID = true;
        public bool SamePID => _SamePID;

        private int? _PID;
        public int? PID
        {
            get { return _PID; }
            set { EditValue(ref _PID, value, PointProperties.PID); }
        }


        private bool _SamePolygon = true;
        public bool SamePolygon => _SamePolygon;

        private TtPolygon _Polygon;
        public TtPolygon Polygon
        {
            get { return _Polygon; }
            set
            {
                Manager.MovePointsToPolygon(SelectedPoints.Cast<TtPoint>(), value, int.MaxValue);
            }
        }


        private bool _SameIndex = true;
        public bool SameIndex => _SameIndex;

        private int? _Index;
        public int? Index
        {
            get { return _Index; }
            set { EditValue(ref _Index, value, PointProperties.INDEX); Manager.RebuildPolygon(SelectedPoint.Polygon); }
        }


        private bool _SameComment = true;
        public bool SameComment => _SameComment;

        private string _Comment;
        public string Comment
        {
            get { return _Comment; }
            set
            {
                bool same = false;

                if (String.IsNullOrWhiteSpace(_Comment) ^ String.IsNullOrWhiteSpace(value))
                {
                    same = (_Comment != null && value != null) ?
                        _Comment == value :
                        String.IsNullOrWhiteSpace(_Comment) == String.IsNullOrWhiteSpace(value);
                }

                _Comment = value;

                if (!same)
                {
                    if (MultipleSelections)
                    {
                        Manager.EditPoints(SelectedPoints.Cast<TtPoint>(), PointProperties.COMMENT, value);
                    }
                    else
                    {
                        Manager.EditPoint(SelectedPoint, PointProperties.COMMENT, value);
                    }
                }
            }
        }


        private bool _SameMetadata = true;
        public bool SameMetadata => _SameMetadata;

        private TtMetadata _Metadata;
        public TtMetadata Metadata
        {
            get { return _Metadata; }
            set { EditValue(ref _Metadata, value, PointProperties.META); }
        }


        private bool _SameGroup = true;
        public bool SameGroup => _SameGroup;

        private TtGroup _Group;
        public TtGroup Group
        {
            get { return _Group; }
            set { EditValue(ref _Group, value, PointProperties.GROUP); }
        }


        private bool _SameOnBound = true;
        public bool SameOnBound => _SameOnBound;

        private bool? _OnBoundary;
        public bool? OnBoundary
        {
            get { return _OnBoundary; }
            set { EditValue(ref _OnBoundary, value, PointProperties.BOUNDARY); }
        }


        private bool _SameUnAdjX = true;
        public bool SameUnAdjX => _SameUnAdjX;

        private double? _UnAdjX;
        public double? UnAdjX
        {
            get { return _UnAdjX; }
            set
            {
                if (!_UnAdjX.Equals(value))
                {
                    _UnAdjX = value;

                    if (MultipleSelections)
                    {
                        Manager.EditPointsMultiValues(SelectedPoints.Cast<GpsPoint>(),
                            new PropertyInfo[] { PointProperties.UNADJX, PointProperties.LON },
                            new object[] { value, null });
                    }
                    else
                    {
                        Manager.EditPoint(SelectedPoint,
                            new PropertyInfo[] { PointProperties.UNADJX, PointProperties.LON },
                            new object[] { value, null });
                    }
                }
            }
        }


        private bool _SameUnAdjY = true;
        public bool SameUnAdjY => _SameUnAdjY;

        private double? _UnAdjY;
        public double? UnAdjY
        {
            get { return _UnAdjY; }
            set
            {
                if (!_UnAdjY.Equals(value))
                {
                    _UnAdjY = value;

                    if (MultipleSelections)
                    {
                        Manager.EditPointsMultiValues(SelectedPoints.Cast<GpsPoint>(),
                            new PropertyInfo[] { PointProperties.UNADJY, PointProperties.LAT },
                            new object[] { value, null });
                    }
                    else
                    {
                        Manager.EditPoint(SelectedPoint,
                            new PropertyInfo[] { PointProperties.UNADJY, PointProperties.LAT },
                            new object[] { value, null });
                    }
                }
            }
        }


        private bool _SameUnAdjZ = true;
        public bool SameUnAdjZ => _SameUnAdjZ;

        private double? _UnAdjZ;
        public double? UnAdjZ
        {
            get { return _UnAdjZ; }
            set
            {
                if (!_UnAdjZ.Equals(value))
                {
                    _UnAdjZ = value;

                    if (MultipleSelections)
                    {
                        Manager.EditPointsMultiValues(SelectedPoints.Cast<GpsPoint>(),
                            new PropertyInfo[] { PointProperties.UNADJZ, PointProperties.ELEVATION },
                            new object[] { value, null });
                    }
                    else
                    {
                        Manager.EditPoint(SelectedPoint,
                            new PropertyInfo[] { PointProperties.UNADJZ, PointProperties.ELEVATION },
                            new object[] { value, null });
                    }
                }
            }
        }


        private bool _SameManAcc = true;
        public bool SameManAcc => _SameManAcc;

        private double? _ManAcc;
        public double? ManAcc
        {
            get { return _ManAcc; }
            set
            {
                if (!_ManAcc.Equals(value))
                {
                    _ManAcc = value;
                    
                    if (MultipleSelections)
                    {
                        List<PropertyInfo> properties = new List<PropertyInfo>();
                        foreach (TtPoint point in SelectedPoints)
                        {
                            if (point.OpType == OpType.Quondam)
                                properties.Add(PointProperties.MAN_ACC_QP);
                            else
                                properties.Add(PointProperties.MAN_ACC_GPS);
                        }

                        Manager.EditPoints(SelectedPoints.Cast<TtPoint>(), properties, value);
                    }
                    else
                    {
                        if (SelectedPoint.OpType == OpType.Quondam)
                            Manager.EditPoint(SelectedPoint, PointProperties.MAN_ACC_QP, value);
                        else
                            Manager.EditPoint(SelectedPoint, PointProperties.MAN_ACC_GPS, value);
                    }
                }
            }
        }


        private bool _SameFwdAz = true;
        public bool SameFwdAz => _SameFwdAz;

        private double? _FwdAz;
        public double? FwdAz
        {
            get { return _FwdAz; }
            set { EditValue(ref _FwdAz, value, PointProperties.FWD_AZ, true); }
        }

        
        private bool _SameBkAz = true;
        public bool SameBkAz => _SameBkAz;

        private double? _BkAz;
        public double? BkAz
        {
            get { return _BkAz; }
            set { EditValue(ref _BkAz, value, PointProperties.BK_AZ, true); }
        }


        private bool _SameSlpAng = true;
        public bool SameSlpAng => _SameSlpAng;

        private double? _SlpAng;
        public double? SlpAng
        {
            get { return _SlpAng; }
            set { EditValue(ref _SlpAng, value, PointProperties.SLP_ANG); }
        }
        

        private bool _SameSlpDist = true;
        public bool SameSlpDist => _SameSlpDist;

        private double? _SlpDist;
        public double? SlpDist
        {
            get { return _SlpDist; }
            set { EditValue(ref _SlpDist, value, PointProperties.SLP_DIST); }
        }
        

        private bool _SameParentPoint = true;
        public bool SameParentPoint => _SameParentPoint;

        private TtPoint _ParentPoint;
        public TtPoint ParentPoint => _ParentPoint;


        private void EditValue<T>(ref T? origValue, T? newValue, PropertyInfo property, bool allowNull = false) where T : struct, IEquatable<T>
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    if (MultipleSelections)
                    {
                        Manager.EditPoints(SelectedPoints.Cast<TtPoint>(), property, newValue);
                    }
                    else
                    {
                        Manager.EditPoint(SelectedPoint, property, newValue);
                    } 
                }
            }
        }

        private void EditValue<T>(ref T origValue, T newValue, PropertyInfo property, bool allowNull = false) where T : class
        {
            if (!origValue.Equals(newValue))
            {
                origValue = newValue;

                if (allowNull || newValue != null)
                {
                    if (MultipleSelections)
                    {
                        Manager.EditPoints(SelectedPoints.Cast<TtPoint>(), property, newValue);
                    }
                    else
                    {
                        Manager.EditPoint(SelectedPoint, property, newValue);
                    }
                }
            }
        }
        #endregion

        #region Point Extended Properties

        private Core.DataDictionary _ExtendedData;
        public Core.DataDictionary ExtendedData
        {
            get { return _ExtendedData; }
        }

        private Dictionary<string, bool> _ExtendedDataSame = new Dictionary<string, bool>();

        public bool ArePropertyValuesSame(string propertyCN)
        {
            return _ExtendedDataSame[propertyCN];
        }

        #endregion

        #region Filters
        Dictionary<string, bool> _CheckedPolygons = new Dictionary<string, bool>();
        Dictionary<string, bool> _CheckedMetadata = new Dictionary<string, bool>();
        Dictionary<string, bool> _CheckedGroups = new Dictionary<string, bool>();
        Dictionary<OpType, bool> _CheckedOpTypes = new Dictionary<OpType, bool>();


        private ObservableCollection<CheckedListItem<TtPolygon>> _Polygons = new ObservableCollection<CheckedListItem<TtPolygon>>();
        public ReadOnlyObservableCollection<CheckedListItem<TtPolygon>> Polygons { get; private set; }

        private ObservableCollection<CheckedListItem<TtMetadata>> _Metadatas = new ObservableCollection<CheckedListItem<TtMetadata>>();
        public ReadOnlyObservableCollection<CheckedListItem<TtMetadata>> Metadatas { get; private set; }

        private ObservableCollection<CheckedListItem<TtGroup>> _Groups = new ObservableCollection<CheckedListItem<TtGroup>>();
        public ReadOnlyObservableCollection<CheckedListItem<TtGroup>> Groups { get; private set; }

        private List<CheckedListItem<string>> _OpTypes = new List<CheckedListItem<string>>();

        public ReadOnlyCollection<CheckedListItem<String>> OpTypes { get; private set; }

        public bool? IsOnBnd { get { return Get<bool?>(); } set { Set(value); } }
        public bool? HasLinks { get { return Get<bool?>(); } set { Set(value); } }
        #endregion


        public PointEditorModel(TtProject project)
        {
            EventManager.RegisterClassHandler(typeof(Control), Control.KeyDownEvent, new KeyEventHandler((s, e) => {
                if (e.Key == Key.LeftCtrl)
                    CtrlKeyPressed = true;
            }), true);
            
            EventManager.RegisterClassHandler(typeof(Control), Control.KeyUpEvent, new KeyEventHandler((s, e) => {
                if (e.Key == Key.LeftCtrl)
                    CtrlKeyPressed = false;
            }), true);

            Project = project;
            Manager = project.Manager;
            Manager.HistoryChanged += (s, e) =>
            {
                if (e.RequireRefresh)
                    Points.Refresh();
                OnSelectionChanged();
            };

            #region Setup Filters
            CheckedListItem<TtPolygon> tmpPoly;
            tmpPoly = new CheckedListItem<TtPolygon>(new TtPolygon() { Name = "All", CN = Consts.FullGuid }, true);
            _Polygons.Add(tmpPoly);
            tmpPoly.ItemCheckedChanged += Polygon_ItemCheckedChanged;

            foreach (TtPolygon polygon in Manager.Polygons)
            {
                tmpPoly = new CheckedListItem<TtPolygon>(polygon, true);
                _Polygons.Add(tmpPoly);
                tmpPoly.ItemCheckedChanged += Polygon_ItemCheckedChanged;
                _CheckedPolygons.Add(polygon.CN, true);
            }

            ((INotifyCollectionChanged)Manager.Polygons).CollectionChanged += Polygons_CollectionChanged;


            CheckedListItem<TtMetadata> tmpMeta;
            tmpMeta = new CheckedListItem<TtMetadata>(new TtMetadata() { Name = "All", CN = Consts.FullGuid }, true);
            _Metadatas.Add(tmpMeta);
            tmpMeta.ItemCheckedChanged += Metadata_ItemCheckedChanged;

            foreach (TtMetadata metadata in Manager.Metadata)
            {
                tmpMeta = new CheckedListItem<TtMetadata>(metadata, true);
                _Metadatas.Add(tmpMeta);
                tmpMeta.ItemCheckedChanged += Metadata_ItemCheckedChanged;
                _CheckedMetadata.Add(metadata.CN, true);
            }

            ((INotifyCollectionChanged)Manager.Metadata).CollectionChanged += Metadata_CollectionChanged;


            CheckedListItem<TtGroup> tmpGroup;
            tmpGroup = new CheckedListItem<TtGroup>(new TtGroup() { Name = "All", CN = Consts.FullGuid }, true);
            _Groups.Add(tmpGroup);
            tmpGroup.ItemCheckedChanged += Group_ItemCheckedChanged;

            foreach (TtGroup group in Manager.Groups)
            {
                tmpGroup = new CheckedListItem<TtGroup>(group, true);
                _Groups.Add(tmpGroup);
                tmpGroup.ItemCheckedChanged += Group_ItemCheckedChanged;
                _CheckedGroups.Add(group.CN, true);
            }

            ((INotifyCollectionChanged)Manager.Groups).CollectionChanged += Groups_CollectionChanged;


            CheckedListItem<string> tmpOpType;
            tmpOpType = new CheckedListItem<string>("All", true);
            tmpOpType.ItemCheckedChanged += OpType_ItemCheckedChanged;
            _OpTypes.Add(tmpOpType);

            foreach (OpType op in Enum.GetValues(typeof(OpType)))
            {
                tmpOpType = new CheckedListItem<string>(op.ToString(), true);
                _OpTypes.Add(tmpOpType);
                tmpOpType.ItemCheckedChanged += OpType_ItemCheckedChanged;
                _CheckedOpTypes.Add(op, true);
            }
            #endregion

            Polygons = new ReadOnlyObservableCollection<CheckedListItem<TtPolygon>>(_Polygons);
            Groups = new ReadOnlyObservableCollection<CheckedListItem<TtGroup>>(_Groups);
            Metadatas = new ReadOnlyObservableCollection<CheckedListItem<TtMetadata>>(_Metadatas);
            OpTypes = new ReadOnlyCollection<CheckedListItem<string>>(_OpTypes);

            IsOnBnd = null;
            HasLinks = null;

            Points = CollectionViewSource.GetDefaultView(Manager.Points) as ListCollectionView;
            
            Points.CustomSort = new PointSorter();

            Points.Filter = Filter;
            
            #region Init Commands
            RefreshPoints = new RelayCommand(x => Points.Refresh());

            CopyCellValueCommand = new BindedRelayCommand<PointEditorModel>(
                x => CopyCellValue(x as DataGrid), x => HasSelection,
                this, x => x.HasSelection);

            ExportValuesCommand = new BindedRelayCommand<PointEditorModel>(
                x => ExportValues(x as DataGrid), x => HasSelection,
                this, x => x.HasSelection);

            ViewPointDetailsCommand = new RelayCommand(x => ViewPointDetails());

            ViewSatInfoCommand = new BindedRelayCommand<PointEditorModel>(
                x => ViewSafeInfo(), x => _SelectedPoints.Cast<TtPoint>().Any(pt => pt.IsGpsType()),
                this, x => x.SelectedPoints);

            ChangeQuondamParentCommand = new BindedRelayCommand<PointEditorModel>(
                x => ChangeQuondamParent(), x => OnlyQuondams && !MultipleSelections,
                this, x => new { x.OnlyQuondams, x.MultipleSelections });

            RenamePointsCommand = new BindedRelayCommand<PointEditorModel>(
                x => RenamePoints(), x => MultipleSelections && SamePolygon,
                this, x => x.MultipleSelections);

            ReverseSelectedCommand = new BindedRelayCommand<PointEditorModel>(
                x => { if (SelectedPoints.Count > 2) ReverseSelection(); else SwapPoints(); },
                x => MultipleSelections && SamePolygon,
                this, x => x.MultipleSelections);

            ResetPointCommand = new BindedRelayCommand<PointEditorModel>(
                x => ResetPoint(), x => HasSelection,
                this, x => x.HasSelection);

            ResetPointFieldCommand = new BindedRelayCommand<PointEditorModel>(
                x => ResetPointField(x as DataGrid), x => HasSelection,
                this, x => x.HasSelection);

            DeleteCommand = new BindedRelayCommand<PointEditorModel>(
                x => DeletePoint(), x => HasSelection,
                this, x => x.HasSelection);

            CreatePointCommand = new BindedRelayCommand<ReadOnlyObservableCollection<TtPolygon>>(
                x => CreateNewPoint(x != null ? (OpType)x : OpType.GPS),
                x => Manager.Polygons.Count > 0,
                Manager.Polygons, x => x.Count);

            CreateQuondamsCommand = new BindedRelayCommand<PointEditorModel>(
                x => CreateQuondams(), x => HasSelection,
                this, x => x.HasSelection);

            ConvertPointsCommand = new BindedRelayCommand<PointEditorModel>(
                x => ConvertPoints(), x => OnlyQuondams || OnlyTravTypes,
                this, x => new { x.OnlyQuondams, x.OnlyTravTypes });

            MovePointsCommand = new BindedRelayCommand<PointEditorModel>(
                x => MovePoints(), x => HasSelection,
                this, x => x.HasSelection);

            RetraceCommand = new BindedRelayCommand<ReadOnlyObservableCollection<TtPoint>>(
                x => Retrace(), x => Manager.Points.Count > 0,
                Manager.Points, x => x.Count);

            ReindexCommand = new BindedRelayCommand<ReadOnlyObservableCollection<TtPoint>>(
                x => Reindex(), x => Manager.Points.Count > 0,
                Manager.Points, x => x.Count);

            CreatePlotsCommand = new BindedRelayCommand<ReadOnlyObservableCollection<TtPolygon>>(
                x => CreatePlots(),
                x => Manager.Polygons.Count > 0 && !PlotToolInUse,
                Manager.Polygons, x => x.Count);

            CreateSubsampleCommand = new BindedRelayCommand<ReadOnlyObservableCollection<TtPolygon>>(
                x => CreateSubsample(),
                x => Manager.Polygons.Count > 0 && !PlotToolInUse,
                Manager.Polygons, x => x.Count);

            CreateCorridorCommand = new BindedRelayCommand<PointEditorModel>(
                x => CreateCorridor(), x => HasPossibleCorridor,
                this, x => x.HasPossibleCorridor);

            ModifyDataDictionaryCommand = new RelayCommand(x => ModifyDataDictionary());

            DeselectCommand = new RelayCommand(x => DeselectAll());

            SelectAlternateCommand = new RelayCommand(x => SelectAlternate());

            SelectGpsCommand = new RelayCommand(x => SelectGps());

            SelectTravCommand = new RelayCommand(x => SelectTraverse());

            SelectInverseCommand = new RelayCommand(x => SelectInverse());

            UpdateAdvInfo = new RelayCommand(x => UpdateAdvInfoItems());
            #endregion

            //BindingOperations.EnableCollectionSynchronization(_SelectedPoints, _lock);

            AdvInfoItems = new ObservableCollection<MenuItem>(GenerateAdvInfoItems());

            SetupUI();
        }


        private void SetupUI()
        {
            #region Setup Columns and Visibility Menu
            void AddColumnAndMenuItem(Tuple<DataGridTextColumn, string> tup)
            {
                DataColumns.Add(tup.Item1);

                MenuItem mi = new MenuItem()
                {
                    Header = tup.Item2,
                    IsCheckable = true,
                    IsChecked = tup.Item1.Visibility == Visibility.Visible
                };

                //Bind model boolean to menu item's check state
                BindingOperations.SetBinding(mi, MenuItem.IsCheckedProperty, new Binding()
                {
                    Path = new PropertyPath("Visibility"),
                    Mode = BindingMode.TwoWay,
                    Source = tup.Item1,
                    Converter = new VisibilityToBooleanConverter()
                });

                //Bind model boolean to column visibility
                BindingOperations.SetBinding(tup.Item1, DataGridColumn.VisibilityProperty, new Binding()
                {
                    Path = new PropertyPath("IsChecked"),
                    Mode = BindingMode.OneWay,
                    Source = mi,
                    Converter = new BooleanToVisibilityConverter()
                });

                VisibleFields.Add(mi);
            }


            DataColumns = new ObservableCollection<DataGridColumn>();

            VisibleFields = new ObservableCollection<Control>(new Control[] {
                new MenuItem()
                {
                    Header = "Default Fields",
                    Command = new RelayCommand(x =>
                    {
                        foreach (DataGridTextColumn col in DataColumns)
                            col.Visibility = IsDefaultField((col.Header as ColumnHeader).Name) ? Visibility.Visible : Visibility.Collapsed;
                    })
                },
                new MenuItem()
                {
                    Header = "Extended Fields",
                    Command = new RelayCommand(x =>
                    {
                        foreach (DataGridTextColumn col in DataColumns)
                            col.Visibility = IsExtendedField((col.Header as ColumnHeader).Name) ? Visibility.Visible : Visibility.Collapsed;
                    })
                },
                new MenuItem()
                {
                    Header = "Default + Extended Fields",
                    Command = new RelayCommand(x =>
                    {
                        foreach (DataGridTextColumn col in DataColumns)
                            col.Visibility = IsDefaultOrExtendedField((col.Header as ColumnHeader).Name) ? Visibility.Visible : Visibility.Collapsed;
                    })
                },
                new MenuItem()
                {
                    Header = "All Fields",
                    Command = new RelayCommand(x =>
                    {
                        foreach (DataGridTextColumn col in DataColumns)
                            col.Visibility = Visibility.Visible;
                    })
                },
                new Separator()
            });

            foreach (Tuple<DataGridTextColumn, string> col in CreateDefaultColumns())
                AddColumnAndMenuItem(col);
            #endregion

            #region Setup DataDictionary Columns

            DataDictionaryTemplate ddt = Manager.GetDataDictionaryTemplate();

            if (ddt != null)
            {
                ExtendedDataFields = new ObservableCollection<DataDictionaryField>(ddt);

                _ExtendedData = new Core.DataDictionary(Consts.EmptyGuid, ddt);
                _ExtendedDataSame = ddt.ToDictionary(ddf => ddf.CN, ddf => true);

                if (ExtendedDataFields.Count > 0)
                    VisibleFields.Add(new Separator());

                foreach (DataDictionaryField ddf in ExtendedDataFields)
                    AddColumnAndMenuItem(CreateDataGridTextColumn(ddf.Name, $"{ nameof(TtPoint.ExtendedData) }.{ $"[{ ddf.CN }]" }", visibility: Visibility.Collapsed));
            }
            else
            {
                ExtendedDataFields = new ObservableCollection<DataDictionaryField>();
            }

            if (ExtendedData != null)
            {
                ExtendedData.PropertyChanged += (sender, e) =>
                {
                    if (!settingFields)
                    {
                        if (MultipleSelections)
                        {
                            _ExtendedDataSame[e.PropertyName] = true;

                            object val = _ExtendedData[e.PropertyName];
                            foreach (TtPoint point in SelectedPoints)
                                point.ExtendedData[e.PropertyName] = val;
                        }
                        else
                        {
                            //if (!SelectedPoint.ExtendedData[e.PropertyName].Equals(_ExtendedData[e.PropertyName]))
                            //{
                            //    if (MultipleSelections)
                            //    {
                            //        //Manager.EditPoints(SelectedPoints.Cast<TtPoint>(), property, newValue);
                            //    }
                            //    else
                            //    {
                            //        //Manager.EditPoint(SelectedPoint, property, newValue);
                            //    }
                            //}
                            
                            Manager.EditPoint(SelectedPoint, PointProperties.EXTENDED_DATA, new Core.DataDictionary(SelectedPoint.CN, _ExtendedData));
                            //SelectedPoint.ExtendedData[e.PropertyName] = _ExtendedData[e.PropertyName];
                        }
                    }

                    OnPropertyChanged(e.PropertyName);
                }; 
            }
            #endregion

            UpdateAdvInfoItems();
        }


        #region Collections Changed
        private void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CheckedListItem<TtPolygon> tmp;

                foreach (TtPolygon polygon in e.NewItems)
                {
                    tmp = new CheckedListItem<TtPolygon>(polygon, true);
                    _Polygons.Add(tmp);
                    _CheckedPolygons.Add(polygon.CN, true);
                    tmp.ItemCheckedChanged += Polygon_ItemCheckedChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TtPolygon polygon in e.OldItems)
                {
                    for (int i = 0; i < _Polygons.Count; i++)
                    {
                        if (_Polygons[i].Item == polygon)
                        {
                            _Polygons.RemoveAt(i);
                            _CheckedPolygons.Remove(polygon.CN);
                            break;
                        }
                    }
                }
            }
        }

        private void Polygon_ItemCheckedChanged(object sender, EventArgs e)
        {
            if (CtrlKeyPressed)
                OnlyCheckThisItem(sender, ref _CheckedPolygons, ref _Polygons);
            else
                UpdateCheckedItems(sender, ref _CheckedPolygons, ref _Polygons);
        }


        private void Metadata_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CheckedListItem<TtMetadata> tmp;
                foreach (TtMetadata Metadata in e.NewItems)
                {
                    tmp = new CheckedListItem<TtMetadata>(Metadata, true);
                    _Metadatas.Add(tmp);
                    _CheckedMetadata.Add(Metadata.CN, true);
                    tmp.ItemCheckedChanged += Metadata_ItemCheckedChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TtMetadata Metadata in e.OldItems)
                {
                    for (int i = 0; i < _Metadatas.Count; i++)
                    {
                        if (_Metadatas[i].Item == Metadata)
                        {
                            _Metadatas.RemoveAt(i);
                            _CheckedMetadata.Remove(Metadata.CN);
                            break;
                        }
                    }
                }
            }
        }

        private void Metadata_ItemCheckedChanged(object sender, EventArgs e)
        {
            if (CtrlKeyPressed)
                OnlyCheckThisItem(sender, ref _CheckedMetadata, ref _Metadatas);
            else
                UpdateCheckedItems(sender, ref _CheckedMetadata, ref _Metadatas);
        }
        

        private void Groups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CheckedListItem<TtGroup> tmp;
                foreach (TtGroup Group in e.NewItems)
                {
                    tmp = new CheckedListItem<TtGroup>(Group, true);
                    _Groups.Add(tmp);
                    _CheckedGroups.Add(Group.CN, true);
                    tmp.ItemCheckedChanged += Group_ItemCheckedChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TtGroup Group in e.OldItems)
                {
                    for (int i = 0; i < _Groups.Count; i++)
                    {
                        if (_Groups[i].Item == Group)
                        {
                            _Groups.RemoveAt(i);
                            _CheckedGroups.Remove(Group.CN);
                            break;
                        }
                    }
                }
            }
        }

        private void Group_ItemCheckedChanged(object sender, EventArgs e)
        {
            if (CtrlKeyPressed)
                OnlyCheckThisItem(sender, ref _CheckedGroups, ref _Groups);
            else
                UpdateCheckedItems(sender, ref _CheckedGroups, ref _Groups);
        }


        private void UpdateCheckedItems<T>(object sender, ref Dictionary<string, bool> checkedItems,
            ref ObservableCollection<CheckedListItem<T>> items) where T : TtObject
        {
            if (sender is CheckedListItem<T> cp)
            {
                if (cp.Item.CN != Consts.FullGuid)
                {
                    checkedItems[cp.Item.CN] = cp.IsChecked;
                }
                else
                {
                    bool isChecked = cp.IsChecked;

                    foreach (var key in checkedItems.Keys.ToList())
                    {
                        checkedItems[key] = isChecked;
                    }

                    foreach (CheckedListItem<T> item in items)
                    {
                        item.SetChecked(isChecked, false);
                    }
                }

                Points.Refresh();
            }
        }

        private void OnlyCheckThisItem<T>(object sender, ref Dictionary<string, bool> checkedItems,
            ref ObservableCollection<CheckedListItem<T>> items) where T : TtObject
        {
            if (sender is CheckedListItem<T> cp)
            {
                if (cp.Item.CN != Consts.FullGuid)
                {
                    foreach (CheckedListItem<T> item in items.Where(cli => cli.Item.CN != cp.Item.CN))
                    {
                        checkedItems[item.Item.CN] = false;
                        item.SetChecked(false, false);
                    }

                    cp.SetChecked(true, false);
                    checkedItems[cp.Item.CN] = true;
                }
                else
                {
                    bool isChecked = cp.IsChecked;

                    foreach (var key in checkedItems.Keys.ToList())
                    {
                        checkedItems[key] = isChecked;
                    }

                    foreach (CheckedListItem<T> item in items)
                    {
                        item.SetChecked(isChecked, false);
                    }
                }

                Points.Refresh();
            }
        }

        private void OpType_ItemCheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckedListItem<string> cp)
            {
                if (cp.Item != "All")
                {
                    if (CtrlKeyPressed)
                    {
                        foreach (CheckedListItem<string> item in _OpTypes)
                        {
                            item.SetChecked(false, false);
                        }

                        foreach (OpType op in Enum.GetValues(typeof(OpType)))
                        {
                            _CheckedOpTypes[op] = false;
                        }

                        _CheckedOpTypes[(OpType)Enum.Parse(typeof(OpType), cp.Item)] = true;
                        cp.SetChecked(true, false);
                    }
                    else
                    {
                        _CheckedOpTypes[(OpType)Enum.Parse(typeof(OpType), cp.Item)] = cp.IsChecked;
                    }
                }
                else
                {
                    bool isChecked = cp.IsChecked;

                    foreach (var key in _CheckedOpTypes.Keys.ToList())
                    {
                        _CheckedOpTypes[key] = isChecked;
                    }

                    foreach (CheckedListItem<string> item in _OpTypes)
                    {
                        item.SetChecked(isChecked, false);
                    }
                }

                Points.Refresh();
            }
        }
        #endregion


        public void OnSelectionChanged()
        {
            if (ignoreSelectionChange)
                return;

            settingFields = true;

            _SelectionChanged = true;

            HasGpsTypes = false;
            HasTravTypes = false;
            HasQndms = false;
            HasPossibleCorridor = false;

            bool ddAvailable = _ExtendedData != null;

            if (ddAvailable)
            {
                foreach (string id in _ExtendedDataSame.Keys.ToArray())
                    _ExtendedDataSame[id] = true;
                _ExtendedData.ClearValues();
            }

            if (HasSelection)
            {
                if (MultipleSelections)
                {
                    TtPoint fpt = SelectedPoints[0] as TtPoint;

                    TtPolygon poly = fpt.Polygon;
                    TtMetadata meta = fpt.Metadata;
                    TtGroup group = fpt.Group;

                    double? unAdjX = null,
                        unAdjY = null,
                        unAdjZ = null,
                        fwdAz = null,
                        bkAz = null,
                        slpDist = null,
                        slpAng = null,
                        manacc = null;

                    bool sameUnAdjX = true, sameUnAdjY = true, sameUnAdjZ = true,
                        sameFwAz = true, sameBkAz = true, sameManAcc = true,
                        sameSlpDist = true, sameSlpAng = true;

                    bool fmanacc = true, fqndm = true;

                    bool hasOnbnd = false, hasOffBnd = false;

                    bool hasSsOnBnd = false, hasTrav = false;

                    bool sameCmt = true;
                    string cmt = fpt.Comment;

                    bool parseTrav = true;
                    bool parseManAcc = true;
                    bool parseQndm = true;

                    TtPoint qParent = null;
                    bool sameQParent = true;

                    foreach (TtPoint point in SelectedPoints)
                    {
                        if (poly != null && poly.CN != point.PolygonCN)
                            poly = null;

                        if (meta != null && meta.CN != point.MetadataCN)
                            meta = null;

                        if (group != null && group.CN != point.GroupCN)
                            group = null;

                        if (point.OnBoundary)
                        {
                            if (!hasOnbnd)
                                hasOnbnd = true;
                        }
                        else
                        {
                            if (!hasOffBnd)
                                hasOffBnd = true;
                        }


                        if (point.IsGpsType() && !HasGpsTypes)
                        {
                            HasGpsTypes = true;

                            parseTrav = false;
                            parseQndm = false;
                        }
                        else if (point.IsTravType())
                        {
                            if (!hasSsOnBnd && point.OpType == OpType.SideShot)
                                hasSsOnBnd |= point.OnBoundary;

                            if (!hasTrav && point.OpType == OpType.Traverse)
                                hasTrav = true;

                            if (!HasTravTypes)
                            {
                                HasTravTypes = true;

                                parseManAcc = false;
                                parseQndm = false;
                            }
                        }
                        else if (point.OpType == OpType.Quondam && !HasQndms)
                        {
                            HasQndms = true;
                            parseTrav = false;
                        }


                        if (sameCmt)
                        {
                            CompareAndSet(ref sameCmt, ref cmt, point.Comment);
                        }

                        if (sameUnAdjX)
                        {
                            CompareAndSet(ref sameUnAdjX, ref unAdjX, point.UnAdjX);
                        }

                        if (sameUnAdjY)
                        {
                            CompareAndSet(ref sameUnAdjY, ref unAdjY, point.UnAdjY);
                        }

                        if (sameUnAdjZ)
                        {
                            CompareAndSet(ref sameUnAdjZ, ref unAdjZ, point.UnAdjZ);
                        }


                        if (parseTrav && point.IsTravType())
                        {
                            TravPoint tp = point as TravPoint;

                            if (sameFwAz && fwdAz != tp.FwdAzimuth)
                            {
                                sameFwAz = false;
                            }

                            if (sameBkAz && bkAz != tp.BkAzimuth)
                            {
                                sameBkAz = false;
                            }

                            if (sameSlpAng && slpAng != tp.SlopeAngle)
                            {
                                sameSlpAng = false;
                            }

                            if (sameSlpDist && slpDist != tp.SlopeDistance)
                            {
                                sameSlpDist = false;
                            }
                        }

                        if (parseManAcc && (point.IsGpsType() || point.OpType == OpType.Quondam))
                        {
                            IManualAccuracy ima = point as IManualAccuracy;

                            if (sameManAcc)
                            {
                                if (!fmanacc && manacc != ima.ManualAccuracy)
                                {
                                    sameManAcc = false;
                                    manacc = null;
                                }
                                else
                                {
                                    manacc = ima.ManualAccuracy;
                                    fmanacc = false;
                                }
                            }

                            if (parseQndm && point.OpType == OpType.Quondam)
                            {
                                QuondamPoint qp = point as QuondamPoint;

                                if (sameQParent)
                                {
                                    if (!fqndm && qp.ParentPointCN != qParent.CN)
                                    {
                                        sameQParent = false;
                                        qParent = null;
                                    }
                                    else
                                    {
                                        qParent = qp.ParentPoint;
                                        fqndm = false;
                                    }
                                }
                            }
                        }

                        if (ddAvailable)
                        {
                            if (point.ExtendedData == null)
                                throw new Exception($"Point { point.PID } ({ point.CN }) does not have ExtendedData");

                            foreach (string id in _ExtendedDataSame.Keys.ToArray())
                            {
                                if (_ExtendedDataSame[id])
                                {
                                    object pval = point.ExtendedData[id];
                                    object oval = _ExtendedData[id];

                                    if (oval == null)
                                        _ExtendedData[id] = pval;
                                    else if (pval == null || !pval.Equals(oval))
                                    {
                                        _ExtendedDataSame[id] = false;
                                        _ExtendedData[id] = null;
                                    }
                                }
                            }
                        }
                    }

                    _PID = null;
                    _SamePID = false;

                    _Index = null;
                    _SameIndex = false;

                    _Comment = cmt;
                    _SameComment = sameCmt;

                    _Polygon = poly;
                    _SamePolygon = poly != null;

                    _Metadata = meta;
                    _SameMetadata = meta != null;

                    _Group = group;
                    _SameGroup = group != null;

                    HasPossibleCorridor = HasGpsTypes && hasSsOnBnd && !HasQndms && !hasTrav && _SamePolygon;

                    if (hasOnbnd ^ hasOffBnd)
                    {
                        _OnBoundary = hasOnbnd;
                        _SameOnBound = true;
                    }
                    else
                    {
                        _OnBoundary = null;
                        _SameOnBound = true;
                    }

                    _ManAcc = parseManAcc ? manacc : null;
                    _SameManAcc = sameManAcc;


                    if (parseTrav)
                    {
                        _FwdAz = sameFwAz ? fwdAz : null;
                        _SameFwdAz = sameFwAz;

                        _BkAz = sameBkAz ? bkAz : null;
                        _SameBkAz = sameBkAz;

                        _SlpAng = SameSlpAng ? slpAng : null;
                        _SameSlpAng = sameSlpAng;

                        _SlpDist = sameSlpDist ? SlpDist : null;
                        _SameSlpDist = sameSlpDist;
                    }
                    else
                    {
                        _FwdAz = null;
                        _SameFwdAz = true;

                        _BkAz = null;
                        _SameBkAz = true;

                        _SlpAng = null;
                        _SameSlpAng = true;

                        _SlpDist = null;
                        _SameSlpDist = true;
                    }

                    if (parseQndm)
                    {
                        _ParentPoint = qParent;
                        _SameParentPoint = sameQParent;
                    }
                    else
                    {
                        _ParentPoint = null;
                        _SameParentPoint = true;
                    }

                    _UnAdjX = sameUnAdjX ? unAdjX : null;
                    _SameUnAdjX = sameUnAdjX;

                    _UnAdjY = sameUnAdjY ? unAdjY : null;
                    _SameUnAdjY = sameUnAdjY;

                    _UnAdjZ = sameUnAdjZ ? unAdjZ : null;
                    _SameUnAdjZ = sameUnAdjZ;
                }
                else
                {
                    SelectedPoint = SelectedPoints[0] as TtPoint;

                    _PID = SelectedPoint.PID;
                    _SamePID = true;

                    _Index = SelectedPoint.Index;
                    _SameIndex = true;

                    _Comment = SelectedPoint.Comment;
                    _SameComment = true;

                    _Polygon = SelectedPoint.Polygon;
                    _SamePolygon = true;

                    _Metadata = SelectedPoint.Metadata;
                    _SameMetadata = true;

                    _Group = SelectedPoint.Group;
                    _SameGroup = true;

                    _OnBoundary = SelectedPoint.OnBoundary;
                    _SameOnBound = true;


                    if (SelectedPoint.IsGpsType())
                    {
                        HasGpsTypes = true;

                        _ManAcc = (SelectedPoint as GpsPoint).ManualAccuracy;
                        _ParentPoint = null;
                    }
                    else if (SelectedPoint.OpType == OpType.Quondam)
                    {
                        QuondamPoint qp = SelectedPoint as QuondamPoint;

                        HasQndms = true;

                        _ManAcc = qp.ManualAccuracy;
                        _ParentPoint = qp.ParentPoint;
                    }
                    else
                    {
                        _ManAcc = null;
                    }

                    _SameManAcc = true;
                    _SameParentPoint = true;


                    if (SelectedPoint.OpType == OpType.Quondam)
                        _ParentPoint = (SelectedPoint as QuondamPoint).ParentPoint;
                    else
                        _ParentPoint = null;

                    _SameParentPoint = true;


                    if (SelectedPoint.IsTravType())
                    {
                        HasTravTypes = true;

                        TravPoint tp = SelectedPoint as TravPoint;

                        _FwdAz = tp.FwdAzimuth;
                        _SameFwdAz = true;

                        _BkAz = tp.BkAzimuth;
                        _SameBkAz = true;

                        _SlpAng = tp.SlopeAngle;
                        _SameSlpAng = true;

                        _SlpDist = tp.SlopeDistance;
                        _SameSlpDist = true;
                    }
                    else
                    {
                        _FwdAz = null;
                        _SameFwdAz = true;

                        _BkAz = null;
                        _SameBkAz = true;

                        _SlpAng = null;
                        _SameSlpAng = true;

                        _SlpDist = null;
                        _SameSlpDist = true;
                    }

                    _UnAdjX = SelectedPoint.UnAdjX;
                    _SameUnAdjX = true;

                    _UnAdjY = SelectedPoint.UnAdjY;
                    _SameUnAdjY = true;

                    _UnAdjZ = SelectedPoint.UnAdjZ;
                    _SameUnAdjZ = true;

                    if (ddAvailable)
                    {
                        foreach (string id in _ExtendedDataSame.Keys)
                            _ExtendedData[id] = SelectedPoint.ExtendedData[id];
                    }
                }
            }
            else
            {
                _PID = null;
                _SamePID = true;

                _Index = null;
                _SameIndex = true;

                _Comment = null;
                _SameComment = true;

                _Polygon = null;
                _SamePolygon = true;

                _Metadata = null;
                _SameMetadata = true;

                _Group = null;
                _SameGroup = true;

                _OnBoundary = false;
                _SameOnBound = true;

                _ManAcc = null;
                _SameManAcc = true;

                _FwdAz = null;
                _SameFwdAz = true;

                _BkAz = null;
                _SameBkAz = true;

                _SlpAng = null;
                _SameSlpAng = true;

                _SlpDist = null;
                _SameSlpDist = true;

                _ParentPoint = null;
                _SameParentPoint = true;

                _UnAdjX = null;
                _SameUnAdjX = true;

                _UnAdjY = null;
                _SameUnAdjY = true;

                _UnAdjZ = null;
                _SameUnAdjZ = true;

                if (ddAvailable)
                {
                    foreach (string id in _ExtendedDataSame.Keys)
                        _ExtendedData[id] = null;
                }
            }

            OnlyTravTypes = HasTravTypes && !HasGpsTypes && !HasQndms;
            OnlyManAccTypes = !HasTravTypes && (HasGpsTypes || HasQndms);
            OnlyGpsTypes = !HasTravTypes && HasGpsTypes && !HasQndms;
            OnlyQuondams = !HasTravTypes && !HasGpsTypes && HasQndms;

            OnPropertyChanged(
                    nameof(HasSelection),
                    nameof(SelectedPoint),
                    nameof(SelectedPoints),
                    nameof(MultipleSelections),
                    nameof(OnlyGpsTypes),
                    nameof(OnlyTravTypes),
                    nameof(OnlyManAccTypes),
                    nameof(OnlyQuondams),
                    nameof(HasPossibleCorridor),
                    nameof(ConvertTypeHeader),
                    nameof(ReverseTypeHeader),
                    nameof(PID),
                    nameof(SamePID),
                    nameof(Index),
                    nameof(SameIndex),
                    nameof(Polygon),
                    nameof(SamePolygon),
                    nameof(Metadata),
                    nameof(SameMetadata),
                    nameof(Group),
                    nameof(SameGroup),
                    nameof(OnBoundary),
                    nameof(SameOnBound),
                    nameof(UnAdjX),
                    nameof(SameUnAdjX),
                    nameof(UnAdjY),
                    nameof(SameUnAdjY),
                    nameof(UnAdjZ),
                    nameof(SameUnAdjZ),
                    nameof(ManAcc),
                    nameof(SameManAcc),
                    nameof(FwdAz),
                    nameof(SameFwdAz),
                    nameof(BkAz),
                    nameof(SameBkAz),
                    nameof(SlpAng),
                    nameof(SameSlpAng),
                    nameof(SlpDist),
                    nameof(SameSlpDist),
                    nameof(ParentPoint),
                    nameof(SameParentPoint),
                    nameof(Comment),
                    nameof(SameComment)
                );
            
            settingFields = false;
        }

        private bool Filter(object obj)
        {
            TtPoint point = obj as TtPoint;
            
            if (!_CheckedPolygons[point.PolygonCN])
                return false;

            if (!_CheckedMetadata[point.MetadataCN])
                return false;

            if (!_CheckedGroups[point.GroupCN])
                return false;

            if (!_CheckedOpTypes[point.OpType])
                return false;

            if (IsOnBnd != null && IsOnBnd != point.OnBoundary)
                return false;

            if (HasLinks != null && HasLinks != point.HasQuondamLinks)
                return false;

            return true;
        }


        #region Actions
        private void ChangeQuondamParent()
        {
            if (!MultipleSelections || MessageBox.Show("Multiple Quondams are selected. Do you wish to change all of their ParentPoints to a new Point?",
                    "Multiple ParentPoints Changing", MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                SelectPointDialog spd = new SelectPointDialog(Manager,
                    !MultipleSelections && (SelectedPoint is QuondamPoint sp) ? sp.ParentPoint : null,
                    new List<TtPoint>(SelectedPoints.Cast<TtPoint>()))
                {
                    Owner = Project.MainModel.MainWindow
                };

                if (spd.ShowDialog() == true)
                {
                    TtPoint point = spd.SelectedPoint;

                    if (point.OpType == OpType.Quondam)
                        point = ((QuondamPoint)point).ParentPoint;

                    if (MultipleSelections)
                    {
                        Manager.EditPoints(SelectedPoints.Cast<TtPoint>(), PointProperties.PARENT_POINT, point);
                    }
                    else
                    {
                        Manager.EditPoint(SelectedPoint, PointProperties.PARENT_POINT, point);
                    }
                }
            }
        }


        private void RenamePoints()
        {
            RenamePointsDialog dialog = new RenamePointsDialog(Manager)
            {
                Owner = Project.MainModel.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                Manager.EditPointsMultiValues(
                    GetSortedSelectedPoints(),
                    PointProperties.PID,
                    Enumerable.Range(0, SelectedPoints.Count)
                        .Select(i => (dialog.StartIndex + dialog.Increment * i))
                );
            }
        }

        private void ReverseSelection()
        {
            if (MultipleSelections && SamePolygon)
            {
                List<TtPoint> points = GetSortedSelectedPoints();

                List<int> indexes = new List<int>();
                List<TtPoint> updatedPoints = new List<TtPoint>();
                
                TtPoint tmpPoint1, tmpPoint2;
                for (int i = 0; i < points.Count / 2; i ++)
                {
                    tmpPoint1 = points[i];
                    tmpPoint2 = points[points.Count - 1 - i];

                    if (tmpPoint1.Index != tmpPoint2.Index)
                    {
                        updatedPoints.Add(tmpPoint1);
                        indexes.Add(tmpPoint2.Index);

                        updatedPoints.Add(tmpPoint2);
                        indexes.Add(tmpPoint1.Index);
                    }
                }

                if (updatedPoints.Count > 0)
                {
                    Manager.EditPointsMultiValues(updatedPoints, PointProperties.INDEX, indexes);
                }

                Points.Refresh();
            }
        }

        private void ResetPoint()
        {
            Func<TtPoint, bool> hasConflict = (currPoint) =>
            {
                TtPoint origPoint = Manager.BaseManager.GetOriginalPoint(currPoint.CN);
                return currPoint.Index != origPoint.Index || currPoint.PolygonCN != origPoint.PolygonCN;
            };

            if (HasSelection)
            {
                if (MultipleSelections)
                {
                    TtPoint[] points = SelectedPoints.Cast<TtPoint>().Where(p => Manager.BaseManager.HasOriginalPoint(p.CN)).ToArray();

                    if (points.Any(hasConflict))
                    {
                        MessageBoxResult result = MessageBox.Show("One or more of the points has a different Index or is in a different Polygon and resetting those points may cause issues "+
                            "to the order of the points within polygons. Would you like to exclude the index and polygon from the resetting process to " +
                            "mitigate ordering problems?",
                            "Point Index and Polygon Conflicts", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            Manager.ResetPoints(points, true);
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            Manager.ResetPoints(points, false);
                        }
                    }
                    else
                    {
                        Manager.ResetPoints(points);
                    }
                }
                else
                {
                    if (hasConflict(SelectedPoint))
                    {
                        MessageBoxResult result = MessageBox.Show("The selected point has a different Index or is in a different Polygon and resetting this point may cause issues " +
                            "to the order of the points within polygons. Would you like to exclude the index and polygon from the resetting process to " +
                            "mitigate ordering problems?",
                            "Point Index and Polygon Conflict", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            Manager.ResetPoint(SelectedPoint, true);
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            Manager.ResetPoint(SelectedPoint, false);
                        }
                    }
                    else
                    {
                        Manager.ResetPoint(SelectedPoint);
                    }
                }
            }
        }

        private void ResetPointField(DataGrid dataGrid)
        {
            if (dataGrid != null)
            {
                int columnIndex = SelectedColumnIndex;
                
                if (dataGrid.Columns[columnIndex] is DataGridTextColumn dgtc && dgtc.Binding is Binding binding && binding.Path != null)
                {
                    if (columnIndex < 26)
                    {
                        TtPoint point = dataGrid.SelectedCells[columnIndex].Item as TtPoint;

                        Type type = dataGrid.SelectedCells[columnIndex].Item?.GetType();
                        PropertyInfo pi = null;
                        
                        bool isGps = false, isTrav = false, isManAcc = false, isNonResetable = false;

                        switch (binding.Path.Path)
                        {
                            case nameof(TtPoint.PID):
                            case nameof(TtPoint.Group):
                            case nameof(TtPoint.Metadata):
                            case nameof(TtPoint.Comment):
                            case nameof(TtPoint.OnBoundary):
                            case nameof(TtPoint.UnAdjX):
                            case nameof(TtPoint.UnAdjY):
                            case nameof(TtPoint.UnAdjZ):
                                pi = typeof(TtPoint).GetProperty(binding.Path.Path);
                                break;
                            case nameof(GpsPoint.Latitude):
                            case nameof(GpsPoint.Longitude):
                            case nameof(GpsPoint.Elevation):
                                pi = typeof(GpsPoint).GetProperty(binding.Path.Path);
                                isGps = true;
                                break;
                            case nameof(TravPoint.FwdAzimuth):
                            case nameof(TravPoint.BkAzimuth):
                            case nameof(TravPoint.SlopeDistance):
                            case nameof(TravPoint.SlopeAngle):
                                pi = typeof(TravPoint).GetProperty(binding.Path.Path);
                                isTrav = true;
                                break;
                            case nameof(IManualAccuracy.ManualAccuracy):
                                pi = typeof(IManualAccuracy).GetProperty(binding.Path.Path);
                                isManAcc = true;
                                break;
                            case nameof(TtPoint.Index):
                            case nameof(TtPoint.Polygon):
                            case nameof(TtPoint.OpType):
                            case nameof(TtPoint.TimeCreated):
                            case nameof(TtPoint.AdjX):
                            case nameof(TtPoint.AdjY):
                            case nameof(TtPoint.AdjZ):
                            case nameof(TtPoint.Accuracy):
                            case nameof(TtPoint.HasQuondamLinks):
                            case nameof(TtPoint.LinkedPoints):
                            case nameof(TtPoint.ExtendedData):
                            case nameof(QuondamPoint.ParentPoint):
                                isNonResetable = true;
                                break;
                            default:
                                break;
                        }
                        
                        IEnumerable<TtPoint> points = null;

                        if (isNonResetable)
                        {
                            MessageBox.Show($"{pi.Name} is not a resetable field.");
                            return;
                        }
                        else if (pi == null)
                        {
                            MessageBox.Show("Invalid field selected.");
                            return;
                        }
                        else if (isGps)
                        {
                            points = GetSortedSelectedPoints().Where(p => p.IsGpsType());
                        }
                        else if (isTrav)
                        {
                            points = GetSortedSelectedPoints().Where(p => p.IsTravType());
                        }
                        else if (isManAcc)
                        {
                            points = GetSortedSelectedPoints().Where(p => p.IsManualAccType());
                        }
                        else
                        {
                            points = GetSortedSelectedPoints();
                        }

                        if (MessageBox.Show($"{(MultipleSelections ? $"{ SelectedPoints.Count } Points" : $"Point { SelectedPoint.PID }")} will have field { pi.Name } reset.",
                            "Reset Field", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
                        {
                            if (MultipleSelections)
                            {
                                points = points.Where(p => Manager.BaseManager.HasOriginalPoint(p.CN));

                                if (points.Any())
                                    Manager.EditPointsMultiValues(points, pi, points.Select(p => Manager.BaseManager.GetOriginalPoint(p.CN)));
                            }
                            else
                            {
                                if (Manager.BaseManager.HasOriginalPoint(point.CN))
                                    Manager.EditPoint(point, pi, pi.GetValue(Manager.BaseManager.GetOriginalPoint(point.CN)));
                                else
                                    MessageBox.Show("Point is not resetable due to being recently created.");
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Datadictionary must first be implemented");
                        ////TODO once datadictionay is implemented
                        //DataDictionaryTemplate template = Manager.GetDataDictionaryTemplate();

                        //DataDictionaryField field = template[binding.Path.Path.Substring(14, 36)];

                        //if (MessageBox.Show($"{(MultipleSelections ? $"{ SelectedPoints.Count } Points" : $"Point { SelectedPoint.PID }")} will have field { field.Name } reset.",
                        //    "Reset Field", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
                        //{

                        //}
                    }
                }
            }
        }

        private void DeletePoint()
        {
            if (Project.Settings.DeviceSettings.DeletePointWarning)
            {
                if (MultipleSelections)
                    Manager.DeletePoints(SelectedPoints.Cast<TtPoint>());
                else
                {
                    TtPoint point = SelectedPoints.Cast<TtPoint>().First();
                    if (point.OpType == OpType.Quondam)
                    {

                    }
                    else if (point.OpType != OpType.SideShot)
                    {
                        TtPoint next = Manager.GetNextPoint(point);
                        if (next != null && next.IsTravType())
                        {

                        }
                    }

                    Manager.DeletePoint(point);
                }
            }
            else
            {
                if (MultipleSelections)
                    Manager.DeletePoints(SelectedPoints.Cast<TtPoint>());
                else
                    Manager.DeletePoint(SelectedPoints.Cast<TtPoint>().First());
            }
        }
        

        private void CreateNewPoint(OpType optype)
        {
            if (Manager.Polygons.Count > 0)
            {
                switch (optype)
                {
                    case OpType.GPS:
                    case OpType.Traverse:
                    case OpType.SideShot:
                    case OpType.WayPoint:
                        {
                            if (CreateGpsPointDialog.ShowDialog(Manager, null, optype, Project.MainModel.MainWindow) == true)
                                Manager.BaseManager.UpdateDataAction(DataActionType.ManualPointCreation);
                            break;
                        }
                    case OpType.Quondam: Retrace(); break;
                }
            }
            else
            {
                MessageBox.Show("Must create Polygon before creating Points.", "No Polygon in Project");
            }
        }

        private void CreateQuondams()
        {
            Project.MainModel.MainWindow.IsEnabled = false;
            PointLocManipDialog.Show(Manager, GetSortedSelectedPoints(), true, false, SessionData.TargetPolygon, Project.MainModel.MainWindow,
                (target) =>
                {
                    Project.MainModel.MainWindow.IsEnabled = true;
                    SessionData.TargetPolygon = target;
                });
        }

        private void ConvertPoints()
        {
            if (OnlyQuondams)
                ConvertQuondams();
            else if (OnlyTravTypes)
                ConvertTravTypes();
        }

        private void ConvertQuondams()
        {
            GpsPoint convertPoint(QuondamPoint q)
            {
                GpsPoint gps = new GpsPoint(q)
                {
                    Comment = string.IsNullOrWhiteSpace(q.Comment) ? q.ParentPoint.Comment : q.Comment,
                    TimeCreated = DateTime.Now
                };

                gps.SetAccuracy(q.Polygon.Accuracy);

                return gps;
            }

            if (MultipleSelections)
                Manager.ReplacePoints(GetSortedSelectedPoints().Select(p => convertPoint(p as QuondamPoint)));
            else
                Manager.ReplacePoint(convertPoint(SelectedPoint as QuondamPoint));

            Manager.BaseManager.UpdateDataAction(DataActionType.ConvertPoints);
        }

        private void ConvertTravTypes()
        {
            TtPoint convertToTrav(TtPoint ss)
            {
                return new TravPoint(ss);
            }

            TtPoint convertToSS(TtPoint trav)
            {
                return new SideShotPoint(trav);
            }


            if (MultipleSelections)
            {
                List<TtPoint> points = GetSortedSelectedPoints();

                if (points[0].OpType == OpType.Traverse)
                    Manager.ReplacePoints(points.Where(pt => pt.OpType == OpType.Traverse).Select(p => convertToSS(p)));
                else
                    Manager.ReplacePoints(points.Where(pt => pt.OpType == OpType.SideShot).Select(p => convertToTrav(p)));
            }
            else
            {
                if (SelectedPoint.OpType == OpType.Traverse)
                    Manager.ReplacePoint(convertToSS(SelectedPoint));
                else
                    Manager.ReplacePoint(convertToTrav(SelectedPoint));
            }

            Manager.BaseManager.UpdateDataAction(DataActionType.ConvertPoints);
        }

        private void MovePoints()
        {
            Project.MainModel.MainWindow.IsEnabled = false;
            PointLocManipDialog.Show(Manager, GetSortedSelectedPoints(), false, false, SessionData.TargetPolygon, Project.MainModel.MainWindow,
                (target) =>
                {
                    Project.MainModel.MainWindow.IsEnabled = true;
                    SessionData.TargetPolygon = target;
                });
        }

        private void Retrace()
        {
            Project.MainModel.MainWindow.IsEnabled = false;
            RetraceDialog.Show(Manager, Project.MainModel.MainWindow, (result) =>
            {
                Project.MainModel.MainWindow.IsEnabled = true;
                if (result == true)
                    Manager.BaseManager.UpdateDataAction(DataActionType.RetracePoints);
            });
        }

        private void Reindex()
        {
            Project.MainModel.MainWindow.IsEnabled = false;
            ReindexDialog.Show(Manager, Project.MainModel.MainWindow, (result) =>
            {
                Project.MainModel.MainWindow.IsEnabled = true;
            });
        }

        private void SwapPoints()
        {
            Manager.EditPointsMultiValues(SelectedPoints.Cast<TtPoint>().Take(2), PointProperties.INDEX,
                new int[] { (SelectedPoints[1] as TtPoint).Index, (SelectedPoints[0] as TtPoint).Index });
        }

        private void CreatePlots()
        {
            if (Manager.Polygons.Any(poly => Manager.GetPoints(poly.CN).HasAtLeast(3, p => p.IsBndPoint())))
            {
                //Project.MainModel.MainWindow.IsEnabled = false;
                PlotToolInUse = true;
                CreatePlotsDialog.Show(Project, Project.MainModel.MainWindow, () =>
                {
                    PlotToolInUse = false;
                    Project.MainModel.MainWindow.Activate();
                });//, () => Project.MainModel.MainWindow.IsEnabled = true);
            }
            else
            {
                MessageBox.Show("No Valid Polygons Found");
            }
        }

        private void CreateSubsample()
        {
            if (Manager.Polygons.Any(poly => Manager.GetPoints(poly.CN).All(p => p.IsWayPointAtBase())))
            {
                PlotToolInUse = true;
                CreateSubsetDialog.Show(Project, Project.MainModel.MainWindow, () =>
                {
                    PlotToolInUse = false;
                    Project.MainModel.MainWindow.Activate();
                });
            }
            else
            {
                MessageBox.Show("No Plot Polygons Found");
            }
        }

        private void CreateCorridor()
        {
            Manager.CreateCorridor(GetSortedSelectedPoints().Where(p => p.OnBoundary), Polygon);
        }

        private void IsPointInPolygon()
        {
            //TODO
        }

        private void GetDistanceToPolygonEdge()
        {
            //TODO
        }

        private void ModifyDataDictionary()
        {
            if (Project.RequiresSave)
            {
                if (MessageBox.Show("This project has unsaved changes. In order to modify the DataDictionary all data must first be saved. Would you like to save now?",
                    "Project Needs Saving", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Yes)
                {
                    Project.Save();
                }
                else
                    return;
            }

            Project.MainModel.MainWindow.IsEnabled = false;
            DataDictionaryEditorDialog.ShowDialog(Project, Project.MainModel.MainWindow, (result) =>
            {
                Project.MainModel.MainWindow.IsEnabled = true;

                if (result == true)
                {
                    Manager.Save();
                    Manager.BaseManager.Reload();
                    
                    Points = CollectionViewSource.GetDefaultView(Manager.Points) as ListCollectionView;

                    SetupUI();
                }
            });
        }


        private void ExportValues(DataGrid dataGrid)
        {
            if (HasSelection)
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    FileName = "SelectedPoints",
                    DefaultExt = ".csv",
                    Filter = "CSV Document (*.csv)|*.csv|All Types (*.*)|*.*"
                };

                if (sfd.ShowDialog() == true)
                {
                    Export.CheckCreateFolder(Path.GetDirectoryName(sfd.FileName));
                    Export.Points(GetSortedSelectedPoints(), sfd.FileName);
                }
            }
        }

        private void CopyCellValue(DataGrid dataGrid)
        {
            if (dataGrid != null)
            {
                var cellInfo = dataGrid.SelectedCells[dataGrid.CurrentCell.Column.DisplayIndex];

                if (cellInfo.Column.GetCellContent(cellInfo.Item) is TextBlock tb)
                    Clipboard.SetText(tb.Text);
            }
        }

        private void CopyColumnValues(string binding)
        {
            StringBuilder clipData = new StringBuilder();

            PropertyInfo propInfo = PointProperties.GetPropertyByName(binding);

            bool isManAcc = false;
            string ddProp = null;
            Type pointType = PointProperties.GetPointTypeByPropertyName(binding);

            if (propInfo == null)
            {
                isManAcc = binding == "ManualAccuracy";
                ddProp = binding.Replace("ExtendedData.[", "").Replace("]", "");
                if (!isManAcc && !_ExtendedData.HasField(ddProp))
                {
                    MessageBox.Show("Unable to Copy Values");
                    return;
                }
            }

            foreach (TtPoint point in VisiblePoints)
            {
                if (propInfo != null)
                {
                    if (pointType.IsAssignableFrom(point.GetType()))
                        clipData.AppendLine(propInfo.GetValue(point).ToString());
                    else
                        clipData.AppendLine(String.Empty);
                }
                else if (isManAcc)
                {
                    if (point is IManualAccuracy iManAcc)
                        clipData.AppendLine(iManAcc.ManualAccuracy.ToString());
                    else
                        clipData.AppendLine(String.Empty);
                }
                else if (ddProp != null)
                {
                    if (point.ExtendedData != null && point.ExtendedData.HasField(ddProp))
                        clipData.AppendLine(point.ExtendedData[ddProp].ToString());
                    else
                        clipData.AppendLine(String.Empty);
                }
                else
                {
                    clipData.AppendLine(String.Empty);
                }
            }

            Clipboard.SetText(clipData.ToString());
        }

        private void ViewPointDetails()
        {
            PointDetailsDialog.ShowDialog(SelectedPoints.Cast<TtPoint>().ToList(), Project.MainModel.MainWindow);
        }

        private void ViewSafeInfo()
        {
            List<TtPoint> points = GetSortedSelectedPoints(pt => pt.IsGpsType());

            if (points.Count > 0)
                ViewSatInfoDialog.ShowDialog(GetSortedSelectedPoints(), Manager, Project.MainModel.MainWindow);
            else
                MessageBox.Show("No GPS type points selected");
        }
        #endregion

        #region Selections
        private List<TtPoint> GetEditSelectionPoints()
        {
            List<TtPoint> points;

            if (SelectedPoints.Count > 1)
            {
                points = new List<TtPoint>(SelectedPoints.Cast<TtPoint>());
                points.Sort();
            }
            else
                points = new List<TtPoint>(VisiblePoints.Cast<TtPoint>());

            return points;
        }

        private void DeselectAll()
        {
            SelectedPoints.Clear();
        }

        private void SelectAlternate()
        {
            List<TtPoint> points = GetEditSelectionPoints();

            ignoreSelectionChange = true;

            SelectedPoints.Clear();

            bool selected = true;
            foreach (TtPoint point in points)
            {
                if (selected)
                    SelectedPoints.Add(point);

                selected = !selected;
            }

            ignoreSelectionChange = false;

            OnSelectionChanged();
        }

        private void SelectGps()
        {
            List<TtPoint> points = GetEditSelectionPoints();

            ignoreSelectionChange = true;

            SelectedPoints.Clear();
            
            foreach (TtPoint point in points)
            {
                if (point.IsGpsType())
                    SelectedPoints.Add(point);
            }

            ignoreSelectionChange = false;

            OnSelectionChanged();
        }

        private void SelectTraverse()
        {
            List<TtPoint> points = GetEditSelectionPoints();

            ignoreSelectionChange = true;

            SelectedPoints.Clear();

            foreach (TtPoint point in points)
            {
                if (point.IsTravType())
                    SelectedPoints.Add(point);
            }

            ignoreSelectionChange = false;

            OnSelectionChanged();
        }

        private void SelectInverse()
        {
            List<TtPoint> selected = new List<TtPoint>(SelectedPoints.Cast<TtPoint>());
            List<TtPoint> visible = new List<TtPoint>(VisiblePoints.Cast<TtPoint>());

            ignoreSelectionChange = true;

            SelectedPoints.Clear();

            foreach (TtPoint point in visible)
            {
                if (!selected.Contains(point))
                    SelectedPoints.Add(point);
            }

            ignoreSelectionChange = false;

            OnSelectionChanged();
        }
        #endregion
        

        private Tuple<DataGridTextColumn, string>[] CreateDefaultColumns()
        {
            string defaultNumberFormat = "{0:0.###}";
            string defaultNumberLongFormat = "{0:0.#####}";

            return new Tuple<DataGridTextColumn, string>[]
            {
                CreateDataGridTextColumn(nameof(TtPoint.Index)),
                CreateDataGridTextColumn(nameof(TtPoint.PID)),
                CreateDataGridTextColumn(nameof(TtPoint.OpType)),
                CreateDataGridTextColumn(nameof(TtPoint.Polygon), "Polygon.Name", 100),
                CreateDataGridTextColumn("OnBound", nameof(TtPoint.OnBoundary)),
                CreateDataGridTextColumn(nameof(TtPoint.AdjX), stringFormat: defaultNumberFormat),
                CreateDataGridTextColumn(nameof(TtPoint.AdjY), stringFormat: defaultNumberFormat),
                CreateDataGridTextColumn("AdjZ (M)", nameof(TtPoint.AdjZ), stringFormat: defaultNumberFormat),
                CreateDataGridTextColumn("Acc (M)", nameof(TtPoint.Accuracy), stringFormat: defaultNumberFormat),
                CreateDataGridTextColumn(nameof(TtPoint.UnAdjX), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn(nameof(TtPoint.UnAdjY), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn("UnAdjZ (M)", nameof(TtPoint.UnAdjZ), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),
                
                CreateDataGridTextColumn(nameof(GpsPoint.Latitude), stringFormat: defaultNumberLongFormat, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn(nameof(GpsPoint.Longitude), stringFormat: defaultNumberLongFormat, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn("Elev", nameof(GpsPoint.Elevation), stringFormat: defaultNumberFormat,
                    converter: new MetadataValueConverter() { Metadata = Manager.DefaultMetadata, MetadataPropertyName = MetadataPropertyName.Elevation }, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn(nameof(IManualAccuracy.ManualAccuracy), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),

                CreateDataGridTextColumn("Fwd Az", nameof(TravPoint.FwdAzimuth), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn("Back Az", nameof(TravPoint.BkAzimuth), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn("Azimuth", nameof(TravPoint.Azimuth), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn("Slp Dist", nameof(TravPoint.SlopeDistance), stringFormat: defaultNumberFormat,
                    converter: new MetadataValueConverter() { Metadata = Manager.DefaultMetadata, MetadataPropertyName = MetadataPropertyName.Distance }, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn("Slp Ang", nameof(TravPoint.SlopeAngle), stringFormat: defaultNumberFormat,
                    converter: new MetadataValueConverter() { Metadata = Manager.DefaultMetadata, MetadataPropertyName = MetadataPropertyName.SlopeAngle }, visibility: Visibility.Collapsed),
                CreateDataGridTextColumn("Horiz Dist", nameof(TravPoint.HorizontalDistance), stringFormat: defaultNumberFormat,
                    converter: new MetadataValueConverter() { Metadata = Manager.DefaultMetadata, MetadataPropertyName = MetadataPropertyName.Distance }, visibility: Visibility.Collapsed),

                CreateDataGridTextColumn("Parent", nameof(QuondamPoint.ParentPoint), stringFormat: defaultNumberFormat, visibility: Visibility.Collapsed),

                CreateDataGridTextColumn("QndmLink", nameof(TtPoint.HasQuondamLinks)),
                CreateDataGridTextColumn("Created", nameof(TtPoint.TimeCreated), visibility: Visibility.Collapsed),
                CreateDataGridTextColumn(nameof(TtPoint.Comment), visibility: Visibility.Collapsed)
            };
        }

        private Tuple<DataGridTextColumn, string> CreateDataGridTextColumn(String name, string binding = null,
            int? width = null, string stringFormat = null, IValueConverter converter = null, Visibility visibility = Visibility.Visible)
        {
            DataGridTextColumn dataGridTextColumn = new DataGridTextColumn()
            {
                IsReadOnly = true,
                Width = width ?? DataGridLength.Auto,
                Visibility = visibility,
                Binding = new Binding(binding ?? name) { StringFormat = stringFormat, Converter = converter }
            };

            dataGridTextColumn.Header = new ColumnHeader()
            {
                Column = dataGridTextColumn,
                Name = name,
                HideColumn = new RelayCommand(x => dataGridTextColumn.Visibility = Visibility.Collapsed),
                CopyColumnValues = new RelayCommand(x => CopyColumnValues(binding ?? name)),
                VisibleFields = VisibleFields
            };

            return Tuple.Create(dataGridTextColumn, name);
        }


        private bool IsDefaultField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(TtPoint.Index):
                case nameof(TtPoint.PID):
                case nameof(TtPoint.OpType):
                case nameof(TtPoint.Polygon):
                case "OnBound":
                case nameof(TtPoint.AdjX):
                case nameof(TtPoint.AdjY):
                case "AdjZ (M)":
                case "Acc (M)":
                case "QndmLink":
                    return true;
            }

            return false;
        }

        public bool IsExtendedField(string fieldName)
        {
            return _ExtendedDataFields.Any(f => f.Name == fieldName);
        }

        public bool IsDefaultOrExtendedField(string fieldName)
        {
            return IsDefaultField(fieldName) || IsExtendedField(fieldName);
        }


        public class ColumnHeader
        {
            public DataGridTextColumn Column { get; set; }
            public String Name { get; set; }
            public ICommand HideColumn { get; set; }
            public ICommand CopyColumnValues { get; set; }
            public ObservableCollection<Control> VisibleFields { get; set; }
        }
    }
}
