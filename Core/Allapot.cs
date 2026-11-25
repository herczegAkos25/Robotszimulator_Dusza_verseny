using System;
using System.Collections.Generic;
using System.Text;

namespace Robotszimulator.Core
{
    public class Allapot
    {
        public int Sor { get; set; }
        public Dictionary<string, object> Valtozok { get; set; }
        public int RobotSor { get; set; }
        public int RobotOszlop { get; set; }
        public Irany Irany { get; set; }
        public bool[,] Felfedezett { get; set; }
        public string[][] Grid { get; set; }
        public int Arany { get; set; }
        public int Energia { get; set; }
        public Dictionary<string, Lista> Listak { get; set; }
        public int FuroFejMaradek { get; set; }
        public int Szenzor { get; set; }
        public int NaploHossz { get; set; }
        public int EnergiaNaploHossz { get; set; }
        public int KonzolHossz { get; set; }
        public string? HibaKod { get; set; }
    }
}
