using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for DataDictionaryEditor.xaml
    /// </summary>
    public partial class DataDictionaryEditorDialog : Window
    {
        public DataDictionaryEditorDialog(TtProject project)
        {
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
