﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Commands;
using TwoTrails.Controls;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {

        public override bool IsDetachable { get; } = false;

        private DataEditorModel DataEditor;
        private DataStyleModel DataStyles;


        public ProjectTab(MainWindowModel mainModel, TtProject project) : base(mainModel, project)
        {
            DataEditor = new DataEditorModel(project);
            DataStyles = new DataStyleModel(project);

            Tab.Content = new ProjectControl(DataEditor, DataStyles);
        }


        protected override void CloseTab()
        {
            MainModel.CloseProject(Project);
        }

        protected override void SaveProject()
        {
            Project.Save();
        }
    }
}
