using RallyApurator.Commands;
using RallyApurator.Models;

namespace RallyApurator
{
    public partial class FormRallyApurador : Form
    {

        private List<PilotoDados> listaPilotosBrutos;

        public FormRallyApurador()
        {
            InitializeComponent();
        }

        private void btnCarregar_Click(object sender, EventArgs e)
        {

            // 1. VALIDAÇÃO LOCAL DA TELA
            if (!ParserCommands.TentarParsearTempo(txtTempoAlvo.Text, out TimeSpan tempoAlvo))
            {
                MessageBox.Show("Por favor, insira um tempo alvo válido (Ex: 1:50.000)", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtVoltasValidas.Text, out int qtdVoltasExigidas) || qtdVoltasExigidas <= 0)
            {
                MessageBox.Show("Por favor, insira uma quantidade de voltas válidas maior que zero.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ParserCommands.TentarParsearTempo(txtTempoDesclassificacao.Text, out TimeSpan tempoLimiteDQ))
            {
                MessageBox.Show("Por favor, insira um tempo de desclassificação válido (Ex: 1:40.000)", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtVoltasDesclassificacao.Text, out int limiteVoltasRapidas) || limiteVoltasRapidas <= 0)
            {
                MessageBox.Show("Por favor, insira um limite de voltas de desclassificação maior que zero.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. PROCESSAR ARQUIVO (Chama a classe do outro Namespace)
            this.listaPilotosBrutos = ApuradorRally.ProcessarArquivoCSV();
            if (listaPilotosBrutos == null || listaPilotosBrutos.Count == 0) return;

            // 3. PROCESSAR AS REGRAS DO RALLY (Chama a classe do outro Namespace)
            List<RankingResultado> rankingFinal = ApuradorRally.ProcessarRegrasRally(
                listaPilotosBrutos,
                tempoAlvo,
                qtdVoltasExigidas,
                tempoLimiteDQ,
                limiteVoltasRapidas,
                txtTempoDesclassificacao.Text
            );

            // 4. ORDENAR E EXIBIR NO GRID
            var rankingOrdenado = ApuradorRally.OrdenarRanking(rankingFinal);
            dgvResultados.DataSource = rankingOrdenado;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // Estilizando as linhas alternadas
            dgvResultados.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 245, 255);
            dgvResultados.RowsDefaultCellStyle.BackColor = Color.White;

            // Estilizando o cabeçalho
            dgvResultados.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 80);
            dgvResultados.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvResultados.EnableHeadersVisualStyles = false;

            // Configurações gerais
            dgvResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResultados.AllowUserToAddRows = false;

        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            // 1. Recupera os dados vinculados ao DataGridView
            var dadosGrid = dgvResultados.DataSource as List<RankingResultado>;

            // Valida se o grid possui dados válidos calculados
            if (dadosGrid == null || dadosGrid.Count == 0)
            {
                MessageBox.Show("Não há dados carregados no ranking para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Configura a janela para salvar o arquivo de forma interativa
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Arquivos CSV (*.csv)|*.csv",
                Title = "Salvar Resultado do Campeonato",
                FileName = "Ranking_Regularidade.csv" // Nome padrão sugerido ao usuário
            };

            // 3. Executa a exportação caso o usuário confirme a pasta e o nome
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Busca as linhas estruturadas direto do Motor de Inteligência
                    List<string> linhasProntas = ApuradorRally.GerarLinhasCsv(dadosGrid);

                    // Salva fisicamente o arquivo usando UTF8 para preservar as posições (Ex: 1°, 2°) e acentuações
                    File.WriteAllLines(saveFileDialog.FileName, linhasProntas, System.Text.Encoding.UTF8);

                    MessageBox.Show("Resultados exportados com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro crítico ao gravar o arquivo: {ex.Message}", "Erro de Gravação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCuriosidades_Click(object sender, EventArgs e)
        {
            // Recupera a lista bruta de pilotos salvando ela em uma variável global ou coletando no processamento
            if (listaPilotosBrutos == null || listaPilotosBrutos.Count == 0)
            {
                MessageBox.Show("Por favor, carregue os dados de um arquivo CSV primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Certifique-se de coletar os tempos da tela para passar para a validação
            ParserCommands.TentarParsearTempo(txtTempoAlvo.Text, out TimeSpan tempoAlvo);
            ParserCommands.TentarParsearTempo(txtTempoDesclassificacao.Text, out TimeSpan tempoMinimo);

            // Busca o texto gerado pela engine
            string resumoCuriosidades = ApuradorRally.ObterCuriosidadesDaEtapa(listaPilotosBrutos, tempoAlvo, tempoMinimo);

            // Exibe na tela de forma organizada
            MessageBox.Show(resumoCuriosidades, "Estatísticas & Curiosidades da Etapa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvResultados_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Verifica se estamos na coluna de Status
            if (dgvResultados.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status.StartsWith("DQL"))
                {
                    // Pinta a linha atual de vermelho claro para dar destaque de desclassificação
                    dgvResultados.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                    dgvResultados.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }
    }


}
