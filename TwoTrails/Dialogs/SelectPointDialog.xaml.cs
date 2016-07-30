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
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectPointDialog.xaml
    /// </summary>
    public partial class SelectPointDialog : Window
    {
        public TtPoint SelectedPoint { get; private set; }

        public SelectPointDialog(ITtManager manager, List<TtPoint> hidePoints = null)
        {
            InitializeComponent();
        }


    }
}
