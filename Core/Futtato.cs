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

        public void Betoltes()
        {
            Beolvas.Beolvasas();
        }

        public void Inditas()
        {
            Inditas(Beolvas.ProgramLista);
        }

        public void Inditas(List<Sor> program)
        {
            Terkep = new Terkep();
            Terkep.SetTerkepMeret(Beolvas.TerkepSor, Beolvas.TerkepOszlop);
            Terkep.TerkepLista = Beolvas.TerkepLista;
            Terkep.ErtekMeghatarozas();
            Robot = new Robot(Terkep);
            Interpreter = new Interpreter(program, Robot);
        }

        public void NaploKiiras()
        {
            File.WriteAllLines(AppSettings.OutputFilePath, Interpreter.Naplo);
        }
    }
}
