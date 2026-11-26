using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Robotszimulator.Core
{
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

        public string Kulcsszo
        {
            get
            {
                if (ElseSor) return "else";

                switch (Blokk.Tipus)
                {
                    case BlokkTipus.Set: return "set";
                    case BlokkTipus.StepForward: return "step forward";
                    case BlokkTipus.TurnBy: return "turn by";
                    case BlokkTipus.MineHere: return "mine here";
                    case BlokkTipus.Print: return "print";
                    case BlokkTipus.If: return "if";
                    case BlokkTipus.While: return "while";
                    case BlokkTipus.Drill: return "drill";
                    case BlokkTipus.SupportHere: return "support here";
                    case BlokkTipus.UnsupportHere: return "unsupport here";
                    case BlokkTipus.PlaceCharger: return "place charger";
                    case BlokkTipus.RemoveCharger: return "remove charger";
                    case BlokkTipus.Charge: return "charge";
                    case BlokkTipus.BuyDrill: return "buy drill";
                    case BlokkTipus.ListDecl: return "list";
                    case BlokkTipus.Push: return "push";
                    case BlokkTipus.Put: return "put";
                    case BlokkTipus.Delete: return "delete";
                    default: return string.Empty;
                }
            }
        }

        // A kulcsszó utáni rész egyben, ez jelenik meg "chipként" a sorban (pl. "db < 6").
        public string Parameterek
        {
            get
            {
                if (ElseSor) return string.Empty;

                string teljes = Blokk.ToString();
                string kulcs = Kulcsszo;
                return teljes.Length > kulcs.Length ? teljes.Substring(kulcs.Length).Trim() : string.Empty;
            }
        }

        public bool VanParameter
        {
            get { return Parameterek.Length > 0; }
        }

        public bool Szerkesztheto
        {
            get { return !ElseSor && Blokk.VanParametere(Blokk.Tipus); }
        }

        public double BalMargo
        {
            get { return (Szint - 1) * 20; }
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