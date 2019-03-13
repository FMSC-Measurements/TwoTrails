﻿using System;
using System.Collections.Generic;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public interface ITtManager
    {
        TtMetadata DefaultMetadata { get; }
        TtGroup MainGroup { get; }
        
        bool HasDataDictionary { get; }

        int PolygonCount { get; }
        int PointCount { get; }

        void RebuildPolygon(TtPolygon polygon, bool reindex = false);
        void RecalculatePolygons();

        bool PointExists(String pointCN);
        TtPoint GetPoint(String pointCN);
        List<TtPoint> GetPoints(String polyCN = null);
        void AddPoint(TtPoint point);
        void AddPoints(IEnumerable<TtPoint> points);
        void ReplacePoint(TtPoint point);
        void ReplacePoints(IEnumerable<TtPoint> replacePoints);
        void MovePointsToPolygon(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex);
        void DeletePoint(TtPoint point);
        void DeletePoints(IEnumerable<TtPoint> points);
        void DeletePointsInPolygon(string polyCN);

        bool PolygonExists(string polyCN);
        TtPolygon GetPolygon(string polyCN);
        List<TtPolygon> GetPolygons();
        void AddPolygon(TtPolygon polygon);
        void DeletePolygon(TtPolygon polygon);

        bool MetadataExists(string metaCN);
        List<TtMetadata> GetMetadata();
        void AddMetadata(TtMetadata metadata);
        void DeleteMetadata(TtMetadata metadata);

        bool GroupExists(string groupCN);
        List<TtGroup> GetGroups();
        void AddGroup(TtGroup group);
        void DeleteGroup(TtGroup group);


        List<TtNmeaBurst> GetNmeaBursts(string pointCN = null);
        List<TtNmeaBurst> GetNmeaBursts(IEnumerable<string> pointCNs);
        void AddNmeaBurst(TtNmeaBurst burst);
        void AddNmeaBursts(IEnumerable<TtNmeaBurst> bursts);
        void DeleteNmeaBursts(string pointCN);
        
        List<TtImage> GetImages(String pointCN = null);
        void InsertMedia(TtMedia media);
        void DeleteMedia(TtMedia media);


        DataDictionaryTemplate GetDataDictionaryTemplate();
        

        PolygonGraphicOptions GetPolygonGraphicOption(string polyCN);
        List<PolygonGraphicOptions> GetPolygonGraphicOptions();

        PolygonGraphicOptions GetDefaultPolygonGraphicOption();


        void UpdateDataAction(DataActionType action, string notes = null);
    }
}
