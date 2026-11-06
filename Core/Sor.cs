using System;
using System.Collections.Generic;
using System.Text;

namespace Robotszimulator.Beolvasas
{
    public class Sor
    {
        public int szokoz;
        public string[] szavak;

        public Sor(int szokoz, string[] szavak)
        {
            this.szokoz = szokoz;
            this.szavak = szavak;
        }

        public static Sor Parse(string line)
        {
            int szokoz = 0;
            foreach (var item in line)
            {
                if (item == ' ')
                {
                    szokoz++;

                }
                else
                {
                    break;
                }

            }
            return new Sor(szokoz, line.TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
