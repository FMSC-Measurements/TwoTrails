﻿using FMSC.Core.Utilities;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;
using TwoTrails.DAL;

namespace TwoTrails.Core
{
    public class TtManager : IObservableTtManager
    {
        private const long POLYGON_UPDATE_DELAY = 1000;

        private readonly ITtSettings _Settings;
        private ITtDataLayer _DAL;
        private ITtMediaLayer _MAL;

        private readonly TtUserAction _Activity;

        private Dictionary<String, TtPoint> _PointsMap, _PointsMapOrig;
        private Dictionary<String, List<TtPoint>> _PointsByPoly;
        private Dictionary<String, TtPolygon> _PolygonsMap, _PolygonsMapOrig;
        private Dictionary<String, TtMetadata> _MetadataMap, _MetadataMapOrig;
        private Dictionary<String, TtGroup> _GroupsMap, _GroupsMapOrig;
        private Dictionary<String, PolygonGraphicOptions> _PolyGraphicOpts, _PolyGraphicOptsOrig;
        private Dictionary<String, TtNmeaBurst> _AddNmea;
        private List<String> _DeleteNmeaByPointCNs;
        private Dictionary<String, TtMediaInfo> _MediaMap, _MediaMapOrig;

        private Dictionary<Guid, Tuple<DataActionType, string>> _DataActions = new Dictionary<Guid, Tuple<DataActionType, string>>();

        private Dictionary<String, DelayActionHandler> _PolygonAPUpdateHandlers, _PolygonAccUpdateHandlers;

        private ObservableCollection<TtPoint> _Points;
        private ObservableCollection<TtPolygon> _Polygons;
        private ObservableCollection<TtMetadata> _Metadata;
        private ObservableCollection<TtGroup> _Groups;
        private ObservableCollection<TtMediaInfo> _MediaInfo;

        public ReadOnlyObservableCollection<TtPoint> Points { get; private set; }
        public ReadOnlyObservableCollection<TtPolygon> Polygons { get; private set; }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get; private set; }
        public ReadOnlyObservableCollection<TtGroup> Groups { get; private set; }
        public ReadOnlyObservableCollection<TtMediaInfo> MediaInfo { get; private set; }


        public bool HasDataDictionary => _DAL.HasDataDictionary;


        public TtGroup MainGroup { get; private set; }

        public TtMetadata DefaultMetadata { get; private set; }

        public bool IgnorePointEvents { get; protected set; } = true;

        public int PolygonCount => _Polygons.Count;
        public int PointCount => _Points.Count;


        private readonly object locker = new object();


        public TtManager(ITtDataLayer dal, ITtMediaLayer mal, ITtSettings settings)
        {
            _DAL = dal;
            _MAL = mal;
            _Settings = settings;

            _Activity = new TtUserAction(_Settings.UserName, _Settings.DeviceName, $"PC: {_Settings.AppVersion}");

            Load();
            LoadMedia();

            IgnorePointEvents = false;
            RecalculatePolygons();
        }

        public void ReplaceDAL(ITtDataLayer dal)
        {
            _DAL = dal;
        }

        public void ReplaceMAL(ITtMediaLayer mal)
        {
            _MAL = mal;
        }


        #region Load & Attaching
        private void Load()
        {
            _PointsMap = new Dictionary<string, TtPoint>();
            _PointsMapOrig = new Dictionary<string, TtPoint>();
            _PointsByPoly = new Dictionary<string, List<TtPoint>>();

            _PolygonsMap = new Dictionary<string, TtPolygon>();
            _PolygonsMapOrig = new Dictionary<string, TtPolygon>();

            _MetadataMap = new Dictionary<string, TtMetadata>();
            _MetadataMapOrig = new Dictionary<string, TtMetadata>();

            _GroupsMap = _DAL.GetGroups().ToDictionary(g => g.CN, g => g);
            _GroupsMapOrig = _GroupsMap.Values.ToDictionary(g => g.CN, g => new TtGroup(g));

            _PolyGraphicOpts = _DAL.GetPolygonGraphicOptions().Select(
                p =>
                {
                    p.AdjWidth = _Settings.PolygonGraphicSettings.AdjWidth;
                    p.UnAdjWidth = _Settings.PolygonGraphicSettings.UnAdjWidth;
                    return p;
                }).ToDictionary(p => p.CN, p => p);
            _PolyGraphicOptsOrig = _PolyGraphicOpts.Values.ToDictionary(p => p.CN, p => new PolygonGraphicOptions(p.CN, p));

            _PolygonAPUpdateHandlers = new Dictionary<string, DelayActionHandler>();
            _PolygonAccUpdateHandlers = new Dictionary<string, DelayActionHandler>();

            _AddNmea = new Dictionary<string, TtNmeaBurst>();
            _DeleteNmeaByPointCNs = new List<string>();

            MainGroup = null;
            DefaultMetadata = null;

            if (_GroupsMap.Count < 1)
            {
                TtGroup mg = _Settings.CreateDefaultGroup();
                _GroupsMap.Add(mg.CN, mg);
            }

            MainGroup = _GroupsMap[Consts.EmptyGuid];


            foreach (TtMetadata meta in _DAL.GetMetadata())
            {
                _MetadataMap.Add(meta.CN, meta);
                _MetadataMapOrig.Add(meta.CN, new TtMetadata(meta));

                AttachMetadataEvents(meta);

                if (meta.CN == Consts.EmptyGuid)
                {
                    DefaultMetadata = meta;
                }
            }

            if (DefaultMetadata == null)
            {
                DefaultMetadata = _Settings.MetadataSettings.CreateDefaultMetadata();
                _MetadataMap.Add(DefaultMetadata.CN, DefaultMetadata);
                AttachMetadataEvents(DefaultMetadata);
            }

            foreach (TtPolygon poly in _DAL.GetPolygons())
            {
                _PolygonsMap.Add(poly.CN, poly);
                _PolygonsMapOrig.Add(poly.CN, new TtPolygon(poly));

                _PointsByPoly.Add(poly.CN, _DAL.GetPoints(poly.CN).ToList());

                AttachPolygonEvents(poly);
            }

            List<TtPoint> points = _PointsByPoly.Values.SelectMany(l => l).ToList();

            _Points = new ObservableCollection<TtPoint>();

            foreach (TtPoint point in points.Where(p => p.OpType != OpType.Quondam))
            {
                _PointsMap.Add(point.CN, point);
                _PointsMapOrig.Add(point.CN, point.DeepCopy());

                _Points.Add(point);

                AttachPoint(point);
            }

            foreach (QuondamPoint qp in points.Where(p => p.OpType == OpType.Quondam))
            {
                if (_PointsMap.ContainsKey(qp.ParentPointCN))
                {
                    qp.ParentPoint = _PointsMap[qp.ParentPointCN]; 
                }
                else
                {
                    throw new Exception("Foreign Quondam");
                }

                _PointsMap.Add(qp.CN, qp);
                _PointsMapOrig.Add(qp.CN, qp.DeepCopy());

                _Points.Add(qp);

                AttachPoint(qp);
            }

            foreach (TtPolygon poly in _PolygonsMap.Values.Where(p => p.ParentUnitCN != null))
            {
                if (_PolygonsMap.ContainsKey(poly.ParentUnitCN))
                {
                    poly.ParentUnit = _PolygonsMap[poly.ParentUnitCN];
                }
                else
                {
                    throw new Exception("Foreign Polygon Parent Unit");
                }
            }

            Points = new ReadOnlyObservableCollection<TtPoint>(_Points);

            _Polygons = new ObservableCollection<TtPolygon>(_PolygonsMap.Values);
            Polygons = new ReadOnlyObservableCollection<TtPolygon>(_Polygons);

            _Metadata = new ObservableCollection<TtMetadata>(_MetadataMap.Values);
            Metadata = new ReadOnlyObservableCollection<TtMetadata>(_Metadata);

            _Groups = new ObservableCollection<TtGroup>(_GroupsMap.Values);
            Groups = new ReadOnlyObservableCollection<TtGroup>(_Groups);

            foreach (TtPolygon poly in _Polygons)
                GeneratePolygonStats(poly);
        }

        private void LoadMedia()
        {
            if (_MAL != null)
            {
                _MediaMap = new Dictionary<string, TtMediaInfo>();
                _MediaMapOrig = new Dictionary<string, TtMediaInfo>();

                _MediaInfo = new ObservableCollection<TtMediaInfo>();
                MediaInfo = new ReadOnlyObservableCollection<TtMediaInfo>(_MediaInfo);

                foreach (TtImage img in _MAL.GetImages())
                {
                    if (_MediaMap.ContainsKey(img.PointCN))
                    {
                        _MediaMap[img.PointCN].AddImage(img);
                        _MediaMapOrig[img.PointCN].AddImage(img.DeepCopy());
                    }
                    else
                    {
                        if (_PointsMap.ContainsKey(img.PointCN))
                        {
                            TtMediaInfo mi = new TtMediaInfo(_PointsMap[img.PointCN]);
                            mi.AddImage(img);
                            _MediaMap.Add(img.PointCN, mi);
                            _MediaInfo.Add(mi);

                            mi = new TtMediaInfo(_PointsMapOrig[img.PointCN]);
                            mi.AddImage(img.DeepCopy());
                            _MediaMapOrig.Add(img.PointCN, mi);
                        }
                    }
                }
            }
        }

        public void Reload()
        {
			IgnorePointEvents = true;
			
            Load();
            LoadMedia();
			
			IgnorePointEvents = false;
			RecalculatePolygons();
        }

        protected void AttachMetadataEvents(TtMetadata meta)
        {
            meta.MagDecChanged += Metadata_MagDecChanged;
            meta.ZoneChanged += Metadata_ZoneChanged;
        }

        protected void DetachMetadataEvents(TtMetadata meta)
        {
            meta.MagDecChanged -= Metadata_MagDecChanged;
            meta.ZoneChanged -= Metadata_ZoneChanged;
        }


        protected void AttachPolygonEvents(TtPolygon poly)
        {
            DelayActionHandler apStatsDAH = new DelayActionHandler(() =>
            {
                GeneratePolygonStats(poly);
            }, POLYGON_UPDATE_DELAY);

            if (!_PolygonAPUpdateHandlers.ContainsKey(poly.CN))
            {
                _PolygonAPUpdateHandlers.Add(poly.CN, apStatsDAH);
            }
            else
            {
                _PolygonAPUpdateHandlers[poly.CN].Cancel();
                _PolygonAPUpdateHandlers[poly.CN] = apStatsDAH;
            }


            DelayActionHandler accDAH = new DelayActionHandler(() =>
            {
                poly.OnPolygonAccuracyChanged();
            }, POLYGON_UPDATE_DELAY);

            if (!_PolygonAccUpdateHandlers.ContainsKey(poly.CN))
            {
                _PolygonAccUpdateHandlers.Add(poly.CN, accDAH);
            }
            else
            {
                _PolygonAccUpdateHandlers[poly.CN].Cancel();
                _PolygonAccUpdateHandlers[poly.CN] = accDAH;
            }
        }

        protected void DetachPolygonEvents(TtPolygon poly)
        {
            if (_PolygonAPUpdateHandlers.ContainsKey(poly.CN))
            {
				_PolygonAPUpdateHandlers[poly.CN].Dispose();
                _PolygonAPUpdateHandlers.Remove(poly.CN);
            }
        }


        protected void AttachPoint(TtPoint point)
        {
            if (_GroupsMap.ContainsKey(point.GroupCN))
            {
                point.Group = _GroupsMap[point.GroupCN];
            }
            else
            {
                throw new MissingDataException(nameof(TtGroup));
            }

            if (_MetadataMap.ContainsKey(point.MetadataCN))
            {
                point.Metadata = _MetadataMap[point.MetadataCN];
            }
            else
            {
                throw new MissingDataException(nameof(TtMetadata));
            }

            if (_PolygonsMap.ContainsKey(point.PolygonCN))
            {
                point.Polygon = _PolygonsMap[point.PolygonCN];
            }
            else
            {
                throw new MissingDataException(nameof(TtPolygon));
            }

            AttachPointEvents(point);
        }

        protected void AttachPointEvents(TtPoint point)
        {
            if (point.IsTravType())
            {
                ((TravPoint)point).PositionChanged += TravPoint_PositionChanged;
            }

            if (point.IsGpsType())
            {
                point.MetadataChanged += Point_MetadataChanged;
            }

            if (point.IsManualAccType())
            {
                point.OnAccuracyChanged += Point_AccuracyChanged;
            }

            point.LocationChanged += Point_LocationChanged;
            point.OnBoundaryChanged += Point_OnBoundaryChanged;
        }

        protected void DetachPointEvents(TtPoint point)
        {
            if (point.IsTravType())
            {
                ((TravPoint)point).PositionChanged -= TravPoint_PositionChanged;
            }

            if (point.IsGpsType())
            {
                point.MetadataChanged -= Point_MetadataChanged;
            }

            if (point.IsManualAccType())
            {
                point.OnAccuracyChanged -= Point_AccuracyChanged;
            }

            point.LocationChanged -= Point_LocationChanged;
            point.OnBoundaryChanged -= Point_OnBoundaryChanged;
        }
        #endregion


        #region Saving
        public bool Save()
        {
            lock (locker)
            {
                SaveGroups();
                SaveMetadata();
                SavePolygons();
                SavePoints();
                SaveNmea();
                SaveProjectInfo();

                foreach (var da in _DataActions.Values)
                {
                    _Activity.UpdateAction(da.Item1);
                    if (!String.IsNullOrWhiteSpace(da.Item2))
                    {
                        _Activity.UpdateNotes(da.Item2);
                    }
                }

                _DAL.InsertActivity(_Activity);
                _Activity.Reset();
                _DataActions.Clear();
            }

            return true;
        }

        private void SavePoints()
        {
            List<TtPoint> pointsToAdd = new List<TtPoint>();
            List<Tuple<TtPoint, TtPoint>> pointsToUpdate = new List<Tuple<TtPoint, TtPoint>>();
            TtPoint old;
            bool pointsConverted = false, pointsMoved = false, pointsIndexChanged = false;

            foreach (TtPoint point in _Points)
            {
                if (_PointsMapOrig.ContainsKey(point.CN))
                {
                    old = _PointsMapOrig[point.CN];

                    if (!point.Equals(old))
                    {
                        pointsToUpdate.Add(Tuple.Create(point, old));

                        if (old.OpType != point.OpType)
                        {
                            pointsConverted = true;
                        }

                        if (old.PolygonCN != point.PolygonCN)
                        {
                            pointsMoved = true;
                        }

                        if (old.Index != point.Index)
                        {
                            pointsIndexChanged = true;
                        }
                    }
                }
                else
                {
                    pointsToAdd.Add(point);
                }
            }

            IEnumerable<TtPoint> pointsToRemove = _PointsMapOrig.Values.Where(g => !_PointsMap.ContainsKey(g.CN)).ToList();

            if (pointsToAdd.Count > 0)
            {
                _DAL.InsertPoints(pointsToAdd);
                _Activity.UpdateAction(DataActionType.InsertedPoints);
            }

            if (pointsToUpdate.Count > 0)
            {
                _DAL.UpdatePoints(pointsToUpdate);
                _Activity.UpdateAction(DataActionType.ModifiedPoints);
            }

            if (pointsToRemove.Any())
            {
                _DAL.DeletePoints(pointsToRemove);
                _Activity.UpdateAction(DataActionType.DeletedPoints);
            }

            if (pointsConverted)
            {
                _Activity.UpdateAction(DataActionType.ConvertedPoints);
            }

            if (pointsMoved)
            {
                _Activity.UpdateAction(DataActionType.MovedPoints);
            }

            if (pointsIndexChanged)
            {
                _Activity.UpdateAction(DataActionType.ReindexPoints);
            }

            _PointsMapOrig = _Points.ToDictionary(p => p.CN, p => p.DeepCopy());
        }

        private void SaveNmea()
        {
            if (_AddNmea.Count > 0)
            {
                _DAL.InsertNmeaBursts(_AddNmea.Values);
                _Activity.UpdateAction(DataActionType.InsertedNmea);
            }

            if (_DeleteNmeaByPointCNs.Count > 0)
            {
                foreach (string pointCN in _DeleteNmeaByPointCNs)
                {
                    _DAL.DeleteNmeaBursts(pointCN);
                }
                _Activity.UpdateAction(DataActionType.DeletedNmea);
            }

            _AddNmea.Clear();
            _DeleteNmeaByPointCNs.Clear();
        }

        private void SavePolygons()
        {
            List<TtPolygon> polygonsToAdd = new List<TtPolygon>();
            List<TtPolygon> polygonsToUpdate = new List<TtPolygon>();

            foreach (TtPolygon polygon in _Polygons)
            {
                if (_PolygonsMapOrig.ContainsKey(polygon.CN))
                {
                    if (!polygon.Equals(_PolygonsMapOrig[polygon.CN]))
                    {
                        polygonsToUpdate.Add(polygon);
                    }
                }
                else
                {
                    polygonsToAdd.Add(polygon);
                }
            }

            IEnumerable<TtPolygon> polygonsToRemove = _PolygonsMapOrig.Values.Where(g => !_PolygonsMap.ContainsKey(g.CN));

            if (polygonsToAdd.Count > 0)
            {
                _DAL.InsertPolygons(polygonsToAdd);
                _Activity.UpdateAction(DataActionType.InsertedPolygons);
            }

            if (polygonsToUpdate.Count > 0)
            {
                _DAL.UpdatePolygons(polygonsToUpdate);
                _Activity.UpdateAction(DataActionType.ModifiedPolygons);
            }

            if (polygonsToRemove.Any())
            {
                _DAL.DeletePolygons(polygonsToRemove);
                _Activity.UpdateAction(DataActionType.DeletedPolygons);
            }

            _PolygonsMapOrig = _Polygons.ToDictionary(p => p.CN, p => new TtPolygon(p));
        }

        private void SaveMetadata()
        {
            List<TtMetadata> metadataToAdd = new List<TtMetadata>();
            List<TtMetadata> metadataToUpdate = new List<TtMetadata>();

            foreach (TtMetadata metadata in _Metadata)
            {
                if (_MetadataMapOrig.ContainsKey(metadata.CN))
                {
                    if (!metadata.Equals(_MetadataMapOrig[metadata.CN]))
                    {
                        metadataToUpdate.Add(metadata);
                    }
                }
                else
                {
                    metadataToAdd.Add(metadata);
                }
            }

            IEnumerable<TtMetadata> metadataToRemove = _MetadataMapOrig.Values.Where(g => !_MetadataMap.ContainsKey(g.CN));

            if (metadataToAdd.Count > 0)
            {
                _DAL.InsertMetadata(metadataToAdd);
                _Activity.UpdateAction(DataActionType.InsertedMetadata);
            }

            if (metadataToUpdate.Count > 0)
            {
                _DAL.UpdateMetadata(metadataToUpdate);
                _Activity.UpdateAction(DataActionType.ModifiedMetadata);
            }

            if (metadataToRemove.Any())
            {
                _DAL.DeleteMetadata(metadataToRemove);
                _Activity.UpdateAction(DataActionType.DeletedMetadata);
            }

            _MetadataMapOrig = _Metadata.ToDictionary(m => m.CN, m => new TtMetadata(m));
        }

        private void SaveGroups()
        {
            List<TtGroup> groupsToAdd = new List<TtGroup>();
            List<TtGroup> groupsToUpdate = new List<TtGroup>();

            foreach (TtGroup group in _Groups)
            {
                if (_GroupsMapOrig.ContainsKey(group.CN))
                {
                    if (!group.Equals(_GroupsMapOrig[group.CN]))
                    {
                        groupsToUpdate.Add(group);
                    }
                }
                else
                {
                    groupsToAdd.Add(group);
                }
            }

            IEnumerable<TtGroup> groupsToRemove = _GroupsMapOrig.Values.Where(g => !_GroupsMap.ContainsKey(g.CN));

            if (groupsToAdd.Count > 0)
            {
                _DAL.InsertGroups(groupsToAdd);
                _Activity.UpdateAction(DataActionType.InsertedGroups);
            }

            if (groupsToUpdate.Count > 0)
            {
                _DAL.UpdateGroups(groupsToUpdate);
                _Activity.UpdateAction(DataActionType.ModifiedGroups);
            }

            if (groupsToRemove.Any())
            {
                _DAL.DeleteGroups(groupsToRemove);
                _Activity.UpdateAction(DataActionType.DeletedGroups);
            }

            _GroupsMapOrig = _Groups.ToDictionary(g => g.CN, g => new TtGroup(g));
        }

        private void SaveProjectInfo()
        {

        }
        #endregion


        #region Data Changing
        private void Metadata_MagDecChanged(TtMetadata metadata)
        {
            lock (locker)
            {
                foreach (TtPolygon polygon in _Polygons)
                {
                    if (_PointsByPoly[polygon.CN].Any(p => p.MetadataCN == metadata.CN))
                    {
                        AdjustAllTravTypesInPolygon(polygon);
                        UpdatePolygonStats(polygon);
                    }
                }
            }
        }

        private void Metadata_ZoneChanged(TtMetadata metadata, int oldZone)
        {
            IgnorePointEvents = true;

            lock (locker)
            {
                Dictionary<String, TtPolygon> adjustPolygons = new Dictionary<string, TtPolygon>();

                foreach (GpsPoint point in _Points.Where(p => p.MetadataCN == metadata.CN && p.IsGpsType()))
                {
                    ChangeGpsZone(point, metadata.Zone, oldZone);

                    if (!adjustPolygons.ContainsKey(point.PolygonCN))
                        adjustPolygons.Add(point.PolygonCN, point.Polygon);
                }

                foreach (TtPolygon polygon in adjustPolygons.Values)
                {
                    AdjustAllTravTypesInPolygon(polygon);
                    UpdatePolygonStats(polygon);
                }
            }

            IgnorePointEvents = false;
        }


        private void Point_OnBoundaryChanged(TtPoint point)
        {
            if (!IgnorePointEvents)
            {
                lock (locker)
                {
                    //AdjustAroundPoint(point, _PointsByPoly[point.PolygonCN]);
                    _PolygonAPUpdateHandlers[point.PolygonCN].DelayInvoke();
                }
            }
        }

        private void Point_LocationChanged(TtPoint point)
        {
            if (!IgnorePointEvents)
            {
                lock (locker)
                {
                    if (point.IsGpsAtBase())
                        AdjustAroundGpsPoint(point);

                    UpdatePolygonStats(point.Polygon);
                }
            }
        }

        private void Point_AccuracyChanged(TtPoint point)
        {
            if (!IgnorePointEvents)
            {
                lock (locker)
                {
                    _PolygonAccUpdateHandlers[point.Polygon.CN].DelayInvoke();
                }
            }
        }

        private void Point_MetadataChanged(TtPoint point, TtMetadata newMetadata, TtMetadata oldMetadata)
        {
            if (point.IsGpsType() && oldMetadata != null && newMetadata != null && newMetadata.Zone != oldMetadata.Zone)
            {
                ChangeGpsZone(point as GpsPoint, newMetadata.Zone, oldMetadata.Zone);
            }
        }

        private void TravPoint_PositionChanged(TravPoint point)
        {
            if (!IgnorePointEvents)
            {
                lock (locker)
                {
                    if (point.OpType == OpType.Traverse)
                    {
                        AdjustTraverseFromAfterStart(point);
                    }
                    else
                    {
                        AdjustSideShot(point as SideShotPoint);
                    }
                }

                UpdatePolygonStats(point.Polygon);
            }
        }
        #endregion


        #region Adjusting
        protected void AdjustAroundPoint(TtPoint point, IList<TtPoint> points)
        {
            if (point.IsGpsAtBase())
            {
                AdjustAroundGpsPoint(point);
            }
            else if (point.OpType == OpType.SideShot)
            {
                AdjustSideShot(point as SideShotPoint, points);
            }
            else if (point.OpType == OpType.Traverse)
            {
                AdjustTraverseFromAfterStart(point);
            }
        }


        protected void AdjustAroundGpsPoint(TtPoint point)
        {
            IList<TtPoint> points = GetPoints(point.PolygonCN);

            if (points.Count > point.Index - 1)
            {
                TtPoint prev = (point.Index > 0) ? points[point.Index - 1] : null;
                TtPoint next = (point.Index < points.Count - 1) ? points[point.Index + 1] : null;

                if (prev != null && prev.OpType == OpType.Traverse)
                {
                    AdjustTraverseFromAfterStart(point, points);
                }

                if (next != null)
                {
                    if (next.OpType == OpType.Traverse)
                    {
                        AdjustTraverseFromStart(point.Index, points);
                    }
                    else if (next.OpType == OpType.SideShot)
                    {
                        AdjustSideShotsFromBasePoint(point, points);
                    }
                }
            }
            //else point may be moving and index rebuild should be called
        }


        private void AdjustSideShotsFromBasePoint(TtPoint basePoint, IList<TtPoint> points)
        {
            TtPoint tmp;
            for (int i = basePoint.Index + 1; i < points.Count; i++)
            {
                tmp = points[i];
                if (tmp.OpType != OpType.SideShot)
                    break;

                (tmp as SideShotPoint).Adjust(basePoint);
            }
        }

        protected void AdjustSideShot(SideShotPoint point, IList<TtPoint> points = null)
        {
            if (points == null)
            {
                points = GetPoints(point.PolygonCN);
            }

            if (point.Index > 0)
            {
                TtPoint prev;

                for (int i = point.Index - 1; i > -1; i--)
                {
                    prev = points[i];

                    if (prev.OpType != OpType.SideShot)
                    {
                        point.Adjust(prev);
                        break;
                    }
                }
            }
        }


        public void AdjustAllTravTypesInPolygon(TtPolygon polygon)
        {
            IgnorePointEvents = true;

            foreach (IPointSegment seg in GetAllTravTypeSegmentsInPolygon(polygon))
            {
                if (seg.IsValid)
                    seg.Adjust();
            }

            IgnorePointEvents = false;
        }

        private void AdjustTraverseFromAfterStart(TtPoint point, IList<TtPoint> points = null)
        {
            if (points == null)
            {
                points = GetPoints(point.PolygonCN);
            }

            if (point.Index < points.Count - 1 || point.IsGpsAtBase()) // make sure traverse isnt at end
            {
                for (int i = point.Index - 1; i > -1; i--)
                {
                    if (points[i].IsGpsAtBase())
                    {
                        AdjustTraverseFromStart(i, points);
                        break;
                    }
                }
            }
        }

        private void AdjustTraverseFromStart(int start, IList<TtPoint> points)
        {
            TraverseSegment seg = GetTraverseSegment(start, points);

            if (seg.IsValid)
            {
                seg.Adjust();
            }
        }


        public void RecalculatePolygons()
        {
            RecalculatePolygons(true);
        }

        public void RecalculatePolygons(bool waitForUpdates = true)
        {
            foreach (TtPolygon poly in _Polygons)
            {
                IgnorePointEvents = true;

                foreach (GpsPoint p in _PointsByPoly[poly.CN].Where(p => p.IsGpsType()))
                {
                    p.Adjust();
                }

                AdjustAllTravTypesInPolygon(poly);

                IgnorePointEvents = false;

                if (waitForUpdates)
                {
                    _PolygonAPUpdateHandlers[poly.CN].Cancel();
                    GeneratePolygonStats(poly);
                }
                else
                    UpdatePolygonStats(poly);
            }
        }
        #endregion


        #region Utils
        protected List<IPointSegment> GetAllTravTypeSegmentsInPolygon(TtPolygon poly)
        {
            List<IPointSegment> segments = new List<IPointSegment>();
            SideShotSegment ssSeg = new SideShotSegment();

            Action<TtPoint> startNewSideShotSeg = (point) =>
            {
                if (ssSeg.Count > 0)
                    segments.Add(ssSeg);

                ssSeg = new SideShotSegment();
                ssSeg.Add(point);
            };

            IList<TtPoint> points = GetPoints(poly.CN);
            if (points.Count > 1)
            {
                TtPoint lastGps = null;

                foreach (TtPoint point in points)
                {
                    if (point.IsGpsAtBase())
                    {
                        startNewSideShotSeg(point);

                        lastGps = point;
                    }
                    else if (point.OpType == OpType.SideShot)
                    {
                        ssSeg.Add(point);
                    }
                    else if (point.OpType == OpType.Traverse)
                    {
                        startNewSideShotSeg(point);

                        if (lastGps != null)
                        {
                            segments.Add(GetTraverseSegment(lastGps.Index, points));
                            lastGps = null;
                        }
                    }
                }

                if (ssSeg.Count > 0 && ssSeg.Count > 1)
                    segments.Add(ssSeg);
            }

            return segments;
        }

        protected TraverseSegment GetTraverseSegment(int start, IList<TtPoint> points)
        {
            TraverseSegment seg = new TraverseSegment(points);
            seg.Add(points[start]);

            TtPoint tmp;

            for (int i = start + 1; i < points.Count; i++)
            {
                tmp = points[i];

                if (tmp.OpType == OpType.Traverse)
                    seg.Add(tmp);
                else if (tmp.IsGpsAtBase())
                {
                    seg.Add(tmp);
                    break;
                }
            }

            return seg;
        }

        protected List<TraverseSegment> GetAllTraverseSegmentsInPolygon(TtPolygon poly)
        {
            List<TraverseSegment> segments = new List<TraverseSegment>();

            IList<TtPoint> points = GetPoints(poly.CN);
            if (points.Count > 2)
            {
                TtPoint lastPoint = points[0];
                foreach (TtPoint point in points)
                {
                    if (point.OpType == OpType.Traverse && lastPoint.IsGpsAtBase())
                    {
                        segments.Add(GetTraverseSegment(lastPoint.Index, points));
                    }

                    lastPoint = point;
                }
            }

            return segments;
        }

        protected List<SideShotSegment> GetAllGpsBasedSideShotSegmentsInPolygon(TtPolygon poly)
        {
            List<SideShotSegment> segments = new List<SideShotSegment>();
            SideShotSegment seg = new SideShotSegment();

            IList<TtPoint> points = GetPoints(poly.CN);
            if (points.Count > 1)
            {
                TtPoint lastPoint = null;
                foreach (TtPoint point in points)
                {
                    if (point.IsGpsAtBase())
                    {
                        lastPoint = point;

                        if (seg.Count > 0)
                        {
                            segments.Add(seg);
                            seg = new SideShotSegment();
                        }
                    }
                    else if (point.OpType == OpType.SideShot)
                    {
                        if (seg.Count == 0)
                        {
                            seg.Add(lastPoint);
                        }

                        seg.Add(point);
                    }
                }

                if (seg.Count > 0)
                    segments.Add(seg);
            }

            return segments;
        }

        protected void ChangeTraverseAccuracy(IList<TtPoint> segment)
        {
            if (segment.Count > 2)
            {
                TtPoint curr = segment[0];

                double acc = curr.Accuracy;
                double accInc = (segment[segment.Count - 1].Accuracy - acc) / (segment.Count - 1);

                for (int i = 1; i < segment.Count - 1; i++)
                {
                    curr = segment[i];
                    acc += accInc;
                    curr.SetAccuracy(acc);
                }
            }
        }

        protected void ChangeGpsZone(GpsPoint point, int zone, int oldZone)
        {
            UTMCoords coords = point.HasLatLon ?
                UTMTools.ConvertLatLonSignedDecToUTM((double)point.Latitude, (double)point.Longitude, zone) :
                UTMTools.ShiftZones(point.UnAdjX, point.UnAdjY, zone, oldZone);

            point.SetUnAdjLocation(coords.X, coords.Y, point.UnAdjZ);
        }

        public void UpdatePolygonStats(TtPolygon polygon)
        {
            _PolygonAPUpdateHandlers[polygon.CN].DelayInvoke();
        }

        protected void GeneratePolygonStats(TtPolygon polygon)
        {
            lock (locker)
            {
                if (_PointsByPoly.ContainsKey(polygon.CN))
                {
                    APStats stats = TtCoreUtils.CalculateBoundaryAreaPerimeterAndTrail(GetPoints(polygon.CN));
                    polygon.Update(stats.Area, stats.Perimeter, stats.LinePerimeter);
                }
            }
        }
        
        public bool ValidatePointIndexes()
        {
            bool valid = true;

            foreach (List<TtPoint> points in _PointsByPoly.Values)
            {
                int index = 0;
                foreach (TtPoint point in points)
                {
                    if (point.Index != index)
                        return false;
                }
            }

            return valid;
        }

        public void ReindexPolygons()
        {
            foreach (List<TtPoint> points in _PointsByPoly.Values)
            {
                int index = 0;
                foreach (TtPoint point in points)
                    point.Index = index++;
            }
        }

        public void RebuildPolygon(TtPolygon poly, bool reindex = false)
        {
            List<TtPoint> points = _Points.Where(p => p.PolygonCN == poly.CN).OrderBy(p => p.Index).ToList();

            if (reindex)
            {
                int index = 0;
                foreach (TtPoint point in points)
                    point.Index = index++;
            }

            _PointsByPoly[poly.CN] = points;

            AdjustAllTravTypesInPolygon(poly);
            UpdatePolygonStats(poly);
        }

        public void Reset()
        {
            Load();
        }

        /// <summary>
        /// Adds an action type to UserActivity
        /// </summary>
        /// <param name="action">Type of Action</param>
        /// <param name="notes">Optional Notes of the action taken</param>
        /// <param name="guid">Optional action ID</param>
        public void AddAction(DataActionType action, string notes = null, Guid? guid = null)
        {
            _DataActions.Add(guid ?? Guid.NewGuid(), Tuple.Create(action, notes));
        }
        /// <summary>
        /// Removes an Action by ID
        /// </summary>
        /// <param name="guid">Action ID</param>
        public void RemoveAction(Guid guid)
        {
            _DataActions.Remove(guid);
        }
        #endregion


        #region Points
        public bool PointExists(String pointCN)
        {
            return _PointsMap.ContainsKey(pointCN);
        }

        public TtPoint GetPoint(String pointCN)
        {
            if (_PointsMap.ContainsKey(pointCN))
                return _PointsMap[pointCN];
            throw new Exception("Point Not Found");
        }

        public bool HasOriginalPoint(String pointCN)
        {
            return _PointsMapOrig.ContainsKey(pointCN);
        }

        public TtPoint GetOriginalPoint(String pointCN)
        {
            if (_PointsMapOrig.ContainsKey(pointCN))
                return _PointsMapOrig[pointCN];
            throw new Exception("Point Not Found");
        }

        public List<TtPoint> GetPoints(string polyCN = null)
        {
            if (polyCN != null)
            {
                if (_PointsByPoly.ContainsKey(polyCN))
                    return _PointsByPoly[polyCN].OrderBy(p => p.Index).ToList();
                throw new Exception("Polygon Not Found");
            }
            else
                return _Points.ToList();

        }

        public TtPoint GetNextPoint(TtPoint point)
        {
            if (_PointsByPoly.ContainsKey(point.PolygonCN) && point.Index < _PointsByPoly[point.PolygonCN].Count - 1)
            {
                return _PointsByPoly[point.PolygonCN][point.Index + 1];
            }

            return null;
        }


        /// <summary>
        /// Create a new Point with polygon, metadata and group attached
        /// </summary>
        /// <param name="op">Operation of the Point</param>
        /// <param name="polygon">The Point's Polygon</param>
        /// <param name="metadata">The Point's Metadata</param>
        /// <param name="group">The Group the Point is in. default:MainGroup</param>
        /// <returns>New Point</returns>
        //public TtPoint CreatePoint(OpType op, TtPolygon polygon, TtMetadata metadata = null, TtGroup group = null)
        //{
        //    TtPoint point = null;
		//
        //    switch (op)
        //    {
        //        case OpType.GPS:
        //            point = new GpsPoint();
        //            break;
        //        case OpType.Take5:
        //            point = new Take5Point();
        //            break;
        //        case OpType.Traverse:
        //            point = new TravPoint();
        //            break;
        //        case OpType.SideShot:
        //            point = new SideShotPoint();
        //            break;
        //        case OpType.Quondam:
        //            point = new QuondamPoint();
        //            break;
        //        case OpType.Walk:
        //            point = new WalkPoint();
        //            break;
        //        case OpType.WayPoint:
        //            point = new WayPoint();
        //            break;
        //    }

        //    point.Polygon = polygon;
        //    point.Metadata = metadata ?? DefaultMetadata;
        //    point.Group = group ?? MainGroup;

        //    return point;
        //}

        /// <summary>
        /// Add a point to a polygon
        /// </summary>
        /// <param name="point">Point to add to a polygon</param>
        public void AddPoint(TtPoint point)
        {
            if (point.PolygonCN == null)
                throw new Exception("No Valid PolygonCN");

            if (point.Polygon == null)
                throw new Exception("Polygon Not Attached");

            lock (locker)
            {
                if (_PointsMap.ContainsKey(point.CN))
                    throw new Exception("Point already exists");

                IList<TtPoint> points = _PointsByPoly[point.PolygonCN];

                if (point.Index < points.Count)
                {
                    int index = point.Index;

                    points.Insert(index, point);

                    for (int i = index + 1; i < points.Count; i++)
                    {
                        points[i].Index = i;
                    }
                }
                else
                {
                    points.Add(point);
                }

                if (point.OpType == OpType.Quondam && point is QuondamPoint qp && qp.ParentPoint == null)
                {
                    if (_PointsMap.ContainsKey(qp.ParentPointCN))
                        qp.ParentPoint = _PointsMap[qp.ParentPointCN];
                    else
                        throw new Exception("No Parent Point Found");
                }

                _PointsMap.Add(point.CN, point);
                _Points.Add(point);
                AttachPointEvents(point);

                AdjustAroundPoint(point, points);
                UpdatePolygonStats(point.Polygon);
            }
        }
        /// <summary>
        /// Adds multiple points
        /// </summary>
        /// <param name="points"></param>
        public void AddPoints(IEnumerable<TtPoint> addPoints)
        {
            lock (locker)
            {
                if (addPoints.Any(p => p.PolygonCN == null))
                    throw new Exception("Some points do not have a valid Polygon");

                if (addPoints.Any(p => _PointsMap.ContainsKey(p.CN)))
                    throw new Exception("Some points already exist");

                TtPoint lastPoint = null;
                List<TtPoint> points = null;

                List<String> polysToAdjustTravsIn = new List<string>();
                List<string> polysToAdjust = new List<string>();

                foreach (TtPoint point in addPoints.OrderBy(p => p.Index))
                {
                    if (lastPoint == null || lastPoint.PolygonCN != point.PolygonCN)
                    {
                        points = _PointsByPoly[point.PolygonCN];
                    }

                    if (lastPoint != null && lastPoint.PolygonCN == point.PolygonCN &&
                        lastPoint.Index == point.Index - 1 && point.Index == points.Count)
                    {
                        points.Add(point);
                    }
                    else
                    {
                        if (point.Index < points.Count)
                        {
                            int index = point.Index;

                            points.Insert(index, point);

                            for (int i = index + 1; i < points.Count; i++)
                            {
                                points[i].Index = i;
                            }
                        }
                        else
                        {
                            points.Add(point);
                        }

                        AttachPointEvents(point);
                    }

                    if (point.OpType == OpType.Quondam && point is QuondamPoint qp && qp.ParentPoint == null)
                    {
                        if (_PointsMap.ContainsKey(qp.ParentPointCN))
                            qp.ParentPoint = _PointsMap[qp.ParentPointCN];
                        else
                            throw new Exception("No Parent Point Found");
                    }

                    _Points.Add(point);
                    _PointsMap.Add(point.CN, point);
                    lastPoint = point;

                    if (!polysToAdjustTravsIn.Contains(point.PolygonCN) && (point.IsTravType() ||
                        (point.IsGpsAtBase() && ((points.Count > point.Index + 1 && points[point.Index + 1].IsTravType()) ||
                        (point.Index > 0 && points[point.Index - 1].IsTravType())))))
                    {
                        polysToAdjustTravsIn.Add(point.PolygonCN);
                    }

                    if (!polysToAdjust.Contains(point.PolygonCN))
                        polysToAdjust.Add(point.PolygonCN);
                }

                foreach (string polyCN in polysToAdjustTravsIn)
                {
                    AdjustAllTravTypesInPolygon(_PolygonsMap[polyCN]);
                }

                foreach (string cn in polysToAdjust)
                    _PolygonAPUpdateHandlers[cn].DelayInvoke();
            }
        }


        /// <summary>
        /// Repalces a point in a polygon which a new point
        /// </summary>
        /// <param name="point">Point to replace with</param>
        public void ReplacePoint(TtPoint point)
        {
            if (point.PolygonCN == null)
                throw new Exception("No Valid Polygon");

            lock (locker)
            {
                if (!_PointsMap.ContainsKey(point.CN))
                    throw new Exception("Point Not Found");

                IList<TtPoint> points = _PointsByPoly[point.PolygonCN];

                TtPoint replacedPoint = _PointsMap[point.CN];

                DetachPointEvents(replacedPoint);
                _Points.Remove(replacedPoint);

                points[point.Index] = point;
                _PointsMap[point.CN] = point;

                AttachPoint(point);

                _Points.Add(point);

                AdjustAroundPoint(point, points);
                UpdatePolygonStats(point.Polygon);
            }
        }
        /// <summary>
        /// Repalces points in polygon(s) which new points
        /// </summary>
        /// <param name="replacePoints">Points to replace with</param>
        public void ReplacePoints(IEnumerable<TtPoint> replacePoints)
        {
            lock (locker)
            {
                if (replacePoints.Any(p => p.PolygonCN == null))
                    throw new Exception("Some points do not have a valid Polygon");

                if (replacePoints.Any(p => !_PointsMap.ContainsKey(p.CN)))
                    throw new Exception("Some points don't exist");

                TtPoint lastPoint = null;
                List<TtPoint> points = null;

                List<TtPolygon> polysToAdjustTravsIn = new List<TtPolygon>();

                foreach (TtPoint point in replacePoints)
                {
                    if (lastPoint == null || lastPoint.PolygonCN != point.PolygonCN)
                    {
                        points = _PointsByPoly[point.PolygonCN];
                    }

                    TtPoint replacedPoint = _PointsMap[point.CN];

                    DetachPointEvents(replacedPoint);
                    _Points.Remove(replacedPoint);

                    points[point.Index] = point;
                    _PointsMap[point.CN] = point;

                    AttachPoint(point);

                    _Points.Add(point);

                    if (!polysToAdjustTravsIn.Contains(point.Polygon) &&
                        (point.IsTravType() ||
                        point.Index > 0 && points[point.Index - 1].IsTravType() ||
                        point.Index < points.Count - 1 && points[point.Index + 1].IsTravType()))
                    {
                        polysToAdjustTravsIn.Add(point.Polygon);
                    }

                    lastPoint = point;
                }

                foreach (TtPolygon poly in polysToAdjustTravsIn)
                {
                    AdjustAllTravTypesInPolygon(poly);
                    UpdatePolygonStats(poly);
                }
            }
        }


        public void MovePointsToPolygon(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex)
        {
            lock (locker)
            {
                IgnorePointEvents = true;

                List<string> reindexPolygons = new List<string>() { targetPolygon.CN };
                List<TtPoint> targetPoints = _PointsByPoly[targetPolygon.CN];

                foreach (TtPoint point in points)
                {
                    _PointsByPoly[point.PolygonCN].Remove(point);

                    if (point.PolygonCN == targetPolygon.CN && point.Index < insertIndex)
                    {
                        insertIndex--;
                    }

                    if (!reindexPolygons.Contains(point.PolygonCN))
                        reindexPolygons.Add(point.PolygonCN);

                    point.Polygon = targetPolygon;
                }

                if (insertIndex < 1)
                {
                    targetPoints.InsertRange(0, points);
                }
                else if (insertIndex < targetPoints.Count)
                {
                    targetPoints.InsertRange(insertIndex, points);
                }
                else
                {
                    targetPoints.AddRange(points);
                }

                int pointIndex = 0;
                foreach (string ripoly in reindexPolygons)
                {
                    if (_PointsByPoly.ContainsKey(ripoly))
                    {
                        pointIndex = 0;
                        foreach (TtPoint point in _PointsByPoly[ripoly])
                            point.Index = pointIndex++;

                        TtPolygon poly = _PolygonsMap[ripoly];
                        AdjustAllTravTypesInPolygon(poly);
                        UpdatePolygonStats(poly);
                    }
                }

                IgnorePointEvents = false;
            }

        }


        /// <summary>
        /// Remove a point from a polygon
        /// </summary>
        /// <param name="point">Point to be removed from a polygon</param>
        public void DeletePoint(TtPoint point)
        {
            lock (locker)
            {
                if (_PointsMap.ContainsKey(point.CN))
                {
                    if (point.HasQuondamLinks)
                        throw new Exception("Point Has Linked Quondam");

                    DetachPointEvents(point);

                    IList<TtPoint> points = _PointsByPoly[point.PolygonCN];

                    points.RemoveAt(point.Index);
                    _PointsMap.Remove(point.CN);
                    _Points.Remove(point);

                    if (point.Index >= points.Count)
                    {
                        for (int i = 0; i < points.Count; i++)
                        {
                            points[i].Index = i;
                        }

                        if (points.Count > 0)
                            point = points.Last();
                    }

                    if (points.Count > 0)
                    {
                        AdjustAroundPoint(points[point.Index], points);
                        UpdatePolygonStats(point.Polygon);
                    }
                }
            }
        }
        /// <summary>
        /// Deletes Points in polygons
        /// </summary>
        /// <param name="points"></param>
        public void DeletePoints(IEnumerable<TtPoint> points)
        {
            lock (locker)
            {
                List<string> pcns = points.Select(p => p.CN).ToList();
                if (points.Any(p => p.HasQuondamLinks && p.LinkedPoints.Any(lpcn => !pcns.Contains(lpcn))))
                    throw new Exception("Points Have Linked Quondams");

                List<TtPolygon> reindexPolys = new List<TtPolygon>();

                foreach (TtPoint point in points)
                {
                    if (_PointsMap.ContainsKey(point.CN))
                    {
                        DetachPointEvents(point);

                        _PointsByPoly[point.PolygonCN].Remove(point);
                        _PointsMap.Remove(point.CN);
                        _Points.Remove(point);

                        if (!reindexPolys.Contains(point.Polygon))
                            reindexPolys.Add(point.Polygon);
                    }
                }

                foreach (TtPolygon polygon in reindexPolys)
                    RebuildPolygon(polygon, true);
            }
        }


        public void DeletePointsInPolygon(string polyCN)
        {
            lock (locker)
            {
                if (_PolygonsMap.ContainsKey(polyCN))
                {
                    List<TtPoint> points = _PointsByPoly[polyCN];

                    if (points.Any(p => p.HasQuondamLinks))
                        throw new Exception("Points Have Linked Quondams");

                    foreach (TtPoint point in points)
                    {
                        DetachPointEvents(point);

                        _PointsMap.Remove(point.CN);
                        _Points.Remove(point);
                    }

                    _PointsByPoly[polyCN].Clear();
                }
            }
        }
        #endregion


        #region Polygons
        public bool PolygonExists(string polyCN)
        {
            return _PolygonsMap.ContainsKey(polyCN);
        }

        public TtPolygon GetPolygon(string polyCN)
        {
            if (_PolygonsMap.ContainsKey(polyCN))
                return _PolygonsMap[polyCN];
            throw new Exception("Polygon Not Found");
        }

        public List<TtPolygon> GetPolygons()
        {
            return _Polygons.ToList();
        }


        /// <summary>
        /// Add a polygon to the project
        /// </summary>
        /// <param name="polygon"></param>
        public void AddPolygon(TtPolygon polygon)
        {
            lock (locker)
            {
                if (_PolygonsMap.ContainsKey(polygon.CN))
                    throw new Exception("Polygon already exists");

                if (!_PointsByPoly.ContainsKey(polygon.CN))
                    _PointsByPoly.Add(polygon.CN, new List<TtPoint>());

                _PolygonsMap.Add(polygon.CN, polygon);
                _Polygons.Add(polygon);

                AttachPolygonEvents(polygon);
            }
        }

        /// <summary>
        /// Remove a polygon from the project
        /// </summary>
        /// <param name="polygon">Polygon to be removed from the project</param>
        public void DeletePolygon(TtPolygon polygon)
        {
            lock (locker)
            {
                List<TtPoint> points = _PointsByPoly[polygon.CN];

                if (points.Count > 0)
                {
                    foreach (TtPoint point in points)
                    {
                        if (point.OpType == OpType.Quondam)
                        {
                            ((QuondamPoint)point).ParentPoint = null;
                        }
                        else if (point.HasQuondamLinks)
                        {
                            foreach (string cn in point.LinkedPoints.ToArray())
                            {
                                QuondamPoint qp = null;

                                if (_PointsMap.ContainsKey(cn))
                                    qp = _PointsMap[cn] as QuondamPoint;

                                if (qp != null)
                                {
                                    if (qp.ParentPoint.PolygonCN != polygon.CN)
                                    {
                                        GpsPoint gps = new GpsPoint(qp);

                                        if (!string.IsNullOrEmpty(gps.Comment) && string.IsNullOrEmpty(point.Comment))
                                            gps.Comment = point.Comment;

                                        if (qp.ManualAccuracy != null)
                                            gps.ManualAccuracy = qp.ManualAccuracy;

                                        qp.ParentPoint = null;

                                        ReplacePoint(gps);
                                    }
                                    else
                                        qp.ParentPoint = null;
                                }
                                else
                                {
                                    Trace.WriteLine("Detached Quondam Found");
                                }
                            }
                        }
                    }
                }

                foreach (TtPoint point in points)
                {
                    _PointsMap.Remove(point.CN);
                    _Points.Remove(point);
                }

                DetachPolygonEvents(polygon);

                _PointsByPoly.Remove(polygon.CN);

                _PolygonsMap.Remove(polygon.CN);
                _Polygons.Remove(polygon);
            }
        }
        #endregion


        #region Groups
        public bool GroupExists(string groupCN)
        {
            return _GroupsMap.ContainsKey(groupCN);
        }

        public List<TtGroup> GetGroups()
        {
            return _Groups.ToList();
        }


        /// <summary>
        /// Adds a group to the project
        /// </summary>
        /// <param name="group">Group to be added to the project</param>
        public void AddGroup(TtGroup group)
        {
            if (group.CN == Consts.EmptyGuid)
                throw new Exception("Default Group Already Exists");

            lock (locker)
            {
                if (_GroupsMap.ContainsKey(group.CN))
                    throw new Exception("Group already exists");
                _GroupsMap.Add(group.CN, group);
                _Groups.Add(group);
            }
        }

        /// <summary>
        /// Removes a group from the project
        /// </summary>
        /// <param name="group">Group to add to the project</param>
        public void DeleteGroup(TtGroup group)
        {
            if (group.CN == Consts.EmptyGuid)
                throw new Exception("Cannot Delete Main Group");

            lock (locker)
            {
                foreach (TtPoint point in _Points.Where(p => p.GroupCN == group.CN))
                    point.Group = MainGroup;

                _GroupsMap.Remove(group.CN);
                _Groups.Remove(group);
            }
        }
        #endregion


        #region Metadata
        public bool MetadataExists(string metaCN)
        {
            return _MetadataMap.ContainsKey(metaCN);
        }

        public List<TtMetadata> GetMetadata()
        {
            return _Metadata.ToList();
        }


        /// <summary>
        /// Adds a metadata to the project
        /// </summary>
        /// <param name="metadata">Metadata to add to the project</param>
        public void AddMetadata(TtMetadata metadata)
        {
            if (metadata.CN == Consts.EmptyGuid)
                throw new Exception("Default Metadata Already Exists");

            lock (locker)
            {
                if (_MetadataMap.ContainsKey(metadata.CN))
                    throw new Exception("Metadata already exists");
                _MetadataMap.Add(metadata.CN, metadata);
                _Metadata.Add(metadata);

                AttachMetadataEvents(metadata);
            }
        }

        /// <summary>
        /// Removes metadata from the project
        /// </summary>
        /// <param name="metadata">Metadata to removed from the project</param>
        public void DeleteMetadata(TtMetadata metadata)
        {
            if (metadata.CN == Consts.EmptyGuid)
                throw new Exception("Cannot Delete Default Metadata");

            lock (locker)
            {
                Dictionary<String, TtPolygon> adjustPolygons = new Dictionary<string, TtPolygon>();

                foreach (TtPoint point in _Points.Where(p => p.MetadataCN == metadata.CN))
                {
                    if (point.IsGpsType())
                        ChangeGpsZone(point as GpsPoint, DefaultMetadata.Zone, metadata.Zone);

                    point.Metadata = DefaultMetadata;

                    if (adjustPolygons.ContainsKey(point.PolygonCN))
                        adjustPolygons.Add(point.PolygonCN, point.Polygon);
                }

                foreach (TtPolygon polygon in adjustPolygons.Values)
                {
                    AdjustAllTravTypesInPolygon(polygon);
                    UpdatePolygonStats(polygon);
                }

                _MetadataMap.Remove(metadata.CN);
                _Metadata.Remove(metadata);

                DetachMetadataEvents(metadata);
            }
        }
        #endregion


        #region NMEA

        public bool NmeaExists(string nmeaCN)
        {
            return _DAL.GetNmeaBurst(nmeaCN) != null;
        }

        public List<TtNmeaBurst> GetNmeaBursts(string pointCN = null)
        {
            return (pointCN != null && _DeleteNmeaByPointCNs.Contains(pointCN)) ?
                new List<TtNmeaBurst>() :
                _DAL.GetNmeaBursts(pointCN)
                    .Concat(_AddNmea.Values.Where(n => pointCN == null || n.PointCN == pointCN)).ToList();
        }

        public List<TtNmeaBurst> GetNmeaBursts(IEnumerable<string> pointCNs)
        {
            return _DAL.GetNmeaBursts(pointCNs)
                .Where(n => !_DeleteNmeaByPointCNs.Contains(n.PointCN))
                .Concat(_AddNmea.Values.Where(n => pointCNs.Contains(n.PointCN))).ToList();
        }


        public void AddNmeaBurst(TtNmeaBurst burst)
        {
            _AddNmea.Add(burst.CN, burst);
        }

        public void AddNmeaBursts(IEnumerable<TtNmeaBurst> bursts)
        {
            foreach (TtNmeaBurst burst in bursts)
            {
                AddNmeaBurst(burst);
            }
        }

        public void RestoreNmeaBurts(string pointCN)
        {
            _DeleteNmeaByPointCNs.Remove(pointCN);
        }

        public void DeleteNmeaBursts(string pointCN)
        {
            foreach (string cn in _AddNmea.Values.Where(n => n.PointCN == pointCN).Select(n => n.CN).ToList())
            {
                _AddNmea.Remove(cn);
            }

            if (_PointsMapOrig.ContainsKey(pointCN))
            {
                _DeleteNmeaByPointCNs.Add(pointCN);
            }
        }
        #endregion


        #region Polygon Graphic Options
        public PolygonGraphicOptions GetPolygonGraphicOption(string polyCN)
        {
            if (_PolyGraphicOpts.ContainsKey(polyCN))
                return _PolyGraphicOpts[polyCN];
            else
            {
                PolygonGraphicOptions pgo = _Settings.PolygonGraphicSettings.CreatePolygonGraphicOptions(polyCN);
                _PolyGraphicOpts.Add(polyCN, pgo);
                return pgo;
            }
        }

        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return _PolyGraphicOpts.Values.ToList();
        }

        public PolygonGraphicOptions GetDefaultPolygonGraphicOption()
        {
            return _Settings.PolygonGraphicSettings.CreatePolygonGraphicOptions(null);
        }
        #endregion


        #region Images
        public List<TtImage> GetImages(string pointCN = null)
        {
            return _MediaMap == null ? new List<TtImage>() :
                (pointCN == null ? _MediaMap.SelectMany(kvp => kvp.Value.Images) : _MediaMap[pointCN].Images).ToList();
        }

        public void InsertMedia(TtMedia media)
        {
            //todo add media
            throw new NotImplementedException();
        }

        public void DeleteMedia(TtMedia media)
        {
            //todo delete media
            throw new NotImplementedException();
        }
        #endregion


        #region Data Dictionary
        public DataDictionaryTemplate GetDataDictionaryTemplate()
        {
            return _DAL.GetDataDictionaryTemplate();
        }
        #endregion

        public List<TtUserAction> GetUserActions()
        {
            return _DAL.GetUserActivity().ToList();
        }
    }

    public class MissingDataException : Exception
    {
        public String PropertyName { get; }

        public MissingDataException(string propertyName = null)
        {
            PropertyName = propertyName;
        }
    }
}
