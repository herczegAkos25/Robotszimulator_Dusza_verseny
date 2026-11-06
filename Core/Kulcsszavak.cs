using System;
using System.Collections.Generic;
using System.Linq;

namespace Robotszimulator.Core
{
    public static class Kulcsszavak
    {
        public static readonly HashSet<string> Lista = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "and", "by", "else", "false", "forward", "here", "if",
            "isThere", "mine", "not", "or", "print", "set", "step",
            "to", "true", "turn", "while"
        };

        public static bool IsKulcsszo(string nev)
        {
            if (string.IsNullOrWhiteSpace(nev)) return false;
            return Lista.Contains(nev);
        }

        public static bool ErvenyesAzonosito(string s) =>
        !string.IsNullOrEmpty(s)
        && char.IsAsciiLetter(s[0])
        && s.All(char.IsAsciiLetterOrDigit)
        && !IsKulcsszo(s);
    }
}