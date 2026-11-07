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

            if (AppSettings.IsQuietMode && (AppSettings.InputFilePath == string.Empty || AppSettings.OutputFilePath == string.Empty))
            {
                Shutdown(1);
                return;
            }

            if (AppSettings.IsQuietMode)
            {
                CsendesModFuttatas();
                Current.Shutdown();
            }
            else
            {
                try
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                catch (Exception hiba)
                {
                    MessageBox.Show(hiba.Message, "Robot szimulátor");
                    Shutdown(1);
                }
            }
        }

        private void CsendesModFuttatas()
        {
            var futtato = new Futtato();

            try
            {
                futtato.Betoltes();
                futtato.Inditas();
                futtato.Interpreter.Futtatas();
                futtato.NaploKiiras();
            }
            catch (Exception)
            {
                if (futtato.Interpreter != null)
                {
                    futtato.NaploKiiras();
                }
                else
                {
                    File.WriteAllLines(AppSettings.OutputFilePath, new[] { "EOR" });
                }
            }
        }
    }
}
