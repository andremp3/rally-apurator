using RallyApurator.Models;

namespace RallyApurator.Commands
{
    /// <summary>
    /// Contrato para qualquer formato de CSV de cronometragem.
    /// Cada formato (VerdeRosso, MyLaps...) implementa a sua própria leitura,
    /// mas todos devolvem a mesma lista de PilotoDados para o resto do sistema.
    /// </summary>
    public interface ILeitorCronometragem
    {
        /// <summary>Nome amigável do formato (usado em mensagens e, se quiser, em ComboBox).</summary>
        string Nome { get; }

        /// <summary>
        /// Converte as linhas do CSV em pilotos com suas voltas.
        /// Deve lançar FormatException (com mensagem amigável) se o arquivo não for do formato esperado.
        /// </summary>
        List<PilotoDados> Ler(string[] linhas);
    }
}
