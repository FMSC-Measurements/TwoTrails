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
    public class TtHistoryManager : NotifyPropertyChangedEx, IObservableTtManager
    {
        public TtManager BaseManager { get; private set; }

        public event EventHandler<HistoryEventArgs> HistoryChanged;
        
        public ReadOnlyObservableCollection<TtPoint> Points { get { return BaseManager.Points; } }
        public ReadOnlyObservableCollection<TtPolygon> Polygons { get { return BaseManager.Polygons; } }
        public ReadOnlyObservableCollection<TtMetadata> Metadata { get { return BaseManager.Metadata; } }
        public ReadOnlyObservableCollection<TtGroup> Groups { get { return BaseManager.Groups; } }
        public ReadOnlyObservableCollection<TtMediaInfo> MediaInfo { get { return BaseManager.MediaInfo; } }

        private readonly Stack<ITtCommand> _UndoStack = new Stack<ITtCommand>();
        private readonly Stack<ITtCommand> _RedoStack = new Stack<ITtCommand>();
        

        public bool CanUndo => _UndoStack.Any();
        public bool CanRedo => _RedoStack.Any();

        public Type UndoCommandType => CanUndo ? _UndoStack.Peek().DataType : null;
        public Type RedoCommandType => CanRedo ? _RedoStack.Peek().DataType : null;


        public bool HasDataDictionary { get { return BaseManager.HasDataDictionary; } }

        public TtGroup MainGroup { get { return BaseManager.MainGroup; } }

        public TtMetadata DefaultMetadata { get { return BaseManager.DefaultMetadata; } }

        public int PolygonCount => Polygons.Count;

        public int PointCount => Points.Count;
        
        private List<ITtCommand> _ComplexActionCommands;
        public bool ComplexActionStarted => _ComplexActionCommands != null;



        public TtHistoryManager(TtManager manager)
        {
            BaseManager = manager;

            DefaultMetadata.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(TtMetadata.Zone):
                    case nameof(TtMetadata.Slope):
                    case nameof(TtMetadata.Distance):
                    case nameof(TtMetadata.Elevation):
                        OnHistoryChanged(HistoryEventType.Reset, typeof(TtMetadata), true);
                        break;
                }
            };
        }


        public void Save()
        {
            try
            {
                BaseManager.Save();
                _UndoStack.Clear();
                _RedoStack.Clear();
                OnHistoryChanged(HistoryEventType.Reset, null, true);
            }
            catch (Exception ex)
            {
                throw new Exception("History Manager unable to save do to: " + ex.Message);
            }
        }


        #region History Management
        internal void AddCommand(ITtCommand command, bool runCommand = true)
        {
            if (ComplexActionStarted)
            {
                _ComplexActionCommands.Add(command);
            }
            else
            {
                if (runCommand)
                {
                    command.Redo();
                }
                _UndoStack.Push(command);
                _RedoStack.Clear();
                OnHistoryChanged(HistoryEventType.Commit, command.DataType, command.RequireRefresh);
            }
        }

        public void StartMultiCommand()
        {
            if (_ComplexActionCommands != null)
                throw new Exception("Complex Action already started");
            _ComplexActionCommands = new List<ITtCommand>();
        }

        public void CommitMultiCommand()
        {
            if (_ComplexActionCommands == null)
                throw new Exception("Complex Action not started");

            if (_ComplexActionCommands.Count > 0)
            {
                MultiTtCommand command = new MultiTtCommand(_ComplexActionCommands);
                _ComplexActionCommands = null;
                AddCommand(command);
            }
            else
                _ComplexActionCommands = null;
        }

        public void RevertMultiCommand()
        {
            _ComplexActionCommands = null;
        }

        public void Undo()
        {
            if (CanUndo)
            {
                ITtCommand command = _UndoStack.Pop();
                _RedoStack.Push(command);
                command.Undo();

                OnHistoryChanged(HistoryEventType.Undone, command.DataType, command.RequireRefresh);
            }
        }

        public void Undo(int levels)
        {
            if (CanUndo)
            {
                ITtCommand command = null;
                bool requireRefresh = false;

                for (int i = 0; i < levels && CanUndo; i++)
                {
                    command = _UndoStack.Pop();
                    requireRefresh |= command.RequireRefresh;
                    _RedoStack.Push(command);
                    command.Undo();
                }

                OnHistoryChanged(HistoryEventType.Undone, command?.DataType, requireRefresh);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                ITtCommand command = _RedoStack.Pop();
                _UndoStack.Push(command);
                command.Redo();

                OnHistoryChanged(HistoryEventType.Redone, command.DataType, command.RequireRefresh);
            }
        }

        public void Redo(int levels)
        {
            if (CanRedo)
            {
                ITtCommand command = null;
                bool requireRefresh = false;

                for (int i = 0; i < levels && CanRedo; i++)
                {
                    command = _RedoStack.Pop();
                    requireRefresh |= command.RequireRefresh;
                    _UndoStack.Push(command);
                    command.Redo();
                }

                OnHistoryChanged(HistoryEventType.Redone, command?.DataType, requireRefresh);
            }
        }

        public void ClearHistory()
        {
            _RedoStack.Clear();
            _UndoStack.Clear();
            OnPropertyChanged(nameof(CanUndo), nameof(CanRedo));
        }

        private void OnHistoryChanged(HistoryEventType historyEventType, Type dataType, bool requireRefresh)
        {
            HistoryChanged?.Invoke(this, new HistoryEventArgs(historyEventType, dataType, requireRefresh));
            OnPropertyChanged(nameof(CanUndo), nameof(CanRedo));
        }
        #endregion


        #region Get / CheckExists
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

        public TtPoint GetNextPoint(TtPoint point)
        {
            return BaseManager.GetNextPoint(point);
        }

        public bool PolygonExists(string polyCN)
        {
            return BaseManager.PolygonExists(polyCN);
        }

        public TtPolygon GetPolygon(string polyCN)
        {
            return BaseManager.GetPolygon(polyCN);
        }

        public List<TtPolygon> GetPolygons()
        {
            return BaseManager.GetPolygons();
        }
        

        public bool MetadataExists(string metaCN)
        {
            return BaseManager.MetadataExists(metaCN);
        }

        public List<TtMetadata> GetMetadata()
        {
            return BaseManager.GetMetadata();
        }


        public bool GroupExists(string groupCN)
        {
            return BaseManager.GroupExists(groupCN);
        }

        public List<TtGroup> GetGroups()
        {
            return BaseManager.GetGroups();
        }
        #endregion

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


        public void CreateQuondamLinks(IEnumerable<TtPoint> points, TtPolygon targetPolygon, int insertIndex, QuondamBoundaryMode bndMode = QuondamBoundaryMode.Inherit, bool reverse = false)
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
            AddCommand(new MovePointsCommand(reverse ? points.Reverse() : points, BaseManager, targetPolygon, insertIndex));
        }


        #region Editing

        #region Points
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

        public void EditPoints<T>(IEnumerable<TtPoint> points, IEnumerable<PropertyInfo> properties, T newValue)
        {
            AddCommand(new EditTtPointsMultiPropertyCommand<T>(points, properties, points.Select(p => newValue)));
        }

        public void EditPointsMultiValues(IEnumerable<TtPoint> points, IEnumerable<PropertyInfo> properties, IEnumerable<object> newValues)
        {
            AddCommand(new EditTtPointsMultiPropertyMultiValueCommand(points, properties, newValues));
        }


        public void ResetPoint(TtPoint point, bool keepIndexAndPoly = false)
        {
            AddCommand(new ResetTtPointCommand(point, BaseManager, keepIndexAndPoly));
        }

        public void ResetPoints(IEnumerable<TtPoint> points, bool keepIndexAndPoly = false)
        {
            AddCommand(new ResetTtPointsCommand(points, BaseManager, keepIndexAndPoly));
        }
        #endregion

        #region Polygons
        public void EditPolygon<T>(TtPolygon polygon, PropertyInfo property, T newValue)
        {
            AddCommand(new EditTtPolygonCommand<T>(polygon, property, newValue));
        }
        #endregion

        #region Metadata
        public void EditMetadata<T>(TtMetadata metadata, PropertyInfo property, T newValue)
        {
            AddCommand(new EditTtMetadataCommand<T>(metadata, property, newValue));
        }
        #endregion

        #region Groups
        public void EditGroup<T>(TtGroup group, PropertyInfo property, T newValue)
        {
            AddCommand(new EditTtGroupCommand<T>(group, property, newValue));
        }
        #endregion
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
            AddCommand(new RebuildPolygonCommand(polygon, reindex, BaseManager));
        }

        public void RecalculatePolygons()
        {
            AddCommand(new RecalculatePolygonsCommand(BaseManager));
        }




        public PolygonGraphicOptions GetPolygonGraphicOption(string polyCN)
        {
            return BaseManager.GetPolygonGraphicOption(polyCN);
        }

        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return BaseManager.GetPolygonGraphicOptions();
        }

        public PolygonGraphicOptions GetDefaultPolygonGraphicOption()
        {
            return BaseManager.GetDefaultPolygonGraphicOption();
        }


        List<TtNmeaBurst> ITtManager.GetNmeaBursts(string pointCN)
        {
            return BaseManager.GetNmeaBursts(pointCN);
        }

        List<TtNmeaBurst> ITtManager.GetNmeaBursts(IEnumerable<string> pointCNs)
        {
            return BaseManager.GetNmeaBursts(pointCNs);
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
            return BaseManager.GetImages(pointCN);
        }

        public void InsertMedia(TtMedia media)
        {
            InsertMedia(media);
        }

        public void DeleteMedia(TtMedia media)
        {
            DeleteMedia(media);
        }


        public DataDictionaryTemplate GetDataDictionaryTemplate()
        {
            return BaseManager.GetDataDictionaryTemplate();
        }


        void ITtManager.UpdateDataAction(DataActionType action, string notes)
        {
            BaseManager.UpdateDataAction(action, notes);
        }
    }

    public enum HistoryEventType
    {
        Commit,
        Undone,
        Redone,
        Reset
    }

    public class HistoryEventArgs : EventArgs
    {
        public HistoryEventType HistoryEventType { get; }
        public Type DataType { get; }
        public bool RequireRefresh { get; }

        public HistoryEventArgs(HistoryEventType historyEventType, Type dataType, bool requireRefresh)
        {
            HistoryEventType = historyEventType;
            DataType = dataType;
            RequireRefresh = requireRefresh;
        }
    }
}
