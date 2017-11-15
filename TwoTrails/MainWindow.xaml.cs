using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
