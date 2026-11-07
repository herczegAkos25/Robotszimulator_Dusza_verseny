using Robotszimulator.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Robotszimulator.UI
{
    public class BlokkAblak : Window
    {
        private readonly BlokkTipus tipus;
        private readonly Blokk? szerkesztett;
        private readonly TextBox tbElso = new TextBox();
        private readonly TextBox tbErtekA = new TextBox { Width = 110 };
        private readonly TextBox tbErtekB = new TextBox { Width = 110 };
        private readonly List<Border> ertekElemek = new List<Border>();
        private readonly StackPanel elonezet = new StackPanel { Orientation = Orientation.Horizontal };
        private int ertekIndex = 0;
        private readonly TextBlock tbElotag = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 6, 0) };
        private readonly TextBlock tbMuvelet = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 6, 0) };
        private static readonly string[] Muveletek = { "+", "-", "*", "=", "<>", "<", "<=", ">", ">=", "and", "or" };
        private readonly TextBlock tbHiba = new TextBlock();

        public Blokk? Eredmeny { get; private set; }

        public BlokkAblak(BlokkTipus tipus, Blokk? szerkesztett = null)
        {
            this.tipus = tipus;
            this.szerkesztett = szerkesztett;

            Title = (szerkesztett == null ? "Új blokk: " : "Szerkesztés: ") + Nev(tipus);
            SotetCimsor.Alkalmaz(this);
            Width = 400;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(0x21, 0x25, 0x2B));

            var panel = new StackPanel { Margin = new Thickness(14) };

            if (tipus == BlokkTipus.Set)
            {
                Mezo(panel, "Változó neve:", tbElso, szerkesztett == null ? "" : szerkesztett.Szavak[0]);
                ErtekBlokkMezok(panel, szerkesztett);
            }
            else
            {
                Mezo(panel, Felirat(tipus), tbElso, szerkesztett == null ? "" : szerkesztett.Szavak[0]);
            }

            tbElso.TextChanged += (kuldo, esemeny) => ElonezetFrissitese();
            tbErtekA.TextChanged += (kuldo, esemeny) => ElonezetFrissitese();
            tbErtekB.TextChanged += (kuldo, esemeny) => ElonezetFrissitese();

            panel.Children.Add(new TextBlock
            {
                Text = "Előnézet:",
                Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)),
                Margin = new Thickness(0, 0, 0, 4)
            });
            panel.Children.Add(new Border
            {
                Child = elonezet,
                Background = new SolidColorBrush(ElonezetSzin()),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(9, 5, 2, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12)
            });
            ElonezetFrissitese();

            tbHiba.Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75));
            tbHiba.TextWrapping = TextWrapping.Wrap;
            tbHiba.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(tbHiba);

            var gombok = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var ok = new Button { Content = "OK", IsDefault = true, Padding = new Thickness(18, 6, 18, 6), Margin = new Thickness(0, 0, 8, 0), Style = (Style)Application.Current.FindResource("RunButton") };
            var megse = new Button { Content = "Mégse", IsCancel = true, Padding = new Thickness(14, 6, 14, 6), Style = (Style)Application.Current.FindResource("DarkButton") };
            ok.Click += Ok_Click;
            gombok.Children.Add(ok);
            gombok.Children.Add(megse);
            panel.Children.Add(gombok);

            Content = panel;
            Loaded += (kuldo, esemeny) =>
            {
                tbElso.Focus();
                tbElso.SelectAll();
            };
        }

        public static string Nev(BlokkTipus tipus)
        {
            switch (tipus)
            {
                case BlokkTipus.Set:
                    return "set (változó beállítása)";
                case BlokkTipus.StepForward:
                    return "step forward (lépés előre)";
                case BlokkTipus.TurnBy:
                    return "turn by (fordulás balra)";
                case BlokkTipus.MineHere:
                    return "mine here (bányászás)";
                case BlokkTipus.Print:
                    return "print (kiírás)";
                case BlokkTipus.If:
                    return "if (feltétel)";
                default:
                    return "while (ciklus)";
            }
        }

        private static string Felirat(BlokkTipus tipus)
        {
            switch (tipus)
            {
                case BlokkTipus.StepForward:
                    return "Lépések száma (szám vagy változó):";
                case BlokkTipus.TurnBy:
                    return "Negyedfordulatok száma (szám vagy változó):";
                case BlokkTipus.Print:
                    return "Kiírandó érték (szám, változó, true vagy false):";
                default:
                    return "Feltétel (változó, true vagy false):";
            }
        }

        private static void Mezo(StackPanel panel, string felirat, TextBox mezo, string ertek)
        {
            panel.Children.Add(new TextBlock
            {
                Text = felirat,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            });

            mezo.Text = ertek;
            mezo.Background = new SolidColorBrush(Color.FromRgb(0x28, 0x2C, 0x34));
            mezo.Foreground = Brushes.White;
            mezo.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x3F, 0x4B));
            mezo.CaretBrush = Brushes.White;
            mezo.Padding = new Thickness(4);
            mezo.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(mezo);
        }

        private static Border Cip(string szoveg)
        {
            return new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xCF, 0xCF, 0xCF)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(8, 1, 8, 1),
                Margin = new Thickness(0, 0, 7, 0),
                Child = new TextBlock { Text = szoveg, FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)) }
            };
        }

        private static TextBlock Szo(string szoveg, bool felkover)
        {
            return new TextBlock
            {
                Text = szoveg,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E)),
                FontWeight = felkover ? FontWeights.Bold : FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 7, 0)
            };
        }

        private void ErtekBlokkMezok(StackPanel panel, Blokk? szerkesztett)
        {
            var szoveg = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));
            panel.Children.Add(new TextBlock { Text = "Érték-blokk (a set jobb oldala):", Foreground = szoveg, Margin = new Thickness(0, 0, 0, 4) });

            var lista = new StackPanel();
            int darab = Muveletek.Length + 3;
            for (int i = 0; i < darab; i++)
            {
                int index = i;
                var tartalom = new StackPanel { Orientation = Orientation.Horizontal };
                if (i == 0)
                {
                    tartalom.Children.Add(Cip("érték"));
                }
                else if (i <= Muveletek.Length)
                {
                    tartalom.Children.Add(Cip("érték"));
                    tartalom.Children.Add(Szo(Muveletek[i - 1], false));
                    tartalom.Children.Add(Cip("érték"));
                }
                else
                {
                    tartalom.Children.Add(Szo(i == Muveletek.Length + 1 ? "not" : "isThere", true));
                    tartalom.Children.Add(Cip("érték"));
                }

                var elem = new Border
                {
                    Child = tartalom,
                    Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xF1, 0xCF)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xE9, 0xC8, 0x77)),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(7),
                    Padding = new Thickness(7, 5, 7, 5),
                    Margin = new Thickness(0, 0, 0, 5),
                    Cursor = Cursors.Hand
                };
                elem.MouseLeftButtonUp += (kuldo, esemeny) => ErtekKivalasztasa(index);
                ertekElemek.Add(elem);
                lista.Children.Add(elem);
            }

            panel.Children.Add(new Border
            {
                Child = new ScrollViewer { Content = lista, MaxHeight = 180, VerticalScrollBarVisibility = ScrollBarVisibility.Auto },
                Background = new SolidColorBrush(Color.FromRgb(0x26, 0x2A, 0x32)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x3F, 0x4B)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 0, 0, 10)
            });

            tbElotag.Foreground = szoveg;
            tbMuvelet.Foreground = szoveg;
            foreach (TextBox mezo in new[] { tbErtekA, tbErtekB })
            {
                mezo.Background = new SolidColorBrush(Color.FromRgb(0x28, 0x2C, 0x34));
                mezo.Foreground = Brushes.White;
                mezo.BorderBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x3F, 0x4B));
                mezo.CaretBrush = Brushes.White;
                mezo.Padding = new Thickness(4);
            }

            var sor = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            sor.Children.Add(tbElotag);
            sor.Children.Add(tbErtekA);
            sor.Children.Add(tbMuvelet);
            sor.Children.Add(tbErtekB);
            panel.Children.Add(sor);

            int kijelolt = 0;
            if (szerkesztett != null)
            {
                string[] jobbOldal = szerkesztett.Szavak.Skip(1).ToArray();
                if (jobbOldal.Length == 1)
                {
                    tbErtekA.Text = jobbOldal[0];
                }
                else if (jobbOldal.Length == 2)
                {
                    kijelolt = jobbOldal[0] == "not" ? Muveletek.Length + 1 : Muveletek.Length + 2;
                    tbErtekA.Text = jobbOldal[1];
                }
                else
                {
                    kijelolt = Array.IndexOf(Muveletek, jobbOldal[1]) + 1;
                    tbErtekA.Text = jobbOldal[0];
                    tbErtekB.Text = jobbOldal[2];
                }
            }

            ErtekKivalasztasa(kijelolt);
        }

        private void ErtekKivalasztasa(int index)
        {
            ertekIndex = index;
            for (int i = 0; i < ertekElemek.Count; i++)
            {
                ertekElemek[i].BorderBrush = i == index
                    ? new SolidColorBrush(Color.FromRgb(0x61, 0xAF, 0xEF))
                    : new SolidColorBrush(Color.FromRgb(0xE9, 0xC8, 0x77));
            }
            ErtekMezokFrissitese();
        }

        private Color ElonezetSzin()
        {
            switch (tipus)
            {
                case BlokkTipus.Set:
                    return Color.FromRgb(0x61, 0xAF, 0xEF);
                case BlokkTipus.If:
                case BlokkTipus.While:
                    return Color.FromRgb(0x98, 0xC3, 0x79);
                case BlokkTipus.Print:
                    return Color.FromRgb(0xC6, 0x78, 0xDD);
                default:
                    return Color.FromRgb(0xE5, 0xC0, 0x7B);
            }
        }

        private static string Ertek(TextBox mezo)
        {
            string szoveg = mezo.Text.Trim();
            return szoveg.Length == 0 ? "…" : szoveg;
        }

        private void ElonezetFrissitese()
        {
            elonezet.Children.Clear();

            if (tipus != BlokkTipus.Set)
            {
                string elotag;
                switch (tipus)
                {
                    case BlokkTipus.StepForward:
                        elotag = "step forward";
                        break;
                    case BlokkTipus.TurnBy:
                        elotag = "turn by";
                        break;
                    case BlokkTipus.Print:
                        elotag = "print";
                        break;
                    case BlokkTipus.If:
                        elotag = "if";
                        break;
                    default:
                        elotag = "while";
                        break;
                }
                elonezet.Children.Add(Szo(elotag, false));
                elonezet.Children.Add(Cip(Ertek(tbElso)));
                return;
            }

            elonezet.Children.Add(Szo("set", false));
            elonezet.Children.Add(Cip(Ertek(tbElso)));
            elonezet.Children.Add(Szo("to", false));

            var ertek = new StackPanel { Orientation = Orientation.Horizontal };
            int index = ertekIndex;
            if (index == 0)
            {
                ertek.Children.Add(Cip(Ertek(tbErtekA)));
            }
            else if (index <= Muveletek.Length)
            {
                ertek.Children.Add(Cip(Ertek(tbErtekA)));
                ertek.Children.Add(Szo(Muveletek[index - 1], false));
                ertek.Children.Add(Cip(Ertek(tbErtekB)));
            }
            else
            {
                ertek.Children.Add(Szo(index == Muveletek.Length + 1 ? "not" : "isThere", false));
                ertek.Children.Add(Cip(Ertek(tbErtekA)));
            }

            elonezet.Children.Add(new Border
            {
                Child = ertek,
                Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xE8, 0xB3)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xA9, 0x3B)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(6, 1, 0, 1)
            });
        }

        private void ErtekMezokFrissitese()
        {
            int index = ertekIndex;
            bool ketOperandus = index >= 1 && index <= Muveletek.Length;

            tbElotag.Text = index == Muveletek.Length + 1 ? "not" : index == Muveletek.Length + 2 ? "isThere" : string.Empty;
            tbElotag.Visibility = tbElotag.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
            tbMuvelet.Text = ketOperandus ? Muveletek[index - 1] : string.Empty;
            tbMuvelet.Visibility = ketOperandus ? Visibility.Visible : Visibility.Collapsed;
            tbErtekB.Visibility = ketOperandus ? Visibility.Visible : Visibility.Collapsed;
            ElonezetFrissitese();
        }

        private string[] ErtekSzavak()
        {
            int index = ertekIndex;
            string a = tbErtekA.Text.Trim();
            string b = tbErtekB.Text.Trim();

            if (a.Length == 0 || (index >= 1 && index <= Muveletek.Length && b.Length == 0))
            {
                throw new ArgumentException("Töltsd ki az érték mezőit.");
            }

            if (index == 0) return new[] { a };
            if (index <= Muveletek.Length) return new[] { a, Muveletek[index - 1], b };
            return new[] { index == Muveletek.Length + 1 ? "not" : "isThere", a };
        }

        private string[] Szavak()
        {
            if (tipus == BlokkTipus.Set)
            {
                return new[] { tbElso.Text.Trim() }.Concat(ErtekSzavak()).ToArray();
            }
            return tbElso.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] szavak = Szavak();
                if (szerkesztett != null)
                {
                    szerkesztett.SzavakCsere(szavak);
                    Eredmeny = szerkesztett;
                }
                else
                {
                    Eredmeny = new Blokk(tipus, szavak);
                }
                DialogResult = true;
            }
            catch (ArgumentException hiba)
            {
                tbHiba.Text = hiba.Message;
            }
        }
    }
}
