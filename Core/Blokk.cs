using Robotszimulator.Beolvasas;

namespace Robotszimulator.Core
{
    public enum BlokkTipus
    {
        Set,
        StepForward,
        TurnBy,
        MineHere,
        Print,
        If,
        While
    }

    public class Blokk
    {
        public const int MaxSzint = 3;
        public const int BehuzasMeret = 2;

        private static readonly HashSet<string> Muveletek = new HashSet<string>
        {
            "+", "-", "*", "=", "<>", "<", "<=", ">", ">=", "and", "or"
        };

        public BlokkTipus Tipus { get; }
        public string[] Szavak { get; private set; }
        public List<Blokk> Gyerekek { get; } = new();
        public List<Blokk> ElseGyerekek { get; } = new();
        public bool VanElse { get; set; }
        public Blokk? Szulo { get; private set; }
        public bool ElseAgban { get; private set; }

        public Blokk(BlokkTipus tipus, params string[] szavak)
        {
            SzavakEllenorzes(tipus, szavak);
            Tipus = tipus;
            Szavak = szavak;
        }

        public bool Vezerlo
        {
            get { return Tipus == BlokkTipus.If || Tipus == BlokkTipus.While; }
        }

        public int Szint
        {
            get
            {
                int szint = 1;
                for (Blokk? s = Szulo; s != null; s = s.Szulo)
                {
                    szint++;
                }
                return szint;
            }
        }

        public int Magassag
        {
            get
            {
                if (!Vezerlo) return 0;

                int legtobb = 0;
                foreach (Blokk gyerek in Gyerekek.Concat(ElseGyerekek))
                {
                    legtobb = Math.Max(legtobb, gyerek.Magassag);
                }
                return legtobb + 1;
            }
        }

        public void SzavakCsere(params string[] szavak)
        {
            SzavakEllenorzes(Tipus, szavak);
            Szavak = szavak;
        }

        public bool BeszurhatoBelsejebe(Blokk uj)
        {
            return Vezerlo && uj.Szulo == null && uj != this && Szint + uj.Magassag <= MaxSzint;
        }

        public void Hozzaad(Blokk uj, bool elseAgba = false, int index = -1)
        {
            if (elseAgba && Tipus != BlokkTipus.If)
            {
                throw new InvalidOperationException("Else ága csak az if blokknak lehet.");
            }
            if (!BeszurhatoBelsejebe(uj))
            {
                throw new InvalidOperationException("A blokk nem szúrható be ide.");
            }

            List<Blokk> lista = elseAgba ? ElseGyerekek : Gyerekek;
            if (index < 0 || index > lista.Count)
            {
                index = lista.Count;
            }

            lista.Insert(index, uj);
            if (elseAgba)
            {
                VanElse = true;
            }

            uj.Szulo = this;
            uj.ElseAgban = elseAgba;
        }

        public bool Eltavolit(Blokk gyerek)
        {
            bool torolve = Gyerekek.Remove(gyerek) || ElseGyerekek.Remove(gyerek);
            if (torolve)
            {
                gyerek.Szulo = null;
                gyerek.ElseAgban = false;
            }
            return torolve;
        }

        public List<Sor> Sorok(int behuzas = 0)
        {
            var sorok = new List<Sor>();
            sorok.Add(new Sor(behuzas, SorSzavak()));

            foreach (Blokk gyerek in Gyerekek)
            {
                sorok.AddRange(gyerek.Sorok(behuzas + BehuzasMeret));
            }

            if (Tipus == BlokkTipus.If && VanElse)
            {
                sorok.Add(new Sor(behuzas, new[] { "else" }));
                foreach (Blokk gyerek in ElseGyerekek)
                {
                    sorok.AddRange(gyerek.Sorok(behuzas + BehuzasMeret));
                }
            }

            return sorok;
        }

        public static List<Sor> ProgramSorok(IEnumerable<Blokk> blokkok)
        {
            var sorok = new List<Sor>();
            foreach (Blokk blokk in blokkok)
            {
                sorok.AddRange(blokk.Sorok());
            }
            return sorok;
        }

        public static string ProgramSzoveg(IEnumerable<Blokk> blokkok)
        {
            return string.Join(Environment.NewLine, ProgramSorok(blokkok).Select(SorSzoveg));
        }

        public static string SorSzoveg(Sor sor)
        {
            return new string(' ', sor.szokoz) + string.Join(' ', sor.szavak);
        }

        public override string ToString()
        {
            return string.Join(' ', SorSzavak());
        }

        private string[] SorSzavak()
        {
            switch (Tipus)
            {
                case BlokkTipus.Set:
                    return new[] { "set", Szavak[0], "to" }.Concat(Szavak.Skip(1)).ToArray();
                case BlokkTipus.StepForward:
                    return new[] { "step", "forward" }.Concat(Szavak).ToArray();
                case BlokkTipus.TurnBy:
                    return new[] { "turn", "by" }.Concat(Szavak).ToArray();
                case BlokkTipus.MineHere:
                    return new[] { "mine", "here" };
                case BlokkTipus.Print:
                    return new[] { "print" }.Concat(Szavak).ToArray();
                case BlokkTipus.If:
                    return new[] { "if" }.Concat(Szavak).ToArray();
                default:
                    return new[] { "while" }.Concat(Szavak).ToArray();
            }
        }

        private static void SzavakEllenorzes(BlokkTipus tipus, string[] szavak)
        {
            if (szavak == null)
            {
                throw new ArgumentNullException(nameof(szavak));
            }

            switch (tipus)
            {
                case BlokkTipus.MineHere:
                    DarabEllenorzes(szavak, 0, 0);
                    break;
                case BlokkTipus.Set:
                    SetEllenorzes(szavak);
                    break;
                default:
                    DarabEllenorzes(szavak, 1, 1);
                    EgyszeruErtekEllenorzes(szavak[0]);
                    break;
            }
        }

        private static void SetEllenorzes(string[] szavak)
        {
            DarabEllenorzes(szavak, 2, 4);

            if (!Kulcsszavak.ErvenyesAzonosito(szavak[0]))
            {
                throw new ArgumentException("Érvénytelen változónév: " + szavak[0]);
            }

            string[] jobbOldal = szavak.Skip(1).ToArray();
            switch (jobbOldal.Length)
            {
                case 1:
                    EgyszeruErtekEllenorzes(jobbOldal[0]);
                    break;
                case 2:
                    if (jobbOldal[0] != "not" && jobbOldal[0] != "isThere")
                    {
                        throw new ArgumentException("Érvénytelen egyoperandusú művelet: " + jobbOldal[0]);
                    }
                    EgyszeruErtekEllenorzes(jobbOldal[1]);
                    break;
                default:
                    EgyszeruErtekEllenorzes(jobbOldal[0]);
                    if (!Muveletek.Contains(jobbOldal[1]))
                    {
                        throw new ArgumentException("Érvénytelen művelet: " + jobbOldal[1]);
                    }
                    EgyszeruErtekEllenorzes(jobbOldal[2]);
                    break;
            }
        }

        private static void DarabEllenorzes(string[] szavak, int legalabb, int legfeljebb)
        {
            if (szavak.Length < legalabb || szavak.Length > legfeljebb)
            {
                throw new ArgumentException("A blokk paramétereinek száma hibás.");
            }
        }

        private static void EgyszeruErtekEllenorzes(string ertek)
        {
            if (ertek == "true" || ertek == "false") return;
            if (SzamE(ertek)) return;
            if (Kulcsszavak.ErvenyesAzonosito(ertek)) return;

            throw new ArgumentException("Érvénytelen érték: " + ertek);
        }

        private static bool SzamE(string ertek)
        {
            if (string.IsNullOrEmpty(ertek) || !ertek.All(char.IsAsciiDigit)) return false;
            return int.TryParse(ertek, out int szam) && szam >= 0 && szam <= 100;
        }
    }
}
