using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public abstract class TtTabModel : NotifyPropertyChangedEx
    {
        public TabItem Tab { get; private set; }

        public virtual String TabTitle
        {
            get { return $"{Project.ProjectName}{(Project.RequiresSave ? "*" : String.Empty)}"; }
        }

        public virtual String TabInfo { get { return String.Empty; } }

        public TtProject Project { get; private set; }

        public virtual String ToolTip { get { return Project.FilePath; } } 

        public abstract bool IsDetachable { get; }

        public bool IsEditingPoints { get { return Get<bool>(); } protected set { Set(value); } }

        public ICommand CloseTabCommand { get; }
        public ICommand SaveCommand { get; }
        public virtual ICommand OpenInWinndowCommand { get; } = null;
        
        public TtTabModel(TtProject project) : base()
        {
            Project = project;

            this.Tab = new TabItem();
            
            SaveCommand = new RelayCommand(x => Project.Save());
            CloseTabCommand = new RelayCommand(x => Project.CloseTab(this));

            Project.PropertyChanged += Project_PropertyChanged;

            Tab.DataContext = this;

            IsEditingPoints = false;
        }

        private void Project_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ProjectName" || e.PropertyName == "RequiresSave")
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

        public virtual void Close()
        {

        }
    }
}
