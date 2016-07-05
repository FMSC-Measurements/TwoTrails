using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TwoTrails.Commands
{
    public class CloseProjectCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private MainWindowModel mainModel;

        public CloseProjectCommand(MainWindowModel mainModel)
        {
            this.mainModel = mainModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            TtProject project = parameter as TtProject;

            if (project != null)
            {
                if (project.RequiresSave)
                {
                    //check to see if wanting to save
                }
                else
                {
                    mainModel.CloseProject(project);
                } 
            }
        }
    }
}
