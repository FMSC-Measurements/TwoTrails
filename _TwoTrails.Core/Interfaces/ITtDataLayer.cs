using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public interface ITtDataLayer
    {
        String FilePath { get; }

        #region Points
        List<TtPoint> GetPoints(String polyCN = null);
        List<TtPoint> GetPointsUnlinked(String polyCN = null);

        bool InsertPoint(TtPoint point);
        int InsertPoints(IEnumerable<TtPoint> points);

        bool UpdatePoint(Tuple<TtPoint, TtPoint> point);
        int UpdatePoints(IEnumerable<Tuple<TtPoint, TtPoint>> points);

        bool ChangePointOp(TtPoint point, TtPoint oldPoint);

        bool DeletePoint(TtPoint point);
        int DeletePoints(IEnumerable<TtPoint> points);
        #endregion


        #region Polygons
        bool HasPolygons();

        List<TtPolygon> GetPolygons();

        bool InsertPolygon(TtPolygon polygon);
        int InsertPolygons(IEnumerable<TtPolygon> polygons);

        bool UpdatePolygon(TtPolygon polygon);
        int UpdatePolygons(IEnumerable<TtPolygon> polygons);

        bool DeletePolygon(TtPolygon polygon);
        int DeletePolygons(IEnumerable<TtPolygon> polygons);
        #endregion


        #region Metadata
        List<TtMetadata> GetMetadata();

        bool InsertMetadata(TtMetadata metadata);
        int InsertMetadata(IEnumerable<TtMetadata> metadata);

        bool UpdateMetadata(TtMetadata metadata);
        int UpdateMetadata(IEnumerable<TtMetadata> metadata);

        bool DeleteMetadata(TtMetadata metadata);
        int DeleteMetadata(IEnumerable<TtMetadata> metadata);
        #endregion


        #region Groups
        List<TtGroup> GetGroups();

        bool InsertGroup(TtGroup group);
        int InsertGroups(IEnumerable<TtGroup> groups);

        bool UpdateGroup(TtGroup group);
        int UpdateGroups(IEnumerable<TtGroup> groups);

        bool DeleteGroup(TtGroup group);
        int DeleteGroups(IEnumerable<TtGroup> groups);
        #endregion


        #region TTNmeaBurst
        List<TtNmeaBurst> GetNmeaBursts(String pointCN = null);

        bool InsertNmeaBurst(TtNmeaBurst burst);
        bool InsertNmeaBursts(IEnumerable<TtNmeaBurst> bursts);
        
        bool UpdateNmeaBurst(TtNmeaBurst burst);
        int UpdateNmeaBursts(IEnumerable<TtNmeaBurst> bursts);

        void DeleteNmeaBursts(String pointCN);
        #endregion


        #region Project
        TtProjectInfo GetProjectInfo();
        Version GetDataVersion();
        bool InsertProjectInfo(TtProjectInfo properties);
        bool UpdateProjectInfo(TtProjectInfo properties);
        #endregion


        #region Polygon Attributes
        List<PolygonGraphicOptions> GetPolygonGraphicOptions();

        bool InsertPolygonGraphicOption(PolygonGraphicOptions option);

        bool DeletePolygonGraphicOption(PolygonGraphicOptions option);
        #endregion


        #region Media
        List<TtImage> GetPictures(String pointCN);

        bool InsertMedia(TtMedia media);

        bool UpdateMedia(TtMedia media);

        bool DeleteMedia(TtMedia media);
        #endregion


        #region Activity
        void InsertActivity(TtUserActivity activity);
        List<TtUserActivity> GetUserActivity();
        #endregion


        #region Util
        //TODO Util
        bool RequiresUpgrade { get; }

        bool Duplicate(ITtDataLayer dataLayer);

        bool Clean();
        #endregion
    }
}
