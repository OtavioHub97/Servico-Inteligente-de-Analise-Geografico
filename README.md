# __ Serviço Inteligente de Análise Geográfica __

**Integrantes:** <br> 
Bruno Maia <br> 
Kaio Silva  
Luna Beatriz <br> 
Otávio Soares

**Instrutor:** Frederico Martins Aguiar

## 1 - O que é nosso projeto?
Um Serviço de Análise de Dados Geográficos especializado no processamento de coordenadas (Latitude/Longitude), cálculo de distâncias e agrupamento por região, com persistência em Firebase e entrega de insights via API REST.

## 2 - Planejamento e Estimativa:
<img width="1225" height="484" alt="image" src="https://github.com/user-attachments/assets/e46dcda1-53ba-4ec6-8ebc-eecad96fd7f7" />

## 3 - Requisitos do Sistema:
### 3.1 - Requisitos Funcionais (RF):

**[RF-001]** Consumir dados de pedidos e localização via API do Squad2. <br>
**[RF-002]** Calcular a distância entre pontos geográficos. <br>
**[RF-003]** Agrupar locais região/bairro. <br>
**[RF-004]** Gerar ranking dos locais. <br>
**[RF-005]** Disponibilizar os insights gerados através de uma API REST própria. <br>

### 3.2 - Requisitos Não Funcionais (RNF):

**[RNF-001] Persistência:** Os dados analíticos processados devem ser armazenados no Firebase. <br>
**[RNF-002] Performance:** O cálculo de distância e agrupamento deve ser otimizado para respostas em tempo real. <br>
**[RNF-003] Escalabilidade:** A arquitetura de microserviço deve suportar o aumento de volume de dados geográficos. <br>

### 4 - 📂 Estrutura do Projeto:

**Configuração do Firebase ao clonar o repositório**

**⚠️ Atenção: O arquivo serviceAccountKey.json não está incluído no repositório por razões de segurança. Ele contém credenciais privadas do Firebase e nunca deve ser versionado no Git.**

Ao clonar o projeto, cada membro do time precisa gerar sua própria chave seguindo os passos abaixo: <br>
**Passo 1** — Acesse console.firebase.google.com e abra o projeto ServiceGeo <br>
**Passo 2** — Clique no ícone de engrenagem ⚙️ → "Configurações do projeto" → aba "Contas de serviço" <br>
**Passo 3** — Clique em "Gerar nova chave privada" e confirme no popup <br>
**Passo 4** — Salve o arquivo baixado com o nome exato: serviceAccountKey.json <br>
**Passo 5** — Cole o arquivo em dois lugares: <br>

Raiz do projeto: ServicoInteligenteGeografico/ <br>
Pasta de saída: ServicoInteligenteGeografico/bin/Debug/net8.0-windows/ <br>

Passo 6 — No Visual Studio, clique com botão direito no arquivo → "Propriedades" → "Copiar para diretório de saída" → selecione "Copiar sempre"
O arquivo já está listado no .gitignore do projeto, então não há risco de ser enviado ao repositório por acidente.

```
📂SistemaInteligenteGeografico
├── 📂Commands
│   └── RelayCommand.cs
├── 📂Data
│   └── Database.cs
├── 📂Models
│   ├── AnaliseGeo.cs
│   └── LocalizacaoGeo.cs
├── 📂Repositories
│   ├── AnaliseRepository.cs
│   └── LocalizacaoRepository.cs
├── 📂Services
│   ├── AnaliseGeoService.cs
│   └── LogService.cs
├── 📂ViewModels
│   ├── BaseViewModel.cs
│   └── MainViewModel.cs
├── 📂Views
│   └── MainWindow.xaml
│       └── MainWindow.xaml.cs
├── App.xaml
│   └── App.xaml.cs
└── AssemblyInfo.cs
```
## 5 - Dificuldades encontradas no decorrer do projeto



**5.1 - Pacotes do Firebase não instalados**
Após adicionar os arquivos de integração ao projeto WPF, a compilação falhou com erros do tipo namespace 'FirebaseAdmin' não pode ser encontrado. O motivo foi que os pacotes NuGet do Firebase ainda não haviam sido instalados no projeto.
Solução: executar os comandos abaixo no terminal dentro da pasta do projeto:
bashcd ServicoInteligenteGeografico
dotnet add package FirebaseAdmin --version 3.1.0
dotnet add package FirebaseDatabase.net --version 4.2.0



**5.2 - Terminal apontando para a pasta errada**
Ao tentar instalar os pacotes pelo terminal do Visual Studio, o comando retornou o erro Não foi possível encontrar nenhum projeto. O terminal estava na pasta raiz do repositório, e não dentro da pasta do projeto .csproj.
Solução: navegar para a pasta correta com cd ServicoInteligenteGeografico antes de rodar os comandos.




**5.3 - Dificuldade com o cálculo da distância entre os pontos**

**5.4 - Dificuldades Encontradas e Soluções (Otavio)**

Durante o desenvolvimento da minha parte no projeto, acabei esbarrando em alguns desafios técnicos bem interessantes que serviram de aprendizado:

* **Curva de aprendizado com o Firebase e divisão de tarefas:**
  Sair do modelo tradicional de banco de dados e entender como o Firebase funciona (com coleções, documentos e chamadas assíncronas) foi meio confuso no começo. Tive bastante dificuldade para fazer essa configuração inicial rodar redonda. No fim das contas, para o projeto não travar, fizemos uma rotação nas tarefas da squad: um colega que tinha mais facilidade assumiu a sequência da integração com o banco, e eu foquei em deixar a parte de modelagem, logs e tratamento de erros robusta.

* **Caminhos de arquivos no Sistema de Logs:**
  Criar o `LogService` centralizado parecia simples na teoria, mas na prática apanhei um pouco para fazer o arquivo `sistema_logs.txt` ser salvo no lugar certo em qualquer computador. Se eu colocasse o caminho da minha máquina, quebrava no PC dos colegas da squad. A solução foi usar o comando `AppDomain.CurrentDomain.BaseDirectory` para achar a pasta raiz dinamicamente, além de colocar um tratamento para garantir que, se o log falhasse ao salvar, o aplicativo não fechasse do nada na cara do usuário.

* **Rastreabilidade de Erros usando os Models:**
Na hora de aplicar o tratamento de erros nos serviços de análise, percebi que apenas capturar a exceção genérica não ajudava a saber onde o problema tinha acontecido. O desafio foi desenhar as mensagens de log aproveitando a estrutura dos nossos models. Como a classe AnaliseGeo possui o LocalizacaoId e a Regiao, passei a injetar esses dados diretamente no bloco catch. Assim, se o processamento falhar, o log registra exatamente qual chave ou região causou o erro, facilitando a depuração posterior.

* **Conflitos do Git quebrando o Visual Studio (Arquivo `.csproj`):**
  Essa foi clássica! Na hora de juntar o código da squad, o Git gerou um conflito de mesclagem direto no arquivo de configuração do projeto (o `.csproj`). O Visual Studio começou a dar um erro crítico (MSB4025) e o projeto não compilava de jeito nenhum. Demorei um tempo para entender que o Git tinha injetado aquelas marcações de setas (`<<<<<<<`) no meio do código XML do projeto e que a solução era abrir o arquivo na mão para limpar a sujeira.


