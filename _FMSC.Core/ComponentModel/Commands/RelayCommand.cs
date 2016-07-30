using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FMSC.Core.ComponentModel.Commands
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;


        private Predicate<object> _CanExecute;
        private Action<object> _Execute;


        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _Execute = execute;
            _CanExecute = canExecute;
        }


        public bool CanExecute(object parameter)
        {
            return _CanExecute == null || _CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _Execute?.Invoke(parameter);
        }
    }
}
