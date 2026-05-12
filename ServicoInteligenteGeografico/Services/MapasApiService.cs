using ServicoInteligenteGeografico.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServicoInteligenteGeografico.Services
{
    /// <summary>
    /// Serviço responsável por consumir a API REST de Mapas.
    /// Implementa os três endpoints disponíveis:
    ///   GET /api/Mapas                          → lista todas as localizações
    ///   GET /api/Mapas/{id}                     → busca por ID
    ///   GET /api/Mapas/logradouro/{logradouro}  → busca por logradouro
    /// </summary>
    public class MapasApiService
    {
        private const string BaseUrl = "https://webapimaps.runasp.net/api/Mapas";

        // HttpClient estático/compartilhado (boa prática para evitar socket exhaustion)
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,          // aceita tanto "Id" quanto "id"
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // ── GET /api/Mapas ─────────────────────────────────────────────────
        /// <summary>
        /// Retorna todas as localizações cadastradas no Firebase via API.
        /// A API retorna um objeto wrapper: { "mensagem": "...", "localizacoes": [...] }
        /// </summary>
        public async Task<List<LocalizacaoGeo>> BuscarTodasAsync()
        {
            LogService.RegistrarLog("MapasApiService: iniciando GET /api/Mapas", "INFO");

            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                LogService.RegistrarLog($"MapasApiService: resposta recebida ({json.Length} chars)", "INFO");

                // A API retorna { "mensagem": "...", "localizacoes": [...] }
                var resposta = JsonSerializer.Deserialize<RespostaMapas>(json, _jsonOptions);
                var lista = resposta?.Localizacoes ?? new List<LocalizacaoGeo>();

                LogService.RegistrarLog($"MapasApiService: {lista.Count} localização(ões) desserializadas.", "INFO");
                return lista;
            }
            catch (HttpRequestException ex)
            {
                LogService.RegistrarLog($"MapasApiService: erro HTTP em BuscarTodasAsync – {ex.Message}", "ERRO");
                throw;
            }
            catch (JsonException ex)
            {
                LogService.RegistrarLog($"MapasApiService: erro de JSON em BuscarTodasAsync – {ex.Message}", "ERRO");
                throw;
            }
        }

        // ── GET /api/Mapas/{id} ────────────────────────────────────────────
        /// <summary>
        /// Retorna uma localização específica pelo seu ID.
        /// Retorna null se a API devolver 404.
        /// </summary>
        public async Task<LocalizacaoGeo?> BuscarPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("O ID não pode ser vazio.", nameof(id));

            string url = $"{BaseUrl}/{Uri.EscapeDataString(id)}";
            LogService.RegistrarLog($"MapasApiService: iniciando GET {url}", "INFO");

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    LogService.RegistrarLog($"MapasApiService: ID '{id}' não encontrado (404).", "AVISO");
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // Tenta wrapper primeiro, depois objeto direto
                if (json.Contains("\"localizacoes\"") || json.Contains("\"mensagem\""))
                {
                    var resposta = JsonSerializer.Deserialize<RespostaMapas>(json, _jsonOptions);
                    return resposta?.Localizacoes?.FirstOrDefault();
                }

                var item = JsonSerializer.Deserialize<LocalizacaoGeo>(json, _jsonOptions);
                LogService.RegistrarLog($"MapasApiService: localização ID '{id}' retornada com sucesso.", "INFO");
                return item;
            }
            catch (HttpRequestException ex)
            {
                LogService.RegistrarLog($"MapasApiService: erro HTTP em BuscarPorIdAsync – {ex.Message}", "ERRO");
                throw;
            }
        }

        // ── GET /api/Mapas/logradouro/{logradouro} ─────────────────────────
        /// <summary>
        /// Retorna as localizações que correspondem ao logradouro informado.
        /// </summary>
        public async Task<List<LocalizacaoGeo>> BuscarPorLogradouroAsync(string logradouro)
        {
            if (string.IsNullOrWhiteSpace(logradouro))
                throw new ArgumentException("O logradouro não pode ser vazio.", nameof(logradouro));

            string url = $"{BaseUrl}/logradouro/{Uri.EscapeDataString(logradouro)}";
            LogService.RegistrarLog($"MapasApiService: iniciando GET {url}", "INFO");

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    LogService.RegistrarLog($"MapasApiService: nenhum resultado para logradouro '{logradouro}' (404).", "AVISO");
                    return new List<LocalizacaoGeo>();
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // Trata wrapper, array ou objeto único
                if (json.Contains("\"localizacoes\"") || json.Contains("\"mensagem\""))
                {
                    var resposta = JsonSerializer.Deserialize<RespostaMapas>(json, _jsonOptions);
                    return resposta?.Localizacoes ?? new List<LocalizacaoGeo>();
                }

                if (json.TrimStart().StartsWith('['))
                {
                    return JsonSerializer.Deserialize<List<LocalizacaoGeo>>(json, _jsonOptions)
                           ?? new List<LocalizacaoGeo>();
                }

                var item = JsonSerializer.Deserialize<LocalizacaoGeo>(json, _jsonOptions);
                var lista = item != null ? new List<LocalizacaoGeo> { item } : new List<LocalizacaoGeo>();

                LogService.RegistrarLog($"MapasApiService: {lista.Count} resultado(s) para logradouro '{logradouro}'.", "INFO");
                return lista;
            }
            catch (HttpRequestException ex)
            {
                LogService.RegistrarLog($"MapasApiService: erro HTTP em BuscarPorLogradouroAsync – {ex.Message}", "ERRO");
                throw;
            }
        }
    }

    /// <summary>
    /// Classe wrapper que reflete o formato de resposta da API:
    /// { "mensagem": "...", "localizacoes": [...] }
    /// </summary>
    public class RespostaMapas
    {
        public string? Mensagem { get; set; }
        public List<LocalizacaoGeo>? Localizacoes { get; set; }
    }
}