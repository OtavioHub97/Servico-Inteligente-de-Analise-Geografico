using Firebase.Database;
using Firebase.Database.Query;
using ServicoInteligenteGeografico.Data;
using ServicoInteligenteGeografico.Models;

namespace ServicoInteligenteGeografico.Repositories
{
    /// <summary>
    /// Responsável por todas as operações de banco de dados
    /// para a entidade AnaliseGeo no Firebase.
    /// </summary>
    public class AnaliseRepository
    {
        private const string No = "analises";

        private readonly FirebaseClient _db;

        public AnaliseRepository()
        {
            _db = Database.GetClient();
        }

        // ── CREATE ──────────────────────────────────────────────────────────

        public async Task<AnaliseGeo> SalvarAsync(AnaliseGeo analise)
        {
            var resultado = await _db
                .Child(No)
                .PostAsync(analise);

            analise.Id = resultado.Key;
            return analise;
        }

        // ── READ ─────────────────────────────────────────────────────────────

        public async Task<List<AnaliseGeo>> BuscarTodasAsync()
        {
            var itens = await _db
                .Child(No)
                .OnceAsync<AnaliseGeo>();

            return itens
                .Select(item =>
                {
                    item.Object.Id = item.Key;
                    return item.Object;
                })
                .ToList();
        }

        /// <summary>
        /// Busca todas as análises de uma localização específica.
        /// </summary>
        public async Task<List<AnaliseGeo>> BuscarPorLocalizacaoAsync(string localizacaoId)
        {
            var todas = await BuscarTodasAsync();

            return todas
                .Where(a => a.LocalizacaoId == localizacaoId)
                .OrderBy(a => a.Ranking)
                .ToList();
        }

        // ── DELETE ───────────────────────────────────────────────────────────

        public async Task DeletarAsync(string id)
        {
            await _db
                .Child(No)
                .Child(id)
                .DeleteAsync();
        }
    }
}
