using RallyApurator.Models;

namespace RallyApurator.Commands
{
    /// <summary>
    /// Formato VerdeRosso: blocos horizontais com "Pos. N, # carro" seguidos da linha de nomes
    /// e, abaixo, uma linha por volta (volta;tempo piloto1;tempo piloto2;...).
    /// </summary>
    public class LeitorVerdeRosso : ILeitorCronometragem
    {
        public string Nome => "VerdeRosso";

        public List<PilotoDados> Ler(string[] linhas)
        {
            List<PilotoDados> todosOsPilotos = new List<PilotoDados>();
            List<PilotoDados> pilotosDoBlocoAtual = new List<PilotoDados>();
            bool encontrouEstruturaValida = false;

            for (int i = 0; i < linhas.Length; i++)
            {
                string textolinhAtual = linhas[i].Trim();
                if (string.IsNullOrWhiteSpace(textolinhAtual)) continue;

                if (textolinhAtual.Contains("Pos. "))
                {
                    encontrouEstruturaValida = true;
                    string[] colunasLinha = textolinhAtual.Split(';');
                    int idxPos = Array.FindIndex(colunasLinha, c => c.Contains("Pos. "));

                    // Em alguns arquivos o cabeçalho "Pos. ..." vem colado no fim da última volta do bloco anterior
                    // (ex.: "12;-;;-;;;Pos. 6, # 56;..."). Essa parte inicial é uma volta normal do bloco anterior.
                    if (idxPos > 0)
                    {
                        RegistrarVoltas(colunasLinha.Take(idxPos).ToArray(), pilotosDoBlocoAtual);
                    }

                    pilotosDoBlocoAtual.Clear();

                    // Realinha os carros para que a coluna N dos carros corresponda à coluna N dos nomes
                    string[] colunasCarrosBrutas = new[] { "" }.Concat(colunasLinha.Skip(idxPos)).ToArray();

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

                RegistrarVoltas(textolinhAtual.Split(';'), pilotosDoBlocoAtual);
            }

            if (!encontrouEstruturaValida)
            {
                throw new FormatException("O arquivo não parece ser um relatório VerdeRosso válido. A marcação de posições ('Pos. ') não foi encontrada.");
            }

            return todosOsPilotos;
        }

        // Lê uma linha de volta (volta;tempo piloto1;tempo piloto2;...) e adiciona aos pilotos do bloco
        private static void RegistrarVoltas(string[] colunasVolta, List<PilotoDados> pilotosDoBloco)
        {
            if (colunasVolta.Length == 0 || !int.TryParse(colunasVolta[0], out _)) return;

            foreach (var piloto in pilotosDoBloco)
            {
                if (piloto.ColunaIndex >= colunasVolta.Length) continue;

                string tempoTexto = colunasVolta[piloto.ColunaIndex];
                if (string.IsNullOrWhiteSpace(tempoTexto) || tempoTexto.Trim() == "-") continue;

                if (ParserCommands.TentarParsearTempo(tempoTexto, out TimeSpan tempoVolta))
                {
                    piloto.Voltas.Add(tempoVolta);
                }
            }
        }
    }
}
