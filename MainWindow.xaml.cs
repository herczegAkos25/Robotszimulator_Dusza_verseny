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
            tbAranyZarva.Text = tbKibanyaszottArany.Text;
            tbIrány.Text = robot.AktualisIrany.ToString();
            EnergiaMegjelenites(robot.Energia, robot.FuroFejMaradek);
            SzenzorMegjelenites(robot.Szenzor());
            tbNapló.Text = string.Empty;
            tbKonzol.Text = string.Empty;
            EnergiaNaploMegjelenites(robot.EnergiaNaplo);
            lvValtozok.ItemsSource = null;
            tbLepesSzamlalo.Text = "– / –";
            AllapotsorBeallitas(uzenet, hiba);
            btnMegallitas.IsEnabled = false;
        }

        private void Megjelenites(Allapot allapot, Interpreter interpreter)
        {
            Kirajzol(allapot.Grid, allapot.Felfedezett, allapot.RobotSor, allapot.RobotOszlop, allapot.Irany);
            tbKibanyaszottArany.Text = allapot.Arany.ToString();
            tbAranyZarva.Text = tbKibanyaszottArany.Text;
            tbIrány.Text = allapot.Irany.ToString();
            EnergiaMegjelenites(allapot.Energia, allapot.FuroFejMaradek);
            SzenzorMegjelenites(allapot.Szenzor);

            tbNapló.Text = string.Join(Environment.NewLine, interpreter.Naplo.Take(allapot.NaploHossz));
            tbNapló.ScrollToEnd();
            tbKonzol.Text = string.Join(Environment.NewLine, interpreter.Konzol.Take(allapot.KonzolHossz));
            tbKonzol.ScrollToEnd();
            EnergiaNaploMegjelenites(interpreter.EnergiaNaplo.Take(allapot.EnergiaNaploHossz));

            var sorok = allapot.Valtozok
                .Select(valtozo => new ValtozoSor(valtozo.Key, TipusNev(valtozo.Value), ErtekSzoveg(valtozo.Value)))
                .ToList();
            foreach (var lista in allapot.Listak)
            {
                sorok.Add(new ValtozoSor(lista.Key, lista.Value.Tipus == "bool" ? "logikai lista" : "egész lista", ListaSzoveg(lista.Value)));
            }
            lvValtozok.ItemsSource = sorok;
        }

        private static readonly SolidColorBrush ZoldSzin = new SolidColorBrush(Color.FromRgb(0x98, 0xC3, 0x79));
        private static readonly SolidColorBrush SargaSzin = new SolidColorBrush(Color.FromRgb(0xE5, 0xC0, 0x7B));
        private static readonly SolidColorBrush PirosSzin = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75));
        private static readonly SolidColorBrush KekSzin = new SolidColorBrush(Color.FromRgb(0x61, 0xAF, 0xEF));

        private void EnergiaMegjelenites(int energia, int furoFejMaradek)
        {
            // Energia: zöld, 30% alatt sárga, 10% alatt piros
            int kapacitas = futtato.Beallitasok.Kapacitas;
            double arany = kapacitas > 0 ? (double)energia / kapacitas : 0;
            int szazalek = (int)Math.Round(arany * 100);

            Brush energiaSzin;
            string energiaInfo;
            if (arany < 0.10)
            {
                energiaSzin = PirosSzin;
                energiaInfo = szazalek + "%, piros 10% alatt";
            }
            else if (arany < 0.30)
            {
                energiaSzin = SargaSzin;
                energiaInfo = szazalek + "%, sárga 30% alatt";
            }
            else
            {
                energiaSzin = ZoldSzin;
                energiaInfo = szazalek + "% a " + kapacitas + " Wh-ból";
            }

            tbEnergia.Text = energia + " Wh";
            tbEnergia.Foreground = energiaSzin;
            pbEnergia.Maximum = Math.Max(kapacitas, 1);
            pbEnergia.Value = Math.Clamp(energia, 0, Math.Max(kapacitas, 1));
            pbEnergia.Foreground = energiaSzin;
            tbEnergiaInfo.Text = energiaInfo;

            FuggolegesSav(sorEnergiaUres, sorEnergiaTelt, savEnergiaTelt, arany, energiaSzin);
            tbEnergiaZarva.Text = szazalek + "%";
            tbEnergiaZarva.Foreground = energiaSzin;

            // Fúrófej: kék, az utolsó 3 fúrásnál piros, 0-nál elhasznált
            int elettartam = futtato.Beallitasok.FuroFejElettartam;
            bool kopott = furoFejMaradek <= 3;
            Brush furoSzin = kopott ? PirosSzin : KekSzin;

            tbFuroFej.Text = furoFejMaradek + " / " + elettartam;
            tbFuroFej.Foreground = furoSzin;
            pbFuroFej.Maximum = Math.Max(elettartam, 1);
            pbFuroFej.Value = Math.Clamp(furoFejMaradek, 0, Math.Max(elettartam, 1));
            pbFuroFej.Foreground = furoSzin;

            FuggolegesSav(sorFuroUres, sorFuroTelt, savFuroTelt, elettartam > 0 ? (double)furoFejMaradek / elettartam : 0, furoSzin);
            tbFuroFejZarva.Text = furoFejMaradek.ToString();
            tbFuroFejZarva.Foreground = furoSzin;
            tbFuroFejInfo.Text = furoFejMaradek == 0
                ? "E_DRILL_WORN, vegyél újat"
                : kopott ? "piros az utolsó 3 fúrásnál" : "fúrás maradt";
        }

        // A függőleges sáv két, csillag-arányú sorból áll (üres felül, teli alul), így nem függ a ProgressBar sablonjától.
        private static void FuggolegesSav(RowDefinition ures, RowDefinition telt, Border kitoltes, double arany, Brush szin)
        {
            arany = Math.Clamp(arany, 0, 1);
            ures.Height = new GridLength(1 - arany, GridUnitType.Star);
            telt.Height = new GridLength(arany, GridUnitType.Star);
            kitoltes.Background = szin;
        }

        private bool allapotNyitva = true;

        private void BtnAllapotToggle_Click(object sender, RoutedEventArgs e)
        {
            allapotNyitva = !allapotNyitva;
            pnlAllapotNyitva.Visibility = allapotNyitva ? Visibility.Visible : Visibility.Collapsed;
            pnlAllapotZarva.Visibility = allapotNyitva ? Visibility.Collapsed : Visibility.Visible;
            oszlopAllapot.Width = new GridLength(allapotNyitva ? 270 : 64);
        }

        private void EnergiaNaploMegjelenites(IEnumerable<string> sorok)
        {
            List<EnergiaSor> tabla = EnergiaSor.Lista(sorok);
            lbEnergiaLog.ItemsSource = tabla;
            if (tabla.Count > 0)
            {
                lbEnergiaLog.ScrollIntoView(tabla[tabla.Count - 1]);
            }
        }

        private static string ListaSzoveg(Lista lista)
        {
            var elemek = new List<string>();
            foreach (object elem in lista.Elemek)
            {
                elemek.Add(ErtekSzoveg(elem));
            }
            return "(" + elemek.Count + " elem) " + string.Join(", ", elemek);
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

            tbLepesSzamlalo.Text = index + " / " + (allapotok.Count - 1);
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
            if (hiba)
            {
                System.Media.SystemSounds.Hand.Play();
            }
        }

        private void SzenzorMegjelenites(int szenzorErtek)
        {
            tbSzenzor.Text = szenzorErtek switch
            {
                0 => "Üres",
                1 => "Fal vagy kőzet",
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
                        "@" or "$" => "arany",
                        "+" => "fúrható_k",
                        "&" => "fúrható_karany",
                        _ => "talaj"
                    };

                    Border cella = new Border
                    {
                        Width = CellaMeret,
                        Height = CellaMeret
                    };

                    Grid tartalom = new Grid();
                    cella.Child = tartalom;
                    bool latszik = felfedezve || robotCella;

                    var kep = terkepKepek.Betoltve ? terkepKepek.Kep(tipus, felfedezve || robotCella, cellaSorsok![r, c]) : null;

                    if (kep != null)
                    {
                        if (tipus != "fal" && tipus != "talaj")
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

                    if (latszik)
                    {
                        KozetJelzesek(tartalom, ertek, kep != null);
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

        private void KozetJelzesek(Grid tartalom, string ertek, bool kepVan)
        {
            if (!kepVan)
            {
                if (ertek == "+" || ertek == "&")
                {
                    tartalom.Children.Add(new Border { Background = Brushes.SaddleBrown, Margin = new Thickness(1) });
                }

                if (ertek == "&")
                {
                    tartalom.Children.Add(new System.Windows.Shapes.Ellipse
                    {
                        Fill = Brushes.Gold,
                        Width = 8,
                        Height = 8
                    });
                }

                if (ertek == "$")
                {
                    tartalom.Children.Add(new Border { Background = Brushes.Gold, Margin = new Thickness(3) });
                }
            }

            if (ertek == "=" || ertek == "*")
            {
                if (terkepKepek.Toltoallomas != null)
                {
                    tartalom.Children.Add(KepElem(terkepKepek.Toltoallomas));
                }
                else
                {
                    tartalom.Children.Add(new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 13,
                        Foreground = Brushes.DarkOrange,
                        Text = "⚡"
                    });
                }
            }

            if (ertek == "%" || ertek == "$" || ertek == "*")
            {
                if (terkepKepek.Tamasz != null)
                {
                    tartalom.Children.Add(KepElem(terkepKepek.Tamasz));
                }
                else
                {
                    tartalom.Children.Add(new Border
                    {
                        BorderBrush = Brushes.DarkSlateBlue,
                        BorderThickness = new Thickness(2.5)
                    });
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
                ("Kőzet", "fúrható_k", true, Brushes.SaddleBrown),
                ("Aranyas kőzet", "fúrható_karany", true, Brushes.Goldenrod),
                ("Támasz", "tamasz", true, Brushes.DarkSlateBlue),
                ("Töltő", "toltoallomas", true, Brushes.DarkOrange),
                ("Robot", "robot", true, Brushes.LightSkyBlue)
            };

            foreach (var elem in elemek)
            {
                BitmapSource? kep = null;
                if (elem.Tipus == "robot")
                {
                    kep = terkepKepek.Robot;
                }
                else if (elem.Tipus == "tamasz")
                {
                    kep = terkepKepek.Tamasz;
                }
                else if (elem.Tipus == "toltoallomas")
                {
                    kep = terkepKepek.Toltoallomas;
                }
                else if (terkepKepek.Betoltve)
                {
                    kep = terkepKepek.Kep(elem.Tipus, elem.Felfedezve, 0);
                }

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
                    futtato.KezziAllapotbol(nyomkovetesAllapotok[nyomkovetesIndex], nyomkovetesInterpreter?.EnergiaNaplo ?? new List<string>());
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

            string? hibaKod = futtato.KezziFordulas(fordulat);
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: " + fordulat + " negyedfordulat balra" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }

        private void BtnKezziLepes_Click(object sender, RoutedEventArgs e)
        {
            if (!SzamBeolvas(tbKezziLepes, out int tavolsag)) return;
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziLepes(tavolsag);
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: " + tavolsag + " lépés előre" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }

        private void BtnKezziBanyaszat_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziBanyaszat();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: bányászás" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }

        private void BtnKezziFuras_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziFuras();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: fúrás" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }

        private void BtnBeallitasok_Click(object sender, RoutedEventArgs e)
        {
            var ablak = new BeallitasokAblak(futtato.Beallitasok) { Owner = this };
            if (ablak.ShowDialog() != true || ablak.Eredmeny == null) return;

            megallitasKerve = true;
            futtato.Beallitasok = ablak.Eredmeny;
            nyomkovetesAllapotok = null;
            nyomkovetesInterpreter = null;
            nyomkovetesIndex = 0;
            kezziAllapotban = false;

            if (!betoltve)
            {
                AllapotsorBeallitas("Az energia-beállítások mentve, a betöltés után lépnek életbe", false);
                return;
            }

            try
            {
                futtato.Inditas();
                EloAllapotMegjelenites("Az energia-beállítások módosultak, a program újraindult a kezdőállapotból", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba az újraindításkor: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void BtnTamaszLerak_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziTamaszLerak();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: támasz lerakása" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }
        private void BtnTamaszFelvesz_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziTamaszFelvesz();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: támasz felvétele" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }


        private void BtnToltoallomasLerak_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziToltoallomasLerak();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: töltőállomás lerakása" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }

        private void BtnToltoallomasFelvesz_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziToltoallomasFelvesz();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: töltőállomás felvétele" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }
        private void BtnToltes_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziToltes(out int toltott);
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: töltés, +" + toltott + " energia" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }

        private void BtnFuroFejVasarlas_Click(object sender, RoutedEventArgs e)
        {
            if (!KezziElokeszites()) return;

            string? hibaKod = futtato.KezziFuroFejVasarlas();
            EloAllapotMegjelenites(
                hibaKod == null ? "Kézi vezérlés: új fúrófej vásárlása" : "Kézi vezérlés: hiba, " + hibaKod,
                hibaKod != null);
        }

    }
}