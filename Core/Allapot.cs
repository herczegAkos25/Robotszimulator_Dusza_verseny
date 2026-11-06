using System;
using System.Collections.Generic;
using System.Text;

namespace Robotszimulator.Core
{
    public class Allapot
    {
        public required int Sor { get; init; }
        public required Dictionary<string, object> Valtozok { get; init; }
        public required int RobotSor { get; init; }
        public required int RobotOszlop { get; init; }
        public required Irany Irany { get; init; }
        public required bool[,] Felfedezett { get; init; }
        public required string[][] Grid { get; init; }
        public required int Arany { get; init; }
        public required int Szenzor { get; init; }
        public required int NaploHossz { get; init; }
        public required int KonzolHossz { get; init; }
        public string? HibaKod { get; init; }
    }
}
