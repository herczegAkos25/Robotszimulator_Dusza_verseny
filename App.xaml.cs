using Robotszimulator.Beolvasas;
using Robotszimulator.Core;
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
            Beolvas beolvas = new Beolvas();
            beolvas.Beolvasas(); // Az AppSettings.InputFilePath-et használja

            // 2. Robot és Értelmező előkészítése
            Robot robot = new Robot(beolvas.TerkepLista, beolvas.TerkepSor, beolvas.TerkepOszlop);
            Interpreter interpreter = new Interpreter(beolvas.ProgramLista, robot);

            // 3. Futtatás és a napló kiírása a kimeneti fájlba
            interpreter.Futtatas();

            File.WriteAllLines(AppSettings.OutputFilePath, interpreter.Naplo);
        }
    }
}