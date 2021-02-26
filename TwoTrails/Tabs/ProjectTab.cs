using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {
        public BindedRelayCommand<TtHistoryManager> UndoCommand { get; }
        public BindedRelayCommand<TtHistoryManager> RedoCommand { get; }

        public TtProject Project { get; private set; }

        public ProjectEditorModel ProjectEditor { get; }

        private readonly ProjectEditorControl _ProjectEditorControl;

        private readonly MainWindowModel _MainModel;


        public override bool IsDetachable { get; } = false;

        public override string TabTitle => $"{Project.ProjectName}{(Project.RequiresSave ? "*" : String.Empty)}";

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


        public ProjectTab(TtProject project, MainWindowModel mainWindowModel) : base()
        {
            Project = project;
            _MainModel = mainWindowModel;

            ProjectEditor = new ProjectEditorModel(Project, _MainModel);
            _ProjectEditorControl = new ProjectEditorControl(ProjectEditor, ProjectTabSection.Points);

            _ProjectEditorControl.Loaded += ProjectEditorControl_Loaded;

            Tab.Content = _ProjectEditorControl;


            //Func<ProjectTabSection, Type, bool> doesTabAndDataMatch = (selectedTab, type) =>
            //{
            //    if (type == null)
            //        return true;
            //    switch (selectedTab)
            //    {
            //        case ProjectTabSection.Project: return type == ProjectProperties.DataType;
            //        case ProjectTabSection.Points: return type.IsAssignableFrom(PointProperties.DataType);
            //        case ProjectTabSection.Polygons: return type == PolygonProperties.DataType;
            //        case ProjectTabSection.Metadata: return type == MetadataProperties.DataType;
            //        case ProjectTabSection.Groups: return type == GroupProperties.DataType;
            //        case ProjectTabSection.Media: return type == PointProperties.DataType;
            //        case ProjectTabSection.Map:
            //        default: return false;
            //    }
            //};


            UndoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Project.HistoryManager.Undo(),
                x => Project.HistoryManager.CanUndo && doesTabAndDataMatch(CurrentTabSection, Project.HistoryManager.UndoCommandType),
                Project.HistoryManager,
                x => x.CanUndo);

            RedoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Project.HistoryManager.Redo(),
                x => Project.HistoryManager.CanRedo && doesTabAndDataMatch(CurrentTabSection, Project.HistoryManager.RedoCommandType),
                Project.HistoryManager,
                x => x.CanRedo);

            //UndoCommand = new BindedRelayCommand<TtHistoryManager>(
            //    (x, m) => m.Undo(),
            //    (x, m) => m.CanUndo && doesTabAndDataMatch(CurrentTabSection, m.UndoCommandType),
            //    Project.HistoryManager,
            //    x => x.CanUndo);

            //RedoCommand = new BindedRelayCommand<TtHistoryManager>(
            //    (x, m) => m.Redo(),
            //    (x, m) => m.CanRedo && doesTabAndDataMatch(CurrentTabSection, m.RedoCommandType),
            //    Project.HistoryManager,
            //    x => x.CanRedo);


            ProjectEditor.PointEditor.PropertyChanged += PointEditor_PropertyChanged;
            _ProjectEditorControl.tabControl.SelectionChanged += ProjectEditor_TabSelectionChanged;
        }

        static bool doesTabAndDataMatch(ProjectTabSection selectedTab, Type type)
        {
                if (type == null)
                    return true;
                switch (selectedTab)
                {
                    case ProjectTabSection.Project: return type == ProjectProperties.DataType;
                    case ProjectTabSection.Points: return type.IsAssignableFrom(PointProperties.DataType);
                    case ProjectTabSection.Polygons: return type == PolygonProperties.DataType;
                    case ProjectTabSection.Metadata: return type == MetadataProperties.DataType;
                    case ProjectTabSection.Groups: return type == GroupProperties.DataType;
                    case ProjectTabSection.Media: return type == PointProperties.DataType;
                    case ProjectTabSection.Map:
                    default: return false;
                }
        }


        private void PointEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectEditor.PointEditor.SelectedPoints))
            {
                OnPropertyChanged(nameof(TabInfo));
            }
            else if (e.PropertyName == "ProjectName" || e.PropertyName == "RequiresSave")
            {
                Tab.Dispatcher.Invoke(() =>
                {
                    Tab.Header = TabTitle;
                });
                OnPropertyChanged(nameof(TabTitle));
            }
            else if (e.PropertyName == "DAL")
            {
                OnPropertyChanged(nameof(ToolTip));
            }
        }

        private void ProjectEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectEditor.ProjectEditorControl = sender as ProjectEditorControl;
        }

        private void ProjectEditor_TabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (_ProjectEditorControl.tabControl.SelectedIndex)
            {
                case 0: CurrentTabSection = ProjectTabSection.Project; break;
                case 1: IsEditingPoints = true; CurrentTabSection = ProjectTabSection.Points; break;
                case 2: CurrentTabSection = ProjectTabSection.Polygons; break;
                case 3: CurrentTabSection = ProjectTabSection.Metadata; break;
                case 4: CurrentTabSection = ProjectTabSection.Groups; break;
                case 5: CurrentTabSection = ProjectTabSection.Media; break;
                case 6: CurrentTabSection = ProjectTabSection.Map; break;
            }

            OnPropertyChanged(nameof(TabInfo));
        }


        public void SwitchToTabSection(ProjectTabSection tab)
        {
            _ProjectEditorControl.SwitchToTab(tab);
        }


        protected override void OnTabClose()
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
                        return;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            _MainModel.RemoveTab(this);
        }

        protected override void Dispose(bool dispoing)
        {
            base.Dispose(dispoing);

            UndoCommand.Dispose();
            RedoCommand.Dispose();

            ProjectEditor.PointEditor.PropertyChanged -= PointEditor_PropertyChanged;
            _ProjectEditorControl.Loaded -= ProjectEditorControl_Loaded;
            _ProjectEditorControl.tabControl.SelectionChanged -= ProjectEditor_TabSelectionChanged;
        }
    }
}
