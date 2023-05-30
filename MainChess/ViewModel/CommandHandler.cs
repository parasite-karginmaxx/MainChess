using System;
using System.Windows.Input;

namespace MainChess.ViewModel;
internal class CommandHandler : ICommand
{
    private readonly Action _action;

    public CommandHandler(Action action)
    {
        _action = action;
    }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _action();

    public event EventHandler CanExecuteChanged;
}