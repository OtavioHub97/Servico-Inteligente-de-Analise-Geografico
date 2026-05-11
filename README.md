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



**4.1 - Pacotes do Firebase não instalados**
Após adicionar os arquivos de integração ao projeto WPF, a compilação falhou com erros do tipo namespace 'FirebaseAdmin' não pode ser encontrado. O motivo foi que os pacotes NuGet do Firebase ainda não haviam sido instalados no projeto.
Solução: executar os comandos abaixo no terminal dentro da pasta do projeto:
bashcd ServicoInteligenteGeografico
dotnet add package FirebaseAdmin --version 3.1.0
dotnet add package FirebaseDatabase.net --version 4.2.0



**4.2 - Terminal apontando para a pasta errada**
Ao tentar instalar os pacotes pelo terminal do Visual Studio, o comando retornou o erro Não foi possível encontrar nenhum projeto. O terminal estava na pasta raiz do repositório, e não dentro da pasta do projeto .csproj.
Solução: navegar para a pasta correta com cd ServicoInteligenteGeografico antes de rodar os comandos.




**4.3 - Dificuldade com o cálculo da distância entre os pontos**


