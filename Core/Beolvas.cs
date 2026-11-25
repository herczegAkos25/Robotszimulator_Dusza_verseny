using Robotszimulator.Beolvasas;
using Robotszimulator.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Robotszimulator
{
    public class Beolvas
    {
        public int TerkepSor { get; private set; }
        public int TerkepOszlop { get; private set; }
        public List<string> TerkepLista { get; private set; } = new List<string>();
        public List<Sor> ProgramLista { get; private set; } = new();

        public void Beolvasas()
        {
            TerkepLista.Clear();
            ProgramLista.Clear();
            string bemenetFajl = AppSettings.InputFilePath;
            using (StreamReader sr = new StreamReader(bemenetFajl))
            {
                string elsoSor = sr.ReadLine();
                if (elsoSor != null)
                {
                    string[] bontas = elsoSor.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    TerkepSor = int.Parse(bontas[0]);
                    TerkepOszlop = int.Parse(bontas[1]);
                }

                for (int i = 0; i < TerkepSor; i++)
                {
                    string sor = sr.ReadLine();
                    if (sor != null)
                    {
                        TiltottJelEllenorzes(sor, i + 1);
                        TerkepLista.Add(sor);
                    }
                }

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        ProgramLista.Add(Sor.Parse(line));
                    }
                }
            }

        }

        private static void TiltottJelEllenorzes(string sor, int sorSzam)
        {
            foreach (char jel in sor)
            {
                if (jel == '%' || jel == '$' || jel == '=' || jel == '*')
                {
                    throw new FormatException("A térkép " + sorSzam + ". sora támaszt vagy töltőállomást (" + jel + ") tartalmaz, ezt a bemeneti térkép nem tartalmazhat.");
                }
            }
        }
    }
}
