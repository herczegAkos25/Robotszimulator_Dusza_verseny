using System;
using System.Collections.Generic;

namespace Robotszimulator.Core
{
    public enum Irany
    {
        Kelet,
        Észak,
        Nyugat,
        Dél
    }

    public class Robot
    {
        public int Oszlop { get; private set; }
        public int Sor { get; private set; }
        public Irany AktualisIrany { get; private set; }
        public int KibanyaszottAranySzam { get; private set; }
        public int Energia { get; private set; }
        public int FuroFejMaradek { get; private set; }
        public EnergiaBeallitasok Beallitasok { get; }
        private Terkep terkep;
        private string[][] aktualisGrid;
        public bool[,] Felfedezett => terkep.Felfedezett;
        public string[][] Grid => aktualisGrid;

        public Robot(Terkep terkep, EnergiaBeallitasok beallitasok)
        {
            this.terkep = terkep;
            Beallitasok = beallitasok;
            Energia = beallitasok.Kapacitas;
            FuroFejMaradek = beallitasok.FuroFejElettartam;
            InicializalasKezdoTerkepbol();
        }

        private void InicializalasKezdoTerkepbol()
        {
            aktualisGrid = terkep.Grid;

            for (int sorIndex = 0; sorIndex < terkep.TerkepSor; sorIndex++)
            {
                for (int oszlopIndex = 0; oszlopIndex < terkep.TerkepOszlop; oszlopIndex++)
                {
                    string cella = aktualisGrid[sorIndex][oszlopIndex];
                    if (cella == ">" || cella == "<" || cella == "^" || cella == "v")
                    {
                        Oszlop = oszlopIndex;
                        Sor = sorIndex;

                        switch (cella)
                        {
                            case ">": AktualisIrany = Irany.Kelet; break;
                            case "^": AktualisIrany = Irany.Észak; break;
                            case "<": AktualisIrany = Irany.Nyugat; break;
                            case "v": AktualisIrany = Irany.Dél; break;
                        }

                        terkep.MezoFelfedezve(oszlopIndex, sorIndex);
                        aktualisGrid[sorIndex][oszlopIndex] = "=";
                    }
                }
            }
            KibanyaszottAranySzam = 0;
        }

        public void AllapotBeallit(string[][] grid, bool[,] felfedezett, int sor, int oszlop, Irany irany, int arany, int energia, int furoFejMaradek)
        {
            terkep.Beallit(grid, felfedezett);
            aktualisGrid = terkep.Grid;
            Sor = sor;
            Oszlop = oszlop;
            AktualisIrany = irany;
            KibanyaszottAranySzam = arany;
            Energia = energia;
            FuroFejMaradek = furoFejMaradek;
        }

        private void EnergiaEllenorzes(long ar)
        {
            if (Energia < ar)
            {
                throw new FutasHiba("E_NO_ENERGY");
            }
        }

        private void Fogyaszt(long ar)
        {
            Energia -= (int)ar;
        }

        public void Turn_by(int negyedfordulatSzam)
        {
            long ar = (long)Beallitasok.FordulasAra * negyedfordulatSzam;
            EnergiaEllenorzes(ar);
            Fogyaszt(ar);

            int fordulat = negyedfordulatSzam % 4;
            int jelenlegiIranySzam = (int)AktualisIrany;
            jelenlegiIranySzam = (jelenlegiIranySzam + fordulat) % 4;
            AktualisIrany = (Irany)jelenlegiIranySzam;
        }

        public void Step_forward(int tavolsag)
        {
            for (int i = 0; i < tavolsag; i++)
            {
                int kovetkezoOszlop = Oszlop;
                int kovetkezoSor = Sor;

                switch (AktualisIrany)
                {
                    case Irany.Kelet: kovetkezoOszlop++; break;
                    case Irany.Nyugat: kovetkezoOszlop--; break;
                    case Irany.Észak: kovetkezoSor--; break;
                    case Irany.Dél: kovetkezoSor++; break;
                }

                EnergiaEllenorzes(Beallitasok.LepesAra);

                if (!terkep.BelulVan(kovetkezoOszlop, kovetkezoSor))
                {
                    throw new FutasHiba("E_BEDROCK_HIT");
                }

                if (Kozet(aktualisGrid[kovetkezoSor][kovetkezoOszlop]))
                {
                    throw new FutasHiba("E_BEDROCK_HIT");
                }

                Fogyaszt(Beallitasok.LepesAra);
                Oszlop = kovetkezoOszlop;
                Sor = kovetkezoSor;
                terkep.MezoFelfedezve(Oszlop, Sor);
            }
        }

        public void Mine_here()
        {
            EnergiaEllenorzes(Beallitasok.BanyaszatAra);

            string cella = aktualisGrid[Sor][Oszlop];
            if (cella == "@" || cella == "$")
            {
                Fogyaszt(Beallitasok.BanyaszatAra);
                aktualisGrid[Sor][Oszlop] = cella == "@" ? "." : "%";
                KibanyaszottAranySzam++;
            }
            else
            {
                throw new FutasHiba("E_NO_GOLD");
            }
        }

        public void ElotteFelfedezes()
        {
            var (celOszlop, celSor) = CelKoordinataKiszamitas();
            terkep.MezoFelfedezve(celOszlop, celSor);
        }

        public bool isThere(int objektumTipus)
        {
            EnergiaEllenorzes(Beallitasok.IsThereAra);
            Fogyaszt(Beallitasok.IsThereAra);

            var (celOszlop, celSor) = CelKoordinataKiszamitas();

            if (!terkep.BelulVan(celOszlop, celSor))
            {
                return objektumTipus == 1;
            }

            terkep.MezoFelfedezve(celOszlop, celSor);
            return CellaVizsgalat(celOszlop, celSor, objektumTipus);
        }

        public bool Szenzor(int objektumTipus)
        {
            var (celOszlop, celSor) = CelKoordinataKiszamitas();

            if (!terkep.BelulVan(celOszlop, celSor))
            {
                return objektumTipus == 1;
            }

            return CellaVizsgalat(celOszlop, celSor, objektumTipus);
        }

        public int Szenzor()
        {
            var (celOszlop, celSor) = CelKoordinataKiszamitas();

            if (!terkep.BelulVan(celOszlop, celSor))
            {
                return 1;
            }

            string celCella = aktualisGrid[celSor][celOszlop];

            if (Kozet(celCella)) return 1;
            if (celCella == "@" || celCella == "$") return 2;
            return 0;
        }

        private (int, int) CelKoordinataKiszamitas()
        {
            int celOszlop = Oszlop;
            int celSor = Sor;

            switch (AktualisIrany)
            {
                case Irany.Kelet: celOszlop++; break;
                case Irany.Nyugat: celOszlop--; break;
                case Irany.Észak: celSor--; break;
                case Irany.Dél: celSor++; break;
            }
            return (celOszlop, celSor);
        }

        private bool CellaVizsgalat(int oszlop, int sor, int objektumTipus)
        {
            string celCella = aktualisGrid[sor][oszlop];

            switch (objektumTipus)
            {
                case 0: return Kamra(celCella);
                case 1: return Kozet(celCella);
                case 2: return celCella == "@" || celCella == "$";
                default: throw new FutasHiba("E_BAD_ARG");
            }
        }

        private static bool Kozet(string cella)
        {
            return cella == "#" || cella == "+" || cella == "&";
        }

        private static bool Kamra(string cella)
        {
            return cella == "." || cella == "%" || cella == "=" || cella == "*";
        }

        private static bool VanTamasz(string cella)
        {
            return cella == "%" || cella == "$" || cella == "*";
        }

        private static bool VanToltoallomas(string cella)
        {
            return cella == "=" || cella == "*";
        }

        private bool UresMezo(int oszlop, int sor)
        {
            if (!terkep.BelulVan(oszlop, sor)) return false;

            string cella = aktualisGrid[sor][oszlop];
            return cella == "." || cella == "@" || cella == "=";
        }

        private int UresSzomszedokSzama()
        {
            int darab = 0;

            for (int sorEltolas = -1; sorEltolas <= 1; sorEltolas++)
            {
                for (int oszlopEltolas = -1; oszlopEltolas <= 1; oszlopEltolas++)
                {
                    if (sorEltolas == 0 && oszlopEltolas == 0) continue;

                    if (UresMezo(Oszlop + oszlopEltolas, Sor + sorEltolas))
                    {
                        darab++;
                    }
                }
            }
            return darab;
        }

        public void Fur()
        {
            EnergiaEllenorzes(Beallitasok.FurasAra);

            if (FuroFejMaradek == 0)
            {
                throw new FutasHiba("E_DRILL_WORN");
            }

            var (celOszlop, celSor) = CelKoordinataKiszamitas();
            if (!terkep.BelulVan(celOszlop, celSor))
            {
                throw new FutasHiba("E_CANNOT_DRILL");
            }

            string celCella = aktualisGrid[celSor][celOszlop];
            if (celCella != "+" && celCella != "&")
            {
                throw new FutasHiba("E_CANNOT_DRILL");
            }

            Fogyaszt(Beallitasok.FurasAra);
            FuroFejMaradek--;
            aktualisGrid[celSor][celOszlop] = celCella == "&" ? "@" : ".";
            Oszlop = celOszlop;
            Sor = celSor;
            terkep.MezoFelfedezve(Oszlop, Sor);

            if (UresSzomszedokSzama() >= 6)
            {
                throw new FutasHiba("E_COLLAPSE");
            }
        }

        public void TamaszRak()
        {
            EnergiaEllenorzes(Beallitasok.TamaszAra);

            string cella = aktualisGrid[Sor][Oszlop];
            if (VanTamasz(cella))
            {
                throw new FutasHiba("E_CANNOT_SUPPORT");
            }

            Fogyaszt(Beallitasok.TamaszAra);
            switch (cella)
            {
                case ".": aktualisGrid[Sor][Oszlop] = "%"; break;
                case "@": aktualisGrid[Sor][Oszlop] = "$"; break;
                case "=": aktualisGrid[Sor][Oszlop] = "*"; break;
            }
        }

        public void TamaszFelvesz()
        {
            EnergiaEllenorzes(Beallitasok.TamaszFelvetelAra);

            string cella = aktualisGrid[Sor][Oszlop];
            if (!VanTamasz(cella))
            {
                throw new FutasHiba("E_CANNOT_UNSUPPORT");
            }

            Fogyaszt(Beallitasok.TamaszFelvetelAra);
            switch (cella)
            {
                case "%": aktualisGrid[Sor][Oszlop] = "."; break;
                case "$": aktualisGrid[Sor][Oszlop] = "@"; break;
                case "*": aktualisGrid[Sor][Oszlop] = "="; break;
            }
        }

        public void ToltoLerak()
        {
            EnergiaEllenorzes(Beallitasok.ToltoLerakasAra);

            string cella = aktualisGrid[Sor][Oszlop];
            if (cella != "." && cella != "%")
            {
                throw new FutasHiba("E_CANNOT_PLACE_CHARGER");
            }

            Fogyaszt(Beallitasok.ToltoLerakasAra);
            aktualisGrid[Sor][Oszlop] = cella == "." ? "=" : "*";
        }

        public void ToltoFelvesz()
        {
            EnergiaEllenorzes(Beallitasok.ToltoFelvetelAra);

            string cella = aktualisGrid[Sor][Oszlop];
            if (!VanToltoallomas(cella))
            {
                throw new FutasHiba("E_CANNOT_REMOVE_CHARGER");
            }

            Fogyaszt(Beallitasok.ToltoFelvetelAra);
            aktualisGrid[Sor][Oszlop] = cella == "=" ? "." : "%";
        }

        public int Tolt()
        {
            if (!VanToltoallomas(aktualisGrid[Sor][Oszlop]))
            {
                throw new FutasHiba("E_CANNOT_CHARGE");
            }

            if ((long)Energia + Beallitasok.Toltes > Beallitasok.Kapacitas)
            {
                throw new FutasHiba("E_OVER_CHARGE");
            }

            Energia += Beallitasok.Toltes;
            return Beallitasok.Toltes;
        }

        public void FuroFejetVesz()
        {
            EnergiaEllenorzes(Beallitasok.FuroFejAra);

            if (!VanToltoallomas(aktualisGrid[Sor][Oszlop]))
            {
                throw new FutasHiba("E_CANNOT_BUY_DRILL");
            }

            Fogyaszt(Beallitasok.FuroFejAra);
            FuroFejMaradek = Beallitasok.FuroFejElettartam;
        }

        public int ElotteLevoTipus()
        {
            EnergiaEllenorzes(Beallitasok.GetTypeAra);
            Fogyaszt(Beallitasok.GetTypeAra);

            var (celOszlop, celSor) = CelKoordinataKiszamitas();
            if (!terkep.BelulVan(celOszlop, celSor))
            {
                return 1;
            }

            string celCella = aktualisGrid[celSor][celOszlop];
            if (celCella == "#") return 1;
            if (celCella == "+") return 2;
            if (celCella == "&") return 3;
            return 0;
        }

        public int SajatMezoTipus()
        {
            EnergiaEllenorzes(Beallitasok.GetHereAra);
            Fogyaszt(Beallitasok.GetHereAra);

            switch (aktualisGrid[Sor][Oszlop])
            {
                case "@": return 1;
                case "%": return 2;
                case "$": return 3;
                case "=": return 4;
                case "*": return 5;
                default: return 0;
            }
        }

        public int EnergiaLekerdezes()
        {
            EnergiaEllenorzes(Beallitasok.GetEnergyAra);
            Fogyaszt(Beallitasok.GetEnergyAra);
            return Energia;
        }

        public int FuroFejLekerdezes()
        {
            EnergiaEllenorzes(Beallitasok.GetDurabilityAra);
            Fogyaszt(Beallitasok.GetDurabilityAra);
            return FuroFejMaradek;
        }
    }
}
