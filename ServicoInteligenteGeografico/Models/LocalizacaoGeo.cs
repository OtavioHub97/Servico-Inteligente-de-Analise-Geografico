using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicoInteligenteGeografico.Models
{
    /// <summary>
    /// Representa um endereço/localização geográfica salvo no Firebase.
    /// Cada propriedade vira um campo no banco de dados.
    /// </summary>
    public class LocalizacaoGeo
    {
        public string? Id { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Cep { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Data/hora em que o registro foi salvo
        public string DataRegistro { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}
