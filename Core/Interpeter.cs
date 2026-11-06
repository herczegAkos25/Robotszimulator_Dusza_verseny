using Robotszimulator.Beolvasas;
using Robotszimulator.Core;
using System;
using System.Collections.Generic;

namespace Robotszimulator
{
    public class Interpreter
    {
        private List<Sor> program;
        private Dictionary<string, object> valtozok = new Dictionary<string, object>();
        private Robot robot;

        public List<string> Naplo { get; private set; } = new List<string>();

        private int utasitasSzamlalo = 0;
        private bool leallitva = false;
        private bool limitElerve = false;

        public Interpreter(List<Sor> prog, Robot r)
        {
            program = prog;
            robot = r;
        }

        public void Futtatas()
        {
            BlokkVegrehajtas(0, program.Count);

            // Ha nem az 1000 soros limit miatt állt meg, a végére kerül az EOR
            if (!limitElerve)
            {
                Naplo.Add("EOR");
            }
        }

        // --- Korlát ellenőrzése (1000 sor) ---
        private bool KeresesElott()
        {
            if (utasitasSzamlalo >= 1000)
            {
                Naplo.Add("INSTRUCTION_COUNT_LIMIT_EXCEEDED");
                leallitva = true;
                limitElerve = true;
                return false;
            }
            utasitasSzamlalo++;
            return true;
        }

        // --- Blokk határainak keresése ---
        private int BlokkVege(int honnan, int behuzas)
        {
            int j = honnan;
            while (j < program.Count && program[j].szokoz > behuzas)
            {
                j++;
            }
            return j;
        }

        // --- Blokkok végrehajtása ---
        private void BlokkVegrehajtas(int honnan, int meddig)
        {
            int i = honnan;
            while (i < meddig && !leallitva)
            {
                string parancs = program[i].szavak[0];
                int behuzas = program[i].szokoz;

                if (parancs == "while")
                {
                    int end = BlokkVege(i + 1, behuzas);
                    while (!leallitva)
                    {
                        if (!FeltetelKiertekeles(i)) break;
                        BlokkVegrehajtas(i + 1, end);
                    }
                    i = end;
                }
                else if (parancs == "if")
                {
                    int thenEnd = BlokkVege(i + 1, behuzas);
                    bool vanElse = thenEnd < meddig && program[thenEnd].szokoz == behuzas && program[thenEnd].szavak[0] == "else";
                    int elseEnd = vanElse ? BlokkVege(thenEnd + 1, behuzas) : thenEnd;

                    bool feltetel = FeltetelKiertekeles(i);
                    if (!leallitva && feltetel)
                    {
                        BlokkVegrehajtas(i + 1, thenEnd);
                    }
                    else if (!leallitva && vanElse)
                    {
                        if (KeresesElott())
                        {
                            Naplo.Add((thenEnd + 1).ToString());
                            BlokkVegrehajtas(thenEnd + 1, elseEnd);
                        }
                    }
                    i = elseEnd;
                }
                else
                {
                    EgyszeruUtasitasVegrehajtas(i);
                    i++;
                }
            }
        }

        // --- Feltétel kiértékelése ---
        private bool FeltetelKiertekeles(int i)
        {
            if (!KeresesElott()) return false;
            string sorBejegyzes = (i + 1).ToString();
            bool eredmeny = false;

            try
            {
                object c = Olvas(program[i].szavak[1]);
                if (!(c is bool)) throw new FutasHiba("E_NON_BOOL_CONDITION");
                eredmeny = (bool)c;
            }
            catch (FutasHiba e)
            {
                sorBejegyzes += " " + e.Kod;
                leallitva = true;
            }

            Naplo.Add(sorBejegyzes);
            return eredmeny;
        }

        // --- Utasítások végrehajtása ---
        private void EgyszeruUtasitasVegrehajtas(int i)
        {
            if (!KeresesElott()) return;

            string[] t = program[i].szavak;
            string sorBejegyzes = (i + 1).ToString();

            try
            {
                switch (t[0])
                {
                    case "print":
                        sorBejegyzes += " print " + Megjelenit(Olvas(t[1]));
                        break;

                    case "turn":
                        sorBejegyzes += " turn by";
                        object nTurn = Olvas(t[2]);
                        sorBejegyzes += " " + Megjelenit(nTurn);
                        if (!(nTurn is int) || (int)nTurn < 0) throw new FutasHiba("E_BAD_ARG");
                        robot.Turn_by((int)nTurn);
                        break;

                    case "step":
                        sorBejegyzes += " step forward";
                        object nStep = Olvas(t[2]);
                        sorBejegyzes += " " + Megjelenit(nStep);
                        if (!(nStep is int) || (int)nStep < 0) throw new FutasHiba("E_BAD_ARG");
                        robot.Step_forward((int)nStep);
                        break;

                    case "mine":
                        sorBejegyzes += " mine here";
                        robot.Mine_here();
                        break;

                    case "set":
                        object ertek;
                        if (t.Length == 4)
                        {
                            ertek = Olvas(t[3]);
                        }
                        else if (t[3] == "isThere")
                        {
                            sorBejegyzes += " isThere";
                            object w = Olvas(t[4]);
                            sorBejegyzes += " " + Megjelenit(w);

                            if (!(w is int) || (int)w < 0 || (int)w > 2)
                                throw new FutasHiba("E_BAD_ARG");

                            ertek = robot.isThere((int)w);
                            sorBejegyzes += " " + Megjelenit(ertek);
                        }
                        else if (t[3] == "not")
                        {
                            object x = Olvas(t[4]);
                            if (!(x is bool)) throw new FutasHiba("E_BAD_OPERAND");
                            ertek = !(bool)x;
                        }
                        else
                        {
                            ertek = BinarisMuveletKiertekeles(t[3], t[4], t[5]);
                        }

                        // Típusváltás ellenőrzése
                        if (valtozok.ContainsKey(t[1]) && valtozok[t[1]].GetType() != ertek.GetType())
                            throw new FutasHiba("E_TYPE_MISMATCH");

                        valtozok[t[1]] = ertek;
                        break;
                }
            }
            catch (FutasHiba e)
            {
                sorBejegyzes += " " + e.Kod;
                leallitva = true;
            }

            Naplo.Add(sorBejegyzes);
        }

        // --- Kétoperandusú kifejezések kiértékelése ---
        private object BinarisMuveletKiertekeles(string a, string op, string b)
        {
            bool aritmetikai = op == "+" || op == "-" || op == "*";
            bool osszehasonlito = op == "<" || op == "<=" || op == ">" || op == ">=";
            bool logikai = op == "and" || op == "or";

            object x = Olvas(a);
            if ((aritmetikai || osszehasonlito) && !(x is int)) throw new FutasHiba("E_BAD_OPERAND");
            if (logikai && !(x is bool)) throw new FutasHiba("E_BAD_OPERAND");

            object y = Olvas(b); // A 2. operandust akkor is beolvassa, ha az 1. alapján már tudható az eredmény!
            if ((aritmetikai || osszehasonlito) && !(y is int)) throw new FutasHiba("E_BAD_OPERAND");
            if (logikai && !(y is bool)) throw new FutasHiba("E_BAD_OPERAND");

            if ((op == "=" || op == "<>") && x.GetType() != y.GetType())
                throw new FutasHiba("E_BAD_OPERAND");

            switch (op)
            {
                case "+": return (int)x + (int)y;
                case "-": return (int)x - (int)y;
                case "*": return (int)x * (int)y;
                case "<": return (int)x < (int)y;
                case "<=": return (int)x <= (int)y;
                case ">": return (int)x > (int)y;
                case ">=": return (int)x >= (int)y;
                case "=": return x.Equals(y);
                case "<>": return !x.Equals(y);
                case "and": return (bool)x && (bool)y;
                default: return (bool)x || (bool)y;
            }
        }

        // --- Token olvasása ---
        private object Olvas(string token)
        {
            if (token == "true") return true;
            if (token == "false") return false;
            if (char.IsDigit(token[0])) return int.Parse(token);
            if (valtozok.ContainsKey(token)) return valtozok[token];

            throw new FutasHiba("E_UNINITIALIZED");
        }

        // --- Megjelenítési formátum a naplóhoz ---
        private static string Megjelenit(object ertek)
        {
            if (ertek is bool b) return b ? "true" : "false";
            return ertek.ToString();
        }
    }
}