using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicoInteligenteGeografico.Models
{
    /// <summary>
    /// Representa o resultado de uma análise geográfica salvo no Firebase.
    /// Vinculado a uma LocalizacaoGeo pelo campo LocalizacaoId...
    /// </summary>
    public class AnaliseGeo
    {
        public string? Id { get; set; }
        public string? LocalizacaoId { get; set; }  // Chave da localização relacionada

        // Dados climáticos retornados pela análise
        public double Temperatura { get; set; }
        public string? Clima { get; set; }  // Ex: "Ensolarado", "Nublado", "Chuvoso"
        public double Umidade { get; set; }

        // Pontuação/ranking da localização
        public int Ranking { get; set; }

        public string DataAnalise { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}
