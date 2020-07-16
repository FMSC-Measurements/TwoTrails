using System;
using System.Windows;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for AnalyzeProjectWindow.xaml
    /// </summary>
    public partial class AnalyzeProjectDialog : Window
    {
        public AnalyzeProjectDialog(TtProject project)
        {
            this.DataContext = new AnalyzeProjectModel(this, project);

            InitializeComponent();
        }

        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            AnalyzeProjectDialog dialog = new AnalyzeProjectDialog(project);

            if (owner != null)
                dialog.Owner = owner;
            else
                dialog.Owner = project.MainModel.MainWindow;

            return dialog.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action onClose = null)
        {
            AnalyzeProjectDialog dialog = new AnalyzeProjectDialog(project);

            if (owner != null)
                dialog.Owner = owner;
            else
                dialog.Owner = project.MainModel.MainWindow;

            if (onClose != null)
            {
                dialog.Closed += (s, e) => onClose();
            }

            dialog.Show();
        }
    }
}
