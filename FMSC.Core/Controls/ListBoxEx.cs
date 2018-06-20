using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FMSC.Core.Controls
{
    public class ListBoxEx : ListBox
    {
        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(ListBoxEx), new PropertyMetadata(null));

        public ListBoxEx() { }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectedItemsList = base.SelectedItems;
            base.OnSelectionChanged(e);
        }
    }
}
