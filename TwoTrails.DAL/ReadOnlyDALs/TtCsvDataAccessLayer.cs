﻿using CsvHelper;
using CsvHelper.Configuration;
using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public class TtCsvDataAccessLayer : IReadOnlyTtDataLayer
    {
        public bool RequiresUpgrade => false;

        public string FilePath => _Options.PointsFile;

        public bool HandlesAllPointTypes => _Options.Mode == ParseMode.Advanced;

        private Dictionary<string, TtPoint> _Points = new Dictionary<string, TtPoint>();
        private Dictionary<string, TtPolygon> _Polygons = new Dictionary<string, TtPolygon>();
        private Dictionary<string, TtMetadata> _Metadata = new Dictionary<string, TtMetadata>();
        private Dictionary<string, TtGroup> _Groups = new Dictionary<string, TtGroup>();
        private Dictionary<string, TtNmeaBurst> _Nmea = new Dictionary<string, TtNmeaBurst>();

        private TtProjectInfo _ProjectInfo;

        private readonly ParseOptions _Options;
        private bool parsed;
        private int milliSecondsInc = 0;
        private int polyInc = 0;

        private static object locker = new object();


        public TtCsvDataAccessLayer(ParseOptions options)
        {
            _Options = options;
            polyInc = options.StartPolygonNumber;
        }

        public void Parse(bool reparse = false)
        {
            lock (locker)
            {
                if (!parsed || reparse)
                {
                    if (reparse)
                    {
                        _Points.Clear();
                        _Polygons.Clear();
                        _Metadata.Clear();
                        _Groups.Clear();
                        _Nmea.Clear();
                        milliSecondsInc = 0;
                    }

                    ParseProject(_Options.ProjectFile);
                    ParsePolygons(_Options.PolygonsFile);
                    ParseMetadata(_Options.MetadataFile);
                    ParseGroups(_Options.GroupsFile);
                    ParseNmea(_Options.NmeaFile);
                    ParsePoints(_Options);

                    parsed = true;
                } 
            }
        }


        private void ParsePoints(ParseOptions options)
        {
            bool useAdvParsing = options.Mode == ParseMode.Advanced;

            IDictionary<PointTextFieldType, int> mapping = _Options.PointMapping.Where(x => x.Value > -1).ToDictionary(x => x.Key, x => x.Value);
            
            Dictionary<string, string> polyNameToCN = new Dictionary<string, string>();
            Dictionary<string, string> groupNameToCN = new Dictionary<string, string>();

            #region Check and get Fields
            bool hasPolyNames = mapping.ContainsKey(PointTextFieldType.POLY_NAME);
            bool hasPIDs = mapping.ContainsKey(PointTextFieldType.PID);
            bool hasCN = mapping.ContainsKey(PointTextFieldType.CN);
            bool hasPolyCN = mapping.ContainsKey(PointTextFieldType.POLY_CN);
            bool hasMetaCN = mapping.ContainsKey(PointTextFieldType.META_CN);

            bool hasGroupNames = mapping.ContainsKey(PointTextFieldType.GROUP_NAME);
            bool hasGroupCN = mapping.ContainsKey(PointTextFieldType.GROUP_CN);

            bool hasUnAdjZ = mapping.ContainsKey(PointTextFieldType.UNADJZ);

            bool hasLatLon = mapping.ContainsKey(PointTextFieldType.LATITUDE) && mapping.ContainsKey(PointTextFieldType.LONGITUDE);
            bool hasElevation = mapping.ContainsKey(PointTextFieldType.ELEVATION);
            bool hasRMSEr = mapping.ContainsKey(PointTextFieldType.RMSER);

            bool hasFwdAz = mapping.ContainsKey(PointTextFieldType.FWD_AZ);
            bool hasBkAz = mapping.ContainsKey(PointTextFieldType.BK_AZ);

            bool hasManAcc = mapping.ContainsKey(PointTextFieldType.MAN_ACC);

            bool hasTime = mapping.ContainsKey(PointTextFieldType.TIME);
            bool hasIndex = mapping.ContainsKey(PointTextFieldType.INDEX);
            bool hasBnd = mapping.ContainsKey(PointTextFieldType.ONBND);
            bool hasComment = mapping.ContainsKey(PointTextFieldType.COMMENT);

            int fPolyName = GetFieldColumn(mapping, PointTextFieldType.POLY_NAME, hasPolyNames);
            int fPolyCN = GetFieldColumn(mapping, PointTextFieldType.POLY_CN, hasPolyCN);
            int fPID = GetFieldColumn(mapping, PointTextFieldType.PID, hasPIDs);
            int fCN = GetFieldColumn(mapping, PointTextFieldType.CN, useAdvParsing);
            int fOp = GetFieldColumn(mapping, PointTextFieldType.OPTYPE, useAdvParsing);
            int fGroupName = GetFieldColumn(mapping, PointTextFieldType.GROUP_NAME, hasGroupNames);
            int fGroupCN = GetFieldColumn(mapping, PointTextFieldType.GROUP_CN, hasGroupNames);
            int fUnAjX = GetFieldColumn(mapping, PointTextFieldType.UNADJX, _Options.Mode != ParseMode.LatLon); //mapping[PointTextFieldType.UNADJX];
            int fUnAjY = GetFieldColumn(mapping, PointTextFieldType.UNADJY, _Options.Mode != ParseMode.LatLon); //mapping[PointTextFieldType.UNADJY];
            int fUnAjZ = GetFieldColumn(mapping, PointTextFieldType.UNADJZ, hasUnAdjZ);
            int fLat = GetFieldColumn(mapping, PointTextFieldType.LATITUDE, hasLatLon);
            int fLon = GetFieldColumn(mapping, PointTextFieldType.LONGITUDE, hasLatLon);
            int fElev = GetFieldColumn(mapping, PointTextFieldType.ELEVATION, hasElevation);
            int fRmser = GetFieldColumn(mapping, PointTextFieldType.RMSER, hasRMSEr);
            int fFwdAz = GetFieldColumn(mapping, PointTextFieldType.FWD_AZ, hasFwdAz);
            int fBkAz = GetFieldColumn(mapping, PointTextFieldType.BK_AZ, hasBkAz);
            int fSlopeDist = GetFieldColumn(mapping, PointTextFieldType.SLOPE_DIST, useAdvParsing);
            int fSlopeDType = GetFieldColumn(mapping, PointTextFieldType.SLOPE_DIST_TYPE, useAdvParsing);
            int fSlopeAng = GetFieldColumn(mapping, PointTextFieldType.SLOPE_ANG, useAdvParsing);
            int fSlopeAngType = GetFieldColumn(mapping, PointTextFieldType.SLOPE_ANG_TYPE, useAdvParsing);
            int fPCN = GetFieldColumn(mapping, PointTextFieldType.PARENT_CN, useAdvParsing);
            int fManAcc = GetFieldColumn(mapping, PointTextFieldType.MAN_ACC, hasManAcc);
            int fTime = GetFieldColumn(mapping, PointTextFieldType.TIME, hasTime);
            int fIndex = GetFieldColumn(mapping, PointTextFieldType.INDEX, hasIndex);
            int fBnd = GetFieldColumn(mapping, PointTextFieldType.ONBND, hasBnd);
            int fCmt = GetFieldColumn(mapping, PointTextFieldType.COMMENT, hasComment);
            int fMetaCN = GetFieldColumn(mapping, PointTextFieldType.META_CN, hasMetaCN);
            #endregion

            List<Tuple<string, QuondamPoint>> quondams = new List<Tuple<string, QuondamPoint>>();

            CsvReader reader = new CsvReader(new StreamReader(_Options.PointsFile),
                new CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    AllowComments = true,
                    HasHeaderRecord = true,
                    ShouldSkipRecord = ssra => ssra.Row.Parser.Record.All(r => String.IsNullOrEmpty(r))
                });

            reader.Read();

            TtPoint prevPoint = null;

            try
            {
                while(reader.Read())
                {
                    TtPoint point;

                    if (useAdvParsing)
                    {
                        OpType op = TtTypes.ParseOpType(reader.GetField<string>(fOp));

                        point = TtCoreUtils.GetPointByType(op);

                        point.CN = reader.GetField<string>(fCN);

                        if (point.IsGpsType())
                        {
                            GpsPoint gps = point as GpsPoint;

                            if (hasLatLon)
                            {
                                gps.Latitude = reader.GetField<double?>(fLat);
                                gps.Longitude = reader.GetField<double?>(fLon);
                            }

                            if (hasElevation)
                                gps.Elevation = reader.GetField<double?>(fElev);

                            if (hasRMSEr)
                                gps.RMSEr = reader.GetField<double?>(fRmser);

                        }
                        else if (point.IsTravType())
                        {
                            TravPoint trav = point as TravPoint;

                            if (hasFwdAz)
                                trav.FwdAzimuth = reader.GetField<double?>(fFwdAz);

                            if (hasBkAz)
                                trav.BkAzimuth = reader.GetField<double?>(fBkAz);

                            if (trav.FwdAzimuth == null && trav.BkAzimuth == null)
                                throw new Exception("Invalid Traverse");

                            double temp = reader.GetField<double>(fSlopeDist);
                            Distance dType = Types.ParseDistance(reader.GetField<string>(fSlopeDType));
                            trav.SlopeDistance = FMSC.Core.Convert.Distance(temp, Distance.Meters, dType);

                            temp = reader.GetField<double>(fSlopeAng);
                            Slope sType = Types.ParseSlope(reader.GetField<string>(fSlopeAngType));
                            trav.SlopeDistance = FMSC.Core.Convert.Angle(temp, Slope.Percent, sType);
                        }
                        else if (point.OpType == OpType.Quondam)
                        {
                            QuondamPoint qp = point as QuondamPoint;
                            quondams.Add(Tuple.Create(reader.GetField<string>(fPCN), qp));
                        }

                        if (hasManAcc && point is IManualAccuracy)
                        {
                            ((IManualAccuracy)point).ManualAccuracy = reader.GetField<double?>(fManAcc);
                        }
                    }
                    else
                    {
                        point = new GpsPoint();
                    }

                    if (_Options.Mode == ParseMode.LatLon)
                    {
                        ((GpsPoint)point).SetUnAdjLocation(
                            reader.GetField<double>(fLat),
                            reader.GetField<double>(fLon),
                            _Options.TargetZone,
                            hasElevation ? reader.GetField<double?>(fElev) ?? 0 : 0);
                    }
                    else
                    {
                        point.SetUnAdjLocation(
                            reader.GetField<double>(fUnAjX),
                            reader.GetField<double>(fUnAjY),
                            hasUnAdjZ ? reader.GetField<double>(fUnAjZ) : 0);
                    }

                    //Time
                    if (hasTime)
                        point.TimeCreated = TtCoreUtils.ParseTime(reader.GetField<string>(fTime));
                    else
                        point.TimeCreated = DateTime.Now;


                    #region Polygons
                    if (hasPolyCN)
                    {
                        string cn = reader.GetField<string>(fPolyCN);
                        if (!_Polygons.ContainsKey(cn))
                        {
                            TtPolygon poly = new TtPolygon()
                            {
                                CN = cn,
                                Name = hasPolyNames ?
                                    reader.GetField<string>(fPolyName) : $"Poly {++polyInc}",
                                PointStartIndex = _Polygons.Count * polyInc + Consts.DEFAULT_POINT_INCREMENT,
                                TimeCreated = _ProjectInfo.CreationDate.AddMilliseconds(milliSecondsInc++)
                            };

                            _Polygons.Add(cn, poly);
                        }

                        point.PolygonCN = cn;
                    }
                    else if (hasPolyNames)
                    {
                        string name = reader.GetField<string>(fPolyName);

                        if (polyNameToCN.ContainsKey(name))
                            point.PolygonCN = polyNameToCN[name];
                        else
                        {
                            TtPolygon poly = new TtPolygon()
                            {
                                Name = reader.GetField<string>(fPolyName),
                                PointStartIndex = (_Polygons.Count + 1) * 1000 + Consts.DEFAULT_POINT_INCREMENT,
                                TimeCreated = _ProjectInfo.CreationDate.AddMilliseconds(milliSecondsInc++)
                            };

                            _Polygons.Add(poly.CN, poly);
                            point.PolygonCN = poly.CN;
                        }
                    }
                    else
                    {
                        TtPolygon poly;
                        if (_Polygons.Count < 1)
                        {
                            poly = new TtPolygon()
                            {
                                Name = "Poly 1"
                            };

                            _Polygons.Add(poly.CN, poly);
                        }
                        else
                        {
                            poly = _Polygons.Values.First();
                        }

                        point.PolygonCN = poly.CN;
                    }
                    #endregion

                    #region Groups
                    if (hasGroupCN)
                    {
                        string cn = reader.GetField<string>(fGroupCN);
                        if (cn != Consts.EmptyGuid && !_Groups.ContainsKey(cn))
                        {
                            TtGroup group = new TtGroup()
                            {
                                CN = cn,
                                Name = hasGroupNames ?
                                    reader.GetField<string>(fGroupName) :
                                    $"Group {_Groups.Count + 1}"
                            };

                            _Groups.Add(cn, group);
                        }

                        point.GroupCN = cn;
                    }
                    else if (hasGroupNames)
                    {
                        string name = reader.GetField<string>(fGroupName);

                        if (name != "Main Group")
                        {
                            if (groupNameToCN.ContainsKey(name))
                                point.GroupCN = groupNameToCN[name];
                            else
                            {
                                TtGroup group = new TtGroup()
                                {
                                    Name = name
                                };

                                _Groups.Add(group.CN, group);

                                point.GroupCN = group.CN;
                            }
                        }
                        else
                            point.GroupCN = Consts.EmptyGuid;
                    }
                    else
                    {
                        point.GroupCN = Consts.EmptyGuid;
                    }
                    #endregion

                    if (hasMetaCN)
                    {
                        string cn = reader.GetField<string>(fMetaCN);
                        if (_Metadata.ContainsKey(cn))
                            point.MetadataCN = cn;
                        else
                            point.MetadataCN = Consts.EmptyGuid;
                    }
                    else
                    {
                        point.MetadataCN = Consts.EmptyGuid;
                    }


                    if (hasPIDs)
                    {
                        point.PID = reader.GetField<int>(fPID);
                    }
                    else
                    {
                        if (prevPoint != null)
                        {
                            TtPolygon poly = _Polygons[point.PolygonCN];

                            point.PID = (prevPoint.PolygonCN == poly.CN) ?
                                PointNamer.NamePoint(poly, prevPoint) :
                                PointNamer.NamePoint(poly);
                        }
                        else
                            PointNamer.NamePoint(_Polygons[point.PolygonCN]);
                    }

                    if (hasIndex)
                    {
                        point.Index = reader.GetField<int>(fIndex);
                    }
                    else
                    {
                        if (prevPoint != null)
                        {
                            point.Index = (prevPoint.PolygonCN == point.PolygonCN) ?
                                prevPoint.Index + 1 : 0;
                        }
                        else
                            point.Index = 0;
                    }

                    point.OnBoundary = hasBnd ? reader.GetField<bool>(fBnd) : true;

                    if (hasComment)
                        point.Comment = reader.GetField<string>(fCmt).UnScrub();

                    _Points.Add(point.CN, point);

                    prevPoint = point;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing points", ex);
            }

            if (useAdvParsing)
            {
                TtPoint parent;
                foreach (Tuple<string, QuondamPoint> kvp in quondams)
                {
                    if (_Points.ContainsKey(kvp.Item1))
                    {
                        parent = _Points[kvp.Item1];
                        parent.AddLinkedPoint(kvp.Item2);
                        kvp.Item2.ParentPoint = parent;
                    }
                    else
                    {
                        throw new Exception("Missing Parent Point: " + kvp.Item1);
                    }
                }
            }
        }

        private IEnumerable<TtPoint> GetLinkedPoints(IEnumerable<TtPoint> points)
        {
            foreach (TtPoint point in _Points.Values)
            {
                if (point.OpType == OpType.Quondam)
                {
                    QuondamPoint qp = new QuondamPoint(point);

                    if (_Points.ContainsKey(qp.ParentPointCN))
                        qp.ParentPoint = _Points[qp.ParentPointCN].DeepCopy();

                    yield return qp;
                }

                yield return point.DeepCopy();
            }
        }

        private int GetFieldColumn(IDictionary<PointTextFieldType, int> map, PointTextFieldType type, bool use)
        {
            if (use && map.ContainsKey(type))
                return map[type];

            return -1;
        }


        private void ParsePolygons(string filePath)
        {
            //TODO Parse Polygons
        }

        private void ParseMetadata(string filePath)
        {
            //TODO Parse Metadata
        }

        private void ParseGroups(string filePath)
        {
            //TODO Parse Groups
        }

        private void ParseNmea(string filePath)
        {
            //TODO Parse Nmea
        }

        private void ParseProject(string filePath)
        {
            filePath = filePath ?? _Options.PointsFile;

            //TODO Parse Project
            _ProjectInfo = new TtProjectInfo(Path.GetFileName(filePath),
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                String.Empty,
                new Version("0.0.0"),
                String.Empty,
                File.GetCreationTime(filePath));
        }
        
        public TtPoint GetPoint(String cn, bool linked = false)
        {
            Parse();

            return _Points.ContainsKey(cn) ? (linked ? GetLinkedPoints(new TtPoint[] { _Points[cn] }).First() : _Points[cn]) : null;
        }

        public IEnumerable<TtPoint> GetPoints(String polyCN = null, bool linked = false)
        {
            Parse();

            IEnumerable<TtPoint> points = (polyCN == null ? _Points.Values : _Points.Values.Where(p => p.PolygonCN == polyCN)).OrderBy(p => p.Index);

            return linked ? GetLinkedPoints(points) : points.DeepCopy();
        }

        public int GetPointCount(params string[] polyCNs)
        {
            if (polyCNs == null || !polyCNs.Any())
            {
                return _Points.Count;
            }
            else
            {
                return _Points.Values.Count(p => polyCNs.Contains(p.PolygonCN));
            }
        }

        public bool HasPolygons()
        {
            Parse();

            return _Polygons.Count > 0;
        }

        public IEnumerable<TtPolygon> GetPolygons()
        {
            Parse();

            return _Polygons.Values.DeepCopy();
        }

        public IEnumerable<TtMetadata> GetMetadata()
        {
            Parse();

            return _Metadata.Values.DeepCopy();
        }

        public IEnumerable<TtGroup> GetGroups()
        {
            Parse();

            return _Groups.Values.DeepCopy();
        }

        public TtNmeaBurst GetNmeaBurst(string nmeaCN)
        {
            Parse();

            return _Nmea.Values.Where(n => n.CN == nmeaCN).FirstOrDefault()?.DeepCopy();
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(String pointCN = null)
        {
            Parse();

            return (pointCN == null ? _Nmea.Values.DeepCopy() : _Nmea.Values.Where(n => n.PointCN == pointCN)).OrderBy(n => n.TimeCreated).DeepCopy();
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(IEnumerable<string> pointCNs)
        {
            Parse();

            List<string> pcns = pointCNs.ToList();

            if (pointCNs.Any())
            {
                foreach (TtNmeaBurst burst in _Nmea.Values)
                {
                    if (pointCNs.Contains(burst.PointCN))
                        yield return burst.DeepCopy();
                }
            }
        }

        public TtProjectInfo GetProjectInfo()
        {
            Parse();

            return new TtProjectInfo(_ProjectInfo);
        }

        public IEnumerable<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return new List<PolygonGraphicOptions>();
        }

        public IEnumerable<TtImage> GetPictures(String pointCN)
        {
            return new List<TtImage>();
        }

        public IEnumerable<TtUserAction> GetUserActivity()
        {
            return new List<TtUserAction>();
        }


        public DataDictionaryTemplate GetDataDictionaryTemplate()
        {
            throw new NotImplementedException();
        }

        public DataDictionary GetExtendedDataForPoint(string pointCN)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DataDictionary> GetExtendedData()
        {
            throw new NotImplementedException();
        }

        public class ParseOptions
        {
            public String PointsFile { get; }
            public string ProjectFile { get; }
            public string PolygonsFile { get; }
            public string MetadataFile { get; }
            public string GroupsFile { get; }
            public string NmeaFile { get; }
            public string MediaFile { get; }
            public string UserActivityFile { get; }

            public int TargetZone { get; }

            public int StartPolygonNumber { get; }


            private Dictionary<PointTextFieldType, int> _PointMapping { get; } = new Dictionary<PointTextFieldType, int>();
            public ReadOnlyDictionary<PointTextFieldType, int> PointMapping { get; }

            public string[] Fields { get; private set; }

            public ParseMode Mode { get; set; }

            public bool HasMultiplePolygons
            {
                get
                {
                    return PointMapping.ContainsKey(PointTextFieldType.POLY_NAME) ||
                        PointMapping.ContainsKey(PointTextFieldType.POLY_CN);
                }
            }


            public bool TravDistanceOverride { get; set; }
            public Distance TravDistance { get; set; }


            public bool TravSlopeOverride { get; set; }
            public Slope TravSlope { get; set; }



            public ParseOptions(string pointsFile, int targetZone, string projectFile = null, string polysFile = null, string metaFile = null,
                string groupsFile = null, string nmeaFile = null, string mediaFile = null, string activityFile = null, int startPolyNumber = 0)
            {
                PointsFile = pointsFile;
                TargetZone = targetZone;
                ProjectFile = projectFile;
                PolygonsFile = polysFile;
                MetadataFile = metaFile;
                GroupsFile = groupsFile;
                NmeaFile = nmeaFile;
                MediaFile = mediaFile;
                activityFile = UserActivityFile;

                StartPolygonNumber = startPolyNumber;

                ResetPointMap();
                PointMapping = new ReadOnlyDictionary<PointTextFieldType, int>(_PointMapping);
            }

            public void ResetPointMap()
            {
                _PointMapping.Clear();

                int index = 0;

                Fields = File.ReadLines(PointsFile).First().Split(',');
                foreach (string header in Fields.Select(s => s.ToLower()))
                {
                    switch (header)
                    {
                        case "point":
                        case "pid":
                        case "point id":
                            EditPointMap(PointTextFieldType.PID, index, false);
                            break;
                        case "optype":
                        case "op":
                        case "operation":
                            EditPointMap(PointTextFieldType.OPTYPE, index, false);
                            break;
                        case "index":
                        case "indx":
                            EditPointMap(PointTextFieldType.INDEX, index, false);
                            break;
                        case "polygon":
                        case "polygon name":
                        case "poly name":
                            EditPointMap(PointTextFieldType.POLY_NAME, index, false);
                            break;
                        case "group":
                        case "group name":
                            EditPointMap(PointTextFieldType.GROUP_NAME, index, false);
                            break;
                        case "time":
                        case "time created":
                        case "datetime":
                            EditPointMap(PointTextFieldType.TIME, index, false);
                            break;
                        case "meta":
                        case "metadata":
                        case "metacn":
                        case "meta cn":
                        case "metadata cn":
                            EditPointMap(PointTextFieldType.META_CN, index, false);
                            break;
                        case "onbnd":
                        case "on bnd":
                        case "onboundary":
                        case "on boundary":
                        case "boundary":
                        case "bnd":
                            EditPointMap(PointTextFieldType.ONBND, index, false);
                            break;
                        case "x":
                        case "unadjx":
                            EditPointMap(PointTextFieldType.UNADJX, index, false);
                            break;
                        case "y":
                        case "unadjy":
                            EditPointMap(PointTextFieldType.UNADJY, index, false);
                            break;
                        case "z":
                        case "unadjz":
                            EditPointMap(PointTextFieldType.UNADJZ, index, false);
                            break;
                        case "manacc":
                        case "man acc":
                        case "manualacc":
                        case "manual acc":
                        case "manacc (m)":
                        case "man acc (m)":
                            EditPointMap(PointTextFieldType.MAN_ACC, index, false);
                            break;
                        case "lat":
                        case "latitude":
                            EditPointMap(PointTextFieldType.LATITUDE, index, false);
                            break;
                        case "lon":
                        case "long":
                        case "longitude":
                            EditPointMap(PointTextFieldType.LONGITUDE, index, false);
                            break;
                        case "elev":
                        case "elev (m)":
                        case "elevation":
                        case "elevation (m)":
                            EditPointMap(PointTextFieldType.ELEVATION, index, false);
                            break;
                        case "rmser":
                            EditPointMap(PointTextFieldType.RMSER, index, false);
                            break;
                        case "fwdaz":
                        case "fwd az":
                        case "fwd azimuth":
                        case "forward az":
                        case "forward azimuth":
                            EditPointMap(PointTextFieldType.FWD_AZ, index, false);
                            break;
                        case "bkaz":
                        case "bk az":
                        case "bk azimuth":
                        case "back az":
                        case "back azimuth":
                        case "backward az":
                        case "backward azimuth":
                            EditPointMap(PointTextFieldType.BK_AZ, index, false);
                            break;
                        case "slope dist":
                        case "slope distance":
                            EditPointMap(PointTextFieldType.SLOPE_DIST, index, false);
                            break;
                        case "dist uom":
                        case "slope dist uom":
                        case "slope d type":
                        case "slope dist type":
                        case "slope distance type":
                            EditPointMap(PointTextFieldType.SLOPE_DIST_TYPE, index, false);
                            break;
                        case "slp ang":
                        case "slp angle":
                        case "slope angle":
                            EditPointMap(PointTextFieldType.SLOPE_ANG, index, false);
                            break;
                        case "angle uom":
                        case "slope ang uom":
                        case "slope a type":
                        case "slope ang type":
                        case "slope angle type":
                            EditPointMap(PointTextFieldType.SLOPE_ANG_TYPE, index, false);
                            break;
                        case "parent":
                        case "parent cn":
                            EditPointMap(PointTextFieldType.PARENT_CN, index, false);
                            break;
                        case "cmt":
                        case "comment":
                            EditPointMap(PointTextFieldType.COMMENT, index, false);
                            break;
                        case "cn":
                        case "point cn":
                            EditPointMap(PointTextFieldType.CN, index, false);
                            break;
                        case "poly cn":
                        case "polygon cn":
                            EditPointMap(PointTextFieldType.POLY_CN, index, false);
                            break;
                        case "group cn":
                            EditPointMap(PointTextFieldType.GROUP_CN, index, false);
                            break;
                    }

                    index++;
                }
            }

            public void EditPointMap(PointTextFieldType field, int index, bool replace = true)
            {
                if (!_PointMapping.ContainsKey(field))
                {
                    _PointMapping.Add(field, index);
                }
                else if (replace)
                {
                    _PointMapping[field] = index;
                }
            }
        }
    }

    internal static class CsvParseExts
    {
        internal static string UnScrub(this string text)
        {
            if (text != null)
                return text.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\"\"", "\"");
            return String.Empty;
        }
    }
}
