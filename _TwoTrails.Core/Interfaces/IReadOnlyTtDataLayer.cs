using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public interface IReadOnlyTtDataLayer
    {
        #region Points
        List<TtPoint> GetPoints(String polyCN = null);
        #endregion


        #region Polygons
        bool HasPolygons();

        List<TtPolygon> GetPolygons();
        #endregion


        #region Metadata
        List<TtMetadata> GetMetadata();
        #endregion


        #region Groups
        List<TtGroup> GetGroups();
        #endregion


        #region TTNmeaBurst
        List<TtNmeaBurst> GetNmeaBursts(String pointCN = null);
        #endregion


        #region Project
        TtProjectInfo GetProjectInfo();
        #endregion


        #region Polygon Attributes
        List<PolygonGraphicOptions> GetPolygonGraphicOptions();
        #endregion


        #region Media
        List<TtImage> GetPictures(String pointCN);
        #endregion


        #region Activity
        List<TtUserActivity> GetUserActivity();
        #endregion


        #region Util
        bool RequiresUpgrade { get; }
        #endregion
    }
}
