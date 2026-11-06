using System;
using System.Collections.Generic;
using System.Drawing;

namespace Robotszimulator.Core
{
    public enum Irany
    {
        Kelet,
        Eszak,
        Nyugat,
        Del
    }

    public class Robot
    {
        public int Oszlop { get; private set; }
        public int Sor { get; private set; }
        public Irany AktualisIrany { get; private set; }
        public int KibanyaszottAranySzam { get; private set; }
        private Terkep terkep;
        private string[][] aktualisGrid;

        public Robot(Terkep terkep)
        {
            this.terkep = terkep;
            InicializalasKezdoTerkepbol();
        }

        public void InicializalasKezdoTerkepbol()
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
                            case "^": AktualisIrany = Irany.Eszak; break;
                            case "<": AktualisIrany = Irany.Nyugat; break;
                            case "v": AktualisIrany = Irany.Del; break;
                        }

                        terkep.MezoFelfedezve(oszlopIndex, sorIndex);
                        aktualisGrid[sorIndex][oszlopIndex] = ".";
                    }
                }
            }
            KibanyaszottAranySzam = 0;
        }

        public void Turn_by(int negyedfordulatSzam)
        {
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
                    case Irany.Eszak: kovetkezoSor--; break;
                    case Irany.Del: kovetkezoSor++; break;
                }

                if (!terkep.BelulVan(kovetkezoOszlop, kovetkezoSor))
                {
                    throw new FutasHiba("E_BEDROCK_HIT");
                }

                if (aktualisGrid[kovetkezoSor][kovetkezoOszlop] == "#")
                {
                    throw new FutasHiba("E_BEDROCK_HIT");
                }

                Oszlop = kovetkezoOszlop;
                Sor = kovetkezoSor;
                terkep.MezoFelfedezve(Oszlop, Sor);
            }
        }

        public void Mine_here()
        {
            if (aktualisGrid[Sor][Oszlop] == "@")
            {
                aktualisGrid[Sor][Oszlop] = ".";
                KibanyaszottAranySzam++;
            }
            else
            {
                throw new FutasHiba("E_NO_GOLD");
            }
        }

        public bool isThere(int objektumTipus)
        {
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

        private (int, int) CelKoordinataKiszamitas()
        {
            int celOszlop = Oszlop;
            int celSor = Sor;

            switch (AktualisIrany)
            {
                case Irany.Kelet: celOszlop++; break;
                case Irany.Nyugat: celOszlop--; break;
                case Irany.Eszak: celSor--; break;
                case Irany.Del: celSor++; break;
            }
            return (celOszlop, celSor);
        }

        private bool CellaVizsgalat(int oszlop, int sor, int objektumTipus)
        {
            string celCella = aktualisGrid[sor][oszlop];

            return objektumTipus switch
            {
                0 => celCella == ".",
                1 => celCella == "#",
                2 => celCella == "@",
                _ => throw new FutasHiba("E_BAD_ARG")
            };
        }
    }
}