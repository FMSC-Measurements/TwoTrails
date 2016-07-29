using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Commands;
using TwoTrails.Controls;
using TwoTrails.Core;

namespace TwoTrails
{
    public abstract class TtTabModel : NotifyPropertyChangedEx
    {
        public TtProject Project { get; private set; }

        public TabItem Tab { get; private set; }

        public virtual String TabTitle
        {
            get { return String.Format("{0}{1}", Project.ProjectName, Project.RequiresSave ? "*" : String.Empty); }
        }


        public abstract bool IsDetachable { get; }
        
        public ICommand CloseCommand { get; }
        public ICommand SaveCommand { get; }


        protected MainWindowModel MainModel;


        public TtTabModel(MainWindowModel mainModel, TtProject project) : base()
        {
            MainModel = mainModel;

            this.Project = project;
            this.Tab = new TabItem();
            
            SaveCommand = new RelayCommand((x) => SaveProject());
            CloseCommand = new RelayCommand((x) => CloseTab());
            
            project.PropertyChanged += Project_PropertyChanged;

            Tab.DataContext = this;
        }

        private void Project_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ProjectName" || e.PropertyName == "RequiresSave")
            {
                Tab.Header = TabTitle;
                OnPropertyChanged(nameof(TabTitle));
            }
        }


        protected abstract void CloseTab();

        protected abstract void SaveProject();
    }
}
