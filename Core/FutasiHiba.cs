using System;
using System.Collections.Generic;
using System.Text;

namespace Robotszimulator.Core
{
    public class FutasHiba : Exception
    {
        public string Kod { get; }

        public FutasHiba(string kod)
        {
            Kod = kod;
        }
    }
}
