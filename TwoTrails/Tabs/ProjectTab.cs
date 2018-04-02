using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {
        private ProjectEditorControl _ProjectEditorControl;

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

        public ProjectTab(TtProject project) : base(project)
        {
            _ProjectEditorControl = new ProjectEditorControl(project, ProjectStartupTab.Points);
            Tab.Content = _ProjectEditorControl;

            _ProjectEditorControl.tabControl.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                IsEditingPoints = _ProjectEditorControl.tabControl.SelectedIndex == 1;
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

        public void SwitchToTab(ProjectStartupTab tab)
        {
            _ProjectEditorControl.SwitchToTab(tab);
        }
    }
}
