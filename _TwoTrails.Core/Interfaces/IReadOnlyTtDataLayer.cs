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
        IEnumerable<TtPoint> GetPoints(String polyCN = null, bool linked = false);
        #endregion


        #region Polygons
        bool HasPolygons();

        IEnumerable<TtPolygon> GetPolygons();
        #endregion


        #region Metadata
        IEnumerable<TtMetadata> GetMetadata();
        #endregion


        #region Groups
        IEnumerable<TtGroup> GetGroups();
        #endregion


        #region TTNmeaBurst
        IEnumerable<TtNmeaBurst> GetNmeaBursts(String pointCN = null);
        #endregion


        #region Project
        TtProjectInfo GetProjectInfo();
        #endregion


        #region Polygon Attributes
        IEnumerable<PolygonGraphicOptions> GetPolygonGraphicOptions();
        #endregion


        #region Media
        IEnumerable<TtImage> GetPictures(String pointCN);
        #endregion


        #region Activity
        IEnumerable<TtUserActivity> GetUserActivity();
        #endregion


        #region Util
        bool RequiresUpgrade { get; }
        #endregion
    }
}
