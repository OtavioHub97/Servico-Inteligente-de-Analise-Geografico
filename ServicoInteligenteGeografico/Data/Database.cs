using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Firebase.Database;
using Google.Apis.Auth.OAuth2;

namespace ServicoInteligenteGeografico.Data
{
    /// <summary>
    /// Classe responsável por criar e manter a conexão com o Firebase.
    /// Toda a aplicação usa esse único ponto de acesso ao banco.
    /// </summary>
    public class Database
    {
        
        private const string FirebaseUrl = "https://servicegeo-b5eb5-default-rtdb.firebaseio.com/";

        // Instância única do cliente (padrão Singleton)
        private static FirebaseClient? _client;

        /// <summary>
        /// Retorna o cliente do Firebase pronto para uso.
        /// Cria a conexão apenas na primeira chamada.
        /// </summary>
        public static FirebaseClient GetClient()
        {
            if (_client == null)
            {
                // Inicializa a autenticação com o arquivo de chave de serviço
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        // Lê o arquivo serviceAccountKey.json que fica na raiz do projeto
                        Credential = GoogleCredential.FromFile("serviceAccountKey.json")
                    });
                }

                // Cria o cliente apontando para a URL do banco
                _client = new FirebaseClient(FirebaseUrl, new FirebaseOptions
                {
                    AuthTokenAsyncFactory = async () =>
                    {
                        // Gera um token de autenticação usando o Admin 
                        string token = await FirebaseAuth.DefaultInstance
                            .CreateCustomTokenAsync("servico-geografico");
                        return token;
                    }
                });
            }

            return _client;
        }
    }
}