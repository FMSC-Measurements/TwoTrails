using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
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
                foreach (string file in files)
                {
                    MainModel.OpenProject(file);
                }
            }
        }
    }
}
