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
        private Dictionary<string, Lista> listak = new();
        private Robot robot;

        public List<string> Naplo { get; private set; } = new();
        public List<string> Konzol { get; private set; } = new();

        public List<Allapot> Allapotok { get; private set; } = new();
        public bool Nyomkovetes { get; set; }

        public IReadOnlyDictionary<string, object> Valtozok
        {
            get { return valtozok; }
        }

        public IReadOnlyDictionary<string, Lista> Listak
        {
            get { return listak; }
        }

        public string? HibaKod { get; private set; }
        public int HibaSor { get; private set; }

        private const int UtasitasLimit = 1000;
        private int utasitasSzamlalo = 0;
        private bool leallitva = false;
        private bool limitElerve = false;

        public Interpreter(List<Sor> program, Robot robot)
        {
            this.program = program;
            this.robot = robot;
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
            if (utasitasSzamlalo >= UtasitasLimit)
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
                    int blokkVege = BlokkVege(i + 1, behuzas);
                    while (!leallitva)
                    {
                        if (!FeltetelKiertekeles(i)) break;
                        BlokkVegrehajtas(i + 1, blokkVege);
                    }
                    i = blokkVege;
                }
                else if (parancs == "if")
                {
                    int igazAgVege = BlokkVege(i + 1, behuzas);
                    bool vanElse = igazAgVege < meddig && program[igazAgVege].szokoz == behuzas && program[igazAgVege].szavak[0] == "else";
                    int hamisAgVege = vanElse ? BlokkVege(igazAgVege + 1, behuzas) : igazAgVege;

                    bool feltetel = FeltetelKiertekeles(i);
                    if (!leallitva && feltetel)
                    {
                        BlokkVegrehajtas(i + 1, igazAgVege);
                    }
                    else if (!leallitva && vanElse)
                    {
                        if (KeresesElott(igazAgVege))
                        {
                            Naplo.Add((igazAgVege + 1).ToString());
                            Pillanatkep(igazAgVege);
                            BlokkVegrehajtas(igazAgVege + 1, hamisAgVege);
                        }
                    }
                    i = hamisAgVege;
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
                object feltetel = Olvas(program[i].szavak[1]);
                if (!(feltetel is bool)) throw new FutasHiba("E_NON_BOOL_CONDITION");
                eredmeny = (bool)feltetel;
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

            string[] szavak = program[i].szavak;
            string sorBejegyzes = (i + 1).ToString();

            try
            {
                switch (szavak[0])
                {
                    case "print":
                        sorBejegyzes += " print";
                        string kiirandoErtek = Megjelenit(Olvas(szavak[1]));
                        sorBejegyzes += " " + kiirandoErtek;
                        Konzol.Add(kiirandoErtek);
                        break;

                    case "turn":
                        sorBejegyzes += " turn by";
                        object fordulasSzam = Olvas(szavak[2]);
                        sorBejegyzes += " " + Megjelenit(fordulasSzam);
                        if (!(fordulasSzam is int) || (int)fordulasSzam < 0) throw new FutasHiba("E_BAD_ARG");
                        robot.Turn_by((int)fordulasSzam);
                        break;

                    case "step":
                        sorBejegyzes += " step forward";
                        object lepesSzam = Olvas(szavak[2]);
                        sorBejegyzes += " " + Megjelenit(lepesSzam);
                        if (!(lepesSzam is int) || (int)lepesSzam < 0) throw new FutasHiba("E_BAD_ARG");
                        robot.Step_forward((int)lepesSzam);
                        break;

                    case "mine":
                        sorBejegyzes += " mine here";
                        robot.Mine_here();
                        break;

                    case "drill":
                        sorBejegyzes += " drill";
                        robot.Fur();
                        break;

                    case "support":
                        sorBejegyzes += " support here";
                        robot.TamaszRak();
                        break;

                    case "unsupport":
                        sorBejegyzes += " unsupport here";
                        robot.TamaszFelvesz();
                        break;

                    case "place":
                        sorBejegyzes += " place charger";
                        robot.ToltoLerak();
                        break;

                    case "remove":
                        sorBejegyzes += " remove charger";
                        robot.ToltoFelvesz();
                        break;

                    case "charge":
                        sorBejegyzes += " charge";
                        int kapottEnergia = robot.Tolt();
                        sorBejegyzes += " " + kapottEnergia;
                        break;

                    case "buy":
                        sorBejegyzes += " buy drill";
                        robot.FuroFejetVesz();
                        break;

                    case "list":
                        if (szavak.Length != 3 || (szavak[2] != "int" && szavak[2] != "bool"))
                            throw new FutasHiba("E_TYPE_MISMATCH");
                        if (valtozok.ContainsKey(szavak[1]) || listak.ContainsKey(szavak[1]))
                            throw new FutasHiba("E_TYPE_MISMATCH");
                        listak[szavak[1]] = new Lista(szavak[2]);
                        break;

                    case "push":
                        sorBejegyzes += " push";
                        Lista hozzafuzottLista = ListaKeres(szavak[1]);
                        object hozzafuzottErtek = Olvas(szavak[2]);
                        sorBejegyzes += " " + Megjelenit(hozzafuzottErtek);
                        if (!hozzafuzottLista.Illik(hozzafuzottErtek)) throw new FutasHiba("E_TYPE_MISMATCH");
                        hozzafuzottLista.Elemek.Add(hozzafuzottErtek);
                        break;

                    case "put":
                        sorBejegyzes += " put";
                        Lista modositottLista = ListaKeres(szavak[1]);
                        object modositasIndex = Olvas(szavak[2]);
                        sorBejegyzes += " " + Megjelenit(modositasIndex);
                        if (!(modositasIndex is int)) throw new FutasHiba("E_BAD_ARG");
                        object ujErtek = Olvas(szavak[3]);
                        sorBejegyzes += " " + Megjelenit(ujErtek);
                        IndexEllenorzes(modositottLista, (int)modositasIndex);
                        if (!modositottLista.Illik(ujErtek)) throw new FutasHiba("E_TYPE_MISMATCH");
                        modositottLista.Elemek[(int)modositasIndex] = ujErtek;
                        break;

                    case "delete":
                        sorBejegyzes += " delete";
                        Lista torlesLista = ListaKeres(szavak[1]);
                        object torlesIndex = Olvas(szavak[2]);
                        sorBejegyzes += " " + Megjelenit(torlesIndex);
                        if (!(torlesIndex is int)) throw new FutasHiba("E_BAD_ARG");
                        IndexEllenorzes(torlesLista, (int)torlesIndex);
                        torlesLista.Elemek.RemoveAt((int)torlesIndex);
                        break;

                    case "set":
                        object ertek;
                        if (szavak.Length == 4)
                        {
                            ertek = Olvas(szavak[3]);
                        }
                        else if (szavak[3] == "isThere")
                        {
                            sorBejegyzes += " isThere";
                            object objektumTipus = Olvas(szavak[4]);
                            sorBejegyzes += " " + Megjelenit(objektumTipus);

                            if (!(objektumTipus is int) || (int)objektumTipus < 0 || (int)objektumTipus > 2)
                                throw new FutasHiba("E_BAD_ARG");

                            ertek = robot.isThere((int)objektumTipus);
                            sorBejegyzes += " " + Megjelenit(ertek);
                        }
                        else if (szavak[3] == "get")
                        {
                            ertek = GetErtek(szavak[4], ref sorBejegyzes);
                        }
                        else if (szavak[3] == "size" && szavak.Length == 5)
                        {
                            ertek = ListaKeres(szavak[4]).Elemek.Count;
                        }
                        else if (szavak[3] == "item" && szavak.Length == 6 && !Muvelet(szavak[4]))
                        {
                            Lista olvasottLista = ListaKeres(szavak[4]);
                            object olvasottIndex = Olvas(szavak[5]);
                            if (!(olvasottIndex is int)) throw new FutasHiba("E_BAD_ARG");
                            IndexEllenorzes(olvasottLista, (int)olvasottIndex);
                            ertek = olvasottLista.Elemek[(int)olvasottIndex];
                        }
                        else if (szavak[3] == "not")
                        {
                            object operandus = Olvas(szavak[4]);
                            if (!(operandus is bool)) throw new FutasHiba("E_BAD_OPERAND");
                            ertek = !(bool)operandus;
                        }
                        else
                        {
                            ertek = BinarisMuveletKiertekeles(szavak[3], szavak[4], szavak[5]);
                        }

                        if (listak.ContainsKey(szavak[1]))
                            throw new FutasHiba("E_TYPE_MISMATCH");

                        if (valtozok.ContainsKey(szavak[1]) && valtozok[szavak[1]].GetType() != ertek.GetType())
                            throw new FutasHiba("E_TYPE_MISMATCH");

                        valtozok[szavak[1]] = ertek;
                        break;
                }
            }
            catch (FutasHiba e)
            {
                sorBejegyzes += " " + e.Kod;
                if (e.Kod != "E_DRILL_WORN")
                {
                    HibaRogzites(i, e.Kod);
                }
            }

            Naplo.Add(sorBejegyzes);
            Pillanatkep(i);
        }

        private Lista ListaKeres(string nev)
        {
            if (!listak.ContainsKey(nev)) throw new FutasHiba("E_UNINITIALIZED");
            return listak[nev];
        }

        private static void IndexEllenorzes(Lista lista, int index)
        {
            if (index < 0 || index >= lista.Elemek.Count) throw new FutasHiba("E_INDEX_OUT_OF_RANGE");
        }

        private static bool Muvelet(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "=" || token == "<>"
                || token == "<" || token == "<=" || token == ">" || token == ">="
                || token == "and" || token == "or";
        }

        private object GetErtek(string nev, ref string sorBejegyzes)
        {
            object eredmeny;
            switch (nev)
            {
                case "type":
                    sorBejegyzes += " get type";
                    eredmeny = robot.ElotteLevoTipus();
                    break;
                case "here":
                    sorBejegyzes += " get here";
                    eredmeny = robot.SajatMezoTipus();
                    break;
                case "energy":
                    sorBejegyzes += " get energy";
                    eredmeny = robot.EnergiaLekerdezes();
                    break;
                default:
                    sorBejegyzes += " get durability";
                    eredmeny = robot.FuroFejLekerdezes();
                    break;
            }

            sorBejegyzes += " " + Megjelenit(eredmeny);
            return eredmeny;
        }

        private object BinarisMuveletKiertekeles(string balToken, string muvelet, string jobbToken)
        {
            bool aritmetikai = muvelet == "+" || muvelet == "-" || muvelet == "*";
            bool osszehasonlito = muvelet == "<" || muvelet == "<=" || muvelet == ">" || muvelet == ">=";
            bool logikai = muvelet == "and" || muvelet == "or";

            object bal = Olvas(balToken);
            if ((aritmetikai || osszehasonlito) && !(bal is int)) throw new FutasHiba("E_BAD_OPERAND");
            if (logikai && !(bal is bool)) throw new FutasHiba("E_BAD_OPERAND");

            object jobb = Olvas(jobbToken);
            if ((aritmetikai || osszehasonlito) && !(jobb is int)) throw new FutasHiba("E_BAD_OPERAND");
            if (logikai && !(jobb is bool)) throw new FutasHiba("E_BAD_OPERAND");

            if ((muvelet == "=" || muvelet == "<>") && bal.GetType() != jobb.GetType())
                throw new FutasHiba("E_BAD_OPERAND");

            switch (muvelet)
            {
                case "+": return (int)bal + (int)jobb;
                case "-": return (int)bal - (int)jobb;
                case "*": return (int)bal * (int)jobb;
                case "<": return (int)bal < (int)jobb;
                case "<=": return (int)bal <= (int)jobb;
                case ">": return (int)bal > (int)jobb;
                case ">=": return (int)bal >= (int)jobb;
                case "=": return bal.Equals(jobb);
                case "<>": return !bal.Equals(jobb);
                case "and": return (bool)bal && (bool)jobb;
                default: return (bool)bal || (bool)jobb;
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
            if (listak.ContainsKey(token)) throw new FutasHiba("E_TYPE_MISMATCH");

            throw new FutasHiba("E_UNINITIALIZED");
        }

        private static string Megjelenit(object ertek)
        {
            if (ertek is bool)
            {
                if ((bool)ertek) return "true";
                return "false";
            }

            return ((int)ertek).ToString(CultureInfo.InvariantCulture);
        }

        public List<string> EnergiaNaplo => robot.EnergiaNaplo;

        private void Pillanatkep(int sorIndex)
        {
            if (!Nyomkovetes) return;

            Allapotok.Add(new Allapot
            {
                Sor = sorIndex + 1,
                Valtozok = new Dictionary<string, object>(valtozok),
                Listak = listak.ToDictionary(lista => lista.Key, lista => lista.Value.Masolat()),
                RobotSor = robot.Sor,
                RobotOszlop = robot.Oszlop,
                Irany = robot.AktualisIrany,
                Felfedezett = (bool[,])robot.Felfedezett.Clone(),
                Grid = robot.Grid.Select(sor => (string[])sor.Clone()).ToArray(),
                Arany = robot.KibanyaszottAranySzam,
                Energia = robot.Energia,
                FuroFejMaradek = robot.FuroFejMaradek,
                Szenzor = robot.Szenzor(),
                NaploHossz = Naplo.Count,
                EnergiaNaploHossz = robot.EnergiaNaplo.Count,
                KonzolHossz = Konzol.Count,
                HibaKod = HibaKod
            });
        }
    }
}