using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServicoInteligenteGeografico.Services
{
    public static class LogService
    {
        public static void RegistrarLog(string mensagem, string tipo = "INFO")
        {
            try
            {
                string caminhoArquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sistema_logs.txt");

                string linhaLog = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] [{tipo}] - {mensagem}{Environment.NewLine}";

                File.AppendAllText(caminhoArquivo, linhaLog);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);  
        
            }
        }
    }
}
