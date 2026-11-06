using System;
using System.Collections.Generic;

namespace Robotszimulator.Core
{
    public class Terkep
    {
        public List<string> TerkepLista { get; set; } = new List<string>();
        public int TerkepSor { get; private set; }
        public int TerkepOszlop { get; private set; }

        public string[][] Grid { get; private set; }
        public bool[,] Felfedezett { get; private set; }

        public void SetTerkepMeret(int sor, int oszlop)
        {
            TerkepSor = sor;
            TerkepOszlop = oszlop;
        }

        public void ErtekMeghatarozas()
        {
            Grid = new string[TerkepSor][];
            Felfedezett = new bool[TerkepSor, TerkepOszlop];

            for (int sor = 0; sor < TerkepSor; sor++)
            {
                Grid[sor] = new string[TerkepOszlop];
                string aktualisSor = TerkepLista[sor];

                for (int oszlop = 0; oszlop < TerkepOszlop; oszlop++)
                {
                    Grid[sor][oszlop] = aktualisSor[oszlop].ToString();
                }
            }
        }

        public bool BelulVan(int oszlop, int sor)
        {
            return sor >= 0 && sor < TerkepSor && oszlop >= 0 && oszlop < TerkepOszlop;
        }

        public void MezoFelfedezve(int oszlop, int sor)
        {
            if (BelulVan(oszlop, sor))
            {
                Felfedezett[sor, oszlop] = true;
            }
        }
    }
}
