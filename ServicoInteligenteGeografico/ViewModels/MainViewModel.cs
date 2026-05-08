using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicoInteligenteGeografico.Commands;
using ServicoInteligenteGeografico.Models;
using ServicoInteligenteGeografico.Repositories;
using ServicoInteligenteGeografico.Services;
using ServicoInteligenteGeografico.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
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

    public ObservableCollection<double> Temperaturas { get; set; } = new();
    public ObservableCollection<LocalizacaoGeo> Resultados { get; set; } = new();

    // Comandos
    public ICommand BuscarCommand { get; }
    public ICommand GerarPdfCommand { get; }
    public ICommand HistoricoCommand { get; }

    public MainViewModel()
    {
        _localizacaoRepo = new LocalizacaoRepository();
        _analiseRepo = new AnaliseRepository();

        BuscarCommand = new RelayCommand(async () => await BuscarAsync());
        GerarPdfCommand = new RelayCommand(async () => await GerarPdfAsync()); // Alterado para Async
        HistoricoCommand = new RelayCommand(async () => await CarregarHistoricoAsync());

        _ = CarregarHistoricoAsync();
    }

    private async Task BuscarAsync()
    {
        if (string.IsNullOrWhiteSpace(Localizacao)) return;

        LogService.RegistrarLog($"Tentativa de busca: {Localizacao}");

        try
        {
            var novaLocalizacao = new LocalizacaoGeo
            {
                Logradouro = Localizacao,
                Latitude = 0,
                Longitude = 0 // Simulação
            };

            var salvo = await _localizacaoRepo.SalvarAsync(novaLocalizacao);
            Resultados.Add(salvo);
            Temperaturas.Add(new Random().Next(15, 35));

            LogService.RegistrarLog($"Sucesso ao salvar no Firebase: {salvo.Id}", "SUCCESS");
        }
        catch (Exception ex)
        {
            LogService.RegistrarLog($"Erro na busca/salvamento: {ex.Message}", "ERROR");
            MessageBox.Show("Erro ao processar busca.");
        }
    }

    private async Task CarregarHistoricoAsync()
    {
        try
        {
            var lista = await _localizacaoRepo.BuscarTodasAsync();
            Resultados.Clear();
            Temperaturas.Clear();

            foreach (var item in lista) Resultados.Add(item);

            LogService.RegistrarLog("Histórico carregado do Firebase.");
        }
        catch (Exception ex)
        {
            LogService.RegistrarLog($"Falha ao carregar histórico: {ex.Message}", "ERROR");
        }
    }

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
                        // 1. Configura a página como Paisagem para caber todas as colunas
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1, Unit.Centimetre);

                        // Define o estilo de texto padrão para toda a página (evita erro de FontSize)
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Verdana));

                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Text("Relatório Histórico de Localizações")
                                .FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);

                            row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        });

                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            // 2. Definição das colunas (Total 8 colunas baseadas no seu modelo)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);  // ID
                                columns.RelativeColumn(3);   // Logradouro
                                columns.ConstantColumn(40);  // Numero
                                columns.RelativeColumn(2);   // Bairro
                                columns.RelativeColumn((float)1.2); // CEP
                                columns.RelativeColumn((float)1.2); // Latitude
                                columns.RelativeColumn((float)1.2); // Longitude
                                columns.RelativeColumn((float)1.5); // Timestamp
                            });

                            // 3. Cabeçalho
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("ID");
                                header.Cell().Element(HeaderStyle).Text("Logradouro");
                                header.Cell().Element(HeaderStyle).Text("Nº");
                                header.Cell().Element(HeaderStyle).Text("Bairro");
                                header.Cell().Element(HeaderStyle).Text("CEP");
                                header.Cell().Element(HeaderStyle).Text("Lat");
                                header.Cell().Element(HeaderStyle).Text("Long");
                                header.Cell().Element(HeaderStyle).Text("Data/Hora");

                                static IContainer HeaderStyle(IContainer c) =>
                                    c.Background(Colors.Grey.Lighten3).Padding(2).Border(0.5f).AlignCenter();
                            });

                            // 4. Loop nos dados (Puxando da Grid/Resultados)
                            foreach (var item in Resultados)
                            {
                                table.Cell().Element(CellStyle).Text(item.Id ?? "-");
                                table.Cell().Element(CellStyle).Text(item.Logradouro ?? "-");
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Numero ?? "-");
                                table.Cell().Element(CellStyle).Text(item.Bairro ?? "-");
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Cep ?? "-");
                                table.Cell().Element(CellStyle).Text(item.Latitude.ToString("F5"));
                                table.Cell().Element(CellStyle).Text(item.Longitude.ToString("F5"));
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Timestamp);

                                static IContainer CellStyle(IContainer c) =>
                                    c.Padding(2).Border(0.5f).AlignMiddle();
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

            LogService.RegistrarLog($"PDF gerado com sucesso: {caminhoPdf}", "SUCCESS");

            // Abre o PDF automaticamente
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(caminhoPdf) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LogService.RegistrarLog($"Erro ao exportar PDF: {ex.Message}", "ERROR");
            MessageBox.Show($"Erro ao gerar o PDF: {ex.Message}");
        }
    }
}
