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
                    case BlokkTipus.Drill:
                        return new List<BlokkResz> { new BlokkResz("drill", false) };
                    case BlokkTipus.SupportHere:
                        return new List<BlokkResz> { new BlokkResz("support here", false) };
                    case BlokkTipus.UnsupportHere:
                        return new List<BlokkResz> { new BlokkResz("unsupport here", false) };
                    case BlokkTipus.PlaceCharger:
                        return new List<BlokkResz> { new BlokkResz("place charger", false) };
                    case BlokkTipus.RemoveCharger:
                        return new List<BlokkResz> { new BlokkResz("remove charger", false) };
                    case BlokkTipus.Charge:
                        return new List<BlokkResz> { new BlokkResz("charge", false) };
                    case BlokkTipus.BuyDrill:
                        return new List<BlokkResz> { new BlokkResz("buy drill", false) };
                    case BlokkTipus.ListDecl:
                        return new List<BlokkResz> { new BlokkResz("list", false), new BlokkResz(szavak[0], true), new BlokkResz(szavak[1], true) };
                    case BlokkTipus.Push:
                        return new List<BlokkResz> { new BlokkResz("push", false), new BlokkResz(szavak[0], true), new BlokkResz(szavak[1], true) };
                    case BlokkTipus.Put:
                        return new List<BlokkResz> { new BlokkResz("put", false), new BlokkResz(szavak[0], true), new BlokkResz(szavak[1], true), new BlokkResz(szavak[2], true) };
                    case BlokkTipus.Delete:
                        return new List<BlokkResz> { new BlokkResz("delete", false), new BlokkResz(szavak[0], true), new BlokkResz(szavak[1], true) };
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
                else if (jobbOldal.Length == 2 && jobbOldal[0] == "get")
                {
                    reszek.Add(new BlokkResz("get " + jobbOldal[1], false));
                }
                else if (jobbOldal.Length == 2)
                {
                    reszek.Add(new BlokkResz(jobbOldal[0], false));
                    reszek.Add(new BlokkResz(jobbOldal[1], true));
                }
                else if (jobbOldal[0] == "item" && !Blokk.MuveletE(jobbOldal[1]))
                {
                    reszek.Add(new BlokkResz("item", false));
                    reszek.Add(new BlokkResz(jobbOldal[1], true));
                    reszek.Add(new BlokkResz(jobbOldal[2], true));
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
                    case BlokkTipus.ListDecl:
                    case BlokkTipus.Push:
                    case BlokkTipus.Put:
                    case BlokkTipus.Delete:
                        return "#56B6C2";
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
