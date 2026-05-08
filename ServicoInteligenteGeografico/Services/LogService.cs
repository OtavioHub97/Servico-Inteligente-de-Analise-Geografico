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
        private static readonly string _caminhoArquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static readonly string _nomeArquivo = "sistema_logs.txt";
        private static readonly object _lock = new object(); // Impede travamento do arquivo

        public static void RegistrarLog(string mensagem, string tipo = "INFO")
        {
            try
            {
                if (!Directory.Exists(_caminhoArquivo))
                    Directory.CreateDirectory(_caminhoArquivo);

                string caminhoCompleto = Path.Combine(_caminhoArquivo, _nomeArquivo);
                string linhaLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tipo.ToUpper()}] - {mensagem}{Environment.NewLine}";

                lock (_lock) // Garante que apenas uma ação escreva no arquivo por vez
                {
                    File.AppendAllText(caminhoCompleto, linhaLog);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Falha Crítica no Log: {ex.Message}");
            }
        }
    }
}

