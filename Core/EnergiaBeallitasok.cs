using System;
using System.Collections.Generic;

namespace Robotszimulator.Core
{
    public class EnergiaBeallitasok
    {
        public int Kapacitas { get; set; } = 10000;
        public int FuroFejElettartam { get; set; } = 20;

        public int LepesAra { get; set; } = 0;
        public int FordulasAra { get; set; } = 0;
        public int BanyaszatAra { get; set; } = 0;
        public int IsThereAra { get; set; } = 0;
        public int FurasAra { get; set; } = 500;
        public int TamaszAra { get; set; } = 300;
        public int TamaszFelvetelAra { get; set; } = 300;
        public int ToltoLerakasAra { get; set; } = 1000;
        public int ToltoFelvetelAra { get; set; } = 0;
        public int FuroFejAra { get; set; } = 1000;
        public int GetTypeAra { get; set; } = 0;
        public int GetHereAra { get; set; } = 0;
        public int GetEnergyAra { get; set; } = 0;
        public int GetDurabilityAra { get; set; } = 0;

        public int Toltes { get; set; } = 5000;

        public EnergiaBeallitasok Masolat()
        {
            return (EnergiaBeallitasok)MemberwiseClone();
        }

        public string? Hiba()
        {
            if (Kapacitas < 1) return "Az akkumulátor kapacitása legalább 1 legyen.";
            if (FuroFejElettartam < 1) return "A fúrófej élettartama legalább 1 legyen.";

            var arak = new Dictionary<string, int>
            {
                { "lépés", LepesAra },
                { "fordulás", FordulasAra },
                { "mine here", BanyaszatAra },
                { "isThere", IsThereAra },
                { "drill", FurasAra },
                { "support here", TamaszAra },
                { "unsupport here", TamaszFelvetelAra },
                { "place charger", ToltoLerakasAra },
                { "remove charger", ToltoFelvetelAra },
                { "buy drill", FuroFejAra },
                { "get type", GetTypeAra },
                { "get here", GetHereAra },
                { "get energy", GetEnergyAra },
                { "get durability", GetDurabilityAra }
            };

            foreach (var ar in arak)
            {
                if (ar.Value < 0) return "A(z) " + ar.Key + " fogyasztása nem lehet negatív.";
                if ((long)ar.Value * 2 > Kapacitas) return "A(z) " + ar.Key + " fogyasztása legfeljebb a kapacitás fele lehet.";
            }

            if (Toltes < 0) return "A töltés nem lehet negatív.";
            if ((long)Toltes * 5 > (long)Kapacitas * 4) return "A töltés legfeljebb a kapacitás 80%-a lehet.";

            return null;
        }
    }
}
