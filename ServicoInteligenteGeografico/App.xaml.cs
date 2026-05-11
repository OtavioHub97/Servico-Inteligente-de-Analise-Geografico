using System.Configuration;
using System.Data;
using System.Windows;

namespace ServicoInteligenteGeografico
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Adicione esta linha antes de qualquer coisa:
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            base.OnStartup(e);
        }
    }

}
