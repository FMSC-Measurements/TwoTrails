using System.Collections.Generic;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class TtHistoryManager : ITtManager
    {
        protected TtManager _Manager;

        private Stack<ITtCommand> _UndoStack = new Stack<ITtCommand>();
        private Stack<ITtCommand> _RedoStack = new Stack<ITtCommand>();

        public bool CanUndo { get { return _UndoStack.Count > 0; } }
        public bool CanRedo { get { return _RedoStack.Count > 0; } }
        

        public TtHistoryManager(TtManager manager)
        {
            _Manager = manager;
        }


        #region History Management
        public void Undo()
        {
            if (CanUndo)
            {
                ITtCommand hist = _UndoStack.Pop();
                _RedoStack.Push(hist);
                hist.Undo();
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
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                ITtCommand hist = _RedoStack.Pop();
                _UndoStack.Push(hist);
                hist.Redo();
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
                    _UndoStack.Push(hist);
                    hist.Redo();
                }
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
            return GetPoints(polyCN);
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
            _UndoStack.Push(new AddTtPointCommand(point, _Manager));
            _RedoStack.Clear();
        }

        public void AddPoints(List<TtPoint> points)
        {

            _UndoStack.Push(new AddTtPointsCommand(points, _Manager));
            _RedoStack.Clear();
        }

        public void DeletePoint(TtPoint point)
        {
            _UndoStack.Push(new DeleteTtPointCommand(point, _Manager));
            _RedoStack.Clear();
        }

        public void DeletePoints(List<TtPoint> points)
        {
            _UndoStack.Push(new DeleteTtPointsCommand(points, _Manager));
            _RedoStack.Clear();

        }


        public void AddPolygon(TtPolygon polygon)
        {
            _UndoStack.Push(new AddTtPolygonCommand(polygon, _Manager));
            _RedoStack.Clear();
        }

        public void DeletePolygon(TtPolygon polygon)
        {
            _UndoStack.Push(new DeleteTtPolygonCommand(polygon, _Manager));
            _RedoStack.Clear();
        }


        public void AddMetadata(TtMetadata metadata)
        {
            _UndoStack.Push(new AddTtMetadataCommand(metadata, _Manager));
            _RedoStack.Clear();
        }

        public void DeleteMetadata(TtMetadata metadata)
        {
            _UndoStack.Push(new DeleteTtMetadataCommand(metadata, _Manager));
            _RedoStack.Clear();
        }


        public void AddGroup(TtGroup group)
        {
            _UndoStack.Push(new AddTtGroupCommand(group, _Manager));
            _RedoStack.Clear();
        }

        public void DeleteGroup(TtGroup group)
        {
            _UndoStack.Push(new DeleteTtGroupCommand(group, _Manager));
            _RedoStack.Clear();
        }
        #endregion
    }
}
