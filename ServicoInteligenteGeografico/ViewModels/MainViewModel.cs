using ServicoInteligenteGeografico.Commands;
using ServicoInteligenteGeografico.Models;
using ServicoInteligenteGeografico.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ServicoInteligenteGeografico.ViewModels
{
    /// <summary>
    /// ViewModel da janela principal.
    /// Conecta a interface (View) com o Firebase via Repositories.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly LocalizacaoRepository _localizacaoRepo;
        private readonly AnaliseRepository _analiseRepo;

        // ── Propriedades vinculadas à View ───────────────────────────────────

        private string _localizacao = string.Empty;
        public string Localizacao
        {
            get => _localizacao;
            set { _localizacao = value; OnPropertyChanged(nameof(Localizacao)); }
        }

        // Lista de temperaturas exibida no ListBox
        public ObservableCollection<double> Temperaturas { get; set; } = new();

        // Lista de resultados exibida no DataGrid
        public ObservableCollection<LocalizacaoGeo> Resultados { get; set; } = new();

        // ── Comandos dos botões ──────────────────────────────────────────────

        public ICommand BuscarCommand { get; }
        public ICommand GerarPdfCommand { get; }
        public ICommand HistoricoCommand { get; }

        // ── Construtor ───────────────────────────────────────────────────────

        public MainViewModel()
        {
            _localizacaoRepo = new LocalizacaoRepository();
            _analiseRepo = new AnaliseRepository();

            BuscarCommand = new RelayCommand(async () => await BuscarAsync());
            GerarPdfCommand = new RelayCommand(GerarPdf);
            HistoricoCommand = new RelayCommand(async () => await CarregarHistoricoAsync());

            // Carrega os dados do Firebase ao abrir a janela
            _ = CarregarHistoricoAsync();
        }

        // ── Métodos dos comandos ─────────────────────────────────────────────

        /// <summary>
        /// Busca localização digitada, salva no Firebase e atualiza a tela.
        /// </summary>
        private async Task BuscarAsync()
        {
            if (string.IsNullOrWhiteSpace(Localizacao))
            {
                MessageBox.Show("Digite uma localização para buscar.", "Atenção");
                return;
            }

            try
            {
                // Cria o objeto com os dados da busca
                var novaLocalizacao = new LocalizacaoGeo
                {
                    Logradouro = Localizacao,
                    // Latitude e Longitude seriam preenchidos por um serviço de geocoding
                    // Ex: Google Maps API, ViaCEP, etc.
                    Latitude = 0,
                    Longitude = 0
                };

                // Salva no Firebase (único ponto de acesso ao banco)
                var salvo = await _localizacaoRepo.SalvarAsync(novaLocalizacao);

                // Atualiza a lista na tela
                Resultados.Add(salvo);

                // Simula uma temperatura para exibir no ListBox
                // (aqui você integraria com a API de clima do seu projeto)
                Temperaturas.Add(new Random().Next(15, 35));

                MessageBox.Show($"Localização '{Localizacao}' salva com sucesso!\nId: {salvo.Id}", "Firebase ✓");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar no Firebase:\n{ex.Message}", "Erro");
            }
        }

        /// <summary>
        /// Carrega todo o histórico do Firebase e exibe no DataGrid.
        /// </summary>
        private async Task CarregarHistoricoAsync()
        {
            try
            {
                var lista = await _localizacaoRepo.BuscarTodasAsync();

                Resultados.Clear();
                Temperaturas.Clear();

                foreach (var item in lista)
                {
                    Resultados.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar histórico:\n{ex.Message}", "Erro");
            }
        }

        private void GerarPdf()
        {
            MessageBox.Show("Funcionalidade de gerar PDF será implementada aqui.", "Gerar PDF");
        }
    }
}
