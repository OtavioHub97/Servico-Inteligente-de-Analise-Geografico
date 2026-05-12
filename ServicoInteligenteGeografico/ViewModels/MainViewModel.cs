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

    private readonly MapasApiService _mapasApiService;


    private string _buscaApiId = string.Empty;
    public string BuscaApiId
    {
        get => _buscaApiId;
        set { _buscaApiId = value; OnPropertyChanged(nameof(BuscaApiId)); }
    }

    private string _buscaApiLogradouro = string.Empty;
    public string BuscaApiLogradouro
    {
        get => _buscaApiLogradouro;
        set { _buscaApiLogradouro = value; OnPropertyChanged(nameof(BuscaApiLogradouro)); }
    }

    private string _statusApi = string.Empty;
    public string StatusApi
    {
        get => _statusApi;
        set { _statusApi = value; OnPropertyChanged(nameof(StatusApi)); }
    }

    private bool _carregandoApi = false;
    public bool CarregandoApi
    {
        get => _carregandoApi;
        set { _carregandoApi = value; OnPropertyChanged(nameof(CarregandoApi)); }
    }

    public ObservableCollection<LocalizacaoGeo> ResultadosApi { get; set; } = new();

    private string _localizacao = string.Empty;
    public string Localizacao
    {
        get => _localizacao;
        set { _localizacao = value; OnPropertyChanged(nameof(Localizacao)); }
    }

    private string _latOrigem = "0";
    public string LatitudeOrigem
    {
        get => _latOrigem;
        set { _latOrigem = value; OnPropertyChanged(nameof(LatitudeOrigem)); }
    }

    private string _longOrigem = "0";
    public string LongitudeOrigem
    {
        get => _longOrigem;
        set { _longOrigem = value; OnPropertyChanged(nameof(LongitudeOrigem)); }
    }

    private string _latDestino = "0";
    public string Latitude
    {
        get => _latDestino;
        set { _latDestino = value; OnPropertyChanged(nameof(Latitude)); }
    }

    private string _longDestino = "0";
    public string Longitude
    {
        get => _longDestino;
        set { _longDestino = value; OnPropertyChanged(nameof(Longitude)); }
    }

    private string _resultadoFormatado;
    public string ResultadoFormatado
    {
        get => _resultadoFormatado;
        set { _resultadoFormatado = value; OnPropertyChanged(nameof(ResultadoFormatado)); }
    }

    private string _filtroTexto = string.Empty;
    public string FiltroTexto
    {
        get => _filtroTexto;
        set
        {
            _filtroTexto = value;
            OnPropertyChanged(nameof(FiltroTexto));
            AplicarFiltro();
        }
    }

    private string _tipoFiltro = "Bairro";
    public string TipoFiltro
    {
        get => _tipoFiltro;
        set { _tipoFiltro = value; OnPropertyChanged(nameof(TipoFiltro)); }
    }

    public ObservableCollection<double> Locais { get; set; } = new();
    public ObservableCollection<LocalizacaoGeo> Dados { get; set; } = new();
    public ObservableCollection<LocalizacaoGeo> Resultados { get; set; } = new();

    public ICommand BuscarCommand { get; }
    public ICommand LimparCommand { get; }
    public ICommand GerarPdfCommand { get; }
    public ICommand HistoricoCommand { get; }
    public ICommand CalcularCommand { get; }
    public ICommand ApiListarTodasCommand { get; }
    public ICommand ApiBuscarPorIdCommand { get; }
    public ICommand ApiBuscarPorLogradouroCommand { get; }
    public ICommand ApiLimparCommand { get; }

    public MainViewModel()
    {
        _mapasApiService = new MapasApiService();

        ApiListarTodasCommand = new RelayCommand(async () => await CarregarRankingAsync());
        ApiBuscarPorIdCommand = new RelayCommand(async () => await BuscarAsync());
        ApiBuscarPorLogradouroCommand = new RelayCommand(async () => await BuscarAsync());
        ApiLimparCommand = new RelayCommand(() => Limpar());

        CalcularCommand = new RelayCommand(() => Calcular());

        _localizacaoRepo = new LocalizacaoRepository();
        _analiseRepo = new AnaliseRepository();

        BuscarCommand = new RelayCommand(async () => await BuscarAsync());
        GerarPdfCommand = new RelayCommand(async () => await GerarPdfAsync());
        HistoricoCommand = new RelayCommand(async () => await CarregarHistoricoAsync());
        LimparCommand = new RelayCommand(() => Limpar());

        _ = CarregarRankingAsync();
    }

    private async Task BuscarAsync()
    {
        try
        {
            var listaDeBanco = await _mapasApiService.BuscarTodasAsync();

            Dados.Clear();

            foreach (var item in listaDeBanco)
                Dados.Add(item);

            if (Dados.Count == 0)
                MessageBox.Show("A API retornou uma lista vazia.");

            LogService.RegistrarLog($"Carga de dados realizada. Itens carregados: {Dados.Count}");
        }
        catch (Exception ex)
        {
            LogService.RegistrarLog($"Erro ao carregar dados: {ex.Message}", "ERROR");
            MessageBox.Show($"Erro ao buscar dados na API:\n{ex.Message}", "Erro de API");
        }
    }

    private void Limpar()
    {
        Dados.Clear();
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

    private async Task Calcular()
    {
        try
        {
            double converter(string valor)
            {
                if (string.IsNullOrWhiteSpace(valor)) return 0;
                string limpo = valor.Replace(',', '.');
                if (double.TryParse(limpo, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double res))
                    return res;
                return 0;
            }

            double lat1 = converter(LatitudeOrigem);
            double lon1 = converter(LongitudeOrigem);
            double lat2 = converter(Latitude);
            double lon2 = converter(Longitude);

            if (lat1 == 0 && lat2 == 0)
            {
                ResultadoFormatado = "Atenção: Destino não informado.";
                return;
            }

            double d = CalcularDistancia(lat1, lon1, lat2, lon2);
            ResultadoFormatado = $"Distância: {d:F2} km";
        }
        catch (Exception ex)
        {
            ResultadoFormatado = "Erro interno: " + ex.Message;
        }
    }

    private double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private async Task CarregarRankingAsync()
    {
        try
        {
            var lista = await _mapasApiService.BuscarTodasAsync();

            Resultados.Clear();
            foreach (var item in lista)
                Resultados.Add(item);

            AplicarFiltro();
            LogService.RegistrarLog("Ranking atualizado.");
        }
        catch (Exception ex)
        {
            // ✅ Agora mostra o erro real
            MessageBox.Show($"Erro ao carregar ranking:\n{ex.Message}", "Erro de API");
        }
    }

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
                            row.RelativeItem().Text("Relatório de Localizações")
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

                            table.Header(header =>
                            {
                                header.Cell().Element(c => HeaderStyle(c)).Text("ID").FontColor(Colors.White).Bold();
                                header.Cell().Element(c => HeaderStyle(c)).Text("Logradouro").FontColor(Colors.White).Bold();
                                header.Cell().Element(c => HeaderStyle(c)).Text("Nº").FontColor(Colors.White).Bold();
                                header.Cell().Element(c => HeaderStyle(c)).Text("Bairro").FontColor(Colors.White).Bold();
                                header.Cell().Element(c => HeaderStyle(c)).Text("CEP").FontColor(Colors.White).Bold();
                                header.Cell().Element(c => HeaderStyle(c)).Text("Lat").FontColor(Colors.White).Bold();
                                header.Cell().Element(c => HeaderStyle(c)).Text("Long").FontColor(Colors.White).Bold();
                                header.Cell().Element(c => HeaderStyle(c)).Text("Data/Hora").FontColor(Colors.White).Bold();
                            });

                            foreach (var item in Resultados)
                            {
                                table.Cell().Element(c => CellStyle(c)).Text(item.Id ?? "-").FontColor(Colors.Black);
                                table.Cell().Element(c => CellStyle(c)).Text(item.Logradouro ?? "-").FontColor(Colors.Black);
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.Numero ?? "-").FontColor(Colors.Black);
                                table.Cell().Element(c => CellStyle(c)).Text(item.Bairro ?? "-").FontColor(Colors.Black);
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.Cep ?? "-").FontColor(Colors.Black);
                                table.Cell().Element(c => CellStyle(c)).Text(item.Latitude.ToString("F5")).FontColor(Colors.Black);
                                table.Cell().Element(c => CellStyle(c)).Text(item.Longitude.ToString("F5")).FontColor(Colors.Black);
                                table.Cell().Element(c => CellStyle(c)).AlignCenter().Text(item.Timestamp).FontColor(Colors.Black);
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

    static QuestPDF.Infrastructure.IContainer HeaderStyle(QuestPDF.Infrastructure.IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Blue.Darken2).Background(Colors.Blue.Darken1)
         .Padding(2).AlignCenter().AlignMiddle();

    static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Lighten1)
         .Padding(2).AlignLeft().AlignMiddle();
}