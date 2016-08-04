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
using TwoTrails.ViewModels;

namespace TwoTrails.ViewModels
{
    public class TtProject : NotifyPropertyChangedEx
    {
        public String FilePath { get { return DAL.FilePath; } }

        public ICommand OpenDataEditor { get; }
        public ICommand OpenProject { get; }
        public ICommand OpenUserActivity { get; }
        public ICommand OpenMap { get; }
        public ICommand OpenMapWindow { get; }
        public ICommand ViewUserActivityCommand { get; set; }

        public ICommand UndoCommand { get; set; }
        public ICommand RedoCommand { get; set; }

        public ICommand DiscardChangesCommand { get; }

        public DataEditorTab DataEditorTab { get; private set; }
        private ProjectTab ProjectTab { get; set; }
        private MapTab MapTab { get; set; }
        private UserActivityTab UserActivityTab { get; set; }
        private MapWindow MapWindow { get; set; }

        public bool DataEditorTabIsOpen { get { return DataEditorTab != null; } }
        public bool MapTabIsOpen { get { return MapTab != null; } }
        public bool ProjectTabIsOpen { get { return ProjectTab != null; } }
        public bool UserActivityTabIsOpen { get { return DataEditorTab != null; } }
        public bool MapWindowIsOpen { get { return DataEditorTab != null; } }

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
            
            UndoCommand = new BindedRelayCommand<TtHistoryManager>(
                (x) => HistoryManager.Undo(), x => HistoryManager.CanUndo, HistoryManager, x => x.CanUndo);

            RedoCommand = new BindedRelayCommand<TtHistoryManager>(
                (x) => HistoryManager.Redo(), x => HistoryManager.CanRedo, HistoryManager, x => x.CanRedo);

            DiscardChangesCommand = new RelayCommand((x) => _Manager.Reset());

            ViewUserActivityCommand = new RelayCommand(x => ViewUserActivityTab());
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
                else if (tab is UserActivityTab)
                {
                    UserActivityTab = null;
                }
            }
        }

        public void CloseWindow(TtWindow window)
        {
            if (window is MapWindow)
            {

            }
        }



        private void OpenDataEditorTab()
        {

        }

        private void OpenProjectTab()
        {

        }

        private void OpenMapTab()
        {

        }

        private void DetachMapTab()
        {

        }

        private void ViewUserActivityTab()
        {

        }
    }
}
