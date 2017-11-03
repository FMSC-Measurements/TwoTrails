using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;
using TwoTrails.Core.Media;

namespace TwoTrails.Core
{
    public class TtHistoryManager : NotifyPropertyChangedEx, ITtManager
    {
        public TtManager BaseManager { get; private set; }

        public event EventHandler<HistoryEventArgs> HistoryChanged;
        
        public ReadOnlyObservableCollection<TtPoint> Points { get { return BaseManager.Points; } }
        public ReadOnlyObservableCollection<TtPolygon> Polygons { get { return BaseManager.Polygons; } }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get { return BaseManager.Metadata; } }
        public ReadOnlyObservableCollection<TtGroup> Groups { get { return BaseManager.Groups; } }

        private Stack<ITtCommand> _UndoStack = new Stack<ITtCommand>();
        private Stack<ITtCommand> _RedoStack = new Stack<ITtCommand>();
        

        public bool CanUndo { get { return _UndoStack.Count > 0; } }
        public bool CanRedo { get { return _RedoStack.Count > 0; } }

        public TtGroup MainGroup { get { return BaseManager.MainGroup; } }

        public TtMetadata DefaultMetadata { get { return BaseManager.DefaultMetadata; } }


        public TtHistoryManager(TtManager manager)
        {
            BaseManager = manager;
        }


        public void Save()
        {
            try
            {
                BaseManager.Save();
                _UndoStack.Clear();
                _RedoStack.Clear();
                OnHistoryChanged(HistoryEventType.Reset, true);
            }
            catch (Exception ex)
            {
                throw new Exception("History Manager unable to save do to: " + ex.Message);
            }
        }


        #region History Management
        protected void AddCommand(ITtCommand command)
        {
            _UndoStack.Push(command);
            _RedoStack.Clear();
            OnHistoryChanged(HistoryEventType.Redone, command.RequireRefresh);
        }

        public void Undo()
        {
            if (CanUndo)
            {
                ITtCommand command = _UndoStack.Pop();
                _RedoStack.Push(command);
                command.Undo();

                OnHistoryChanged(HistoryEventType.Undone, command.RequireRefresh);
            }
        }

        public void Undo(int levels)
        {
            if (CanUndo)
            {
                ITtCommand command;
                bool requireRefresh = false;

                for (int i = 0; i < levels && CanUndo; i++)
                {
                    command = _UndoStack.Pop();
                    requireRefresh |= command.RequireRefresh;
                    _RedoStack.Push(command);
                    command.Undo();
                }


                OnHistoryChanged(HistoryEventType.Undone, requireRefresh);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                ITtCommand command = _RedoStack.Pop();
                _UndoStack.Push(command);   //AddCommand(command);
                command.Redo();

                OnHistoryChanged(HistoryEventType.Undone, command.RequireRefresh);
            }
        }

        public void Redo(int levels)
        {
            if (CanRedo)
            {
                ITtCommand command;
                bool requireRefresh = false;

                for (int i = 0; i < levels && CanRedo; i++)
                {
                    command = _RedoStack.Pop();
                    requireRefresh |= command.RequireRefresh;
                    _UndoStack.Push(command);   //AddCommand(command);
                    command.Redo();
                }

                OnHistoryChanged(HistoryEventType.Undone, requireRefresh);
            }
        }

        public void ClearHistory()
        {
            _RedoStack.Clear();
            _UndoStack.Clear();
            OnPropertyChanged(nameof(CanUndo), nameof(CanRedo));
        }

        private void OnHistoryChanged(HistoryEventType historyEventType, bool requireRefresh)
        {
            HistoryChanged?.Invoke(this, new HistoryEventArgs(historyEventType, requireRefresh));
            OnPropertyChanged(nameof(CanUndo), nameof(CanRedo));
        }
        #endregion


        public bool PointExists(string pointCN)
        {
            return BaseManager.PointExists(pointCN);
        }

        public TtPoint GetPoint(string pointCN)
        {
            return BaseManager.GetPoint(pointCN);
        }

        public List<TtPoint> GetPoints(string polyCN = null)
        {
            return BaseManager.GetPoints(polyCN);
        }

        public TtPolygon GetPolygon(string polyCN)
        {
            return BaseManager.GetPolygon(polyCN);
        }

        public List<TtPolygon> GetPolygons()
        {
            return BaseManager.GetPolygons();
        }

        public List<TtMetadata> GetMetadata()
        {
            return BaseManager.GetMetadata();
        }

        public List<TtGroup> GetGroups()
        {
            return BaseManager.GetGroups();
        }


        #region Adding and Deleting
        public void AddPoint(TtPoint point)
        {
            AddCommand(new AddTtPointCommand(point, BaseManager));
        }

        public void AddPoints(IEnumerable<TtPoint> points)
        {
            AddCommand(new AddTtPointsCommand(points, BaseManager));
        }

        public void DeletePoint(TtPoint point)
        {
            AddCommand(new DeleteTtPointCommand(point, BaseManager));
        }

        public void DeletePoints(IEnumerable<TtPoint> points)
        {
            AddCommand(new DeleteTtPointsCommand(points, BaseManager));
        }

        public void DeletePointsInPolygon(string polyCN)
        {
            AddCommand(new DeleteTtPointsCommand(BaseManager.GetPoints(polyCN), BaseManager));
        }


        public void AddPolygon(TtPolygon polygon)
        {
            AddCommand(new AddTtPolygonCommand(polygon, BaseManager));
        }

        public void DeletePolygon(TtPolygon polygon)
        {
            AddCommand(new DeleteTtPolygonCommand(polygon, BaseManager));
        }


        public void AddMetadata(TtMetadata metadata)
        {
            AddCommand(new AddTtMetadataCommand(metadata, BaseManager));
        }

        public void DeleteMetadata(TtMetadata metadata)
        {
            AddCommand(new DeleteTtMetadataCommand(metadata, BaseManager));
        }


        public void AddGroup(TtGroup group)
        {
            AddCommand(new AddTtGroupCommand(group, BaseManager));
        }

        public void DeleteGroup(TtGroup group)
        {
            AddCommand(new DeleteTtGroupCommand(group, BaseManager));
        }
        #endregion


        public void CreateQuondamLinks(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex, bool? bndMode = null, bool reverse = false)
        {
            AddCommand(new CreateQuondamsCommand(reverse ? points.Reverse() : points, BaseManager, targetPolygon, insertIndex, bndMode));
        }

        public void CreateCorridor(IEnumerable<TtPoint> points, TtPolygon targetPolygon)
        {
            AddCommand(new CreateCorridorCommand(points, targetPolygon, BaseManager));
        }

        public void MovePointsToPolygon(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex)
        {
            AddCommand(new MovePointsCommand(points, BaseManager, targetPolygon, insertIndex));
        }

        public void MovePointsToPolygon(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex, bool reverse)
        {
            MovePointsToPolygon(reverse? points.Reverse() : points, targetPolygon, insertIndex);
        }


        #region Editing

        public void EditPoint<T>(TtPoint point, PropertyInfo property, T newValue)
        {
            AddCommand(new EditTtPointCommand<T>(point, property, newValue));
        }

        public void EditPoint(TtPoint point, IEnumerable<PropertyInfo> properties, IEnumerable<object> newValues)
        {
            AddCommand(new EditTtPointMultiPropertyCommand(point, properties, newValues));
        }


        public void EditPoints<T>(IEnumerable<TtPoint> points, PropertyInfo property, T newValue)
        {
            AddCommand(new EditTtPointsCommand(points, property, newValue));
        }

        public void EditPointsMultiValues<T>(IEnumerable<TtPoint> points, PropertyInfo property, IEnumerable<T> newValues)
        {
            AddCommand(new EditTtPointsMultiValueCommand<T>(points, property, newValues));
        }

        public void EditPoints<T>(IEnumerable<TtPoint> points, List<PropertyInfo> properties, T newValue)
        {
            AddCommand(new EditTtPointsMultiPropertyCommand<T>(points, properties, points.Select(p => newValue)));
        }

        public void EditPointsMultiValues(IEnumerable<TtPoint> points, List<PropertyInfo> properties, List<object> newValues)
        {
            AddCommand(new EditTtPointsMultiPropertyCommand(points, properties, newValues));
        }


        public void ResetPoint(TtPoint point)
        {
            AddCommand(new ResetTtPointCommand(point, BaseManager));
        }

        public void ResetPoints(IEnumerable<TtPoint> points)
        {
            AddCommand(new ResetTtPointsCommand(points, BaseManager));
        }

        #endregion
        

        public void ReplacePoint(TtPoint point)
        {

            AddCommand(new ReplaceTtPointCommand(point, BaseManager));
        }

        public void ReplacePoints(IEnumerable<TtPoint> points)
        {
            AddCommand(new ReplaceTtPointsCommand(points, BaseManager));
        }
        
        public void RebuildPolygon(TtPolygon polygon, bool reindex = false)
        {
            BaseManager.RebuildPolygon(polygon, reindex);
        }

        public void RecalculatePolygons()
        {
            BaseManager.RecalculatePolygons();
        }




        public PolygonGraphicOptions GetPolygonGraphicOption(string polyCN)
        {
            return BaseManager.GetPolygonGraphicOption(polyCN);
        }

        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return BaseManager.GetPolygonGraphicOptions();
        }


        List<TtNmeaBurst> ITtManager.GetNmeaBursts(string pointCN)
        {
            return BaseManager.GetNmeaBursts(pointCN);
        }

        void ITtManager.AddNmeaBurst(TtNmeaBurst burst)
        {
            BaseManager.AddNmeaBurst(burst);
        }

        void ITtManager.AddNmeaBursts(IEnumerable<TtNmeaBurst> bursts)
        {
            BaseManager.AddNmeaBursts(bursts);
        }

        void ITtManager.DeleteNmeaBursts(string pointCN)
        {
            BaseManager.DeleteNmeaBursts(pointCN);
        }


        public List<TtImage> GetImages(string pointCN)
        {
            throw new NotImplementedException();
        }

        public void InsertMedia(TtMedia media)
        {
            throw new NotImplementedException();
        }

        public void DeleteMedia(TtMedia media)
        {
            throw new NotImplementedException();
        }

        public DataDictionaryTemplate GetDataDictionaryTemplate()
        {
            return BaseManager.GetDataDictionaryTemplate();
        }
    }

    public enum HistoryEventType
    {
        Undone,
        Redone,
        Reset
    }

    public class HistoryEventArgs : EventArgs
    {
        public bool RequireRefresh { get; }
        public HistoryEventType HistoryEventType { get; }

        public HistoryEventArgs(HistoryEventType historyEventType, bool requireRefresh)
        {
            RequireRefresh = requireRefresh;
            HistoryEventType = historyEventType;
        }
    }
}
