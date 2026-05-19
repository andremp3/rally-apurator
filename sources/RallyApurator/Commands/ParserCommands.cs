using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RallyApurator.Commands
{
    public static class ParserCommands
    {
        public static bool TentarParsearTempo(string input, out TimeSpan tempo)
        {
            tempo = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(input) || input.Trim() == "-") return false;

            string[] formatos = {
        @"m\:ss\.fff",
        @"mm\:ss\.fff",
        @"h\:mm\:ss\.fff",
        @"hh\:mm\:ss\.fff"
    };

            return TimeSpan.TryParseExact(input.Trim().Replace(',', '.'), formatos,
                System.Globalization.CultureInfo.InvariantCulture, out tempo);
        }
    }
}
