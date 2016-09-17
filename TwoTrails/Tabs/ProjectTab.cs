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
                return String.Format("{0}{1}",
                  Project.ProjectName,
                  Project.RequiresSave ? "*" : String.Empty);
            }
        }

        public override string TabInfo
        {
            get
            {
                switch (_ProjectEditorControl.tabControl.SelectedIndex)
                {
                    case 1:
                        return String.Format("{1}/{0}", Project.DataEditor.Points.Count, Project.DataEditor.SelectedPoints.Count);
                    case 2:
                        {
                            TtPolygon poly = _ProjectEditorControl.lbPolys.SelectedItem as TtPolygon;
                            if (poly != null)
                            {
                                return String.Format("{0} Points in {1}",
                                    Project.Manager.Points.Where(p => p.PolygonCN == poly.CN).Count(),
                                    poly.Name);
                            }

                            return String.Empty;
                        }
                    case 0:
                    default:
                        return String.Empty;
                }
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
