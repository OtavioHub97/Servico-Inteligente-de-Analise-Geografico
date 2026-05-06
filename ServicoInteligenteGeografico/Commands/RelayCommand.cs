using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServicoInteligenteGeografico.Commands
{
    class RelayCommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        //verifica se pode ser executado
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        //executando sistema
        public void Execute(object parameter)
        {
            execute();
        }

        //verifica se mudou e vai adicionar ou remover um evento.
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
