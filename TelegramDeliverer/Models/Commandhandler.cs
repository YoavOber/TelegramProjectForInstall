using System;
using System.Windows.Input;

namespace TelegramDeliverer.Models
{
    public class CommandHandler : ICommand
    {
        private Action<object> action;
        private Func<bool> canExecute;

        public CommandHandler(Action<object> _action, Func<bool> _canExec)
        {
            action = _action;
            canExecute = _canExec;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object param) => canExecute.Invoke();
        public void Execute(object param) => action(param);
    }
}
