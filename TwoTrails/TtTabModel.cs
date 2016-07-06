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
        
        public ICommand Close { get; }
        public ICommand Save { get; }


        protected MainWindowModel MainModel;


        public TtTabModel(MainWindowModel mainModel, TtProject project) : base()
        {
            MainModel = mainModel;

            this.Project = project;
            this.Tab = new TabItem();


            Save = new RelayCommand((x) => SaveProject());
            Close = new RelayCommand((x) => CloseTab());

            Tab.Content = new ProjectControl(this);
            Tab.DataContext = this;
            

            project.PropertyChanged += Project_PropertyChanged;
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
