﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for ProjectEditorControl.xaml
    /// </summary>
    public partial class ProjectEditorControl : UserControl
    {
        public ProjectEditorControl(TtProject project, ProjectStartupTab tab = ProjectStartupTab.Project)
        {
            this.DataContext = new ProjectEditorModel(project);
            
            InitializeComponent();

            ccPoints.Content = new DataEditorControl(project.DataEditor, new DataStyleModel(project));

            SwitchToTab(tab);
        }

        public void SwitchToTab(ProjectStartupTab tab)
        {
            tabControl.SelectedIndex = (int)tab;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = string.IsNullOrEmpty(e.Text) ? false : !e.Text.All(char.IsDigit);
        }
    }

    public enum ProjectStartupTab
    {
        Project = 0,
        Points = 1,
        Polygons = 2,
        Metadata = 3,
        Groups = 4
    }
}