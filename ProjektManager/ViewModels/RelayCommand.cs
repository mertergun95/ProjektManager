using System.Windows.Input;

namespace ProjektManager.ViewModels
{
    /// <summary>Einfache ICommand-Implementierung für Button/MenuItem-Bindings ohne Parameter.</summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _ausfuehren;
        private readonly Func<bool>? _kannAusfuehren;

        public RelayCommand(Action ausfuehren, Func<bool>? kannAusfuehren = null)
        {
            _ausfuehren = ausfuehren;
            _kannAusfuehren = kannAusfuehren;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _kannAusfuehren?.Invoke() ?? true;

        public void Execute(object? parameter) => _ausfuehren();
    }

    /// <summary>Einfache ICommand-Implementierung mit typisiertem Parameter (z.B. eine Leistung/ein Projekt).</summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _ausfuehren;
        private readonly Func<T?, bool>? _kannAusfuehren;

        public RelayCommand(Action<T?> ausfuehren, Func<T?, bool>? kannAusfuehren = null)
        {
            _ausfuehren = ausfuehren;
            _kannAusfuehren = kannAusfuehren;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _kannAusfuehren?.Invoke((T?)parameter) ?? true;

        public void Execute(object? parameter) => _ausfuehren((T?)parameter);
    }
}
