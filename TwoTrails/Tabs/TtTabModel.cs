using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;

namespace TwoTrails
{
    public abstract class TtTabModel : NotifyPropertyChangedEx
    {
        public TabItem Tab { get; private set; }

        public virtual String TabTitle
        {
            get { return String.Format("{0}{1}",
                Project.ProjectName,
                Project.RequiresSave ? "*" : String.Empty); }
        }

        public TtProject Project { get; private set; }

        public abstract bool IsDetachable { get; }

        public abstract bool IsPointsEditable { get; }
        
        public ICommand CloseTabCommand { get; }
        public ICommand SaveCommand { get; }
        
        public TtTabModel(TtProject project) : base()
        {
            Project = project;

            this.Tab = new TabItem();
            
            SaveCommand = new RelayCommand((x) => Project.Save());
            CloseTabCommand = new RelayCommand((x) => Project.CloseTab(this));

            Project.PropertyChanged += Project_PropertyChanged;

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
    }
}
