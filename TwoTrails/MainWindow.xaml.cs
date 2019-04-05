using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.Utils;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowModel MainModel;

        public MainWindow()
        {
            InitializeComponent();

            if (SystemParameters.PrimaryScreenHeight < 840 || SystemParameters.PrimaryScreenWidth < 1000)
            {
                this.Width = 700;
                this.Height = 500;
            }

            MainModel = new MainWindowModel(this);
            this.DataContext = MainModel;

            if (Application.Current is App app)
                app.ExternalInstanceArgs += (object sender, IEnumerable<string> args) =>
                {
                    this.Activate();
                    if (args != null)
                    {
                        foreach (string proj in args)
                            MainModel.OpenProject(proj); 
                    }
                };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = MainModel != null ? !MainModel.CanExit : false;
        }

        private void FilesDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                Task.Run(() => //Done so not to lock up file explorer from drag/drop
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (MainModel.CurrentProject == null)
                        {
                            if (files.All(file => TtUtils.IsImportableFileType(file) && !file.EndsWith(Consts.FILE_EXTENSION, System.StringComparison.InvariantCultureIgnoreCase)))
                            {
                                MainModel.CreateAndOpenProjectFromImportable(null, files);
                            }
                            else
                            {
                                foreach (string file in files)
                                {
                                    if (file.EndsWith(Consts.FILE_EXTENSION, System.StringComparison.InvariantCultureIgnoreCase) ||
                                        file.EndsWith(Consts.FILE_EXTENSION_MEDIA, System.StringComparison.InvariantCultureIgnoreCase))
                                        MainModel.OpenProject(file);
                                    else if (TtUtils.IsImportableFileType(file))
                                        MainModel.CreateAndOpenProjectFromImportable(null, file);
                                }
                            }
                        }
                        else
                        {
                            foreach (string file in files)
                            {
                                MainModel.OpenProject(file);
                            }
                        }
                    });
                });
            }
        }
    }
}
