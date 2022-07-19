using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TreeProject.Models
{
    public class Command:ICommand
    {
        private Action<object> execute;
        private Func<bool> canExecute;
        public Command(Action<object> execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute();
        }
    }
}
