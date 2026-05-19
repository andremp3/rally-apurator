using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyApurator.Models
{
    public class RankingResultado
    {
        public string Posicao { get; set; }
        public string Carro { get; set; }
        public string Nome { get; set; }
        public int TotalVoltas { get; set; }
        public int VoltasValidas { get; set; }
        public string MediaTempo { get; set; }
        public string MediaErro { get; set; } // String para exibir "DQ" ou a pontuação formatada
        public string Status { get; set; }
    }

}
