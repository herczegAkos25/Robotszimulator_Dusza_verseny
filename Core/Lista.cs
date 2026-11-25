using System;
using System.Collections.Generic;

namespace Robotszimulator.Core
{
    public class Lista
    {
        public string Tipus { get; }
        public List<object> Elemek { get; private set; } = new List<object>();

        public Lista(string tipus)
        {
            Tipus = tipus;
        }

        public bool Illik(object ertek)
        {
            if (Tipus == "int") return ertek is int;
            return ertek is bool;
        }

        public Lista Masolat()
        {
            var masolat = new Lista(Tipus);
            masolat.Elemek = new List<object>(Elemek);
            return masolat;
        }
    }
}
