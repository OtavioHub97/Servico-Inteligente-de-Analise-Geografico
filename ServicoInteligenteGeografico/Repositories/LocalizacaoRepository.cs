using ServicoInteligenteGeografico.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicoInteligenteGeografico.Repositories
{
    /// <summary>
    /// Responsável por todas as operações de banco de dados
    /// para a entidade LocalizacaoGeo no Firebase.
    /// </summary>
    public class LocalizacaoRepository
    {
        // Nome do "nó" (tabela) no Firebase Realtime Database
        private const string No = "localizacoes";

        private readonly FirebaseClient _db;

        public LocalizacaoRepository()
        {
            // Pega o cliente já configurado com a URL do Firebase
            _db = Database.GetClient();
        }

        // ── CREATE ──────────────────────────────────────────────────────────

        /// <summary>
        /// Salva uma nova localização no Firebase.
        /// O banco gera a chave (Id) automaticamente.
        /// </summary>
        public async Task<LocalizacaoGeo> SalvarAsync(LocalizacaoGeo localizacao)
        {
            var resultado = await _db
                .Child(No)
                .PostAsync(localizacao);

            // Guarda a chave gerada pelo Firebase no objeto
            localizacao.Id = resultado.Key;
            return localizacao;
        }

        // ── READ ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Busca todas as localizações salvas no banco.
        /// </summary>
        public async Task<List<LocalizacaoGeo>> BuscarTodasAsync()
        {
            var itens = await _db
                .Child(No)
                .OnceAsync<LocalizacaoGeo>();

            return itens
                .Select(item =>
                {
                    item.Object.Id = item.Key;  // Preenche o Id com a chave do Firebase
                    return item.Object;
                })
                .ToList();
        }

        /// <summary>
        /// Busca uma localização pelo Id.
        /// Retorna null se não encontrar.
        /// </summary>
        public async Task<LocalizacaoGeo?> BuscarPorIdAsync(string id)
        {
            var item = await _db
                .Child(No)
                .Child(id)
                .OnceSingleAsync<LocalizacaoGeo>();

            if (item != null)
                item.Id = id;

            return item;
        }

        /// <summary>
        /// Busca localizações filtrando por bairro.
        /// </summary>
        public async Task<List<LocalizacaoGeo>> BuscarPorBairroAsync(string bairro)
        {
            var todos = await BuscarTodasAsync();

            return todos
                .Where(l => l.Bairro != null &&
                            l.Bairro.Contains(bairro, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // ── UPDATE ───────────────────────────────────────────────────────────

        /// <summary>
        /// Atualiza todos os dados de uma localização existente.
        /// </summary>
        public async Task AtualizarAsync(LocalizacaoGeo localizacao)
        {
            if (string.IsNullOrEmpty(localizacao.Id))
                throw new ArgumentException("Id é obrigatório para atualizar.");

            await _db
                .Child(No)
                .Child(localizacao.Id)
                .PutAsync(localizacao);
        }

        // ── DELETE ───────────────────────────────────────────────────────────

        /// <summary>
        /// Remove uma localização pelo Id.
        /// </summary>
        public async Task DeletarAsync(string id)
        {
            await _db
                .Child(No)
                .Child(id)
                .DeleteAsync();
        }
    }
}
