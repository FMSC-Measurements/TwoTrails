using System;
using System.Collections.Generic;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core
{
    public interface ITtManager
    {
        TtMetadata DefaultMetadata { get; }
        TtGroup MainGroup { get; }

        bool HasDataDictionary { get; }

        int UnitCount { get; }
        int PointCount { get; }

        void RebuildUnit(TtUnit unit, bool reindex = false);
        void RecalculateUnits();

        bool PointExists(String pointCN);
        TtPoint GetPoint(String pointCN);
        TtPoint GetNextPoint(TtPoint point);
        List<TtPoint> GetPoints(String unitCN = null);
        void AddPoint(TtPoint point);
        void AddPoints(IEnumerable<TtPoint> points);
        void ReplacePoint(TtPoint point);
        void ReplacePoints(IEnumerable<TtPoint> replacePoints);
        void MovePointsToUnit(IEnumerable<TtPoint> points, TtUnit targetUnit, int insertIndex);
        void DeletePoint(TtPoint point);
        void DeletePoints(IEnumerable<TtPoint> points);
        void DeletePointsInUnit(string unitCN);

        bool UnitExists(string unitCN);
        TtUnit GetUnit(string unitCN);
        List<TtUnit> GetUnits();
        void AddUnit(TtUnit unit);
        void DeleteUnit(TtUnit unit);

        bool MetadataExists(string metaCN);
        List<TtMetadata> GetMetadata();
        void AddMetadata(TtMetadata metadata);
        void DeleteMetadata(TtMetadata metadata);

        bool GroupExists(string groupCN);
        List<TtGroup> GetGroups();
        void AddGroup(TtGroup group);
        void DeleteGroup(TtGroup group);


        bool NmeaExists(string nmeaCN);
        List<TtNmeaBurst> GetNmeaBursts(string pointCN = null);
        List<TtNmeaBurst> GetNmeaBursts(IEnumerable<string> pointCNs);
        void AddNmeaBurst(TtNmeaBurst burst);
        void AddNmeaBursts(IEnumerable<TtNmeaBurst> bursts);
        void DeleteNmeaBursts(string pointCN);

        List<TtImage> GetImages(String pointCN = null);
        void InsertMedia(TtMedia media);
        void DeleteMedia(TtMedia media);

        List<TtUserAction> GetUserActions();


        DataDictionaryTemplate GetDataDictionaryTemplate();


        UnitGraphicOptions GetUnitGraphicOption(string unitCN);
        List<UnitGraphicOptions> GetUnitGraphicOptions();

        UnitGraphicOptions GetDefaultUnitGraphicOption();
    }
}
