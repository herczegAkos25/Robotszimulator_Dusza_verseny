using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Robotszimulator.UI
{
    public class IndulasiAblak : Window
    {
        private const string Logo = """
        ______      _           _         _                 _       _
        | ___ \    | |         | |       (_)               | |     | |
        | |_/ /___ | |__   ___ | |_   ___ _ _ __ ___  _   _| | __ _| |_ ___  _ __
        |    // _ \| '_ \ / _ \| __| / __| | '_ ` _ \| | | | |/ _` | __/ _ \| '__|
        | |\ \ (_) | |_) | (_) | |_  \__ \ | | | | | | |_| | | (_| | || (_) | |
        \_| \_\___/|_.__/ \___/ \__| |___/_|_| |_| |_|\__,_|_|\__,_|\__\___/|_|
        """;

        private readonly TextBlock tbKonzol = new TextBlock();
        private readonly List<string> kepkockak = new List<string>();
        private readonly StringBuilder szoveg = new StringBuilder();
        private readonly DispatcherTimer ora = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(35) };
        private int index = 0;
        private bool vege = false;

        public event Action? Befejezodott;

        public IndulasiAblak()
        {
            Title = "robot.exe";
            Width = 900;
            Height = 560;
            ResizeMode = ResizeMode.CanMinimize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(0x0C, 0x0C, 0x0C));

            tbKonzol.FontFamily = new FontFamily("Consolas");
            tbKonzol.FontSize = 14;
            tbKonzol.Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            tbKonzol.Margin = new Thickness(10, 8, 10, 8);
            Content = tbKonzol;

            Osszeallitas();

            ora.Tick += Ora_Tick;
            Loaded += (kuldo, esemeny) => ora.Start();
            KeyDown += (kuldo, esemeny) => Befejez();
            MouseDown += (kuldo, esemeny) => Befejez();
        }

        private void Kep(int darab = 1)
        {
            for (int i = 0; i < darab; i++)
            {
                kepkockak.Add(szoveg.ToString());
            }
        }

        private void Gepel(string szovegresz, int kockaBetunkent)
        {
            foreach (char betu in szovegresz)
            {
                szoveg.Append(betu);
                Kep(kockaBetunkent);
            }
        }

        private void Sor(string tartalom, int kocka = 1)
        {
            szoveg.AppendLine(tartalom);
            Kep(kocka);
        }

        private void Folyamat(string cimke, string meret, int kockak)
        {
            int kezdet = szoveg.Length;

            for (int i = 0; i <= kockak; i++)
            {
                int szazalek = i * 100 / kockak;
                int kitoltott = szazalek / 5;

                szoveg.Length = kezdet;
                szoveg.Append(cimke.PadRight(38));
                szoveg.Append('[').Append(new string('#', kitoltott)).Append(new string('.', 20 - kitoltott)).Append("] ");
                szoveg.Append(szazalek.ToString().PadLeft(3)).Append('%');
                if (meret.Length > 0)
                {
                    szoveg.Append("  ").Append(meret);
                }
                Kep();
            }

            szoveg.AppendLine();
            Kep(4);
        }

        private void Osszeallitas()
        {
            szoveg.AppendLine(Logo.Replace("\r", ""));
            Sor("", 14);

            Folyamat("Downloading MineScript interpreter", "1.2 MB", 22);
            Folyamat("Downloading robot simulator", "3.4 MB", 22);
            Folyamat("Downloading block editor assets", "0.8 MB", 22);
            Folyamat("Downloading WPF runtime components", "12.7 MB", 26);
            Folyamat("Downloading UI theme and icons", "2.1 MB", 22);
            Folyamat("Installing map renderer", "", 18);
            Folyamat("Installing tracer", "", 18);
            Folyamat("Installing block editor", "", 18);
            Folyamat("Installing UI controls", "", 18);
            Folyamat("Installing sensor panel", "", 18);
            Folyamat("Loading UI stylesheets", "", 16);
            Folyamat("Building block palette", "", 16);
            Folyamat("Initializing simulator canvas", "", 16);
            Folyamat("Configuring UI layout", "", 16);
            Folyamat("Configuring workspace", "", 16);

            Sor("");
            Sor("Verifying installation ... OK", 14);
            Sor("Starting Robot Szimulátor ...", 24);
        }

        private void Ora_Tick(object? sender, EventArgs e)
        {
            if (index >= kepkockak.Count)
            {
                Befejez();
                return;
            }

            tbKonzol.Text = kepkockak[index] + ((index / 8) % 2 == 0 ? "_" : " ");
            index++;
        }

        private void Befejez()
        {
            if (vege) return;

            vege = true;
            ora.Stop();
            Befejezodott?.Invoke();
        }
    }
}
