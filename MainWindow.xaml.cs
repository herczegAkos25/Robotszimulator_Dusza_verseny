using Robotszimulator.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator
{
    public partial class MainWindow : Window
    {
        private const double CellaMeret = 20;
        private Futtato futtato = new Futtato();
        private bool lejatszasFut = false;
        private bool megallitasKerve = false;

        public MainWindow()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(AppSettings.InputFilePath))
            {
                AppSettings.InputFilePath = "input.txt";
            }

            try
            {
                futtato.Betoltes();
                futtato.Inditas();
                KezdoallapotMegjelenites();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltéskor: " + ex.Message);
            }
        }

        private void KezdoallapotMegjelenites()
        {
            var terkep = futtato.Terkep;
            var robot = futtato.Robot;
            if (terkep == null || terkep.Grid == null || robot == null) return;

            Kirajzol(terkep.Grid, terkep.Felfedezett, robot.Sor, robot.Oszlop, robot.AktualisIrany);
            tbKibanyaszottArany.Text = "0";
            tbIrány.Text = robot.AktualisIrany.ToString();
            SzenzorMegjelenites(robot.Szenzor());
            tbNapló.Text = string.Empty;
            tbKonzol.Text = string.Empty;
            lvValtozok.ItemsSource = null;
        }

        private void Megjelenites(Allapot allapot)
        {
            Kirajzol(allapot.Grid, allapot.Felfedezett, allapot.RobotSor, allapot.RobotOszlop, allapot.Irany);
            tbKibanyaszottArany.Text = allapot.Arany.ToString();
            tbIrány.Text = allapot.Irany.ToString();
            SzenzorMegjelenites(allapot.Szenzor);

            var interpreter = futtato.Interpreter;
            tbNapló.Text = string.Join(Environment.NewLine, interpreter.Naplo.Take(allapot.NaploHossz));
            tbNapló.ScrollToEnd();
            tbKonzol.Text = string.Join(Environment.NewLine, interpreter.Konzol.Take(allapot.KonzolHossz));
            tbKonzol.ScrollToEnd();

            lvValtozok.ItemsSource = allapot.Valtozok
                .Select(valtozo => new ValtozoSor(valtozo.Key, TipusNev(valtozo.Value), ErtekSzoveg(valtozo.Value)))
                .ToList();
        }

        private void TeljesNaploMegjelenites()
        {
            var interpreter = futtato.Interpreter;
            tbNapló.Text = string.Join(Environment.NewLine, interpreter.Naplo);
            tbNapló.ScrollToEnd();
            tbKonzol.Text = string.Join(Environment.NewLine, interpreter.Konzol);
            tbKonzol.ScrollToEnd();
        }

        private void SzenzorMegjelenites(int szenzorErtek)
        {
            tbSzenzor.Text = szenzorErtek switch
            {
                0 => "Üres (.)",
                1 => "Fal (#)",
                2 => "Arany (@)",
                _ => "Ismeretlen"
            };
        }

        private static string TipusNev(object ertek)
        {
            return ertek is bool ? "logikai" : "egész";
        }

        private static string ErtekSzoveg(object ertek)
        {
            if (ertek is bool logikai) return logikai ? "true" : "false";
            return Convert.ToString(ertek, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private void Kirajzol(string[][] grid, bool[,] felfedezett, int robotSor, int robotOszlop, Irany irany)
        {
            if (grid == null || grid.Length == 0) return;

            int sorokSzama = grid.Length;
            int oszlopokSzama = grid[0].Length;

            TerkepCanvas.Children.Clear();
            TerkepCanvas.Width = oszlopokSzama * CellaMeret;
            TerkepCanvas.Height = sorokSzama * CellaMeret;

            for (int r = 0; r < sorokSzama; r++)
            {
                for (int c = 0; c < oszlopokSzama; c++)
                {
                    string ertek = grid[r][c];
                    bool felfedezve = felfedezett[r, c];

                    Border cella = new Border
                    {
                        Width = CellaMeret,
                        Height = CellaMeret,
                        BorderBrush = Brushes.DarkSlateGray,
                        BorderThickness = new Thickness(0.5),
                        Background = felfedezve ? Brushes.White : Brushes.Black
                    };

                    TextBlock szoveg = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 14,
                        FontWeight = FontWeights.Bold
                    };

                    if (felfedezve)
                    {
                        switch (ertek)
                        {
                            case "#":
                                cella.Background = Brushes.DarkGray;
                                break;
                            case "@":
                                cella.Background = Brushes.Gold;
                                szoveg.Text = "★";
                                break;
                        }
                    }

                    if (r == robotSor && c == robotOszlop)
                    {
                        cella.Background = Brushes.LightSkyBlue;
                        szoveg.Text = irany switch
                        {
                            Irany.Kelet => ">",
                            Irany.Nyugat => "<",
                            Irany.Eszak => "^",
                            _ => "v"
                        };
                    }

                    cella.Child = szoveg;
                    Canvas.SetLeft(cella, c * CellaMeret);
                    Canvas.SetTop(cella, r * CellaMeret);
                    TerkepCanvas.Children.Add(cella);
                }
            }
        }

        private async void BtnFuttatas_Click(object sender, RoutedEventArgs e)
        {
            if (lejatszasFut) return;

            lejatszasFut = true;
            megallitasKerve = false;

            try
            {
                futtato.Nyomkovetes = true;
                futtato.Inditas();
                futtato.Interpreter.Futtatas();

                foreach (var allapot in futtato.Interpreter.Allapotok)
                {
                    if (megallitasKerve) break;

                    Megjelenites(allapot);

                    int csuszoErtek = (int)slSebesseg.Value;
                    await Task.Delay(Math.Max(10, 1000 - csuszoErtek * 100));
                }

                if (!megallitasKerve)
                {
                    TeljesNaploMegjelenites();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Futási hiba történt: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                lejatszasFut = false;
            }
        }

        private void BtnMegallitas_Click(object sender, RoutedEventArgs e)
        {
            megallitasKerve = true;
        }
    }
}
