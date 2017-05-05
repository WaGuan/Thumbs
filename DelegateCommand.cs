using System;
using System.Windows.Input;

namespace Thumbs
{
    class DelegateCommand : ICommand
    {
        Action _execute;
        Func<bool> _canExecute;

        public DelegateCommand(Action Execute, Func<bool> CanExecute = null)
        {
            _execute = Execute;
            _canExecute = CanExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute?.Invoke();
    }
}
