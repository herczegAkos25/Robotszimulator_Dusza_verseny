using System.ComponentModel;

namespace Robotszimulator.Core
{
    public record BlokkResz(string Szoveg, bool Mezo);

    public class BlokkSor : INotifyPropertyChanged
    {
        private string keret = "Transparent";

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Sorszam { get; }
        public Blokk Blokk { get; }
        public bool ElseSor { get; }
        public int Szint { get; }

        public BlokkSor(int sorszam, Blokk blokk, bool elseSor, int szint)
        {
            Sorszam = sorszam;
            Blokk = blokk;
            ElseSor = elseSor;
            Szint = szint;
        }

        public bool ErtekLathato
        {
            get { return !ElseSor && Blokk.Tipus == BlokkTipus.Set; }
        }

        public List<BlokkResz> Reszek
        {
            get
            {
                if (ElseSor) return new List<BlokkResz> { new BlokkResz("else", false) };

                string[] szavak = Blokk.Szavak;
                switch (Blokk.Tipus)
                {
                    case BlokkTipus.Set:
                        return new List<BlokkResz> { new BlokkResz("set", false), new BlokkResz(szavak[0], true), new BlokkResz("to", false) };
                    case BlokkTipus.StepForward:
                        return new List<BlokkResz> { new BlokkResz("step forward", false), new BlokkResz(szavak[0], true) };
                    case BlokkTipus.TurnBy:
                        return new List<BlokkResz> { new BlokkResz("turn by", false), new BlokkResz(szavak[0], true) };
                    case BlokkTipus.MineHere:
                        return new List<BlokkResz> { new BlokkResz("mine here", false) };
                    case BlokkTipus.Print:
                        return new List<BlokkResz> { new BlokkResz("print", false), new BlokkResz(szavak[0], true) };
                    case BlokkTipus.If:
                        return new List<BlokkResz> { new BlokkResz("if", false), new BlokkResz(szavak[0], true) };
                    default:
                        return new List<BlokkResz> { new BlokkResz("while", false), new BlokkResz(szavak[0], true) };
                }
            }
        }

        public List<BlokkResz> ErtekReszek
        {
            get
            {
                var reszek = new List<BlokkResz>();
                if (!ErtekLathato) return reszek;

                string[] jobbOldal = Blokk.Szavak.Skip(1).ToArray();
                if (jobbOldal.Length == 1)
                {
                    reszek.Add(new BlokkResz(jobbOldal[0], true));
                }
                else if (jobbOldal.Length == 2)
                {
                    reszek.Add(new BlokkResz(jobbOldal[0], false));
                    reszek.Add(new BlokkResz(jobbOldal[1], true));
                }
                else
                {
                    reszek.Add(new BlokkResz(jobbOldal[0], true));
                    reszek.Add(new BlokkResz(jobbOldal[1], false));
                    reszek.Add(new BlokkResz(jobbOldal[2], true));
                }
                return reszek;
            }
        }

        public double BalMargo
        {
            get { return (Szint - 1) * 22; }
        }

        public string Szin
        {
            get
            {
                if (ElseSor) return "#98C379";

                switch (Blokk.Tipus)
                {
                    case BlokkTipus.Set:
                        return "#61AFEF";
                    case BlokkTipus.If:
                    case BlokkTipus.While:
                        return "#98C379";
                    case BlokkTipus.Print:
                        return "#C678DD";
                    default:
                        return "#E5C07B";
                }
            }
        }

        public string Keret
        {
            get { return keret; }
            set
            {
                if (keret == value) return;

                keret = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keret)));
            }
        }
    }
}
