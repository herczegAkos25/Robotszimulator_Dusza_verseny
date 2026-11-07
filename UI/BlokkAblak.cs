using Robotszimulator.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator.UI
{
    public class BlokkAblak : Window
    {
        private readonly BlokkTipus tipus;
        private readonly Blokk? szerkesztett;
        private readonly TextBox tbElso = new TextBox();
        private readonly TextBox tbMasodik = new TextBox();
        private readonly TextBlock tbHiba = new TextBlock();

        public Blokk? Eredmeny { get; private set; }

        public BlokkAblak(BlokkTipus tipus, Blokk? szerkesztett = null)
        {
            this.tipus = tipus;
            this.szerkesztett = szerkesztett;

            Title = (szerkesztett == null ? "Új blokk: " : "Szerkesztés: ") + Nev(tipus);
            Width = 400;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(0x21, 0x25, 0x2B));

            var panel = new StackPanel { Margin = new Thickness(14) };

            if (tipus == BlokkTipus.Set)
            {
                Mezo(panel, "Változó neve:", tbElso, szerkesztett == null ? "" : szerkesztett.Szavak[0]);
                Mezo(panel, "Érték vagy kifejezés (pl. 5, x + 1, not b, isThere 2):", tbMasodik,
                    szerkesztett == null ? "" : string.Join(' ', szerkesztett.Szavak.Skip(1)));
            }
            else
            {
                Mezo(panel, Felirat(tipus), tbElso, szerkesztett == null ? "" : szerkesztett.Szavak[0]);
            }

            tbHiba.Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75));
            tbHiba.TextWrapping = TextWrapping.Wrap;
            tbHiba.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(tbHiba);

            var gombok = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var ok = new Button { Content = "OK", IsDefault = true, Padding = new Thickness(18, 5, 18, 5), Margin = new Thickness(0, 0, 8, 0) };
            var megse = new Button { Content = "Mégse", IsCancel = true, Padding = new Thickness(14, 5, 14, 5) };
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

        private string[] Szavak()
        {
            if (tipus == BlokkTipus.Set)
            {
                return new[] { tbElso.Text.Trim() }
                    .Concat(tbMasodik.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    .ToArray();
            }
            return tbElso.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string[] szavak = Szavak();

            try
            {
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
