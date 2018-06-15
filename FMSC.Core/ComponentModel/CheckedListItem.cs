using CSUtil.ComponentModel;
using System;

namespace FMSC.Core.ComponentModel
{
    public class CheckedListItem<T> : NotifyPropertyChangedEx
    {
        public event EventHandler ItemCheckedChanged;

        public T Item
        {
            get { return Get<T>(); }
            set { Set(value); }
        }

        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { SetField(ref _IsChecked, value, () => ItemCheckedChanged?.Invoke(this, new EventArgs())); }
        }

        public CheckedListItem() { }

        public CheckedListItem(T item, bool isChecked = false)
        {
            Item = item;
            IsChecked = isChecked;
        }

        public void SetChecked(bool isChecked, bool triggerCheckChanged = true)
        {
            _IsChecked = isChecked;
            OnPropertyChanged(nameof(IsChecked));

            if (triggerCheckChanged)
                ItemCheckedChanged?.Invoke(this, new EventArgs());
        }
    }
}
