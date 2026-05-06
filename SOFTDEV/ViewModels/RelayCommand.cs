using System;
using System.Windows.Input;

namespace SOFTDEV.ViewModels
{
    /// <summary>
    /// A lightweight ICommand implementation that delegates CanExecute and Execute
    /// to caller-supplied delegates.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <inheritdoc />
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>Raised when the result of <see cref="CanExecute"/> may have changed.</summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>Manually raises <see cref="CanExecuteChanged"/> to prompt WPF to re-query <see cref="CanExecute"/>.</summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
