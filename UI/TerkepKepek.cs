using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Robotszimulator.UI
{
    public class TerkepKepek
    {
        private const string Mappa = "Képek";

        private readonly Dictionary<string, List<BitmapSource>> felfedezettKepek = new();
        private readonly Dictionary<string, List<BitmapSource>> felfedezetlenKepek = new();
        private readonly Dictionary<string, List<string>> felfedezettVegzodesek = new();
        private readonly Dictionary<string, List<string>> felfedezetlenVegzodesek = new();

        public bool Betoltve { get; private set; }

        public TerkepKepek()
        {
            string? mappa = MappaKeresese();
            if (mappa == null) return;

            foreach (string tipus in new[] { "talaj", "fal", "arany" })
            {
                TipusBetoltese(mappa, tipus);
            }

            Betoltve = felfedezettKepek.ContainsKey("talaj")
                && felfedezettKepek.ContainsKey("fal")
                && felfedezettKepek.ContainsKey("arany");
        }

        public BitmapSource? Kep(string tipus, bool felfedezve, int sors)
        {
            if (!felfedezettKepek.TryGetValue(tipus, out var kepek) || kepek.Count == 0) return null;

            int index = sors % kepek.Count;
            if (felfedezve) return kepek[index];

            string vegzodes = felfedezettVegzodesek[tipus][index];
            if (felfedezetlenKepek.TryGetValue(tipus, out var sotetek) && sotetek.Count > 0)
            {
                int par = felfedezetlenVegzodesek[tipus].IndexOf(vegzodes);
                return par >= 0 ? sotetek[par] : sotetek[sors % sotetek.Count];
            }

            return kepek[index];
        }

        private void TipusBetoltese(string mappa, string tipus)
        {
            var felfedezett = new List<(string Vegzodes, BitmapSource Kep)>();
            var felfedezetlen = new List<(string Vegzodes, BitmapSource Kep)>();

            var felfedezettMinta = new Regex("^" + tipus + @"(\d*)$", RegexOptions.IgnoreCase);
            var felfedezetlenMinta = new Regex("^" + tipus + @"_felfedezetlen(\d*)$", RegexOptions.IgnoreCase);

            foreach (string fajl in Directory.GetFiles(mappa, "*.png").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string nev = Path.GetFileNameWithoutExtension(fajl);
                var egyezesFelfedezett = felfedezettMinta.Match(nev);
                var egyezesFelfedezetlen = felfedezetlenMinta.Match(nev);

                if (!egyezesFelfedezett.Success && !egyezesFelfedezetlen.Success) continue;

                BitmapSource? kep = KepBetoltese(fajl);
                if (kep == null) continue;

                if (egyezesFelfedezett.Success)
                {
                    felfedezett.Add((egyezesFelfedezett.Groups[1].Value, kep));
                }
                else
                {
                    felfedezetlen.Add((egyezesFelfedezetlen.Groups[1].Value, kep));
                }
            }

            if (felfedezett.Count == 0) return;

            felfedezettKepek[tipus] = felfedezett.Select(x => x.Kep).ToList();
            felfedezettVegzodesek[tipus] = felfedezett.Select(x => x.Vegzodes).ToList();
            felfedezetlenKepek[tipus] = felfedezetlen.Select(x => x.Kep).ToList();
            felfedezetlenVegzodesek[tipus] = felfedezetlen.Select(x => x.Vegzodes).ToList();
        }

        private static BitmapSource? KepBetoltese(string fajl)
        {
            try
            {
                var kep = new BitmapImage();
                kep.BeginInit();
                kep.CacheOption = BitmapCacheOption.OnLoad;
                kep.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                kep.DecodePixelWidth = 64;
                kep.UriSource = new Uri(Path.GetFullPath(fajl));
                kep.EndInit();
                kep.Freeze();
                return kep;
            }
            catch
            {
                return null;
            }
        }

        private static string? MappaKeresese()
        {
            string? konyvtar = AppContext.BaseDirectory;
            for (int i = 0; i < 6 && konyvtar != null; i++)
            {
                string utvonal = Path.Combine(konyvtar, Mappa);
                if (Directory.Exists(utvonal)) return utvonal;
                konyvtar = Path.GetDirectoryName(konyvtar.TrimEnd(Path.DirectorySeparatorChar));
            }

            return null;
        }
    }
}
