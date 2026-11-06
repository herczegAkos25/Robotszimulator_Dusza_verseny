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
        private bool[,] felfedezettMezok;
        private string[][] aktualisGrid;

        public Robot(Terkep terkep)
        {
            this.terkep = terkep;
            InicializalasKezdoTerkepbol();
        }

        public void InicializalasKezdoTerkepbol()
        {
            aktualisGrid = terkep.Grid;
            felfedezettMezok = new bool[terkep.TerkepSor, terkep.TerkepOszlop];

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

                        felfedezettMezok[sorIndex, oszlopIndex] = true;
                        aktualisGrid[sorIndex][oszlopIndex] = ".";
                    }
                }
            }
            KibanyaszottAranySzam = 0;
        }

        public void turn_by(int negyedfordulatSzam)
        {
            int fordulat = negyedfordulatSzam % 4;
            int jelenlegiIranySzam = (int)AktualisIrany;
            jelenlegiIranySzam = (jelenlegiIranySzam + fordulat) % 4;
            AktualisIrany = (Irany)jelenlegiIranySzam;
        }

        public void step_forward(int tavolsag)
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

                if (kovetkezoSor < 0 || kovetkezoSor >= terkep.TerkepSor || kovetkezoOszlop < 0 || kovetkezoOszlop >= terkep.TerkepOszlop)
                {
                    throw new FutasiHiba("E_BEDROCK_HIT");
                }

                if (aktualisGrid[kovetkezoSor][kovetkezoOszlop] == "#")
                {
                    throw new FutasiHiba("E_BEDROCK_HIT");
                }

                Oszlop = kovetkezoOszlop;
                Sor = kovetkezoSor;
                felfedezettMezok[Sor, Oszlop] = true;
            }
        }

        public void mine_here()
        {
            if (aktualisGrid[Sor][Oszlop] == "@")
            {
                aktualisGrid[Sor][Oszlop] = ".";
                KibanyaszottAranySzam++;
            }
            else
            {
                throw new FutasiHiba("E_NO_GOLD");
            }
        }

        public bool isThere(int objektumTipus)
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

            if (celSor < 0 || celSor >= terkep.TerkepSor || celOszlop < 0 || celOszlop >= terkep.TerkepOszlop)
            {
                return objektumTipus == 1;
            }

            felfedezettMezok[celSor, celOszlop] = true;

            string celCella = aktualisGrid[celSor][celOszlop];

            return objektumTipus switch
            {
                0 => celCella == ".",
                1 => celCella == "#",
                2 => celCella == "@",
                _ => throw new FutasiHiba("E_BAD_ARG")
            };
        }

        public bool VanMezoFelfedezve(int sorIndex, int oszlopIndex)
        {
            return felfedezettMezok[sorIndex, oszlopIndex];
        }
    }
}