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

            if (!encontrouEstruturaValida)
            {
                throw new FormatException("O arquivo não parece ser um relatório VerdeRosso válido. A marcação de posições ('Pos. ') não foi encontrada.");
            }

            return todosOsPilotos;
        }
    }
}
