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
            // 1. Validações dos inputs do Form
            if (!ParserCommands.TentarParsearTempo(txtTempoAlvo.Text, out TimeSpan tempoAlvo))
            {
                MessageBox.Show("Por favor, insira um tempo alvo válido (Ex: 1:50.000)");
                return;
            }

            if (!int.TryParse(txtVoltasValidas.Text, out int qtdVoltasExigidas) || qtdVoltasExigidas <= 0)
            {
                MessageBox.Show("Por favor, insira uma quantidade de voltas válidas maior que zero.");
                return;
            }

            // NOVAS VALIDAÇÕES: Tempo de corte e limite de voltas rápidas
            if (!ParserCommands.TentarParsearTempo(txtTempoDesclassificacao.Text, out TimeSpan tempoLimiteDQ))
            {
                MessageBox.Show("Por favor, insira um tempo de desclassificação válido (Ex: 1:40.000)");
                return;
            }

            if (!int.TryParse(txtVoltasDesclassificacao.Text, out int limiteVoltasRapidas) || limiteVoltasRapidas <= 0)
            {
                MessageBox.Show("Por favor, insira um limite de voltas de desclassificação maior que zero.");
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Arquivos CSV (*.csv)|*.csv" };
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            // --- LEITURA E EXTRAÇÃO DO CSV (SEM DUPLICAR PILOTOS) ---
            List<PilotoDados> todosOsPilotos = new List<PilotoDados>();

            // Lista para controlar quem são os pilotos ativos nas colunas do bloco que está sendo lido agora
            List<PilotoDados> pilotosDoBlocoAtual = new List<PilotoDados>();

            string[] linhas = File.ReadAllLines(openFileDialog.FileName, System.Text.Encoding.Default);

            for (int i = 0; i < linhas.Length; i++)
            {
                string textolinhAtual = linhas[i].Trim();
                if (string.IsNullOrWhiteSpace(textolinhAtual)) continue;

                // DETECTOU O INÍCIO DE UM BLOCO (Linha de Posições/Carros)
                if (textolinhAtual.Contains("Pos. "))
                {
                    pilotosDoBlocoAtual.Clear(); // Limpa as referências de colunas do bloco anterior

                    string[] colunasCarrosBrutas = textolinhAtual.Split(';');

                    if (i + 1 >= linhas.Length) break;
                    string[] colunasNomesBrutas = linhas[i + 1].Split(';');

                    // Mapeia as colunas físicas deste bloco específico
                    for (int col = 1; col < colunasNomesBrutas.Length; col++)
                    {
                        if (string.IsNullOrWhiteSpace(colunasNomesBrutas[col])) continue;

                        string nomePiloto = colunasNomesBrutas[col].Trim();
                        if (nomePiloto.Equals("Lap", StringComparison.OrdinalIgnoreCase)) continue;

                        // Extrai o número do carro
                        string numCarro = "S/N";
                        if (col < colunasCarrosBrutas.Length && colunasCarrosBrutas[col].Contains("#"))
                        {
                            var partes = colunasCarrosBrutas[col].Split('#');
                            if (partes.Length > 1 && !string.IsNullOrWhiteSpace(partes[1]))
                            {
                                numCarro = partes[1].Trim();
                            }
                        }

                        // CHAVE DA SOLUÇÃO: Procura se esse piloto já foi cadastrado em algum bloco anterior
                        PilotoDados pilotoExistente = todosOsPilotos.FirstOrDefault(p => p.Nome.Equals(nomePiloto, StringComparison.OrdinalIgnoreCase));

                        if (pilotoExistente != null)
                        {
                            // Se já existe, atualizamos o número do carro (caso estivesse S/N antes) e o índice da coluna atual dele
                            if (numCarro != "S/N") pilotoExistente.Carro = numCarro;
                            pilotoExistente.ColunaIndex = col;

                            // Vincula o piloto existente ao bloco atual
                            pilotosDoBlocoAtual.Add(pilotoExistente);
                        }
                        else
                        {
                            // Se for um piloto inédito no arquivo, cria um novo objeto
                            var novoPiloto = new PilotoDados
                            {
                                Nome = nomePiloto,
                                Carro = numCarro,
                                ColunaIndex = col,
                                Voltas = new List<TimeSpan>() // Garante a inicialização da lista de voltas
                            };

                            todosOsPilotos.Add(novoPiloto);
                            pilotosDoBlocoAtual.Add(novoPiloto);
                        }
                    }

                    i++; // Pula a linha dos nomes que já processamos
                    continue;
                }

                // DETECTOU LINHA DE VOLTA (Aqui o índice aponta para o piloto correto do bloco)
                string[] colunasVolta = textolinhAtual.Split(';');
                if (colunasVolta.Length > 0 && int.TryParse(colunasVolta[0], out int numeroVolta))
                {
                    foreach (var piloto in pilotosDoBlocoAtual)
                    {
                        if (piloto.ColunaIndex < colunasVolta.Length)
                        {
                            string tempoTexto = colunasVolta[piloto.ColunaIndex];

                            // Ignora células com traço ou vazias de pilotos que não completaram aquela volta específica
                            if (!string.IsNullOrWhiteSpace(tempoTexto) && tempoTexto.Trim() != "-")
                            {
                                if (ParserCommands.TentarParsearTempo(tempoTexto, out TimeSpan tempoVolta))
                                {
                                    piloto.Voltas.Add(tempoVolta);
                                }
                            }
                        }
                    }
                }
            }

            // 2. PROCESSAMENTO DO RALLY E CRITÉRIOS DE CORTE
            List<RankingResultado> rankingFinal = new List<RankingResultado>();

            foreach (var piloto in todosOsPilotos)
            {
                // Guarda a quantidade total de voltas que o piloto deu na pista
                int totalVoltasRegistradas = piloto.Voltas.Count;

                // NOVA REGRA: Contar quantas voltas foram abaixo do tempo limite estipulado (ex: 1:40.000)
                int voltasAbaixoDoLimite = piloto.Voltas.Count(v => v < tempoLimiteDQ);

                if (voltasAbaixoDoLimite >= limiteVoltasRapidas)
                {
                    rankingFinal.Add(new RankingResultado
                    {
                        Carro = piloto.Carro,
                        Nome = piloto.Nome,
                        VoltasValidas = 0,
                        MediaTempo = "-",
                        MediaErro = "999999", // Peso artificial alto para ordenar para o fim da tabela
                        Status = $"DQL ({voltasAbaixoDoLimite} voltas abaixo de {txtTempoDesclassificacao.Text})"
                    });
                    continue;
                }

                // Regra 1: Filtra apenas voltas maiores ou iguais ao tempo alvo (Ex: >= 1:50.000)
                var voltasValidas = piloto.Voltas.Where(v => v >= tempoAlvo).ToList();

                // Regra 2: Desclassificação se não tiver a quantidade mínima exigida de voltas acima do alvo
                if (voltasValidas.Count < qtdVoltasExigidas)
                {
                    rankingFinal.Add(new RankingResultado
                    {
                        Carro = piloto.Carro,
                        Nome = piloto.Nome,
                        TotalVoltas = totalVoltasRegistradas,
                        VoltasValidas = voltasValidas.Count,
                        MediaTempo = "-",
                        MediaErro = "999999",
                        Status = $"DQL (Menos de {qtdVoltasExigidas} voltas válidas)"
                    });
                    continue;
                }

                // Regra 3: Pega as X melhores voltas válidas (as mais próximas do alvo, por cima)
                var melhoresVoltas = voltasValidas.OrderBy(v => v).Take(qtdVoltasExigidas).ToList();

                // --- NOVA LÓGICA: Cálculo da Média de Erro (Deltas individuais) ---
                // Calcula o erro total acumulado em segundos e depois acha a média desse erro por volta
                double erroTotalSegundos = melhoresVoltas.Sum(v => (v - tempoAlvo).TotalSeconds);
                double mediaErroSegundos = erroTotalSegundos / qtdVoltasExigidas;

                // Regra 4: Cálculo do Tempo Médio das voltas válidas selecionadas
                double totalMilissegundos = melhoresVoltas.Sum(v => v.TotalMilliseconds);
                double mediaMilissegundos = totalMilissegundos / qtdVoltasExigidas;
                TimeSpan tempoMedio = TimeSpan.FromMilliseconds(mediaMilissegundos);

                string mediaFormatada = string.Format("{0:D2}:{1:D2}.{2:D3}",
                    (int)tempoMedio.TotalMinutes,
                    tempoMedio.Seconds,
                    tempoMedio.Milliseconds);

                rankingFinal.Add(new RankingResultado
                {
                    Carro = piloto.Carro,
                    Nome = piloto.Nome,
                    TotalVoltas = totalVoltasRegistradas,
                    VoltasValidas = voltasValidas.Count,
                    MediaTempo = mediaFormatada,
                    MediaErro = mediaErroSegundos.ToString("F3"), // Exibe a média do erro com precisão de 3 casas (Ex: 0.760 ou 1.233)
                    Status = "OK"
                });
            }

            // 3. ORDENAÇÃO E EXIBIÇÃO NO GRID
            var rankingOrdenado = rankingFinal
                .OrderBy(r => r.Status.StartsWith("DQ") ? 1 : 0) // Joga quem foi desclassificado para baixo
                .ThenBy(r => double.Parse(r.MediaErro))          // Quem teve a MENOR média de erro fica em 1°
                .ToList();

            int classificacaoVisual = 1;
            foreach (var item in rankingOrdenado)
            {
                if (item.Status == "OK")
                {
                    item.Posicao = $"{classificacaoVisual}°";
                    classificacaoVisual++;
                }
                else
                {
                    item.Posicao = "DQL";
                    item.MediaErro = "-"; // Limpa o "999999" para exibição visual limpa
                }
            }

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
            // 1. Verifica se o Grid possui dados para serem exportados
            if (dgvResultados.Rows.Count == 0)
            {
                MessageBox.Show("Não há dados na tabela para exportar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Configura a janela para salvar o arquivo
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Arquivos CSV (*.csv)|*.csv",
                Title = "Salvar Resultado do Ranking",
                FileName = "Ranking_Regularidade.csv" // Nome padrão sugerido
            };

            // 3. Se o usuário clicar em "Salvar"
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    List<string> linhasCsv = new List<string>();

                    // 4. Monta o cabeçalho do CSV baseado nas colunas visíveis do DataGridView
                    List<string> cabecalhos = new List<string>();
                    foreach (DataGridViewColumn coluna in dgvResultados.Columns)
                    {
                        cabecalhos.Add(coluna.HeaderText);
                    }
                    // Junta os cabeçalhos com ";" (Ex: Posicao;Carro;Nome;TotalVoltas...)
                    linhasCsv.Add(string.Join(";", cabecalhos));

                    // 5. Varre as linhas do grid para capturar os dados dos pilotos
                    foreach (DataGridViewRow linha in dgvResultados.Rows)
                    {
                        // Ignora linhas de nova linha vazias (se houver)
                        if (linha.IsNewRow) continue;

                        List<string> celulas = new List<string>();
                        foreach (DataGridViewCell celula in linha.Cells)
                        {
                            // Trata valores nulos para evitar quebras de código
                            string valor = celula.Value != null ? celula.Value.ToString().Trim() : "";

                            // Se o dado contiver ponto e vírgula por acidente, removemos para não quebrar o CSV
                            valor = valor.Replace(";", " ");

                            celulas.Add(valor);
                        }

                        // Junta as células da linha com ";"
                        linhasCsv.Add(string.Join(";", celulas));
                    }

                    // 6. Grava todas as linhas no arquivo selecionado, usando o Encoding padrão para manter acentos corretos
                    File.WriteAllLines(saveFileDialog.FileName, linhasCsv, System.Text.Encoding.UTF8);

                    MessageBox.Show("Resultados exportados com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao exportar os dados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
