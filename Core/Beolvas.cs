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
        public List<string> ProgramLista { get; private set; } = new List<string>();

        public void Beolvasas()
        {
            string inputFile = AppSettings.InputFilePath;
            using (StreamReader sr = new StreamReader(inputFile))
            {
                string firstLine = sr.ReadLine();
                if (firstLine != null)
                {
                    string[] parts = firstLine.Split(' ');
                    TerkepSor = int.Parse(parts[0]);
                    TerkepOszlop = int.Parse(parts[1]);
                }

                for (int i = 0; i < TerkepSor; i++)
                {
                    string line = sr.ReadLine();
                    if (line != null)
                    {
                        TerkepLista.Add(line);
                    }
                }

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        ProgramLista.Add(line);
                    }
                }
            }
        }
    }
}