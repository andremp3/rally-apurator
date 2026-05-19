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

            string[] linhas = File.ReadAllLines(openFileDialog.FileName, System.Text.Encoding.Default);

            for (int i = 0; i < linhas.Length; i++)
            {
                string textolinhAtual = linhas[i].Trim();
                if (string.IsNullOrWhiteSpace(textolinhAtual)) continue;

                if (textolinhAtual.Contains("Pos. "))
                {
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
                                // Aqui assume que o ParserCommands está acessível ou no mesmo namespace
                                if (ParserCommands.TentarParsearTempo(tempoTexto, out TimeSpan tempoVolta))
                                {
                                    piloto.Voltas.Add(tempoVolta);
                                }
                            }
                        }
                    }
                }
            }

            return todosOsPilotos;
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
    }
}
