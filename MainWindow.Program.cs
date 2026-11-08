using Robotszimulator.Beolvasas;
using Robotszimulator.Core;
using Robotszimulator.UI;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Robotszimulator
{
    public partial class MainWindow
    {
        private static readonly SolidColorBrush AktivGomb = new SolidColorBrush(Color.FromRgb(0x3A, 0x3F, 0x4B));
        private static readonly SolidColorBrush InaktivGomb = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E));

        private BlokkProgram blokkProgram = new BlokkProgram();
        private bool blokkNezet = true;
        private bool betoltve = false;
        private bool nemMentett = false;
        private bool szovegBeallitas = false;
        private int kiemeltSzovegSor = 0;
        private bool kiemeltSzovegHiba = false;

        private BlokkSor? KijeloltSor()
        {
            return lbBlokkok.SelectedItem as BlokkSor;
        }

        private List<Sor> AktualisProgramSorok()
        {
            return blokkNezet ? blokkProgram.Sorok() : BlokkProgram.SzovegSorok(tbProgramSzoveg.Text);
        }

        private bool ProgramAtvetele()
        {
            if (!betoltve)
            {
                AllapotsorBeallitas("Nincs betöltött bemeneti fájl, használd a Betöltés gombot", true);
                return false;
            }

            if (blokkNezet)
            {
                string? hiba = blokkProgram.Hiba();
                if (hiba != null)
                {
                    AllapotsorBeallitas(hiba, true);
                    return false;
                }
            }

            futtato.Program = AktualisProgramSorok();
            return true;
        }

        private void NyomkovetesTorlese()
        {
            megallitasKerve = true;
            nyomkovetesAllapotok = null;
            nyomkovetesInterpreter = null;
            nyomkovetesIndex = 0;
        }

        private void BlokkokFrissitese(Blokk? kijelolt = null, bool elseSor = false)
        {
            NyomkovetesTorlese();

            List<BlokkSor> sorok = blokkProgram.Lapit();
            lbBlokkok.ItemsSource = sorok;

            if (kijelolt != null)
            {
                BlokkSor? uj = sorok.FirstOrDefault(sor => sor.Blokk == kijelolt && sor.ElseSor == elseSor);
                if (uj != null)
                {
                    lbBlokkok.SelectedItem = uj;
                    lbBlokkok.ScrollIntoView(uj);
                }
            }
        }

        private void BlokkNezetBe()
        {
            blokkNezet = true;
            lbBlokkok.Visibility = Visibility.Visible;
            pnlBlokkEszkozok.Visibility = Visibility.Visible;
            gdSzovegNezet.Visibility = Visibility.Collapsed;
            btnBlokkNezet.Background = AktivGomb;
            btnSzovegNezet.Background = InaktivGomb;
        }

        private void SzovegNezetBe()
        {
            blokkNezet = false;
            lbBlokkok.Visibility = Visibility.Collapsed;
            pnlBlokkEszkozok.Visibility = Visibility.Collapsed;
            gdSzovegNezet.Visibility = Visibility.Visible;
            btnBlokkNezet.Background = InaktivGomb;
            btnSzovegNezet.Background = AktivGomb;
        }

        private void MentettsegBeallitasa(bool valtozott)
        {
            nemMentett = valtozott;
            tbNemMentett.Visibility = valtozott ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SzovegBeallitasa(string szoveg)
        {
            szovegBeallitas = true;
            tbProgramSzoveg.Text = szoveg;
            szovegBeallitas = false;
        }

        private void SorszamokFrissitese()
        {
            string[] sorok = tbProgramSzoveg.Text.Split('\n');
            int szamlalo = 0;

            tbSorszamok.Inlines.Clear();

            for (int i = 0; i < sorok.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(sorok[i]))
                {
                    szamlalo++;
                    var szam = new System.Windows.Documents.Run(szamlalo.ToString());
                    if (szamlalo == kiemeltSzovegSor)
                    {
                        szam.Foreground = Ecset(kiemeltSzovegHiba ? "#E06C75" : "#E5C07B");
                        szam.FontWeight = FontWeights.Bold;
                    }

                    tbSorszamok.Inlines.Add(szam);
                }

                if (i < sorok.Length - 1) tbSorszamok.Inlines.Add(new System.Windows.Documents.LineBreak());
            }
        }

        private void TbProgramSzoveg_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            tbSorszamok.RenderTransform = new TranslateTransform(0, -e.VerticalOffset);
        }

        private bool ProgramMentese()
        {
            if (!betoltve)
            {
                AllapotsorBeallitas("Nincs betöltött bemeneti fájl, amit menteni lehetne", true);
                return false;
            }

            string programSzoveg;
            if (blokkNezet)
            {
                string? hiba = blokkProgram.Hiba();
                if (hiba != null)
                {
                    AllapotsorBeallitas(hiba, true);
                    return false;
                }
                programSzoveg = blokkProgram.Szoveg();
            }
            else
            {
                programSzoveg = tbProgramSzoveg.Text;
            }

            var ablak = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Program mentése",
                Filter = "Szöveges fájl (*.txt)|*.txt|Minden fájl (*.*)|*.*",
                FileName = System.IO.Path.GetFileName(AppSettings.InputFilePath),
                InitialDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(AppSettings.InputFilePath))
            };
            if (ablak.ShowDialog() != true) return false;

            try
            {
                var sorok = new List<string> { futtato.Beolvas.TerkepSor + " " + futtato.Beolvas.TerkepOszlop };
                sorok.AddRange(futtato.Beolvas.TerkepLista);
                sorok.AddRange(programSzoveg.Split('\n').Select(sor => sor.TrimEnd('\r')).Where(sor => !string.IsNullOrWhiteSpace(sor)));
                System.IO.File.WriteAllLines(ablak.FileName, sorok);
            }
            catch (Exception hiba)
            {
                MessageBox.Show("A program mentése nem sikerült: " + hiba.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            MentettsegBeallitasa(false);
            AllapotsorBeallitas("Program mentve: " + System.IO.Path.GetFileName(ablak.FileName), false);
            return true;
        }

        private void NaploFajlba(Interpreter interpreter, int hossz)
        {
            if (string.IsNullOrEmpty(AppSettings.OutputFilePath)) return;

            try
            {
                System.IO.File.WriteAllLines(AppSettings.OutputFilePath, interpreter.Naplo.Take(hossz));
            }
            catch (System.IO.IOException)
            {
            }
        }

        private bool NaploMentese()
        {
            if (futtato.Interpreter == null || futtato.Interpreter.Naplo.Count == 0)
            {
                AllapotsorBeallitas("Még nincs napló, előbb futtasd le a programot", true);
                return false;
            }

            string alapNev = string.IsNullOrEmpty(AppSettings.OutputFilePath) ? "output.txt" : System.IO.Path.GetFileName(AppSettings.OutputFilePath);
            var ablak = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Napló mentése",
                Filter = "Szöveges fájl (*.txt)|*.txt|Minden fájl (*.*)|*.*",
                FileName = alapNev
            };
            if (ablak.ShowDialog() != true) return false;

            try
            {
                System.IO.File.WriteAllLines(ablak.FileName, futtato.Interpreter.Naplo);
            }
            catch (Exception hiba)
            {
                MessageBox.Show("A napló mentése nem sikerült: " + hiba.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            AllapotsorBeallitas("Napló mentve: " + System.IO.Path.GetFileName(ablak.FileName), false);
            return true;
        }

        private void MentesValasztassal()
        {
            bool vanNaplo = futtato.Interpreter != null && futtato.Interpreter.Naplo.Count > 0;
            var ablak = new MentesAblak(vanNaplo) { Owner = this };
            if (ablak.ShowDialog() != true) return;

            if (ablak.ProgramMentes) ProgramMentese();
            if (ablak.NaploMentes) NaploMentese();
        }

        private void Ablak_SourceInitialized(object? sender, EventArgs e)
        {
            SotetCimsor.Beallit(this);
        }

        private void Ablak_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                MentesValasztassal();
                e.Handled = true;
            }
        }

        private void ProgramBetoltese()
        {
            SzovegBeallitasa(string.Join(Environment.NewLine, futtato.Program.Select(Blokk.SorSzoveg)));
            MentettsegBeallitasa(false);

            try
            {
                blokkProgram = BlokkProgram.Feldolgoz(futtato.Program);
                BlokkokFrissitese();
                BlokkNezetBe();
            }
            catch (FormatException hiba)
            {
                blokkProgram = new BlokkProgram();
                BlokkokFrissitese();
                SzovegNezetBe();
                AllapotsorBeallitas("A program nem alakítható blokkokká (" + hiba.Message + "), ezért a Szöveg nézet látszik", true);
            }
        }

        private void BtnBlokkNezet_Click(object sender, RoutedEventArgs e)
        {
            if (blokkNezet) return;

            try
            {
                blokkProgram = BlokkProgram.Feldolgoz(tbProgramSzoveg.Text);
            }
            catch (FormatException hiba)
            {
                AllapotsorBeallitas("Nem alakítható blokkokká: " + hiba.Message, true);
                return;
            }

            BlokkokFrissitese();
            BlokkNezetBe();
            AllapotsorBeallitas("Blokkok nézet", false);
        }

        private void BtnSzovegNezet_Click(object sender, RoutedEventArgs e)
        {
            if (!blokkNezet) return;

            SzovegBeallitasa(blokkProgram.Szoveg());
            SzovegNezetBe();
            AllapotsorBeallitas("Szöveg nézet", false);
        }

        private void TbProgramSzoveg_TextChanged(object sender, TextChangedEventArgs e)
        {
            kiemeltSzovegSor = 0;
            SorszamokFrissitese();
            NyomkovetesTorlese();
            if (!szovegBeallitas) MentettsegBeallitasa(true);
        }

        private static SolidColorBrush Ecset(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        private void BtnBlokkHozzaadas_Click(object sender, RoutedEventArgs e)
        {
            var lista = new StackPanel();
            var ablak = new Popup
            {
                PlacementTarget = (UIElement)sender,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true
            };

            void Elem(string nev, string leiras, string szin, Action muvelet)
            {
                var pont = new Border { Width = 10, Height = 22, CornerRadius = new CornerRadius(3), Background = Ecset(szin), Margin = new Thickness(0, 0, 10, 0) };
                var cim = new TextBlock { Text = nev, Foreground = Ecset("#E6EDF3"), VerticalAlignment = VerticalAlignment.Center };
                var megjegyzes = new TextBlock { Text = leiras, Foreground = Ecset("#7D8594"), FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(14, 0, 0, 0) };
                DockPanel.SetDock(megjegyzes, Dock.Right);

                var sor = new DockPanel { LastChildFill = true };
                sor.Children.Add(megjegyzes);
                sor.Children.Add(pont);
                sor.Children.Add(cim);

                var elem = new Border { Child = sor, Padding = new Thickness(8, 6, 8, 6), CornerRadius = new CornerRadius(6), Background = Brushes.Transparent, Cursor = Cursors.Hand };
                elem.MouseEnter += (kuldo, esemeny) => elem.Background = Ecset("#323843");
                elem.MouseLeave += (kuldo, esemeny) => elem.Background = Brushes.Transparent;
                elem.MouseLeftButtonUp += (kuldo, esemeny) =>
                {
                    ablak.IsOpen = false;
                    muvelet();
                };
                lista.Children.Add(elem);
            }

            void Elvalaszto()
            {
                lista.Children.Add(new Border { Height = 1, Background = Ecset("#3A3F4B"), Margin = new Thickness(4, 5, 4, 5) });
            }

            Elem("set", "változó beállítása", "#61AFEF", () => UjBlokkHozzaadasa(BlokkTipus.Set));
            Elem("step forward", "lépés előre", "#E5C07B", () => UjBlokkHozzaadasa(BlokkTipus.StepForward));
            Elem("turn by", "fordulás balra", "#E5C07B", () => UjBlokkHozzaadasa(BlokkTipus.TurnBy));
            Elem("mine here", "bányászás", "#E5C07B", () => UjBlokkHozzaadasa(BlokkTipus.MineHere));
            Elem("print", "kiírás", "#C678DD", () => UjBlokkHozzaadasa(BlokkTipus.Print));
            Elvalaszto();
            Elem("if", "feltétel", "#98C379", () => UjBlokkHozzaadasa(BlokkTipus.If));
            Elem("if-else", "feltétel, két ággal", "#98C379", () => UjBlokkHozzaadasa(BlokkTipus.If, true));
            Elem("while", "ciklus", "#98C379", () => UjBlokkHozzaadasa(BlokkTipus.While));

            ablak.Child = new Border
            {
                Child = lista,
                Width = 270,
                Padding = new Thickness(6),
                CornerRadius = new CornerRadius(8),
                Background = Ecset("#262A32"),
                BorderBrush = Ecset("#3A3F4B"),
                BorderThickness = new Thickness(1)
            };
            ablak.IsOpen = true;
        }

        private void UjBlokkHozzaadasa(BlokkTipus tipus, bool ifElse = false)
        {
            Blokk? uj;

            if (tipus == BlokkTipus.MineHere)
            {
                uj = new Blokk(tipus);
            }
            else
            {
                var ablak = new BlokkAblak(tipus) { Owner = this };
                if (ablak.ShowDialog() != true) return;
                uj = ablak.Eredmeny;
            }

            if (uj == null) return;

            if (ifElse)
            {
                uj.VanElse = true;
            }

            try
            {
                blokkProgram.Beszur(uj, KijeloltSor(), (BeszurasMod)cbHely.SelectedIndex);
            }
            catch (InvalidOperationException hiba)
            {
                AllapotsorBeallitas(hiba.Message, true);
                return;
            }

            MentettsegBeallitasa(true);
            BlokkokFrissitese(uj);
            cbHely.SelectedIndex = uj.Vezerlo ? 1 : 0;
            AllapotsorBeallitas("Blokk hozzáadva", false);
        }

        private void BlokkSzerkesztese()
        {
            BlokkSor? sor = KijeloltSor();
            if (sor == null)
            {
                AllapotsorBeallitas("Előbb jelölj ki egy blokkot", true);
                return;
            }
            if (sor.ElseSor || sor.Blokk.Tipus == BlokkTipus.MineHere)
            {
                AllapotsorBeallitas("Ennek a sornak nincs szerkeszthető tartalma", true);
                return;
            }

            var ablak = new BlokkAblak(sor.Blokk.Tipus, sor.Blokk) { Owner = this };
            if (ablak.ShowDialog() == true)
            {
                MentettsegBeallitasa(true);
                BlokkokFrissitese(sor.Blokk);
                AllapotsorBeallitas("Blokk módosítva", false);
            }
        }

        private void BlokkTorlese()
        {
            BlokkSor? sor = KijeloltSor();
            if (sor == null)
            {
                AllapotsorBeallitas("Előbb jelölj ki egy blokkot", true);
                return;
            }

            bool vanTartalom = sor.ElseSor
                ? sor.Blokk.ElseGyerekek.Count > 0
                : (sor.Blokk.Gyerekek.Count > 0 || sor.Blokk.ElseGyerekek.Count > 0);

            if (vanTartalom && MessageBox.Show(
                    "A kijelölt sor a benne lévő blokkokkal együtt törlődik. Folytatod?",
                    "Törlés", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (sor.ElseSor)
            {
                blokkProgram.ElseTorlese(sor.Blokk);
            }
            else
            {
                blokkProgram.Torol(sor.Blokk);
            }

            MentettsegBeallitasa(true);
            BlokkokFrissitese();
            AllapotsorBeallitas("Törölve", false);
        }

        private void BlokkMozgatasa(int irany)
        {
            BlokkSor? sor = KijeloltSor();
            if (sor == null || sor.ElseSor)
            {
                AllapotsorBeallitas("Előbb jelölj ki egy (nem else) blokkot", true);
                return;
            }

            if (blokkProgram.Mozgat(sor.Blokk, irany))
            {
                MentettsegBeallitasa(true);
                BlokkokFrissitese(sor.Blokk);
            }
        }

        private void BtnBlokkBe_Click(object sender, RoutedEventArgs e)
        {
            BlokkSor? sor = KijeloltSor();
            if (sor == null || sor.ElseSor)
            {
                AllapotsorBeallitas("Előbb jelölj ki egy (nem else) blokkot", true);
                return;
            }

            if (blokkProgram.BeleMozgat(sor.Blokk))
            {
                MentettsegBeallitasa(true);
                BlokkokFrissitese(sor.Blokk);
                AllapotsorBeallitas("Blokk áthelyezve a szomszédos blokkba", false);
            }
            else
            {
                AllapotsorBeallitas("Nincs szomszédos while/if blokk, vagy nem fér bele (legfeljebb 3 szint)", true);
            }
        }

        private void BtnBlokkKi_Click(object sender, RoutedEventArgs e)
        {
            BlokkSor? sor = KijeloltSor();
            if (sor == null || sor.ElseSor)
            {
                AllapotsorBeallitas("Előbb jelölj ki egy (nem else) blokkot", true);
                return;
            }

            if (blokkProgram.KiMozgat(sor.Blokk))
            {
                MentettsegBeallitasa(true);
                BlokkokFrissitese(sor.Blokk);
                AllapotsorBeallitas("Blokk kivéve a szülő blokkból", false);
            }
            else
            {
                AllapotsorBeallitas("Ez a blokk nincs while/if blokkban", true);
            }
        }

        private void BtnBlokkFel_Click(object sender, RoutedEventArgs e)
        {
            BlokkMozgatasa(-1);
        }

        private void BtnBlokkLe_Click(object sender, RoutedEventArgs e)
        {
            BlokkMozgatasa(1);
        }

        private void BtnBlokkSzerkesztes_Click(object sender, RoutedEventArgs e)
        {
            BlokkSzerkesztese();
        }

        private void BtnBlokkTorles_Click(object sender, RoutedEventArgs e)
        {
            BlokkTorlese();
        }

        private void LbBlokkok_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                BlokkTorlese();
                e.Handled = true;
            }
        }

        private void LbBlokkok_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (KijeloltSor() != null)
            {
                BlokkSzerkesztese();
            }
        }

        private void SorKiemeles(int sorszam, bool hiba)
        {
            if (!blokkNezet)
            {
                SzovegSorKiemeles(sorszam, hiba);
                return;
            }

            if (lbBlokkok.ItemsSource is not List<BlokkSor> sorok) return;

            foreach (BlokkSor sor in sorok)
            {
                sor.Keret = "Transparent";
            }

            BlokkSor? cel = sorszam > 0 ? sorok.FirstOrDefault(sor => sor.Sorszam == sorszam) : null;
            if (cel != null)
            {
                cel.Keret = hiba ? "#E06C75" : "#E5C07B";
                lbBlokkok.ScrollIntoView(cel);
            }
        }

        private void SzovegSorKiemeles(int sorszam, bool hiba)
        {
            kiemeltSzovegSor = sorszam;
            kiemeltSzovegHiba = hiba;
            tbProgramSzoveg.SelectionBrush = Ecset(hiba ? "#E06C75" : "#E5C07B");
            tbProgramSzoveg.SelectionTextBrush = Brushes.Black;
            SorszamokFrissitese();

            if (sorszam <= 0)
            {
                tbProgramSzoveg.Select(0, 0);
                return;
            }

            string szoveg = tbProgramSzoveg.Text;
            int szamlalo = 0;
            int kezdet = 0;

            while (kezdet <= szoveg.Length)
            {
                int vege = szoveg.IndexOf('\n', kezdet);
                if (vege < 0) vege = szoveg.Length;

                string sor = szoveg.Substring(kezdet, vege - kezdet).TrimEnd('\r');
                if (!string.IsNullOrWhiteSpace(sor))
                {
                    szamlalo++;
                    if (szamlalo == sorszam)
                    {
                        tbProgramSzoveg.Select(kezdet, sor.Length);
                        tbProgramSzoveg.ScrollToLine(tbProgramSzoveg.GetLineIndexFromCharacterIndex(kezdet));
                        return;
                    }
                }

                if (vege >= szoveg.Length) break;
                kezdet = vege + 1;
            }
        }
    }
}
