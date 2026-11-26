using System.Text.RegularExpressions;

namespace Robotszimulator.Core
{
    // Az energia-napló egy sora a táblázatos megjelenítéshez: Művelet | Változás | Maradt.
    // A Robot szöveges naplósoraiból készül ("drill: -500 Wh, maradt: 9500 Wh"), így a Robot és az Interpreter nem változik.
    public record EnergiaSor(string Muvelet, string Valtozas, string Maradt, string Szin)
    {
        private static readonly Regex Minta = new Regex(@"^(?<m>.+): (?<v>[+-]\d+) Wh, maradt: (?<r>-?\d+) Wh$", RegexOptions.Compiled);

        public static EnergiaSor? Feldolgoz(string sor)
        {
            Match talalat = Minta.Match(sor);
            if (!talalat.Success) return null;

            string valtozas = talalat.Groups["v"].Value;
            return new EnergiaSor(
                talalat.Groups["m"].Value,
                valtozas + " Wh",
                talalat.Groups["r"].Value + " Wh",
                valtozas.StartsWith('+') ? "#98C379" : "#E06C75");
        }

        public static List<EnergiaSor> Lista(IEnumerable<string> sorok)
        {
            var tabla = new List<EnergiaSor>();
            foreach (string sor in sorok)
            {
                EnergiaSor? elem = Feldolgoz(sor);
                tabla.Add(elem ?? new EnergiaSor(sor, string.Empty, string.Empty, "#C8CED8"));
            }
            return tabla;
        }
    }
}