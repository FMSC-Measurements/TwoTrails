using FMSC.Core.Collections;
using FMSC.Core.Windows.Controls;
using FMSC.Core.Windows.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for RenamePointsDialog.xaml
    /// </summary>
    public partial class CreateSubsetDialog : Window
    {
        private TtProject _Project;

        public int SubsetValue { get; set; }
        public bool IsPercentMode { get; set; } = true;
        public bool DeleteExisting { get; set; }

        public ObservableFilteredCollection<TtPolygon> PlotPolygons { get; }

        public CreateSubsetDialog(TtProject project)
        {
            _Project = project;

            PlotPolygons = new ObservableFilteredCollection<TtPolygon>(_Project.Manager.Polygons,
                poly => _Project.Manager.GetPoints(poly.CN).All(p => p.IsWayPointAtBase()));

            InitializeComponent();
            this.DataContext = this;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsUnsignedInteger(sender, e);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsShownAsDialog())
                this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsShownAsDialog())
                this.DialogResult = false;
            this.Close();
        }

        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            CreateSubsetDialog dialog = new CreateSubsetDialog(project);

            if (owner != null)
                dialog.Owner = owner;
            else
                dialog.Owner = project.MainModel.MainWindow;

            return dialog.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action onClose = null)
        {
            CreateSubsetDialog dialog = new CreateSubsetDialog(project);

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
