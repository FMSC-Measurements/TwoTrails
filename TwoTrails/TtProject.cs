using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails
{
    public class TtProject : NotifyPropertyChangedEx
    {
        public String FilePath { get { return DAL.FilePath; } }
        

        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }

        public ICommand DiscardChangesCommand { get; }

        public ICommand ViewUserActivityCommand { get; private set; }

        public ProjectTab ProjectTab { get; private set; }
        public DataEditorTab DataEditorTab { get; private set; }
        public MapTab MapTab { get; private set; }
        public UserActivityTab UserActivityTab { get; private set; }
        public MapWindow MapWindow { get; private set; }
        
        private bool MultipleProjectViews
        {
            get
            {
                int views = 0;

                if (ProjectTab != null)
                    views++;

                if (DataEditorTab != null)
                    views++;

                if (MapTab != null)
                    views++;

                if (MapWindow != null)
                    views++;

                return views > 1;
            }
        }



        public TtProjectInfo ProjectInfo { get; private set; }
        
        public String ProjectName
        {
            get { return Get<String>(); }
            private set { Set(value); }
        }
        
        public bool RequiresSave
        {
            get { return Get<bool>(); }
            private set { Set(value); }
        }


        public bool RequiresUpgrade { get; private set; }


        public bool DataChanged { get; private set; }


        public ITtDataLayer DAL { get; private set; }

        private TtManager _Manager;
        public TtHistoryManager HistoryManager { get; private set; }

        private DataEditorModel _DataEditor;
        public DataEditorModel DataEditor
        {
            get { return _DataEditor ?? (_DataEditor = new DataEditorModel(this)); }
        }

        private MainWindowModel _MainModel;




        public TtProject(ITtDataLayer dal, ITtSettings settings, MainWindowModel mainModel)
        {
            DAL = dal;

            _MainModel = mainModel;

            ProjectInfo = dal.GetProjectInfo();
            ProjectName = ProjectInfo.Name;

            RequiresSave = false;

            _Manager = new TtManager(dal, settings);
            HistoryManager = new TtHistoryManager(_Manager);
            HistoryManager.HistoryChanged += Manager_HistoryChanged;

            DataEditorTab = new DataEditorTab(this);



            UndoCommand = new RelayCommand((x) => HistoryManager.Undo());
            RedoCommand = new RelayCommand((x) => HistoryManager.Redo());

            DiscardChangesCommand = new RelayCommand((x) => _Manager.Reset());

            ViewUserActivityCommand = new RelayCommand((x) => ViewUserActivity());

            OnPropertyChanged(nameof(DataEditor));
        }

        private void Manager_HistoryChanged(object sender, EventArgs e)
        {
            Set(HistoryManager.CanUndo || DataChanged, nameof(RequiresSave));
        }


        public void Save()
        {
            if (RequiresSave)
            {
                try
                {
                    _Manager.Save();
                    RequiresSave = DataChanged = false;
                }
                catch (Exception ex)
                {
                    //Unable to save
                }
            }
        }

        public void Close()
        {
            if (_MainModel.CloseProject(this))
            {
                _MainModel.CloseTab(ProjectTab);
                _MainModel.CloseTab(DataEditorTab);
                _MainModel.CloseTab(DataEditorTab);
                MapWindow?.Close();
            }
        }

        public void CloseTab(TtTabModel tab)
        {
            if (!MultipleProjectViews)
            {
                //warn closing project
                Close();
            }
            else
            {
                if (tab is DataEditorTab)
                {
                    if (RequiresSave)
                    {
                        //warn closing editor
                    }
                    else
                    {
                        _MainModel.CloseTab(tab);
                    }

                    DataEditorTab = null;
                }
                else if (tab is MapTab)
                {

                    MapTab = null;
                }
                else if (tab is ProjectTab)
                {
                    ProjectTab = null;
                }
            }
        }

        public void CloseWindow(TtWindow window)
        {
            if (window is MapWindow)
            {

            }
        }



        private void ViewUserActivity()
        {

        }
    }
}
