using System;
using System.Windows.Input;

namespace IVPN.ViewModels
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> __MethodToExecute;
        private readonly Func<bool> __CanExecuteEvaluator;

        public RelayCommand(Action<T> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            __MethodToExecute = methodToExecute;
            __CanExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action<T> methodToExecute)
            : this(methodToExecute, null)
        {}

        public bool CanExecute(object parameter)
        {
            if (__CanExecuteEvaluator == null)
                return true;
            
            bool result = __CanExecuteEvaluator.Invoke();
            CanExecuteChanged(this, EventArgs.Empty);
            return result;
        }

        public void Execute(object parameter)
        {
            __MethodToExecute.Invoke((T)parameter);
        }

        public event EventHandler CanExecuteChanged = delegate {};
    }
}
