using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicoInteligenteGeografico.Models
{
    public class AnaliseGeo
    {
        public string Regiao { get; set; } = string.Empty;
        public int TotalConsultas { get; set; }
        public double DistanciaMedia {  get; set; }
        public string LocalMaisConsultado { get; set; } = string.Empty;
        public int Ranking {  get; set; }

    }
}
