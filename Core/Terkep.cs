using System;
using System.Collections.Generic;
using System.Drawing;

namespace Robotszimulator.Core
{
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
            public char RobotIrany { get; private set; } // '<', '>', '^', 'v'

            // A térkép rácsa (kőzet, arany, üres mező) és a felfedezett mezők mátrixa
            public char[,] Grid { get; private set; }
            public bool[,] Felfedezett { get; private set; }

            public void SetTerkepMeret(int sor, int oszlop)
            {
                TerkepSor = sor;
                TerkepOszlop = oszlop;
            }

            public void ErtekMeghatarozas()
            {
                Grid = new char[TerkepSor, TerkepOszlop];
                Felfedezett = new bool[TerkepSor, TerkepOszlop];

                for (int sor = 0; sor < TerkepSor; sor++)
                {
                    string aktualisSor = TerkepLista[sor];

                    for (int oszlop = 0; oszlop < TerkepOszlop; oszlop++)
                    {
                        char karakter = aktualisSor[oszlop];
                        Grid[sor, oszlop] = karakter;

                        // Point(X, Y) -> Point(Oszlop, Sor)
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
                                RobotIrany = karakter;
                                // A robot indulási mezője automatikusan felfedezetté válik
                                Felfedezett[sor, oszlop] = true;
                                break;
                        }
                    }
                }
            }

            // Segédmetódus: ellenőrzi, hogy a megadott koordináta a bányaterületen belül van-e
            public bool BelulVan(int oszlop, int sor)
            {
                return sor >= 0 && sor < TerkepSor && oszlop >= 0 && oszlop < TerkepOszlop;
            }

            // Segédmetódus: mező felfedezetté jelölése
            public void MezoFelfedezve(int oszlop, int sor)
            {
                if (BelulVan(oszlop, sor))
                {
                    Felfedezett[sor, oszlop] = true;
                }
            }
        }
    }
}