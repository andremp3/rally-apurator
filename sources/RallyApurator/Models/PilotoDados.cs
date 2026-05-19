using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyApurator.Models
{
    public class PilotoDados
    {
        public string Nome { get; set; }
        public string Carro { get; set; }
        public int ColunaIndex { get; set; }
        public List<TimeSpan> Voltas { get; set; } = new List<TimeSpan>();
    }

}
