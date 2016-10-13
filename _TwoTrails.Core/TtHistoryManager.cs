using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class TtHistoryManager : NotifyPropertyChangedEx, ITtManager
    {
        private TtManager _Manager;
        public TtManager BaseManager { get { return _Manager; } }

        public event EventHandler<HistoryEventArgs> HistoryChanged;
        
        public ReadOnlyObservableCollection<TtPoint> Points { get { return _Manager.Points; } }
        public ReadOnlyObservableCollection<TtPolygon> Polygons { get { return _Manager.Polygons; } }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get { return _Manager.Metadata; } }
        public ReadOnlyObservableCollection<TtGroup> Groups { get { return _Manager.Groups; } }

        private Stack<ITtCommand> _UndoStack = new Stack<ITtCommand>();
        private Stack<ITtCommand> _RedoStack = new Stack<ITtCommand>();
        

        public bool CanUndo { get { return _UndoStack.Count > 0; } }
        public bool CanRedo { get { return _RedoStack.Count > 0; } }

        public TtGroup MainGroup { get { return _Manager.MainGroup; } }

        public TtMetadata DefaultMetadata { get { return _Manager.DefaultMetadata; } }


        public TtHistoryManager(TtManager manager)
        {
            _Manager = manager;
            List<TtPoint> points = _Manager.GetPoints();
            points.Sort();
        }


        public void Save()
        {
            try
            {
                _Manager.Save();
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
                AddCommand(command);
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
                    AddCommand(command);
                    command.Redo();
                }

                OnHistoryChanged(HistoryEventType.Undone, requireRefresh);
            }
        }
        #endregion


        public bool PointExists(string pointCN)
        {
            return _Manager.PointExists(pointCN);
        }

        public TtPoint GetPoint(string pointCN)
        {
            return _Manager.GetPoint(pointCN);
        }

        public List<TtPoint> GetPoints(string polyCN = null)
        {
            return _Manager.GetPoints(polyCN);
        }

        public TtPolygon GetPolygon(string polyCN)
        {
            return _Manager.GetPolygon(polyCN);
        }

        public List<TtPolygon> GetPolygons()
        {
            return _Manager.GetPolygons();
        }

        public List<TtMetadata> GetMetadata()
        {
            return _Manager.GetMetadata();
        }

        public List<TtGroup> GetGroups()
        {
            return _Manager.GetGroups();
        }


        #region Adding and Deleting
        public void AddPoint(TtPoint point)
        {
            AddCommand(new AddTtPointCommand(point, _Manager));
        }

        public void AddPoints(IEnumerable<TtPoint> points)
        {
            AddCommand(new AddTtPointsCommand(points, _Manager));
        }

        public void DeletePoint(TtPoint point)
        {
            AddCommand(new DeleteTtPointCommand(point, _Manager));
        }

        public void DeletePoints(IEnumerable<TtPoint> points)
        {
            AddCommand(new DeleteTtPointsCommand(points, _Manager));
        }


        public void AddPolygon(TtPolygon polygon)
        {
            AddCommand(new AddTtPolygonCommand(polygon, _Manager));
        }

        public void DeletePolygon(TtPolygon polygon)
        {
            AddCommand(new DeleteTtPolygonCommand(polygon, _Manager));
        }


        public void AddMetadata(TtMetadata metadata)
        {
            AddCommand(new AddTtMetadataCommand(metadata, _Manager));
        }

        public void DeleteMetadata(TtMetadata metadata)
        {
            AddCommand(new DeleteTtMetadataCommand(metadata, _Manager));
        }


        public void AddGroup(TtGroup group)
        {
            AddCommand(new AddTtGroupCommand(group, _Manager));
        }

        public void DeleteGroup(TtGroup group)
        {
            AddCommand(new DeleteTtGroupCommand(group, _Manager));
        }
        #endregion


        public void CreateQuondamLinks(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex, bool? bndMode = null, bool reverse = false)
        {
            AddCommand(new CreateQuondamsCommand(reverse ? points.Reverse() : points, _Manager, targetPolygon, insertIndex, bndMode));
        }

        public void CreateCorridor(IEnumerable<TtPoint> points, TtPolygon targetPolygon)
        {
            AddCommand(new CreateCorridorCommand(points, targetPolygon, _Manager));
        }

        public void MovePointsToPolygon(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex)
        {
            AddCommand(new MovePointsCommand(points, _Manager, targetPolygon, insertIndex));
        }

        public void MovePointsToPolygon(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex, bool reverse)
        {
            MovePointsToPolygon(reverse? points.Reverse() : points, targetPolygon, insertIndex);
        }


        #region Editing

        public void EditPoint(TtPoint point, PropertyInfo property, object newValue)
        {
            AddCommand(new EditTtPointCommand(point, property, newValue));
        }

        public void EditPoint(TtPoint point, IEnumerable<PropertyInfo> properties, IEnumerable<object> newValues)
        {
            AddCommand(new EditTtPointMultiPropertyCommand(point, properties, newValues));
        }


        public void EditPoints(IEnumerable<TtPoint> points, PropertyInfo property, object newValue)
        {
            AddCommand(new EditTtPointsCommand(points, property, newValue));
        }

        public void EditPointsMultiValues(IEnumerable<TtPoint> points, PropertyInfo property, IEnumerable<object> newValues)
        {
            AddCommand(new EditTtPointsMultiValueCommand(points, property, newValues));
        }

        public void EditPoints(IEnumerable<TtPoint> points, List<PropertyInfo> properties, object newValue)
        {
            AddCommand(new EditTtPointsMultiPropertyCommand(points, properties, points.Select(p => newValue).Cast<object>()));
        }

        public void EditPointsMultiValues(IEnumerable<TtPoint> points, List<PropertyInfo> properties, List<object> newValues)
        {
            AddCommand(new EditTtPointsMultiPropertyCommand(points, properties, newValues));
        }


        public void ResetPoint(TtPoint point)
        {
            AddCommand(new ResetTtPointCommand(point, _Manager));
        }

        public void ResetPoints(IEnumerable<TtPoint> points)
        {
            AddCommand(new ResetTtPointsCommand(points, _Manager));
        }

        #endregion


        private void OnHistoryChanged(HistoryEventType historyEventType, bool requireRefresh)
        {
            HistoryChanged?.Invoke(this, new HistoryEventArgs(historyEventType, requireRefresh));
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }
        

        void ITtManager.ReplacePoint(TtPoint point)
        {
            _Manager.ReplacePoint(point);
        }

        void ITtManager.ReplacePoints(IEnumerable<TtPoint> replacePoints)
        {
            _Manager.ReplacePoints(replacePoints);
        }

        void ITtManager.ReindexPolygon(TtPolygon polygon)
        {
            _Manager.ReindexPolygon(polygon);
        }

        void ITtManager.RebuildPolygon(TtPolygon polygon)
        {
            _Manager.RebuildPolygon(polygon);
        }

        public void RecalculatePolygons()
        {
            _Manager.RecalculatePolygons();
        }




        public PolygonGraphicOptions GetPolygonGraphicOption(string polyCN)
        {
            return _Manager.GetPolygonGraphicOption(polyCN);
        }

        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return _Manager.GetPolygonGraphicOptions();
        }


        List<TtNmeaBurst> ITtManager.GetNmeaBursts(string pointCN = null)
        {
            return _Manager.GetNmeaBursts(pointCN);
        }

        void ITtManager.AddNmeaBurst(TtNmeaBurst burst)
        {
            _Manager.AddNmeaBurst(burst);
        }

        void ITtManager.AddNmeaBursts(IEnumerable<TtNmeaBurst> bursts)
        {
            _Manager.AddNmeaBursts(bursts);
        }

        void ITtManager.DeleteNmeaBursts(string pointCN)
        {
            _Manager.DeleteNmeaBursts(pointCN);
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
