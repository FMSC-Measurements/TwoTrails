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
        protected TtManager _Manager;

        public event EventHandler HistoryChanged;
        
        public ReadOnlyObservableCollection<TtPoint> Points { get { return _Manager.Points; } }
        public ReadOnlyObservableCollection<TtPolygon> Polygons { get { return _Manager.Polygons; } }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get { return _Manager.Metadata; } }
        public ReadOnlyObservableCollection<TtGroup> Groups { get { return _Manager.Groups; } }

        private Stack<ITtCommand> _UndoStack = new Stack<ITtCommand>();
        private Stack<ITtCommand> _RedoStack = new Stack<ITtCommand>();
        

        public bool CanUndo { get { return _UndoStack.Count > 0; } }
        public bool CanRedo { get { return _RedoStack.Count > 0; } }
        

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
                OnHistoryChanged();
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
            OnHistoryChanged();
        }

        public void Undo()
        {
            if (CanUndo)
            {
                ITtCommand hist = _UndoStack.Pop();
                _RedoStack.Push(hist);
                hist.Undo();

                OnHistoryChanged();
            }
        }

        public void Undo(int levels)
        {
            if (CanUndo)
            {
                ITtCommand hist;

                for (int i = 0; i < levels && CanUndo; i++)
                {
                    hist = _UndoStack.Pop();
                    _RedoStack.Push(hist);
                    hist.Undo();
                }

                OnHistoryChanged();
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                ITtCommand hist = _RedoStack.Pop();
                AddCommand(hist);
                hist.Redo();

                OnHistoryChanged();
            }
        }

        public void Redo(int levels)
        {
            if (CanRedo)
            {
                ITtCommand hist;

                for (int i = 0; i < levels && CanRedo; i++)
                {
                    hist = _RedoStack.Pop();
                    AddCommand(hist);
                    hist.Redo();
                }

                OnHistoryChanged();
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

        public List<TtPolygon> GetPolyons()
        {
            return _Manager.GetPolyons();
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

        public void AddPoints(List<TtPoint> points)
        {
            AddCommand(new AddTtPointsCommand(points, _Manager));
        }

        public void DeletePoint(TtPoint point)
        {
            AddCommand(new DeleteTtPointCommand(point, _Manager));
        }

        public void DeletePoints(List<TtPoint> points)
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


        #region Editing

        public void EditPoint(TtPoint point, PropertyInfo property, object newValue)
        {
            AddCommand(new EditTtPointCommand(point, property, newValue));
        }

        public void EditPoints(IEnumerable<TtPoint> points, PropertyInfo property, object newValue)
        {
            AddCommand(new EditTtPointsCommand(points, property, newValue));
        }

        public void EditPoints(IEnumerable<TtPoint> points, PropertyInfo property, List<object> newValues)
        {
            AddCommand(new EditTtPointsMultiValueCommand(points, property, newValues));
        }

        public void EditPoints(IEnumerable<TtPoint> points, List<PropertyInfo> properties, object newValue)
        {
            AddCommand(new EditTtPointsMultiPropertyCommand(points, properties, points.Select(p => newValue).Cast<object>()));
        }

        public void EditPoints(IEnumerable<TtPoint> points, List<PropertyInfo> properties, List<object> newValues)
        {
            AddCommand(new EditTtPointsMultiPropertyCommand(points, properties, newValues));
        }

        #endregion


        private void OnHistoryChanged()
        {
            HistoryChanged?.Invoke(this, new EventArgs());
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }
    }
}
