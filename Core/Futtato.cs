using Robotszimulator.Beolvasas;
using System.IO;

namespace Robotszimulator.Core
{
    public class Futtato
    {
        public Beolvas Beolvas { get; } = new Beolvas();
        public Terkep Terkep { get; private set; }
        public Robot Robot { get; private set; }
        public Interpreter Interpreter { get; private set; }
        public bool Nyomkovetes { get; set; }
        public List<Sor> Program { get; set; } = new();

        public void Betoltes()
        {
            Beolvas.Beolvasas();
            Program = new List<Sor>(Beolvas.ProgramLista);
        }

        public void Inditas()
        {
            Inditas(Program);
        }

        public void Inditas(List<Sor> program)
        {
            Terkep = new Terkep();
            Terkep.SetTerkepMeret(Beolvas.TerkepSor, Beolvas.TerkepOszlop);
            Terkep.TerkepLista = Beolvas.TerkepLista;
            Terkep.ErtekMeghatarozas();
            Robot = new Robot(Terkep);
            Interpreter = new Interpreter(program, Robot) { Nyomkovetes = Nyomkovetes };
        }

        public void KezziFordulas(int negyedfordulatSzam)
        {
            Robot.Turn_by(negyedfordulatSzam);
        }

        public string? KezziLepes(int tavolsag)
        {
            try
            {
                Robot.Step_forward(tavolsag);
                return null;
            }
            catch (FutasHiba hiba)
            {
                return hiba.Kod;
            }
        }

        public string? KezziBanyaszat()
        {
            try
            {
                Robot.Mine_here();
                return null;
            }
            catch (FutasHiba hiba)
            {
                return hiba.Kod;
            }
        }

        public void NaploKiiras()
        {
            File.WriteAllLines(AppSettings.OutputFilePath, Interpreter.Naplo);
        }
    }
}