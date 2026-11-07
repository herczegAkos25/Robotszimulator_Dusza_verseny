using Robotszimulator.Beolvasas;
using Robotszimulator.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Robotszimulator
{
    public class Interpreter
    {
        private List<Sor> program;
        private Dictionary<string, object> valtozok = new();
        private Robot robot;

        public List<string> Naplo { get; private set; } = new();
        public List<string> Konzol { get; private set; } = new();

        public List<Allapot> Allapotok { get; private set; } = new();
        public bool Nyomkovetes { get; set; }

        public IReadOnlyDictionary<string, object> Valtozok => valtozok;
        public string? HibaKod { get; private set; }
        public int HibaSor { get; private set; }

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
            Pillanatkep(-1);

            try
            {
                BlokkVegrehajtas(0, program.Count);
            }
            finally
            {
                if (!limitElerve)
                {
                    Naplo.Add("EOR");
                }
            }
        }

        private void HibaRogzites(int sorIndex, string kod)
        {
            HibaKod = kod;
            HibaSor = sorIndex + 1;
            leallitva = true;
        }

        private bool KeresesElott(int sorIndex)
        {
            if (utasitasSzamlalo >= 1000)
            {
                Naplo.Add("INSTRUCTION_COUNT_LIMIT_EXCEEDED");
                HibaRogzites(sorIndex, "INSTRUCTION_COUNT_LIMIT_EXCEEDED");
                limitElerve = true;
                Pillanatkep(sorIndex);
                return false;
            }
            utasitasSzamlalo++;
            return true;
        }

        private int BlokkVege(int honnan, int behuzas)
        {
            int j = honnan;
            while (j < program.Count && program[j].szokoz > behuzas)
            {
                j++;
            }
            return j;
        }

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
                        if (KeresesElott(thenEnd))
                        {
                            Naplo.Add((thenEnd + 1).ToString());
                            Pillanatkep(thenEnd);
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

        private bool FeltetelKiertekeles(int i)
        {
            if (!KeresesElott(i)) return false;
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
                HibaRogzites(i, e.Kod);
            }

            Naplo.Add(sorBejegyzes);
            Pillanatkep(i);
            return eredmeny;
        }

        private void EgyszeruUtasitasVegrehajtas(int i)
        {
            if (!KeresesElott(i)) return;

            string[] t = program[i].szavak;
            string sorBejegyzes = (i + 1).ToString();

            try
            {
                switch (t[0])
                {
                    case "print":
                        sorBejegyzes += " print";
                        string kiirandoErtek = Megjelenit(Olvas(t[1]));
                        sorBejegyzes += " " + kiirandoErtek;
                        Konzol.Add(kiirandoErtek);
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

                        if (valtozok.ContainsKey(t[1]) && valtozok[t[1]].GetType() != ertek.GetType())
                            throw new FutasHiba("E_TYPE_MISMATCH");

                        valtozok[t[1]] = ertek;
                        break;
                }
            }
            catch (FutasHiba e)
            {
                sorBejegyzes += " " + e.Kod;
                HibaRogzites(i, e.Kod);
            }

            Naplo.Add(sorBejegyzes);
            Pillanatkep(i);
        }

        private object BinarisMuveletKiertekeles(string a, string op, string b)
        {
            bool aritmetikai = op == "+" || op == "-" || op == "*";
            bool osszehasonlito = op == "<" || op == "<=" || op == ">" || op == ">=";
            bool logikai = op == "and" || op == "or";

            object x = Olvas(a);
            if ((aritmetikai || osszehasonlito) && !(x is int)) throw new FutasHiba("E_BAD_OPERAND");
            if (logikai && !(x is bool)) throw new FutasHiba("E_BAD_OPERAND");

            object y = Olvas(b);
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

        private object Olvas(string token)
        {
            if (token == "true") return true;
            if (token == "false") return false;
            if (char.IsDigit(token[0]) && int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int szamErtek))
            {
                return szamErtek;
            }
            if (valtozok.ContainsKey(token)) return valtozok[token];

            throw new FutasHiba("E_UNINITIALIZED");
        }

        private static string Megjelenit(object ertek)
        {
            if (ertek is bool b) return b ? "true" : "false";
            if (ertek is int szam) return szam.ToString(CultureInfo.InvariantCulture);
            return ertek.ToString() ?? string.Empty;
        }

        private void Pillanatkep(int sorIndex)
        {
            if (!Nyomkovetes) return;

            Allapotok.Add(new Allapot
            {
                Sor = sorIndex + 1,
                Valtozok = new Dictionary<string, object>(valtozok),
                RobotSor = robot.Sor,
                RobotOszlop = robot.Oszlop,
                Irany = robot.AktualisIrany,
                Felfedezett = (bool[,])robot.Felfedezett.Clone(),
                Grid = robot.Grid.Select(sor => (string[])sor.Clone()).ToArray(),
                Arany = robot.KibanyaszottAranySzam,
                Szenzor = robot.Szenzor(),
                NaploHossz = Naplo.Count,
                KonzolHossz = Konzol.Count,
                HibaKod = HibaKod
            });
        }
    }
}