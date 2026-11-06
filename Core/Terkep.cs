using System;
using System.Collections.Generic;
using System.Drawing;

namespace Robotszimulator.Core
{
    public class Terkep
    {
        public List<string> TerkepLista { get; private set; } = new List<string>();
        public int TerkepSor { get; private set; }
        public int TerkepOszlop { get; private set; }
        public List<Point> Akadalyok { get; } = new();
        public List<Point> Arany { get; } = new();
        public Point Robothelye { get; private set; }

        public string[][] Grid { get; private set; } = new string[0][];

        public void SetTerkepMeret(int sor, int oszlop)
        {
            TerkepSor = sor;
            TerkepOszlop = oszlop;
        }

        public void ErtekMeghatarozas()
        {
            Grid = new string[TerkepSor][];

            for (int sor = 0; sor < TerkepSor; sor++)
            {
                Grid[sor] = new string[TerkepOszlop];
                string currentLine = TerkepLista[sor];

                for (int oszlop = 0; oszlop < TerkepOszlop; oszlop++)
                {
                    char karakter = currentLine[oszlop];
                    Grid[sor][oszlop] = karakter.ToString();
                    Point p = new Point(sor, oszlop);

                    switch (karakter)
                    {
                        case '#': Akadalyok.Add(p); break;
                        case '@': Arany.Add(p); break;
                        case '<':
                        case '>':
                        case '^':
                        case 'v':
                            Robothelye = p;
                            break;
                    }
                }
            }
        }
    }
}