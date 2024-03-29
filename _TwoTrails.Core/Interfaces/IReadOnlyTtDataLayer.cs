﻿using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public interface IReadOnlyTtDataLayer
    {
        bool HandlesAllPointTypes { get; }

        String FilePath { get; }

        #region Points
        TtPoint GetPoint(String cn = null, bool linked = false);
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
        TtNmeaBurst GetNmeaBurst(String nmeaCN);
        IEnumerable<TtNmeaBurst> GetNmeaBursts(String pointCN = null);
        IEnumerable<TtNmeaBurst> GetNmeaBursts(IEnumerable<String> pointCNs);
        #endregion


        #region Project
        TtProjectInfo GetProjectInfo();
        #endregion


        #region Polygon Attributes
        IEnumerable<PolygonGraphicOptions> GetPolygonGraphicOptions();
        #endregion


        #region Activity
        IEnumerable<TtUserAction> GetUserActivity();
        #endregion


        #region DataDictionary
        DataDictionaryTemplate GetDataDictionaryTemplate();

        DataDictionary GetExtendedDataForPoint(string pointCN);

        IEnumerable<DataDictionary> GetExtendedData();
        #endregion


        #region Util
        bool RequiresUpgrade { get; }

        int GetPointCount(params string[] polyCNs);
        #endregion
    }
}
