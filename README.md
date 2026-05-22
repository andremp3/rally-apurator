# Trex Rally - Apurador de Regularidade 🏁

O **Trex Rally - Apurador** é um software desktop desenvolvido em C# (Windows Forms) projetado para automatizar e gerenciar a apuração de provas de **Rally de Regularidade em Autódromo**. O sistema processa arquivos de cronometragem bruta (CSV), valida critérios rigorosos do regulamento, calcula médias de erro milimetricamente e gera estatísticas avançadas da etapa de forma automatizada.

---

## 📸 Interface do Sistema

### Painel de Controle e Grid de Resultados
O sistema conta com um painel dinâmico de configurações de pista, exibindo em tempo real o ranking ordenado de forma decrescente pelo menor erro médio.
![Painel de Apuração e Resultados](image_442125.jpg)

### Estatísticas e Curiosidades Automatizadas
Ao processar os dados, um motor estatístico extrai métricas de performance e comportamentos dos pilotos durante a etapa.
![Métricas e Destaques](image_4e2456.jpg)

---

## 🛠️ Funcionalidades Principais

* **Importação Inteligente de CSV:** Lê relatórios de cronometragem oficial baseados na estrutura de marcação por colunas e posições (`Pos. `), associando pilotos aos seus respectivos carros e tempos de volta.
* **Mecanismo de Validação e Consistência:** Trata exceções críticas locais, como arquivos vazios, estruturas corrompidas e arquivos bloqueados por edição simultânea (ex: Excel).
* **Aplicação Rígida do Regulamento:**
    * **Cálculo da Média de Erro:** Avalia e ordena o ranking com base nas $N$ melhores voltas válidas de cada piloto frente ao **Tempo Alvo**.
    * **Desclassificação Automática por Tempo (DQL):** Penaliza e desclassifica pilotos que excedem o limite de voltas rápidas estipulado (**Tempo Mínimo**), evitando que andem acima do limite de segurança da pista.
    * **Controle de Voltas Mínimas:** Identifica pilotos que não atingiram a quantidade de voltas limpas exigidas.
* **Exportação de Resultados:** Consolida o ranking oficial gerando um novo arquivo CSV formatado em padrão universal (UTF-8).
* **Métricas de Grid (Gamificação):** Através de algoritmos estatísticos (incluindo cálculo de desvio padrão), o sistema gera os seguintes destaques:
    * *🎯 O Cirurgião:* A volta mais próxima do tempo alvo (com precisão de milissegundos).
    * *⏱️ O Relógio Suíço:* O piloto com a maior constância e regularidade em ritmo de pista.
    * *⚠️ Salvo pelo Gongo:* O piloto que passou mais rápido, porém no limite da desclassificação.
    * *🐌 O Inimigo do Giro:* A maior "tirada de pé" válida (filtrando anomalias e entradas de box até 10s do alvo).
    * *🏁 O Maratonista:* Quem mais completou giros no circuito.
    * *📈 Ritmo Geral do Grid:* A média real de tempo de todas as voltas de pista do evento.

---

## 🗂️ Arquitetura e Estrutura do Projeto

O projeto segue um padrão arquitetural limpo e modularizado dentro do ecossistema .NET, dividindo responsabilidades de visualização, comandos de processamento e modelos de dados:

```text
RallyApurator/
│
├── Commands/
│   ├── ApuradorRally.cs       # Core Engine: Processamento do CSV, Regras do Rally e Estatísticas
│   └── ParserCommands.cs      # Parser customizado para tratamento de strings e formatação de TimeSpan
│
├── Models/
│   ├── PilotoDados.cs         # Modelo de persistência dos dados e histórico de voltas de cada piloto
│   └── RankingResultado.cs    # Estrutura de dados moldada para o DataGridView e exportações
│
├── Resources/
│   └── rallyApuratorBanner.png # Identidade visual do cabeçalho da aplicação
│
├── FormRallyApurador.cs       # Camada de Apresentação (UI), validações de tela e interações do usuário
└── Program.cs                 # Ponto de entrada (Bootstrap) da aplicação