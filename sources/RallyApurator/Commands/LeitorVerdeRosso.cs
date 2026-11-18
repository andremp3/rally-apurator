using RallyApurator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyApurator.Commands
{
    public class ApuradorRally
    {
        // Método para Processar o Arquivo CSV
        public static List<PilotoDados> ProcessarArquivoCSV()
        {
            List<PilotoDados> todosOsPilotos = new List<PilotoDados>();
            List<PilotoDados> pilotosDoBlocoAtual = new List<PilotoDados>();

            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Arquivos CSV (*.csv)|*.csv" };
            if (openFileDialog.ShowDialog() != DialogResult.OK) return null;

            try
            {
                // 1. VALIDAÇÃO: O arquivo existe e tem conteúdo?
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                if (fileInfo.Length == 0)
                {
                    MessageBox.Show("O arquivo selecionado está totalmente vazio (0 bytes).", "CSV Inválido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                string[] linhas = File.ReadAllLines(openFileDialog.FileName, System.Text.Encoding.Default);

                // 2. VALIDAÇÃO: Tem linhas de dados suficientes para processar?
                if (linhas == null || linhas.Length < 2)
                {
                    MessageBox.Show("O arquivo CSV não possui linhas suficientes para conter dados de pilotos e tempos.", "CSV Inválido", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // Variável de controle para sabermos se encontramos a estrutura padrão da cronometragem
                bool encontrouEstruturaValida = false;

                for (int i = 0; i < linhas.Length; i++)
                {
                    string textolinhAtual = linhas[i].Trim();
                    if (string.IsNullOrWhiteSpace(textolinhAtual)) continue;

                    if (textolinhAtual.Contains("Pos. "))
                    {
                        encontrouEstruturaValida = true; // Achou pelo menos um cabeçalho padrão
                        pilotosDoBlocoAtual.Clear();
                        string[] colunasCarrosBrutas = textolinhAtual.Split(';');

                        if (i + 1 >= linhas.Length) break;
                        string[] colunasNomesBrutas = linhas[i + 1].Split(';');

                        for (int col = 1; col < colunasNomesBrutas.Length; col++)
                        {
                            if (string.IsNullOrWhiteSpace(colunasNomesBrutas[col])) continue;

                            string nomePiloto = colunasNomesBrutas[col].Trim();
                            if (nomePiloto.Equals("Lap", StringComparison.OrdinalIgnoreCase)) continue;

                            string numCarro = "S/N";
                            if (col < colunasCarrosBrutas.Length && colunasCarrosBrutas[col].Contains("#"))
                            {
                                var partes = colunasCarrosBrutas[col].Split('#');
                                if (partes.Length > 1 && !string.IsNullOrWhiteSpace(partes[1]))
                                {
                                    numCarro = partes[1].Trim();
                                }
                            }

                            PilotoDados pilotoExistente = todosOsPilotos.FirstOrDefault(p => p.Nome.Equals(nomePiloto, StringComparison.OrdinalIgnoreCase));

                            if (pilotoExistente != null)
                            {
                                if (numCarro != "S/N") pilotoExistente.Carro = numCarro;
                                pilotoExistente.ColunaIndex = col;
                                pilotosDoBlocoAtual.Add(pilotoExistente);
                            }
                            else
                            {
                                var novoPiloto = new PilotoDados
                                {
                                    Nome = nomePiloto,
                                    Carro = numCarro,
                                    ColunaIndex = col,
                                    Voltas = new List<TimeSpan>()
                                };
                                todosOsPilotos.Add(novoPiloto);
                                pilotosDoBlocoAtual.Add(novoPiloto);
                            }
                        }
                        i++;
                        continue;
                    }

                    string[] colunasVolta = textolinhAtual.Split(';');
                    if (colunasVolta.Length > 0 && int.TryParse(colunasVolta[0], out int numeroVolta))
                    {
                        foreach (var piloto in pilotosDoBlocoAtual)
                        {
                            if (piloto.ColunaIndex < colunasVolta.Length)
                            {
                                string tempoTexto = colunasVolta[piloto.ColunaIndex];
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

                // 3. VALIDAÇÃO: O arquivo tinha texto, mas não era o CSV da cronometragem (não tinha os blocos "Pos. ")
                if (!encontrouEstruturaValida)
                {
                    MessageBox.Show("O arquivo não parece ser um relatório de tempos válido. A marcação de posições ('Pos. ') não foi encontrada.", "Formato Incorreto", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // 4. VALIDAÇÃO: Encontrou os blocos, mas nenhum piloto válido foi extraído
                if (todosOsPilotos.Count == 0)
                {
                    MessageBox.Show("Nenhum piloto ou tempo de volta válido pôde ser extraído deste arquivo.", "Dados Ausentes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                return todosOsPilotos;
            }
            catch (IOException ioEx)
            {
                // Trata o caso clássico de tentar abrir um CSV que já está aberto no Excel
                MessageBox.Show($"O arquivo está sendo usado por outro programa (provavelmente o Excel).\n\nFeche o arquivo e tente novamente.\nDetalhes: {ioEx.Message}", "Arquivo Bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                // Captura qualquer outro erro inesperado (falta de memória, caracteres corrompidos, etc)
                MessageBox.Show($"Falha crítica ao ler o arquivo CSV:\n{ex.Message}", "Erro Inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

        }

        // Método para Aplicar as Regras do Regulamento
        public static List<RankingResultado> ProcessarRegrasRally(List<PilotoDados> todosOsPilotos, TimeSpan tempoAlvo, int qtdVoltasExigidas, TimeSpan tempoLimiteDQ, int limiteVoltasRapidas, string tempoDqTextoVisual)
        {
            List<RankingResultado> rankingFinal = new List<RankingResultado>();

            foreach (var piloto in todosOsPilotos)
            {
                int totalVoltasRegistradas = piloto.Voltas.Count;
                int voltasAbaixoDoLimite = piloto.Voltas.Count(v => v < tempoLimiteDQ);

                if (voltasAbaixoDoLimite >= limiteVoltasRapidas)
                {
                    rankingFinal.Add(new RankingResultado
                    {
                        Carro = piloto.Carro,
                        Nome = piloto.Nome,
                        TotalVoltas = totalVoltasRegistradas,
                        VoltasValidas = 0,
                        MediaTempo = "-",
                        MediaErro = "999999",
                        Status = $"DQL ({voltasAbaixoDoLimite} voltas abaixo de {tempoDqTextoVisual})"
                    });
                    continue;
                }

                var voltasValidas = piloto.Voltas.Where(v => v >= tempoAlvo).ToList();

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

                var melhoresVoltas = voltasValidas.OrderBy(v => v).Take(qtdVoltasExigidas).ToList();

                double erroTotalSegundos = melhoresVoltas.Sum(v => (v - tempoAlvo).TotalSeconds);
                double mediaErroSegundos = erroTotalSegundos / qtdVoltasExigidas;

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
                    MediaErro = mediaErroSegundos.ToString("F3"),
                    Status = "OK"
                });
            }

            return rankingFinal;
        }

        // Método para Gerar a Ordenação Final do Ranking
        public static List<RankingResultado> OrdenarRanking(List<RankingResultado> rankingFinal)
        {
            var rankingOrdenado = rankingFinal
                .OrderBy(r => r.Status.StartsWith("DQL") ? 1 : 0)
                .ThenBy(r => double.Parse(r.MediaErro))
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
                    item.MediaErro = "-";
                }
            }

            return rankingOrdenado;
        }

        // Novo método para gerar a estrutura do arquivo CSV
        public static List<string> GerarLinhasCsv(List<RankingResultado> ranking)
        {
            List<string> linhasCsv = new List<string>();

            // 1. Monta o cabeçalho de forma fixa e padronizada
            linhasCsv.Add("Posição;Carro;Piloto;Total Voltas;Voltas Válidas;Média de Tempo;Média de Erro;Status");

            // 2. Transforma cada piloto em uma linha de texto separada por ";"
            foreach (var item in ranking)
            {
                // Substitui eventuais pontos e vírgulas no nome ou status para não quebrar colunas do CSV
                string nomeLimpo = item.Nome?.Replace(";", " ") ?? "";
                string statusLimpo = item.Status?.Replace(";", " ") ?? "";

                string linha = string.Join(";",
                    item.Posicao,
                    item.Carro,
                    nomeLimpo,
                    item.TotalVoltas,
                    item.VoltasValidas,
                    item.MediaTempo,
                    item.MediaErro,
                    statusLimpo
                );

                linhasCsv.Add(linha);
            }

            return linhasCsv;
        }

        public static string ObterCuriosidadesDaEtapa(List<PilotoDados> todosOsPilotos, TimeSpan tempoAlvo, TimeSpan tempoMinimo)
        {
            if (todosOsPilotos == null || todosOsPilotos.Count == 0) return "Nenhum dado carregado.";

            System.Text.StringBuilder pb = new System.Text.StringBuilder();
            pb.AppendLine("📊 CURIOSIDADES E DESTAQUES DA ETAPA 📊\n");

            // Define o teto máximo aceitável para estatísticas de pista (Tempo Alvo + 10 segundos)
            TimeSpan tetoMaximoEstatistica = tempoAlvo.Add(TimeSpan.FromSeconds(10));

            // 1. Lista base contendo ABSOLUTAMENTE todas as voltas maiores ou iguais ao tempo alvo (usada no Cirurgião e Maratonista)
            var todasAsVoltasMaioresQueAlvo = todosOsPilotos
                .SelectMany(p => p.Voltas.Where(v => v >= tempoAlvo).Select(v => new { Piloto = p, Tempo = v }))
                .ToList();

            // 2. NOVA LISTA FILTRADA: Ignora voltas de box ou incidentes graves (entre Alvo e Alvo + 10s)
            var voltasValidasSemBox = todosOsPilotos
                .SelectMany(p => p.Voltas.Where(v => v >= tempoAlvo && v <= tetoMaximoEstatistica).Select(v => new { Piloto = p, Tempo = v }))
                .ToList();


            // ==========================================
            // 🎯 O CIRURGIÃO
            // ==========================================
            if (todasAsVoltasMaioresQueAlvo.Any())
            {
                var voltaMaisProxima = todasAsVoltasMaioresQueAlvo
                    .OrderBy(v => Math.Abs((v.Tempo - tempoAlvo).TotalMilliseconds))
                    .First();

                double deltaMilissegundos = Math.Abs((voltaMaisProxima.Tempo - tempoAlvo).TotalMilliseconds);

                pb.AppendLine("🎯 O CIRURGIÃO (Volta mais próxima do Alvo):");
                pb.AppendLine($"   Piloto: {voltaMaisProxima.Piloto.Nome} (Carro {voltaMaisProxima.Piloto.Carro})");
                pb.AppendLine($"   Tempo da Volta: {FormatarTempo(voltaMaisProxima.Tempo)}");
                pb.AppendLine($"   Diferença: +{(deltaMilissegundos / 1000).ToString("F3")}s do alvo!\n");
            }


            // ==========================================
            // ⏱️ O RELÓGIO SUÍÇO (Baseado nas voltas sem anomalia de box)
            // ==========================================
            var pilotosComVoltasSuficientes = todosOsPilotos
                .Where(p => p.Voltas.Count(v => v >= tempoAlvo && v <= tetoMaximoEstatistica) >= 3)
                .ToList();

            if (pilotosComVoltasSuficientes.Any())
            {
                var melhorConstancia = pilotosComVoltasSuficientes
                    .Select(p => {
                        var validas = p.Voltas.Where(v => v >= tempoAlvo && v <= tetoMaximoEstatistica).Select(v => v.TotalSeconds).ToList();
                        double media = validas.Average();
                        double somaDosQuadrados = validas.Sum(v => Math.Pow(v - media, 2));
                        double desvioPadrao = Math.Sqrt(somaDosQuadrados / validas.Count);
                        return new { Piloto = p, Desvio = desvioPadrao };
                    })
                    .OrderBy(p => p.Desvio)
                    .First();

                pb.AppendLine("⏱️ O RELÓGIO SUÍÇO (Maior Constância em Ritmo de Pista):");
                pb.AppendLine($"   Piloto: {melhorConstancia.Piloto.Nome} (Carro {melhorConstancia.Piloto.Carro})");
                pb.AppendLine($"   Variação Média: ±{melhorConstancia.Desvio.ToString("F3")} segundos\n");
            }


            // ==========================================
            // ⚠️ SALVO PELO GONGO
            // ==========================================
            var voltasNoLimite = todosOsPilotos
                .SelectMany(p => p.Voltas.Where(v => v >= tempoMinimo).Select(v => new { Piloto = p, Tempo = v }))
                .ToList();

            if (voltasNoLimite.Any())
            {
                var oMaisRapidoValido = voltasNoLimite
                    .OrderBy(v => (v.Tempo - tempoMinimo).TotalMilliseconds)
                    .First();

                double folgaMilissegundos = (oMaisRapidoValido.Tempo - tempoMinimo).TotalMilliseconds;

                pb.AppendLine("⚠️ SALVO PELO GONGO (Quase tomou DQ por passar rápido demais):");
                pb.AppendLine($"   Piloto: {oMaisRapidoValido.Piloto.Nome} (Carro {oMaisRapidoValido.Piloto.Carro})");
                pb.AppendLine($"   Tempo da Volta: {FormatarTempo(oMaisRapidoValido.Tempo)}");
                pb.AppendLine($"   Folga de apenas: {(folgaMilissegundos / 1000).ToString("F3")}s acima do limite!\n");
            }


            // ==========================================
            // 🐌 O INIMIGO DO GIRO (Agora limitado a Alvo + 10s)
            // ==========================================
            if (voltasValidasSemBox.Any())
            {
                var voltaMaisLenta = voltasValidasSemBox
                    .OrderByDescending(v => v.Tempo.TotalMilliseconds)
                    .First();

                double deltaLento = (voltaMaisLenta.Tempo - tempoAlvo).TotalSeconds;

                pb.AppendLine("🐌 O INIMIGO DO GIRO (A maior 'tirada de pé' válida - Max Alvo + 10s):");
                pb.AppendLine($"   Piloto: {voltaMaisLenta.Piloto.Nome} (Carro {voltaMaisLenta.Piloto.Carro})");
                pb.AppendLine($"   Tempo da Volta: {FormatarTempo(voltaMaisLenta.Tempo)} (+{deltaLento.ToString("F3")}s do alvo)\n");
            }


            // ==========================================
            // 🏁 O MARATONISTA
            // ==========================================
            var maratonista = todosOsPilotos.OrderByDescending(p => p.Voltas.Count).FirstOrDefault();
            if (maratonista != null)
            {
                pb.AppendLine("🏁 O MARATONISTA (Quem mais completou voltas na pista):");
                pb.AppendLine($"   Piloto: {maratonista.Nome} (Carro {maratonista.Carro})");
                pb.AppendLine($"   Total: {maratonista.Voltas.Count} voltas registradas.\n");
            }


            // ==========================================
            // 📈 RITMO GERAL DO GRID (Média real sem os tempos de box)
            // ==========================================
            if (voltasValidasSemBox.Any())
            {
                double mediaGeralMs = voltasValidasSemBox.Average(v => v.Tempo.TotalMilliseconds);
                TimeSpan tempoMediaGeral = TimeSpan.FromMilliseconds(mediaGeralMs);

                pb.AppendLine("📈 RITMO GERAL DO GRID (Excluindo entrada de box/erros graves):");
                pb.AppendLine($"   Tempo Médio das voltas de pista: {FormatarTempo(tempoMediaGeral)}");
            }

            return pb.ToString();
        }

        private static string FormatarTempo(TimeSpan tempo)
        {
            return string.Format("{0:D2}:{1:D2}.{2:D3}", (int)tempo.TotalMinutes, tempo.Seconds, tempo.Milliseconds);
        }
    }
}
