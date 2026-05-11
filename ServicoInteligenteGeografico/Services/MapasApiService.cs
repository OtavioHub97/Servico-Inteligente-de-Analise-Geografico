using ServicoInteligenteGeografico.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServicoInteligenteGeografico.Services
{
    public class MapasApiService
    {
        private const string BaseUrl = "https://webapimaps.runasp.net/api/Mapas";

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // GET /api/Mapas
        public async Task<List<LocalizacaoGeo>> BuscarTodasAsync()
        {
            LogService.RegistrarLog("MapasApiService: iniciando GET /api/Mapas", "INFO");
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<LocalizacaoGeo>>(json, _jsonOptions) ?? new List<LocalizacaoGeo>();
                LogService.RegistrarLog($"MapasApiService: {lista.Count} localização(ões) desserializadas.", "INFO");
                return lista;
            }
            catch (HttpRequestException ex)
            {
                LogService.RegistrarLog($"MapasApiService: erro HTTP em BuscarTodasAsync – {ex.Message}", "ERRO");
                throw;
            }
        }

        // GET /api/Mapas/{id}
        public async Task<LocalizacaoGeo?> BuscarPorIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("O ID não pode ser vazio.", nameof(id));
            string url = $"{BaseUrl}/{Uri.EscapeDataString(id)}";
            LogService.RegistrarLog($"MapasApiService: iniciando GET {url}", "INFO");
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LocalizacaoGeo>(json, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                LogService.RegistrarLog($"MapasApiService: erro HTTP em BuscarPorIdAsync – {ex.Message}", "ERRO");
                throw;
            }
        }

        // GET /api/Mapas/logradouro/{logradouro}
        public async Task<List<LocalizacaoGeo>> BuscarPorLogradouroAsync(string logradouro)
        {
            if (string.IsNullOrWhiteSpace(logradouro)) throw new ArgumentException("O logradouro não pode ser vazio.", nameof(logradouro));
            string url = $"{BaseUrl}/logradouro/{Uri.EscapeDataString(logradouro)}";
            LogService.RegistrarLog($"MapasApiService: iniciando GET {url}", "INFO");
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return new List<LocalizacaoGeo>();
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                if (json.TrimStart().StartsWith('['))
                    return JsonSerializer.Deserialize<List<LocalizacaoGeo>>(json, _jsonOptions) ?? new List<LocalizacaoGeo>();
                var item = JsonSerializer.Deserialize<LocalizacaoGeo>(json, _jsonOptions);
                return item != null ? new List<LocalizacaoGeo> { item } : new List<LocalizacaoGeo>();
            }
            catch (HttpRequestException ex)
            {
                LogService.RegistrarLog($"MapasApiService: erro HTTP em BuscarPorLogradouroAsync – {ex.Message}", "ERRO");
                throw;
            }
        }
    }
}