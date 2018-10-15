using System;
using System.Windows.Input;

namespace FMSC.Core.ComponentModel.Commands
{
    public interface ICanExecuteChanged
    {
        void RaiseCanExecuteChanged();
    }
    
    public class RelayCommand : ICommand, ICanExecuteChanged
    {
        public event EventHandler CanExecuteChanged;


        private Predicate<object> _CanExecute;
        private Action<object> _Execute;


        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _Execute = execute ?? throw new ArgumentNullException("execute");
            _CanExecute = canExecute;
        }

        public void OnCanExecuteChanged(object sender = null, EventArgs e = null)
        {
            CanExecuteChanged?.Invoke(sender, e);
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute == null || _CanExecute(parameter);
        }
        
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            _Execute?.Invoke(parameter);
        }
    }
}
