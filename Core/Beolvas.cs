using Robotszimulator.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace Robotszimulator
{
    public class Beolvas
    {
        public Terkep Terkep { get; private set; } = new Terkep();
        public List<string> ProgramLista { get; private set; } = new List<string>();

        public void Beolvasas()
        {
            string bemenetFajl = AppSettings.InputFilePath;
            using (StreamReader sr = new StreamReader(bemenetFajl))
            {
                string elsoSor = sr.ReadLine();
                if (elsoSor != null)
                {
                    string[] bontas = elsoSor.Split(' ');
                    Terkep.SetTerkepMeret(int.Parse(bontas[0]), int.Parse(bontas[1]));
                }

                for (int i = 0; i < Terkep.TerkepSor; i++)
                {
                    string sor = sr.ReadLine();
                    if (sor != null)
                    {
                        Terkep.TerkepLista.Add(sor);
                    }
                }

                while (!sr.EndOfStream)
                {
                    string sor = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(sor))
                    {
                        ProgramLista.Add(sor);
                    }
                }
            }

            // Mátrix és pozíciók feltöltése a beolvasott adatokból
            Terkep.ErtekMeghatarozas();
        }
    }
}