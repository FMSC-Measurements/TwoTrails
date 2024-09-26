using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.Core.Units;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {
        public BindedRelayCommand<TtHistoryManager> UndoCommand { get; }
        public BindedRelayCommand<TtHistoryManager> RedoCommand { get; }


        public String UndoCommandInfo => Project.HistoryManager.CanUndo ? Project.HistoryManager.UndoCommandInfo.Description : "No Actions";
        public String RedoCommandInfo => Project.HistoryManager.CanRedo ? Project.HistoryManager.RedoCommandInfo.Description : "No Actions";


        public TtProject Project { get; private set; }

        public ProjectEditorModel ProjectEditor { get; }

        private readonly ProjectEditorControl _ProjectEditorControl;


        public override bool IsDetachable { get; } = false;

        public override string TabTitle => $"{Project.ProjectName}{(Project.RequiresSave ? "*" : String.Empty)}";

        public override string ToolTip => Project.FilePath;

        public override string TabInfo
        {
            get
            {
                switch (_ProjectEditorControl.tabControl.SelectedIndex)
                {
                    case 1:
                        return $"{ProjectEditor.PointEditor.SelectedPoints.Count}/{ProjectEditor.PointEditor.Points.Count}";
                    case 2:
                        {
                            if (_ProjectEditorControl.lbPolys.SelectedItem is TtPolygon poly)
                            {
                                return $"{Project.HistoryManager.Points.Where(p => p.PolygonCN == poly.CN).Count()} Points in {poly.Name}";
                            }

                            return String.Empty;
                        }
                    case 3:
                        {
                            if (_ProjectEditorControl.lbMetadata.SelectedItem is TtMetadata meta)
                            {
                                return $"{Project.HistoryManager.Points.Where(p => p.MetadataCN == meta.CN).Count()} Points use {meta.Name}";
                            }

                            return String.Empty;
                        }
                    case 4:
                        {
                            if (_ProjectEditorControl.lbGroups.SelectedItem is TtGroup group)
                            {
                                return $"{Project.HistoryManager.Points.Where(p => p.GroupCN == group.CN).Count()} Points in {group.Name}";
                            }

                            return String.Empty;
                        }
                    default:
                        break;
                }

                return String.Empty;
            }
        }

        public bool IsEditingPoints { get { return Get<bool>(); } protected set { Set(value); } }


        public ProjectTabSection CurrentTabSection { get { return Get<ProjectTabSection>(); } protected set { Set(value); } }


        public ProjectTab(TtProject project, MainWindowModel mainWindowModel) : base(mainWindowModel)
        {
            Project = project;

            ProjectEditor = new ProjectEditorModel(Project, MainModel);
            _ProjectEditorControl = new ProjectEditorControl(ProjectEditor, ProjectTabSection.Points);

            _ProjectEditorControl.Loaded += ProjectEditorControl_Loaded;

            Tab.Content = _ProjectEditorControl;

            UndoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Undo(),
                x => Project.HistoryManager.CanUndo,
                Project.HistoryManager,
                x => x.CanUndo);

            RedoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Redo(),
                x => Project.HistoryManager.CanRedo,
                Project.HistoryManager,
                x => x.CanRedo);

            Project.HistoryManager.HistoryChanged += HistoryManager_HistoryChanged;


            Project.PropertyChanged += Project_PropertyChanged;
            ProjectEditor.PointEditor.PropertyChanged += PointEditor_PropertyChanged;
            _ProjectEditorControl.tabControl.SelectionChanged += ProjectEditor_TabSelectionChanged;

            ProjectEditor_TabSelectionChanged(null, null);
        }

        private void Undo()
        {
            String dt = GetActionTypeFromDataType(Project.HistoryManager.UndoCommandType);

            if (DoesTabAndDataMatch(CurrentTabSection, Project.HistoryManager.UndoCommandType) ||
                MessageBox.Show($"You are about to undo a{(dt != null ? $" {dt}": "n")} action but are not on the {(dt != null ? dt : "correct")} tab. Would you like to continue?", "About to Undo",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Project.HistoryManager.Undo();
            }
        }

        private void Redo()
        {
            String dt = GetActionTypeFromDataType(Project.HistoryManager.RedoCommandType);

            if (DoesTabAndDataMatch(CurrentTabSection, Project.HistoryManager.RedoCommandType) ||
                MessageBox.Show($"You are about to redo a{(dt != null ? $" {dt}" : "n")} action but are not on the {(dt != null ? dt : "correct")} tab. Would you like to continue?", "About to Redo",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                Project.HistoryManager.Redo();
            }
        }

        public static bool DoesTabAndDataMatch(ProjectTabSection selectedTab, DataActionType type)
        {
            if (type == DataActionType.None)
            {
                return true;
            }

            switch (selectedTab)
            {
                case ProjectTabSection.Project: return type.AffectsProject();
                case ProjectTabSection.Points: return type.AffectsPoints();
                case ProjectTabSection.Polygons: return type.AffectsPolygons();
                case ProjectTabSection.Metadata: return type.AffectsMetadata();
                case ProjectTabSection.Groups: return type.AffectsGroups();
                case ProjectTabSection.Media: return type.AffectsMedia();
                case ProjectTabSection.DataDictionary: return type.AffectsDataDictionary();
                case ProjectTabSection.Map:
                case ProjectTabSection.Actions:
                default: return false;
            }
        }

        private static String GetActionTypeFromDataType(DataActionType type)
        {
            if (type.AffectsPolygons())
                return "Polygon";
            else if (type.AffectsMetadata())
                return "Metadata";
            else if (type.AffectsProject())
                return "Project";
            else if (type.AffectsGroups())
                return "Group";
            else if (type.AffectsMedia())
                return "Media";
            else if (type.AffectsDataDictionary())
                return "Data Dictionary";
            else if (type.AffectsPoints())
                return "Point";

            return null;
        }

        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TtProject.ProjectName) || e.PropertyName == nameof(TtProject.RequiresSave))
            {
                Tab.Dispatcher.Invoke(() =>
                {
                    Tab.Header = TabTitle;
                });
                OnPropertyChanged(nameof(TabTitle));
            }
            else if (e.PropertyName == nameof(TtProject.DAL))
            {
                OnPropertyChanged(nameof(ToolTip));
            }
        }

        private void PointEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectEditor.PointEditor.SelectedPoints) ||
                e.PropertyName == nameof(ProjectEditor.PointEditor.Polygons))
            {
                OnPropertyChanged(nameof(TabInfo));
            }
        }

        private void ProjectEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectEditor.ProjectEditorControl = sender as ProjectEditorControl;
        }

        private void ProjectEditor_TabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tabName = (_ProjectEditorControl.tabControl.SelectedItem as TabItem)?.Name;

            IsEditingPoints = tabName == "tiPoints";

            if (tabName != null)
            {
                switch (tabName)
                {
                    case "tiProject": CurrentTabSection = ProjectTabSection.Project; break;
                    case "tiPoints": CurrentTabSection = ProjectTabSection.Points; break;
                    case "tiPolygons": CurrentTabSection = ProjectTabSection.Polygons; break;
                    case "tiMetadata": CurrentTabSection = ProjectTabSection.Metadata; break;
                    case "tiGroups": CurrentTabSection = ProjectTabSection.Groups; break;
                    case "tiMedia": CurrentTabSection = ProjectTabSection.Media; break;
                    case "tiDataDictionary": CurrentTabSection = ProjectTabSection.DataDictionary; break;
                    case "tiMap": CurrentTabSection = ProjectTabSection.Map; break;
                    case "tiActivity": CurrentTabSection = ProjectTabSection.Actions; break;
                } 
            }

            OnPropertyChanged(nameof(TabInfo));

            UndoCommand.OnCanExecuteChanged();
            RedoCommand.OnCanExecuteChanged();
        }

        private void HistoryManager_HistoryChanged(object sender, HistoryEventArgs e)
        {

            OnPropertyChanged(nameof(UndoCommandInfo), nameof(RedoCommandInfo));
        }

        public void SwitchToTabSection(ProjectTabSection tab)
        {
            _ProjectEditorControl.SwitchToTab(tab);
        }


        protected override bool OnTabClose()
        {
            if (Project.RequiresSave)
            {
                MessageBoxResult result = MessageBox.Show("Would you like to save before closing this project?",
                                                String.Empty,
                                                MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Project.Save();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void Dispose(bool dispoing)
        {
            base.Dispose(dispoing);

            try
            {
                if (UndoCommand != null) UndoCommand.Dispose();
                if (RedoCommand != null) RedoCommand.Dispose();

                if (ProjectEditor != null)
                {
                    ProjectEditor.PointEditor.PropertyChanged -= PointEditor_PropertyChanged;
                    _ProjectEditorControl.Loaded -= ProjectEditorControl_Loaded;
                    _ProjectEditorControl.tabControl.SelectionChanged -= ProjectEditor_TabSelectionChanged;

                    ProjectEditor.Dispose();
                }

                if (ProjectEditor != null && Project.HistoryManager != null)
                {
                    Project.HistoryManager.HistoryChanged -= HistoryManager_HistoryChanged;
                }
            }
            catch (Exception)
            {
                //
            }
        }
    }
}
