using RallyApurator.Models;

namespace RallyApurator.Commands
{
    /// <summary>
    /// Formato MyLaps: cabeçalho "Time of Day;Lap;Lap Tm" e, para cada piloto,
    /// uma linha de identificação "Carro - Nome - Modelo - Classe;;" seguida de
    /// uma linha por volta "hora do dia;nº da volta;tempo da volta".
    /// </summary>
    public class LeitorMyLaps : ILeitorCronometragem
    {
        public string Nome => "MyLaps";

        public List<PilotoDados> Ler(string[] linhas)
        {
            var todosOsPilotos = new List<PilotoDados>();
            PilotoDados pilotoAtual = null;
            bool encontrouCabecalho = false;

            foreach (var linhaBruta in linhas)
            {
                string linha = linhaBruta.Trim();
                if (string.IsNullOrWhiteSpace(linha)) continue;

                string[] col = linha.Split(';');

                // Cabeçalho do arquivo
                if (col[0].Trim().Equals("Time of Day", StringComparison.OrdinalIgnoreCase))
                {
                    encontrouCabecalho = true;
                    continue;
                }

                // Linha de volta: hora;nº volta;tempo
                if (col.Length >= 3 && int.TryParse(col[1].Trim(), out _))
                {
                    if (pilotoAtual != null && ParserCommands.TentarParsearTempo(col[2], out TimeSpan tempoVolta))
                    {
                        pilotoAtual.Voltas.Add(tempoVolta);
                    }
                    continue;
                }

                // Linha de piloto: "Carro - Nome - Modelo - Classe"
                string[] partes = col[0].Split(new[] { " - " }, StringSplitOptions.None);
                if (partes.Length >= 2)
                {
                    string numCarro = partes[0].Trim();
                    string nomePiloto = partes[1].Trim();

                    // Identifica pelo nº do carro: o mesmo piloto pode correr com carros diferentes
                    pilotoAtual = todosOsPilotos.FirstOrDefault(p => p.Carro == numCarro);
                    if (pilotoAtual == null)
                    {
                        pilotoAtual = new PilotoDados
                        {
                            Nome = nomePiloto,
                            Carro = numCarro,
                            Voltas = new List<TimeSpan>()
                        };
                        todosOsPilotos.Add(pilotoAtual);
                    }
                }
            }

            if (!encontrouCabecalho)
            {
                throw new FormatException("O arquivo não parece ser um relatório MyLaps válido. O cabeçalho 'Time of Day;Lap;Lap Tm' não foi encontrado.");
            }

            return todosOsPilotos;
        }
    }
}
