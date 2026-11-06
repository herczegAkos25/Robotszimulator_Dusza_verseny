using Robotszimulator.Beolvasas;
using Robotszimulator.Core;
using Robotszimulator.Core.Robotszimulator.Core;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace Robotszimulator
{
    /// <summary>
    /// Az App.xaml logikai kódja
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = e.Args;

            if (args.Length > 0 && args[0] == "-q")
            {
                AppSettings.IsQuietMode = true;
                if (args.Length >= 3)
                {
                    AppSettings.InputFilePath = args[1];
                    AppSettings.OutputFilePath = args[2];
                }
            }
            else if (args.Length >= 2)
            {
                AppSettings.IsQuietMode = false;
                AppSettings.InputFilePath = args[0];
                AppSettings.OutputFilePath = args[1];
            }

            if (AppSettings.IsQuietMode)
            {
                CsendesModFuttatas();
                Current.Shutdown();
            }
            else
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void CsendesModFuttatas()
        {
            // 1. Beolvasás elindítása
            // 1. Beolvasás elindítása
            Beolvas b = new Beolvas();
            b.Beolvasas(); // Beolvassa az AppSettings.InputFilePath fájlt

            // 2. A Terkep osztály feltöltése a beolvasott adatokkal
            Terkep terkep = new Terkep();
            terkep.SetTerkepMeret(b.TerkepSor, b.TerkepOszlop);
            terkep.TerkepLista = b.TerkepLista;
            terkep.ErtekMeghatarozas();

            // 3. A Robot példányosítása a felkészített térképpel
            Robot robot = new Robot(terkep);

            // 4. Az Interpreter (értelmező) példányosítása a beolvasott programkóddal és a robottal
            Interpreter interpreter = new Interpreter(b.ProgramLista, robot);

            // 5. A MineScript program lefuttatása
            interpreter.Futtatas();

            // 6. A keletkezett napló kiírása a kimeneti fájlba
            File.WriteAllLines(AppSettings.OutputFilePath, interpreter.Naplo);
        }
    }
}