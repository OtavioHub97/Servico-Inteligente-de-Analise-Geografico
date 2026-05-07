using System;
using System.Windows.Input;

namespace ServicoInteligenteGeografico.Commands
{
    /// <summary>
    /// Implementação de ICommand que suporta tanto ações síncronas quanto assíncronas.
    /// Usado para vincular botões da View a métodos do ViewModel.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<Task>? _executeAsync;
        private readonly Action? _execute;
        private readonly Func<bool>? _canExecute;

        // Construtor para ações ASSÍNCRONAS (ex: chamadas ao Firebase)
        public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        // Construtor para ações SÍNCRONAS
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public async void Execute(object? parameter)
        {
            if (_executeAsync != null)
                await _executeAsync();
            else
                _execute?.Invoke();
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}