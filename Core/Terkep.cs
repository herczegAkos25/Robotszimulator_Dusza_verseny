using System;
using System.Collections.Generic;
using System.Drawing;

namespace Robotszimulator.Core
{
    public class Terkep
    {
        public List<string> TerkepLista { get; set; } = new List<string>();
        public int TerkepSor { get; private set; }
        public int TerkepOszlop { get; private set; }

        public List<Point> Akadalyok { get; } = new List<Point>();
        public List<Point> Arany { get; } = new List<Point>();
        public Point Robothelye { get; set; }

        public string[][] Grid { get; private set; }
        public bool[,] Felfedezett { get; private set; }

        public void SetTerkepMeret(int sor, int oszlop)
        {
            TerkepSor = sor;
            TerkepOszlop = oszlop;
        }

        public void ErtekMeghatarozas()
        {
            Akadalyok.Clear();
            Arany.Clear();
            Grid = new string[TerkepSor][];
            Felfedezett = new bool[TerkepSor, TerkepOszlop];

            for (int sor = 0; sor < TerkepSor; sor++)
            {
                Grid[sor] = new string[TerkepOszlop];
                string aktualisSor = TerkepLista[sor];

                for (int oszlop = 0; oszlop < TerkepOszlop; oszlop++)
                {
                    char karakter = aktualisSor[oszlop];
                    Grid[sor][oszlop] = karakter.ToString();

                    Point p = new Point(oszlop, sor);

                    switch (karakter)
                    {
                        case '#':
                            Akadalyok.Add(p);
                            break;
                        case '@':
                            Arany.Add(p);
                            break;
                        case '<':
                        case '>':
                        case '^':
                        case 'v':
                            Robothelye = p;
                            Felfedezett[sor, oszlop] = true;
                            break;
                    }
                }
            }
        }

        public bool BelulVan(int oszlop, int sor)
        {
            return sor >= 0 && sor < TerkepSor && oszlop >= 0 && oszlop < TerkepOszlop;
        }
    }
}