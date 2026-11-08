using Microsoft.Win32;
using Robotszimulator.Core;
using Robotszimulator.UI;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Robotszimulator
{
    public partial class MainWindow : Window
    {
        private const double CellaMeret = 20;
        private readonly TerkepKepek terkepKepek = new TerkepKepek();
        private readonly Random veletlen = new Random();
        private int[,]? cellaSorsok = null;
        private static readonly SolidColorBrush NormalSzin = new SolidColorBrush(Color.FromRgb(0x9D, 0xA5, 0xB4));
        private static readonly SolidColorBrush HibaSzin = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75));

        private Interpreter Interpreter => futtato.Interpreter;
        private Futtato futtato = new Futtato();
        private bool lejatszasFut = false;
        private bool megallitasKerve = false;
        private bool kezziAllapotban = false;
        private bool megallva = false;

        private List<Allapot>? nyomkovetesAllapotok = null;
        private Interpreter? nyomkovetesInterpreter = null;
        private int nyomkovetesIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            BlokkNezetBe();
            LegendaFeltoltese();

            

            if (string.IsNullOrEmpty(AppSettings.InputFilePath))
            {
                AllapotsorBeallitas("Nincs betöltött bemeneti fájl, használd a Betöltés gombot", false);
                return;
            }

            try
            {
                futtato.Betoltes();
                futtato.Inditas();
                betoltve = true;
                EloAllapotMegjelenites("Kezdőállapot", false);
                ProgramBetoltese();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltéskor: " + ex.Message);
            }
        }

        private void EloAllapotMegjelenites(string uzenet, bool hiba)
        {
            var terkep = futtato.Terkep;
            var robot = futtato.Robot;
            if (terkep == null || terkep.Grid == null || robot == null) return;

            SorKiemeles(0, false);

            Kirajzol(terkep.Grid, terkep.Felfedezett, robot.Sor, robot.Oszlop, robot.AktualisIrany);
            tbKibanyaszottArany.Text = robot.KibanyaszottAranySzam.ToString();
            tbIrány.Text = robot.AktualisIrany.ToString();
            SzenzorMegjelenites(robot.Szenzor());
            tbNapló.Text = string.Empty;
            tbKonzol.Text = string.Empty;
            lvValtozok.ItemsSource = null;
            tbLepesSzamlalo.Text = "Lépés – / –";
            AllapotsorBeallitas(uzenet, hiba);
            btnMegallitas.IsEnabled = false;
        }

        private void Megjelenites(Allapot allapot, Interpreter interpreter)
        {
            Kirajzol(allapot.Grid, allapot.Felfedezett, allapot.RobotSor, allapot.RobotOszlop, allapot.Irany);
            tbKibanyaszottArany.Text = allapot.Arany.ToString();
            tbIrány.Text = allapot.Irany.ToString();
            SzenzorMegjelenites(allapot.Szenzor);

            tbNapló.Text = string.Join(Environment.NewLine, interpreter.Naplo.Take(allapot.NaploHossz));
            tbNapló.ScrollToEnd();
            tbKonzol.Text = string.Join(Environment.NewLine, interpreter.Konzol.Take(allapot.KonzolHossz));
            tbKonzol.ScrollToEnd();

            lvValtozok.ItemsSource = allapot.Valtozok
                .Select(valtozo => new ValtozoSor(valtozo.Key, TipusNev(valtozo.Value), ErtekSzoveg(valtozo.Value)))
                .ToList();
        }

        private void TeljesNaploMegjelenites(Interpreter interpreter)
        {
            tbNapló.Text = string.Join(Environment.NewLine, interpreter.Naplo);
            tbNapló.ScrollToEnd();
            tbKonzol.Text = string.Join(Environment.NewLine, interpreter.Konzol);
            tbKonzol.ScrollToEnd();
        }

        private void LepesMegjelenites(int index)
        {
            var allapotok = nyomkovetesAllapotok;
            var interpreter = nyomkovetesInterpreter;
            if (allapotok == null || interpreter == null || index < 0 || index >= allapotok.Count) return;

            nyomkovetesIndex = index;
            bool utolso = index == allapotok.Count - 1;
            var allapot = allapotok[index];

            Megjelenites(allapot, interpreter);
            NaploFajlba(interpreter, utolso ? interpreter.Naplo.Count : allapot.NaploHossz);
            SorKiemeles(allapot.Sor, allapot.HibaKod != null);
            if (utolso)
            {
                TeljesNaploMegjelenites(interpreter);
            }

            tbLepesSzamlalo.Text = "Lépés " + index + " / " + (allapotok.Count - 1);
            AllapotsorFrissites(allapot, interpreter, utolso);
        }

        private void AllapotsorFrissites(Allapot allapot, Interpreter interpreter, bool utolso)
        {
            if (allapot.HibaKod != null)
            {
                AllapotsorBeallitas("Hiba: " + allapot.HibaKod + ", a " + allapot.Sor + ". sorban", true);
            }
            else if (allapot.Sor == 0)
            {
                AllapotsorBeallitas("Kezdőállapot", false);
            }
            else if (utolso)
            {
                AllapotsorBeallitas("Futás kész, hiba nélkül · " + interpreter.Naplo.Count + " naplósor", false);
            }
            else
            {
                AllapotsorBeallitas("Futás · " + allapot.Sor + ". sor", false);
            }
        }

        private void AllapotsorBeallitas(string szoveg, bool hiba)
        {
            tbAllapotsor.Foreground = hiba ? HibaSzin : NormalSzin;
            tbAllapotsor.Text = szoveg;
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

            if (cellaSorsok == null || cellaSorsok.GetLength(0) != sorokSzama || cellaSorsok.GetLength(1) != oszlopokSzama)
            {
                SorsokUjrasorsolasa(sorokSzama, oszlopokSzama);
            }

            TerkepCanvas.Children.Clear();
            TerkepCanvas.Width = oszlopokSzama * CellaMeret;
            TerkepCanvas.Height = sorokSzama * CellaMeret;

            for (int r = 0; r < sorokSzama; r++)
            {
                for (int c = 0; c < oszlopokSzama; c++)
                {
                    string ertek = grid[r][c];
                    bool felfedezve = felfedezett[r, c];
                    bool robotCella = r == robotSor && c == robotOszlop;

                    string tipus = ertek switch
                    {
                        "#" => "fal",
                        "@" => "arany",
                        _ => "talaj"
                    };

                    Border cella = new Border
                    {
                        Width = CellaMeret,
                        Height = CellaMeret
                    };

                    Grid tartalom = new Grid();
                    cella.Child = tartalom;

                    var kep = terkepKepek.Betoltve ? terkepKepek.Kep(tipus, felfedezve || robotCella, cellaSorsok![r, c]) : null;

                    if (kep != null)
                    {
                        if (tipus == "arany")
                        {
                            var talaj = terkepKepek.Kep("talaj", felfedezve || robotCella, cellaSorsok![r, c]);
                            if (talaj != null)
                            {
                                tartalom.Children.Add(KepElem(talaj));
                            }
                        }

                        tartalom.Children.Add(KepElem(kep));
                    }
                    else
                    {
                        cella.BorderBrush = Brushes.DarkSlateGray;
                        cella.BorderThickness = new Thickness(0.5);
                        cella.Background = felfedezve ? Brushes.White : Brushes.Black;

                        if (felfedezve && ertek == "#") cella.Background = Brushes.DarkGray;
                        if (felfedezve && ertek == "@") cella.Background = Brushes.Gold;
                    }

                    if (robotCella && terkepKepek.Robot != null)
                    {
                        var robotKep = KepElem(terkepKepek.Robot);
                        robotKep.RenderTransformOrigin = new Point(0.5, 0.5);
                        robotKep.RenderTransform = new RotateTransform(irany switch
                        {
                            Irany.Kelet => 90,
                            Irany.Nyugat => 270,
                            Irany.Észak => 0,
                            _ => 180
                        });
                        tartalom.Children.Add(robotKep);
                    }
                    else if (robotCella)
                    {
                        tartalom.Children.Add(new Border
                        {
                            Background = Brushes.LightSkyBlue,
                            Opacity = 0.6
                        });

                        tartalom.Children.Add(new TextBlock
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Black,
                            Text = irany switch
                            {
                                Irany.Kelet => ">",
                                Irany.Nyugat => "<",
                                Irany.Észak => "^",
                                _ => "v"
                            }
                        });
                    }

                    Canvas.SetLeft(cella, c * CellaMeret);
                    Canvas.SetTop(cella, r * CellaMeret);
                    TerkepCanvas.Children.Add(cella);
                }
            }
        }

        private static Image KepElem(BitmapSource kep)
        {
            var elem = new Image
            {
                Source = kep,
                Stretch = Stretch.Fill
            };
            RenderOptions.SetBitmapScalingMode(elem, BitmapScalingMode.HighQuality);
            return elem;
        }

        private void LegendaFeltoltese()
        {
            spLegenda.Children.Clear();

            var elemek = new (string Nev, string Tipus, bool Felfedezve, Brush Szin)[]
            {
                ("Felfedezett", "talaj", true, Brushes.White),
                ("Felfedezetlen", "talaj", false, Brushes.Black),
                ("Fal", "fal", true, Brushes.Gray),
                ("Arany", "arany", true, Brushes.Gold),
                ("Robot", "robot", true, Brushes.LightSkyBlue)
            };

            foreach (var elem in elemek)
            {
                BitmapSource? kep = elem.Tipus == "robot"
                    ? terkepKepek.Robot
                    : (terkepKepek.Betoltve ? terkepKepek.Kep(elem.Tipus, elem.Felfedezve, 0) : null);

                UIElement jel;
                if (kep != null)
                {
                    jel = KepElem(kep);
                }
                else
                {
                    jel = new Border { Background = elem.Szin };
                }

                var keret = (FrameworkElement)jel;
                keret.Width = 18;
                keret.Height = 18;
                keret.VerticalAlignment = VerticalAlignment.Center;

                spLegenda.Children.Add(jel);
                spLegenda.Children.Add(new Label
                {
                    Content = elem.Nev,
                    Foreground = NormalSzin,
                    Margin = new Thickness(2, 0, elem.Tipus == "robot" ? 0 : 14, 0)
                });
            }
        }

        private void SorsokUjrasorsolasa(int sorokSzama, int oszlopokSzama)
        {
            cellaSorsok = new int[sorokSzama, oszlopokSzama];
            for (int r = 0; r < sorokSzama; r++)
            {
                for (int c = 0; c < oszlopokSzama; c++)
                {
                    cellaSorsok[r, c] = veletlen.Next(0, 1000);
                }
            }
        }

        private bool NyomkovetesInditas()
        {
            if (!ProgramAtvetele()) return false;

            megallitasKerve = true;
            kezziAllapotban = false;

            try
            {
                futtato.Nyomkovetes = true;
                futtato.Inditas();
                futtato.Interpreter.Futtatas();

                nyomkovetesInterpreter = futtato.Interpreter;
                nyomkovetesAllapotok = futtato.Interpreter.Allapotok;
                LepesMegjelenites(0);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Futási hiba történt: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async void BtnFuttatas_Click(object sender, RoutedEventArgs e)
        {
            if (lejatszasFut) return;

            btnMegallitas.IsEnabled = true;
            lejatszasFut = true;
            btnMegallitas.Content = "⏹ Megállítás";

            try
            {
                if (cbNyomkovetes.IsChecked != true)
                {
                    if (!NyomkovetesInditas())
                    {
                        btnMegallitas.IsEnabled = false;
                        return;
                    }

                    LepesMegjelenites(nyomkovetesAllapotok!.Count - 1);
                    return;
                }

                if (nyomkovetesAllapotok == null)
                {
                    if (!NyomkovetesInditas())
                    {
                        btnMegallitas.IsEnabled = false;
                        return;
                    }
                }

                megallva = false;
                megallitasKerve = false;
                var allapotok = nyomkovetesAllapotok!;

                if (nyomkovetesIndex >= allapotok.Count - 1)
                {
                    nyomkovetesIndex = 0;
                }

                for (int i = nyomkovetesIndex; i < allapotok.Count; i++)
                {
                    while (megallitasKerve)
                    {
                        await Task.Delay(100);
                        if (!lejatszasFut) return;
                    }

                    LepesMegjelenites(i);

                    int csuszoErtek = (int)slSebesseg.Value;
                    await Task.Delay(Math.Max(10, 1000 - csuszoErtek * 100));
                }
            }
            finally
            {
                lejatszasFut = false;
                megallva = false;
                btnMegallitas.Content = "⏹ Megállítás";
                btnMegallitas.IsEnabled = false;
            }
        }

        private void BtnNyomkovetes_Click(object sender, RoutedEventArgs e)
        {
            NyomkovetesInditas();
            lejatszasFut = false;
            megallva = false;
            btnMegallitas.Content = "⏹ Megállítás";
            btnMegallitas.IsEnabled = false;
        }

        private void BtnMegallitas_Click(object sender, RoutedEventArgs e)
        {
            megallva = !megallva;
            if (megallva)
            {
                megallitasKerve = true;
                btnMegallitas.Content = "▶ Folytatás";
            }
            else
            {
                btnMegallitas.Content = "⏹ Megállítás";
                megallitasKerve = false;
            }
        }

        private void BtnKovetkezo_Click(object sender, RoutedEventArgs e)
        {
            megallitasKerve = true;
            if (nyomkovetesAllapotok == null && !NyomkovetesInditas()) return;
            LepesMegjelenites(nyomkovetesIndex + 1);
        }

        private void BtnVissza_Click(object sender, RoutedEventArgs e)
        {
            megallitasKerve = true;
            if (nyomkovetesAllapotok == null && !NyomkovetesInditas()) return;
            LepesMegjelenites(nyomkovetesIndex - 1);
        }

        private bool KezziElokeszites()
        {
            megallitasKerve = true;
            if (!betoltve)
            {
                AllapotsorBeallitas("Nincs betöltött bemeneti fájl, használd a Betöltés gombot", true);
                return false;
            }
            if (kezziAllapotban) return true;

            try
            {
                futtato.Nyomkovetes = false;
                if (nyomkovetesAllapotok != null && nyomkovetesIndex >= 0 && nyomkovetesIndex < nyomkovetesAllapotok.Count)
                {
                    futtato.KezziAllapotbol(nyomkovetesAllapotok[nyomkovetesIndex]);
                }
                else
                {
                    futtato.Inditas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba az indításkor: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            nyomkovetesAllapotok = null;
            nyomkovetesInterpreter = null;
            nyomkovetesIndex = 0;
            kezziAllapotban = true;
            return true;
        }

        private bool SzamBeolvas(TextBox mezo, out int ertek)
        {
            if (int.TryParse(mezo.Text, out ertek) && ertek >= 0) return true;

            AllapotsorBeallitas("A kézi vezérlés értéke csak nem negatív egész szám lehet", true);
            return false;
        }

        private void BtnKezziFordulas_Click(object sender, RoutedEventArgs e)
        {
            if (!SzamBeolvas(tbKezziFordulas, out int fordulat)) return;
            if (!KezziElokeszites()) return;

            futtato.KezziFordulas(fordulat);
            EloAllapotMegjelenites("Kézi vezérlés: " + fordulat + " negyedfordulat balra", false);
        }

        private void BtnKezziLepes_Click(object sender, RoutedEventArgs e)
        {
            if (!SzamBeolvas(tbKezziLepes, out int tavolsag)) return;
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziLepes(tavolsag);
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: " + tavolsag + " lépés előre" : "Kézi vezérlés: a robot falnak ütközött",
                hibaKod != null);
        }

        private void BtnKezziBanyaszat_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziBanyaszat();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: bányászás" : "Kézi vezérlés: nincs arany ezen a mezőn",
                hibaKod != null);
        }

        private void BtnMentes_Click(object sender, RoutedEventArgs e)
        {
            MentesValasztassal();
        }

        private void BtnBetoltes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Kérem válassza ki a fájlt!";
            ofd.Filter = "Szöveges fájl (*.txt)|*.txt|Minden fájl (*.*)|*.*";
            if (ofd.ShowDialog() != true) return;

            string regiUtvonal = AppSettings.InputFilePath;
            megallitasKerve = true;

            try
            {
                AppSettings.InputFilePath = ofd.FileName;
                futtato.Betoltes();
                futtato.Inditas();
            }
            catch (Exception hiba)
            {
                AppSettings.InputFilePath = regiUtvonal;
                MessageBox.Show("A fájl nem olvasható: " + hiba.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            betoltve = true;
            kezziAllapotban = false;
            NyomkovetesTorlese();
            cellaSorsok = null;
            EloAllapotMegjelenites("Betöltve: " + System.IO.Path.GetFileName(ofd.FileName), false);
            ProgramBetoltese();
        }

    }
}
