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
                    string[] bontas = elsoSor.Split(' ');
                    TerkepSor = int.Parse(bontas[0]);
                    TerkepOszlop = int.Parse(bontas[1]);
                }

                for (int i = 0; i < TerkepSor; i++)
                {
                    string sor = sr.ReadLine();
                    if (sor != null)
                    {
                        TerkepLista.Add(sor);
                    }
                }

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        ProgramLista.Add(Sor.Parse(line));
                    }
                }
            }
            
        }
    }
}