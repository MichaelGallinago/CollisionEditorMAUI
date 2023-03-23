using System.Windows.Input;
using System;

namespace CollisionEditor.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action action;
        public RelayCommand(Action action) => this.action = action;
        public bool CanExecute(object? parameter) => true;
        public event EventHandler? CanExecuteChanged;
        public void Execute(object? parameter) => action();
    }
}
