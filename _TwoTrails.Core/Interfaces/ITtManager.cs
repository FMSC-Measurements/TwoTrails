using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public interface ITtManager
    {
        TtMetadata DefaultMetadata { get; }
        TtGroup MainGroup { get; }
        
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

        TtPolygon GetPolygon(string polyCN);
        List<TtPolygon> GetPolygons();
        void AddPolygon(TtPolygon polygon);
        void DeletePolygon(TtPolygon polygon);

        List<TtMetadata> GetMetadata();
        void AddMetadata(TtMetadata metadata);
        void DeleteMetadata(TtMetadata metadata);

        List<TtGroup> GetGroups();
        void AddGroup(TtGroup group);
        void DeleteGroup(TtGroup group);


        List<TtNmeaBurst> GetNmeaBursts(string pointCN = null);
        void AddNmeaBurst(TtNmeaBurst burst);
        void AddNmeaBursts(IEnumerable<TtNmeaBurst> bursts);
        void DeleteNmeaBursts(string pointCN);
        
        List<TtImage> GetImages(String pointCN);
        void InsertMedia(TtMedia media);
        void DeleteMedia(TtMedia media);


        PolygonGraphicOptions GetPolygonGraphicOption(string polyCN);
        List<PolygonGraphicOptions> GetPolygonGraphicOptions();
    }
}
