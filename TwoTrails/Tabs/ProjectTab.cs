using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {
        private ProjectEditorControl _ProjectEditorControl;
        private ProjectEditorModel _ProjectEditorModel;

        public override bool IsDetachable { get; } = false;

        public override string TabTitle
        {
            get
            {
                return $"{Project.ProjectName}{(Project.RequiresSave ? "*" : String.Empty)}";
            }
        }

        public override string TabInfo
        {
            get
            {
                switch (_ProjectEditorControl.tabControl.SelectedIndex)
                {
                    case 1:
                        return $"{Project.DataEditor.SelectedPoints.Count}/{Project.DataEditor.Points.Count}";
                    case 2:
                        {
                            if (_ProjectEditorControl.lbPolys.SelectedItem is TtPolygon poly)
                            {
                                return $"{Project.Manager.Points.Where(p => p.PolygonCN == poly.CN).Count()} Points in {poly.Name}";
                            }

                            return String.Empty;
                        }
                    case 3:
                        {
                            if (_ProjectEditorControl.lbMetadata.SelectedItem is TtMetadata meta)
                            {
                                return $"{Project.Manager.Points.Where(p => p.MetadataCN == meta.CN).Count()} Points use {meta.Name}";
                            }

                            return String.Empty;
                        }
                    case 4:
                        {
                            if (_ProjectEditorControl.lbGroups.SelectedItem is TtGroup group)
                            {
                                return $"{Project.Manager.Points.Where(p => p.GroupCN == group.CN).Count()} Points in {group.Name}";
                            }

                            return String.Empty;
                        }
                    default:
                        break;
                }

                return String.Empty;
            }
        }

        public ProjectTabSection CurrentTabSection { get { return Get<ProjectTabSection>(); } protected set { Set(value); } }

        public ProjectTab(TtProject project) : base(project)
        {
            _ProjectEditorControl = new ProjectEditorControl(_ProjectEditorModel = new ProjectEditorModel(project), ProjectTabSection.Points);
            Tab.Content = _ProjectEditorControl;

            _ProjectEditorControl.tabControl.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
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
            };

            Project.DataEditor.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == nameof(Project.DataEditor.SelectedPoints))
                {
                    OnPropertyChanged(nameof(TabInfo));
                }
            };
        }

        public void SwitchToTabSection(ProjectTabSection tab)
        {
            _ProjectEditorControl.SwitchToTab(tab);
        }

        public override void Close()
        {
            base.Close();

            _ProjectEditorModel.CloseWindows();
        }
    }
}
