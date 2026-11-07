using System.ComponentModel;

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

        public string Szoveg
        {
            get { return ElseSor ? "else" : Blokk.ToString(); }
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
