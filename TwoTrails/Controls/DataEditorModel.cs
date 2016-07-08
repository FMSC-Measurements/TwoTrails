using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Controls
{
    public class DataEditorModel : NotifyPropertyChangedEx
    {
        private ICollectionView _Points;
        public ICollectionView Points
        {
            get { return _Points; }
            set { SetField(ref _Points, value); }
        }

        ITtManager _Manager;


        public DataEditorModel(TtProject project)
        {
            _Manager = project.Manager;

            Points = CollectionViewSource.GetDefaultView(_Manager.GetPoints());
            Points.Filter = Filter;
        }
        

        private bool Filter(object obj)
        {
            return true;
        }

    }
}
