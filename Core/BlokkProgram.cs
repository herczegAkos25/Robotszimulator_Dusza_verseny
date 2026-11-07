using Robotszimulator.Beolvasas;

namespace Robotszimulator.Core
{
    public enum BeszurasMod
    {
        Utana,
        Belsejebe,
        ElseAgba
    }

    public class BlokkProgram
    {
        private record Nyitott(Blokk Blokk, int Behuzas);

        public List<Blokk> Gyokerek { get; } = new();

        public List<Sor> Sorok()
        {
            return Blokk.ProgramSorok(Gyokerek);
        }

        public string Szoveg()
        {
            return Blokk.ProgramSzoveg(Gyokerek);
        }

        public static List<Sor> SzovegSorok(string szoveg)
        {
            var sorok = new List<Sor>();
            foreach (string nyers in szoveg.Split('\n'))
            {
                string sor = nyers.TrimEnd('\r');
                if (!string.IsNullOrWhiteSpace(sor))
                {
                    sorok.Add(Sor.Parse(sor));
                }
            }
            return sorok;
        }

        public static BlokkProgram Feldolgoz(string szoveg)
        {
            return Feldolgoz(SzovegSorok(szoveg));
        }

        public static BlokkProgram Feldolgoz(IEnumerable<Sor> sorok)
        {
            var program = new BlokkProgram();
            var verem = new List<Nyitott>();
            int sorszam = 0;

            foreach (Sor sor in sorok)
            {
                sorszam++;
                try
                {
                    SorFeldolgozas(program, verem, sor);
                }
                catch (ArgumentException hiba)
                {
                    throw new FormatException(sorszam + ". sor: " + hiba.Message);
                }
                catch (InvalidOperationException hiba)
                {
                    throw new FormatException(sorszam + ". sor: " + hiba.Message);
                }
            }

            return program;
        }

        public List<BlokkSor> Lapit()
        {
            var kimenet = new List<BlokkSor>();
            foreach (Blokk blokk in Gyokerek)
            {
                Lapit(blokk, 1, kimenet);
            }
            return kimenet;
        }

        public void Beszur(Blokk uj, BlokkSor? kijelolt, BeszurasMod mod)
        {
            if (kijelolt == null)
            {
                Gyokerek.Add(uj);
                return;
            }

            Blokk cel = kijelolt.Blokk;

            switch (mod)
            {
                case BeszurasMod.Utana:
                    if (kijelolt.ElseSor)
                    {
                        cel.Hozzaad(uj, true, 0);
                    }
                    else if (cel.Szulo == null)
                    {
                        Gyokerek.Insert(Gyokerek.IndexOf(cel) + 1, uj);
                    }
                    else
                    {
                        List<Blokk> testverek = Testverek(cel);
                        cel.Szulo.Hozzaad(uj, cel.ElseAgban, testverek.IndexOf(cel) + 1);
                    }
                    break;

                case BeszurasMod.Belsejebe:
                    if (kijelolt.ElseSor || !cel.Vezerlo)
                    {
                        throw new InvalidOperationException("A belsejébe csak if vagy while blokkot lehet beszúrni.");
                    }
                    cel.Hozzaad(uj);
                    break;

                default:
                    if (kijelolt.ElseSor || cel.Tipus != BlokkTipus.If)
                    {
                        throw new InvalidOperationException("Else ága csak az if blokknak lehet.");
                    }
                    cel.Hozzaad(uj, true);
                    break;
            }
        }

        public bool Torol(Blokk blokk)
        {
            if (blokk.Szulo != null)
            {
                return blokk.Szulo.Eltavolit(blokk);
            }
            return Gyokerek.Remove(blokk);
        }

        public bool ElseTorlese(Blokk ifBlokk)
        {
            if (ifBlokk.Tipus != BlokkTipus.If || !ifBlokk.VanElse)
            {
                return false;
            }

            foreach (Blokk gyerek in ifBlokk.ElseGyerekek.ToList())
            {
                ifBlokk.Eltavolit(gyerek);
            }
            ifBlokk.VanElse = false;
            return true;
        }

        public bool Mozgat(Blokk blokk, int irany)
        {
            List<Blokk> lista = Testverek(blokk);
            int index = lista.IndexOf(blokk);
            int uj = index + irany;
            if (index < 0 || uj < 0 || uj >= lista.Count)
            {
                return false;
            }

            lista.RemoveAt(index);
            lista.Insert(uj, blokk);
            return true;
        }

        public string? Hiba()
        {
            foreach (BlokkSor sor in Lapit())
            {
                if (sor.ElseSor && sor.Blokk.ElseGyerekek.Count == 0)
                {
                    return "A(z) " + sor.Sorszam + ". sorban lévő else ág üres.";
                }
                if (!sor.ElseSor && sor.Blokk.Vezerlo && sor.Blokk.Gyerekek.Count == 0)
                {
                    return "A(z) " + sor.Sorszam + ". sorban lévő " + sor.Blokk.Tipus.ToString().ToLower() + " blokk üres.";
                }
            }
            return null;
        }

        private List<Blokk> Testverek(Blokk blokk)
        {
            if (blokk.Szulo == null)
            {
                return Gyokerek;
            }
            return blokk.ElseAgban ? blokk.Szulo.ElseGyerekek : blokk.Szulo.Gyerekek;
        }

        private static void Lapit(Blokk blokk, int szint, List<BlokkSor> kimenet)
        {
            kimenet.Add(new BlokkSor(kimenet.Count + 1, blokk, false, szint));
            foreach (Blokk gyerek in blokk.Gyerekek)
            {
                Lapit(gyerek, szint + 1, kimenet);
            }

            if (blokk.Tipus == BlokkTipus.If && blokk.VanElse)
            {
                kimenet.Add(new BlokkSor(kimenet.Count + 1, blokk, true, szint));
                foreach (Blokk gyerek in blokk.ElseGyerekek)
                {
                    Lapit(gyerek, szint + 1, kimenet);
                }
            }
        }

        private static void SorFeldolgozas(BlokkProgram program, List<Nyitott> verem, Sor sor)
        {
            string[] szavak = sor.szavak;

            if (szavak[0] == "else")
            {
                if (szavak.Length != 1)
                {
                    throw new InvalidOperationException("Az else sorban nem állhat más.");
                }

                while (verem.Count > 0 && sor.szokoz < verem[^1].Behuzas)
                {
                    verem.RemoveAt(verem.Count - 1);
                }

                if (verem.Count == 0 || verem[^1].Behuzas != sor.szokoz
                    || verem[^1].Blokk.Tipus != BlokkTipus.If || verem[^1].Blokk.VanElse)
                {
                    throw new InvalidOperationException("Az else nem tartozik egyetlen if blokkhoz sem.");
                }

                verem[^1].Blokk.VanElse = true;
                return;
            }

            while (verem.Count > 0 && sor.szokoz <= verem[^1].Behuzas)
            {
                verem.RemoveAt(verem.Count - 1);
            }

            Blokk uj = UjBlokk(szavak);
            if (verem.Count == 0)
            {
                program.Gyokerek.Add(uj);
            }
            else
            {
                Blokk szulo = verem[^1].Blokk;
                szulo.Hozzaad(uj, szulo.VanElse);
            }

            if (uj.Vezerlo)
            {
                verem.Add(new Nyitott(uj, sor.szokoz));
            }
        }

        private static Blokk UjBlokk(string[] szavak)
        {
            switch (szavak[0])
            {
                case "set":
                    if (szavak.Length < 4 || szavak[2] != "to")
                    {
                        throw new InvalidOperationException("A set utasítás alakja: set név to érték.");
                    }
                    return new Blokk(BlokkTipus.Set, new[] { szavak[1] }.Concat(szavak.Skip(3)).ToArray());

                case "step":
                    if (szavak.Length != 3 || szavak[1] != "forward")
                    {
                        throw new InvalidOperationException("A step utasítás alakja: step forward n.");
                    }
                    return new Blokk(BlokkTipus.StepForward, szavak[2]);

                case "turn":
                    if (szavak.Length != 3 || szavak[1] != "by")
                    {
                        throw new InvalidOperationException("A turn utasítás alakja: turn by n.");
                    }
                    return new Blokk(BlokkTipus.TurnBy, szavak[2]);

                case "mine":
                    if (szavak.Length != 2 || szavak[1] != "here")
                    {
                        throw new InvalidOperationException("A mine utasítás alakja: mine here.");
                    }
                    return new Blokk(BlokkTipus.MineHere);

                case "print":
                    if (szavak.Length != 2)
                    {
                        throw new InvalidOperationException("A print utasítás alakja: print érték.");
                    }
                    return new Blokk(BlokkTipus.Print, szavak[1]);

                case "if":
                case "while":
                    if (szavak.Length != 2)
                    {
                        throw new InvalidOperationException("Az " + szavak[0] + " után pontosan egy feltétel állhat.");
                    }
                    return new Blokk(szavak[0] == "if" ? BlokkTipus.If : BlokkTipus.While, szavak[1]);

                default:
                    throw new InvalidOperationException("Ismeretlen utasítás: " + szavak[0]);
            }
        }
    }
}
