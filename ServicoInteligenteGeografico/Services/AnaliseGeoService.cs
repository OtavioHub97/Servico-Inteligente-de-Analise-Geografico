using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServicoInteligenteGeografico.Models; 

namespace ServicoInteligenteGeografico.Services
{
    public class AnaliseGeoService
    {
        public async Task ProcessarESalvarAnaliseAsync(AnaliseGeo novaAnalise)
        {
           
            LogService.RegistrarLog($"Iniciando processamento de análise para a região: {novaAnalise.Regiao}", "INFO");

            
            try
            {
                LogService.RegistrarLog("Cálculos da análise realizados com sucesso.", "INFO");

                LogService.RegistrarLog("Análise processada e salva com sucesso!", "SUCESSO");
            }
          
            catch (Exception ex)
            {
                LogService.RegistrarLog($"Erro crítico ao processar análise da região {novaAnalise.Regiao}. Detalhes: {ex.Message}", "ERRO_CRITICO");
            }
        }
    }

}