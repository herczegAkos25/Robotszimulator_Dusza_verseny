using Robotszimulator.Beolvasas;

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
            Terkep = new Terkep();
            Terkep.SetTerkepMeret(Beolvas.TerkepSor, Beolvas.TerkepOszlop);
            Terkep.TerkepLista = Beolvas.TerkepLista;
            Terkep.ErtekMeghatarozas();
            Robot = new Robot(Terkep);
            Interpreter = new Interpreter(Beolvas.ProgramLista, Robot);
        }
    }
}