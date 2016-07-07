using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl, ITtTabContent
    {

        public TtTabModel TabModel { get; }




        public ObservableCollection<TtPoint> _Points { get; set; }
        //public ICollectionView Points { get; set; }
        public CollectionViewSource Points { get; set; }



        ITtManager manager;

        public ProjectControl(TtTabModel tabModel)
        {
            TabModel = tabModel;

            this.manager = TabModel.Project.Manager;

            InitializeComponent();
            
            _Points = new ObservableCollection<TtPoint>(manager.GetPoints());

            Points = new CollectionViewSource();
            Points.Source = _Points;
            //Points.SortDescriptions = new SortDescriptionCollection();
            //Points = CollectionViewSource.GetDefaultView(_Points);

            this.DataContext = this;
        }





    }
}
