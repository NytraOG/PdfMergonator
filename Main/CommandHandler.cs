using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Main;

public class CommandHandler : ICommand
{
    private readonly Func<Task> action;
    private readonly Func<bool> canExecute;

    public CommandHandler(Func<Task> action, Func<bool> canExecute)
    {
        this.action     = action;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canExecute.Invoke();

    public async void Execute(object? parameter) => await action();

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}