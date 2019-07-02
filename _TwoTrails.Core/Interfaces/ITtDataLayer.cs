using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public interface ITtDataLayer : IReadOnlyTtDataLayer
    {
        bool HasDataDictionary { get; }

        #region Points
        bool InsertPoint(TtPoint point);
        int InsertPoints(IEnumerable<TtPoint> points);

        bool UpdatePoint(Tuple<TtPoint, TtPoint> point);
        int UpdatePoints(IEnumerable<Tuple<TtPoint, TtPoint>> points);

        bool ChangePointOp(TtPoint point, TtPoint oldPoint);

        bool DeletePoint(TtPoint point);
        int DeletePoints(IEnumerable<TtPoint> points);
        #endregion


        #region Polygons
        bool InsertPolygon(TtPolygon polygon);
        int InsertPolygons(IEnumerable<TtPolygon> polygons);

        bool UpdatePolygon(TtPolygon polygon);
        int UpdatePolygons(IEnumerable<TtPolygon> polygons);

        bool DeletePolygon(TtPolygon polygon);
        int DeletePolygons(IEnumerable<TtPolygon> polygons);
        #endregion


        #region Metadata
        bool InsertMetadata(TtMetadata metadata);
        int InsertMetadata(IEnumerable<TtMetadata> metadata);

        bool UpdateMetadata(TtMetadata metadata);
        int UpdateMetadata(IEnumerable<TtMetadata> metadata);

        bool DeleteMetadata(TtMetadata metadata);
        int DeleteMetadata(IEnumerable<TtMetadata> metadata);
        #endregion


        #region Groups
        bool InsertGroup(TtGroup group);
        int InsertGroups(IEnumerable<TtGroup> groups);

        bool UpdateGroup(TtGroup group);
        int UpdateGroups(IEnumerable<TtGroup> groups);

        bool DeleteGroup(TtGroup group);
        int DeleteGroups(IEnumerable<TtGroup> groups);
        #endregion


        #region TTNmeaBurst
        bool InsertNmeaBurst(TtNmeaBurst burst);
        int InsertNmeaBursts(IEnumerable<TtNmeaBurst> bursts);
        
        bool UpdateNmeaBurst(TtNmeaBurst burst);
        int UpdateNmeaBursts(IEnumerable<TtNmeaBurst> bursts);

        int DeleteNmeaBursts(String pointCN);
        #endregion


        #region Project
        Version GetDataVersion();
        bool UpdateProjectInfo(TtProjectInfo properties);
        #endregion


        #region Polygon Attributes
        bool InsertPolygonGraphicOption(PolygonGraphicOptions option);

        bool DeletePolygonGraphicOption(PolygonGraphicOptions option);
        #endregion


        #region Activity
        void InsertActivity(TtUserAction activity);
        #endregion


        #region DataDictionary
        bool CreateDataDictionary(DataDictionaryTemplate template);
        bool ModifyDataDictionary(DataDictionaryTemplate template);
        #endregion


        #region Util
        void FixErrors(bool removeErrors = false);
        bool HasErrors();
        DalError GetErrors();
        #endregion
    }

    [Flags]
    public enum DalError
    {
        None            = 0,
        PointIndexes    = 1 << 0,
        NullAdjLocs     = 1 << 1,
        MissingPolygon  = 1 << 2,
        MissingMetadata = 1 << 3,
        MissingGroup    = 1 << 4
    }
}
