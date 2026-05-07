using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicoInteligenteGeografico.Models
{
    public class LocalizacaoGeo
    {
        public int Id { get; set; } 
        public string Nome { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro {  get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public double Latitute { get; set; }    
        public double Longitute { get; set; }
        public DateTime TimeStamp { get; set; } 

    }
}
