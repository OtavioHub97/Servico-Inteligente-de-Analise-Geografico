
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicoInteligenteGeografico.Commands;
using ServicoInteligenteGeografico.Models;
using ServicoInteligenteGeografico.Repositories;
using ServicoInteligenteGeografico.Services;
using ServicoInteligenteGeografico.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

public class MainViewModel : BaseViewModel
{
    private readonly LocalizacaoRepository _localizacaoRepo;
    private readonly AnaliseRepository _analiseRepo;

    // Propriedades e Listas...
    private string _localizacao = string.Empty;
    public string Localizacao
    {
        get => _localizacao;
        set { _localizacao = value; OnPropertyChanged(nameof(Localizacao)); }
    }

    // Propriedades para o cálculo de distância
    private string _latitude;
    public string Latitude
    {
        get => _latitude;
        set { _latitude = value; OnPropertyChanged(nameof(Latitude)); }
    }

    private string _longitude;
    public string Longitude
    {
        get => _longitude;
        set { _longitude = value; OnPropertyChanged(nameof(Longitude)); }
    }

    private string _resultadoFormatado;
    public string ResultadoFormatado
    {
        get => _resultadoFormatado;
        set { _resultadoFormatado = value; OnPropertyChanged(nameof(ResultadoFormatado)); }
    }

    // Propriedades para filtro
    private string _filtroTexto = string.Empty;
    public string FiltroTexto
    {
        get => _filtroTexto;
        set
        {
            _filtroTexto = value;
            OnPropertyChanged(nameof(FiltroTexto));
            AplicarFiltro(); // Filtra enquanto o usuário digita
        }
    }

    private string _tipoFiltro = "Bairro"; // Padrão
    public string TipoFiltro
    {
        get => _tipoFiltro;
        set { _tipoFiltro = value; OnPropertyChanged(nameof(TipoFiltro)); }
    }

    public ObservableCollection<double> Locais { get; set; } = new();
    public ObservableCollection<LocalizacaoGeo> Resultados { get; set; } = new();


    // Comandos
    public ICommand BuscarCommand { get; }
    public ICommand GerarPdfCommand { get; }
    public ICommand HistoricoCommand { get; }

    public ICommand CalcularCommand { get; }

    public ICommand AbrirLogsCommand => new RelayCommand(() =>
    {
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.txt");
        if (File.Exists(logPath))
        {
            Process.Start(new ProcessStartInfo(logPath) { UseShellExecute = true });
        }
        else
        {
            MessageBox.Show("Arquivo de logs não encontrado.");
        }
    });

    public MainViewModel()
    {
        _localizacaoRepo = new LocalizacaoRepository();
        _analiseRepo = new AnaliseRepository();

        BuscarCommand = new RelayCommand(async () => await BuscarAsync());
        GerarPdfCommand = new RelayCommand(async () => await GerarPdfAsync()); // Alterado para Async
        HistoricoCommand = new RelayCommand(async () => await CarregarHistoricoAsync());
        CalcularCommand = new RelayCommand(() => Calcular());

        _ = CarregarHistoricoAsync();
    }

    private async Task BuscarAsync()
    {
        if (string.IsNullOrWhiteSpace(Localizacao)) return;

        LogService.RegistrarLog($"Iniciando busca e salvamento: {Localizacao}");

        try
        {
            // 1. Cria o objeto (Aqui você poderia usar uma API de Geocoding para pegar Lat/Long reais)
            var novaLocalizacao = new LocalizacaoGeo
            {
                Logradouro = Localizacao,
                Bairro = "Bairro Exemplo", // Ideal obter via serviço
                Latitude = -23.5505,       // Simulação de coordenada real
                Longitude = -46.6333,      // Simulação de coordenada real
                Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            };

            // 2. Salva no Firebase
            var salvo = await _localizacaoRepo.SalvarAsync(novaLocalizacao);
            LogService.RegistrarLog($"Salvo no Firebase ID: {salvo.Id}", "SUCCESS");

            // 3. Atualiza a lista completa e aplica o RANKING (Ordenação)
            var listaDeBanco = await _localizacaoRepo.BuscarTodasAsync();

            // Aplica o ranking por Bairro e Logradouro
            var listaRankeada = listaDeBanco
                .OrderBy(x => x.Bairro)
                .ThenBy(x => x.Logradouro)
                .ToList();

            // 4. Atualiza a UI de forma limpa
            Resultados.Clear();
            foreach (var item in listaRankeada)
            {
                Resultados.Add(item);
            }

            // Simulação de temperatura/clima para o novo local
            Locais.Add(new Random().Next(15, 35));
        }
        catch (Exception ex)
        {
            LogService.RegistrarLog($"Erro no processo: {ex.Message}", "ERROR");
            MessageBox.Show("Erro ao processar busca e atualizar ranking.");
        }
    }

    private async Task CarregarHistoricoAsync()
    {
        try
        {
            var lista = await _localizacaoRepo.BuscarTodasAsync();
            Resultados.Clear();
            Locais.Clear();

            foreach (var item in lista) Resultados.Add(item);

            LogService.RegistrarLog("Histórico carregado do Firebase.");
        }
        catch (Exception ex)
        {
            LogService.RegistrarLog($"Falha ao carregar histórico: {ex.Message}", "ERROR");
        }
    }

    // Implementação do Cálculo de distância 
    private void Calcular()
    {
        if (double.TryParse(Latitude, out double distaciaLat) && double.TryParse(Longitude, out double distanciaLon))
        {
            // Exemplo calculando a partir do primeiro item da lista ou localização atual
            var pontoA = Resultados.FirstOrDefault();
            if (pontoA == null) return;

            double distancia = CalcularDistancia(pontoA.Latitude, pontoA.Longitude, distaciaLat, distanciaLon);
            ResultadoFormatado = $"Distância: {distancia:F2} km";
        }
    }

    private double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Raio da Terra em km
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    // Método para aplicar o filtro na lista que a DataGrid exibe
    private void AplicarFiltro()
    {
        var view = CollectionViewSource.GetDefaultView(Resultados);

        if (string.IsNullOrWhiteSpace(FiltroTexto))
        {
            view.Filter = null;
        }
        else
        {
            view.Filter = (obj) =>
            {
                var item = obj as LocalizacaoGeo;
                if (item == null) return false;

                // Lógica dinâmica baseada no que foi selecionado no ComboBox
                return TipoFiltro switch
                {
                    "Bairro" => item.Bairro?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false,
                    "Logradouro" => item.Logradouro?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false,
                    "CEP" => item.Cep?.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ?? false,
                    _ => true
                };
            };
        }
    }

    private double ToRadians(double angle) => Math.PI * angle / 180.0;

    // Implementação básica de INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    private async Task GerarPdfAsync()
    {
        if (Resultados == null || !Resultados.Any())
        {
            MessageBox.Show("Não há dados no histórico para gerar o PDF.", "Aviso");
            return;
        }

        try
        {
            string caminhoPdf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Relatorio_Geografico.pdf");

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Verdana));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Text("Relatório Histórico de Localizações")
                                .FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);

                            row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.5f);
                            });

                            // 3. Cabeçalho corrigido com Lambda
                            table.Header(header =>
                            {
                                header.Cell().Element(c => HeaderStyle(c)).Text("ID");
                                header.Cell().Element(c => HeaderStyle(c)).Text("Logradouro");
                                header.Cell().Element(c => HeaderStyle(c)).Text("Nº");
                                header.Cell().Element(c => HeaderStyle(c)).Text("Bairro");
                                header.Cell().Element(c => HeaderStyle(c)).Text("CEP");
                                header.Cell().Element(c => HeaderStyle(c)).Text("Lat");
                                header.Cell().Element(c => HeaderStyle(c)).Text("Long");
                                header.Cell().Element(c => HeaderStyle(c)).Text("Data/Hora");
                            });

                            // 4. Loop corrigido com Lambda
                            foreach (var item in Resultados)
                            {
                                table.Cell().Element(c => CellStyle(c)).Text(item.Id ?? "-");
                                table.Cell().Element(c => CellStyle(c)).Text(item.Logradouro ?? "-");
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.Numero ?? "-");
                                table.Cell().Element(c => CellStyle(c)).Text(item.Bairro ?? "-");
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.Cep ?? "-");
                                table.Cell().Element(c => CellStyle(c)).Text(item.Latitude.ToString("F5"));
                                table.Cell().Element(c => CellStyle(c)).Text(item.Longitude.ToString("F5"));
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.Timestamp);
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                })
                .GeneratePdf(caminhoPdf);
            });

            LogService.RegistrarLog($"PDF gerado: {caminhoPdf}", "SUCCESS");
            Process.Start(new ProcessStartInfo(caminhoPdf) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LogService.RegistrarLog($"Erro PDF: {ex.Message}", "ERROR");
            MessageBox.Show($"Erro ao gerar o PDF: {ex.Message}");
        }
    }

    // Funções auxiliares (Certifique-se que estão acessíveis no escopo ou como static)
    static QuestPDF.Infrastructure.IContainer HeaderStyle(QuestPDF.Infrastructure.IContainer c) =>
        c.Background(Colors.Grey.Lighten3).Padding(2).Border(0.5f).AlignCenter();

    static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer c) =>
        c.Padding(2).Border(0.5f).AlignMiddle();
}
