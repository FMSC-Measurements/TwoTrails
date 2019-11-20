using CSUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
            e.Cancel = MainModel != null ? !MainModel.ShouldExit() : false;
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
                            if (files.HasAtLeast(2))
                            {
                                if (MessageBox.Show("Would you like to import all of the files into one project?", "Multi File Import",
                                MessageBoxButton.YesNo, MessageBoxImage.Hand) == MessageBoxResult.Yes)
                                {
                                    MainModel.CreateAndOpenProjectFromImportable(null, files);
                                }
                                else
                                {
                                    foreach (string file in files)
                                    {
                                        if (file.EndsWith(Consts.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase) ||
                                            file.EndsWith(Consts.FILE_EXTENSION_MEDIA, StringComparison.InvariantCultureIgnoreCase) ||
                                            file.EndsWith(Consts.FILE_EXTENSION_V2, StringComparison.InvariantCultureIgnoreCase))
                                            MainModel.OpenProject(file);
                                        else if (TtUtils.IsImportableFileType(file))
                                            MainModel.CreateAndOpenProjectFromImportable(null, file);
                                        else
                                            MessageBox.Show($"File '{Path.GetFileName(file)}' is not a valid openable or importable type.", "Invalid File Type", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            else
                            {
                                MainModel.OpenProject(files[0]);
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
