using System;
using System.Windows;
using TwoTrails.ViewModels;
using TwoTrails.ViewModels.DataDictionary;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for DataDictionaryEditor.xaml
    /// </summary>
    public partial class DataDictionaryEditorDialog : Window
    {
        public DataDictionaryEditorDialog(TtProject project)
        {
            this.DataContext = new DataDictionaryEditorModel(this, project);
            InitializeComponent();
        }
        
        public static void Show(TtProject project, Window owner = null, Action<bool?> onClose = null)
        {
            DataDictionaryEditorDialog dialog = new DataDictionaryEditorDialog(project);

            if (owner != null)
                dialog.Owner = owner;
            else
                dialog.Owner = project.MainModel.MainWindow;

            if (onClose != null)
            {
                dialog.Closed += (s, e) => onClose(dialog.DialogResult);
            }

            dialog.Show();
        }
    }
}
