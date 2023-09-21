using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.DAL
{
    public interface IReadOnlyTtDataLayer
    {
        bool HandlesAllPointTypes { get; }

        String FilePath { get; }

        #region Points
        TtPoint GetPoint(String cn = null, bool linked = false);
        IEnumerable<TtPoint> GetPoints(String unitCN = null, bool linked = false);
        #endregion


        #region Units
        bool HasUnits();

        IEnumerable<TtUnit> GetUnits();
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


        #region Unit Attributes
        IEnumerable<UnitGraphicOptions> GetUnitGraphicOptions();
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

        int GetPointCount(params string[] unitCNs);
        #endregion
    }
}
