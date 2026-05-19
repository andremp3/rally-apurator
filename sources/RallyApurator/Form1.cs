using RallyApurator.Commands;
using RallyApurator.Models;

namespace RallyApurator
{
    public partial class Form1 : Form
    {
        public Form1()
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
            List<PilotoDados> todosOsPilotos = ApuradorRally.ProcessarArquivoCSV();
            if (todosOsPilotos == null || todosOsPilotos.Count == 0) return;

            // 3. PROCESSAR AS REGRAS DO RALLY (Chama a classe do outro Namespace)
            List<RankingResultado> rankingFinal = ApuradorRally.ProcessarRegrasRally(
                todosOsPilotos,
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

        private void panelCabecalho_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. DESENHA O FUNDO ESCURO (Base para a Fibra de Carbono)
            Rectangle rect = panelCabecalho.ClientRectangle;
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(20, 20, 22)))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // 2. CRIA O EFEITO DE TEXTURA DE FIBRA DE CARBONO (Padrão de micro-quadrados)
            int tamanhoPadrao = 4;
            using (SolidBrush carbonoBrush = new SolidBrush(Color.FromArgb(32, 32, 36)))
            {
                for (int x = 0; x < rect.Width; x += tamanhoPadrao * 2)
                {
                    for (int y = 0; y < rect.Height; y += tamanhoPadrao * 2)
                    {
                        g.FillRectangle(carbonoBrush, x, y, tamanhoPadrao, tamanhoPadrao);
                        g.FillRectangle(carbonoBrush, x + tamanhoPadrao, y + tamanhoPadrao, tamanhoPadrao, tamanhoPadrao);
                    }
                }
            }

            // 3. DESENHA UM GRADIENTE SUAVE PARA DAR PROFUNDIDADE
            using (System.Drawing.Drawing2D.LinearGradientBrush gradiente = new System.Drawing.Drawing2D.LinearGradientBrush(
                rect, Color.FromArgb(40, 0, 0, 0), Color.FromArgb(220, 0, 0, 0), 90F))
            {
                g.FillRectangle(gradiente, rect);
            }

            // 4. DESENHA A LINHA RACING VERMELHA NO FUNDO DO BANNER
            using (Pen penVermelha = new Pen(Color.FromArgb(220, 53, 69), 4))
            {
                g.DrawLine(penVermelha, 0, panelCabecalho.Height - 2, panelCabecalho.Width, panelCabecalho.Height - 2);
            }

            // 5. ESCREVE OS TEXTOS ESTÁTICOS (Sem buscar dados do Grid)
            using (Font fontTitulo = new Font("Segoe UI", 24, FontStyle.Bold))
            using (SolidBrush brushTexto = new SolidBrush(Color.White))
            {
                g.DrawString("TREX - RALLY", fontTitulo, brushTexto, 40, 60);
            }

            using (Font fontSub = new Font("Segoe UI", 11, FontStyle.Regular))
            using (SolidBrush brushSub = new SolidBrush(Color.FromArgb(160, 160, 170)))
            {
                g.DrawString("Apurador de Rally de Regularidade • Capuava SP", fontSub, brushSub, 42, 110);
            }

            // 6. DESENHA O ÍCONE DO CRONÓMETRO VINTAGE (Lado Direito)
            int cronoX = panelCabecalho.Width - 180;
            int cronoY = 45;
            int cronoTamanho = 120;

            // Corpo do Cronômetro (Círculo Cromado)
            using (Pen penCromo = new Pen(Color.FromArgb(180, 185, 190), 6))
            {
                g.DrawEllipse(penCromo, cronoX, cronoY, cronoTamanho, cronoTamanho);
            }
            // Fundo do Cronômetro (Mostrador Branco)
            using (SolidBrush brushMostrador = new SolidBrush(Color.FromArgb(245, 245, 245)))
            {
                g.FillEllipse(brushMostrador, cronoX + 4, cronoY + 4, cronoTamanho - 8, cronoTamanho - 8);
            }
            // Botão Superior do Cronômetro
            using (SolidBrush brushBotao = new SolidBrush(Color.FromArgb(130, 135, 140)))
            {
                g.FillRectangle(brushBotao, cronoX + (cronoTamanho / 2) - 10, cronoY - 12, 20, 12);
            }
            // Ponteiro Vermelho do Cronômetro
            using (Pen penPonteiro = new Pen(Color.FromArgb(220, 53, 69), 3))
            {
                int centroX = cronoX + (cronoTamanho / 2);
                int centroY = cronoY + (cronoTamanho / 2);
                g.DrawLine(penPonteiro, centroX, centroY, centroX + 30, centroY - 30);
            }
        }
    }


}
