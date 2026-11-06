using System.Configuration;
using System.Data;
using System.Windows;

namespace Robotszimulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string[] args = e.Args;

            bool isQuietMode = false;
            string inputFile = string.Empty;
            string outputFile = string.Empty;

            if (args.Length > 0 && args[0] == "-q")
            {
                isQuietMode = true;
                if (args.Length >= 3)
                {
                    inputFile = args[1];
                    outputFile = args[2];
                }
            }
            else if (args.Length >= 2)
            {
                inputFile = args[0];
                outputFile = args[1];
            }

            if (isQuietMode)
            {
                RunQuietMode(inputFile, outputFile);
                Current.Shutdown();
            }
            else
            {
                var mainWindow = new MainWindow(inputFile, outputFile);
                mainWindow.Show();
            }
        }

        private void RunQuietMode(string inputPath, string outputPath)
        {
        }
    }

}
