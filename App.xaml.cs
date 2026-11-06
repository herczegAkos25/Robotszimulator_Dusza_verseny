using System.Configuration;
using System.Data;
using System.Windows;

namespace Robotszimulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    using System.Windows;

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
                RunQuietMode();
                Current.Shutdown();
            }
            else
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void RunQuietMode()
        {
            // Itt már bárhol elérhetitek az AppSettings.InputFilePath és OutputFilePath értékeket
            // pl. MineScriptInterpreter.Execute(AppSettings.InputFilePath, AppSettings.OutputFilePath);
        }
    }
}
