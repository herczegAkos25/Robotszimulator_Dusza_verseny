using Robotszimulator.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator.UI
{
    public class BeallitasokAblak : Window
    {
        private static readonly string[] Cimkek =
        {
            "Akkumulátor kapacitása (Wh)",
            "Fúrófej élettartama (fúrás)",
            "Lépés fogyasztása (cellánként)",
            "Fordulás fogyasztása (negyedfordulatonként)",
            "Bányászás fogyasztása",
            "isThere fogyasztása",
            "Fúrás fogyasztása",
            "Támasz lerakásának fogyasztása",
            "Támasz felvételének fogyasztása",
            "Töltőállomás lerakásának fogyasztása",
            "Töltőállomás felvételének fogyasztása",
            "Új fúrófej vételének fogyasztása",
            "get type fogyasztása",
            "get here fogyasztása",
            "get energy fogyasztása",
            "get durability fogyasztása",
            "Egy töltés mennyisége (Wh)"
        };

        private static readonly string[] RovidCimkek =
        {
            "Kapacitás",
            "Élettartam",
            "Lépés (cellánként)",
            "Fordulás (negyedfordulatonként)",
            "Bányászás",
            "isThere",
            "Fúrás",
            "Támasz lerakása",
            "Támasz felvétele",
            "Töltőállomás lerakása",
            "Töltőállomás felvétele",
            "Új fúrófej vétele",
            "get type",
            "get here",
            "get energy",
            "get durability",
            "Egy töltés mennyisége"
        };

        private static readonly SolidColorBrush Szoveg = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));
        private static readonly SolidColorBrush Halvany = new SolidColorBrush(Color.FromRgb(0x7D, 0x85, 0x94));
        private static readonly SolidColorBrush Piros = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75));
        private static readonly SolidColorBrush MezoKeret = new SolidColorBrush(Color.FromRgb(0x3A, 0x3F, 0x4B));

        private readonly TextBox[] mezok = new TextBox[Cimkek.Length];
        private readonly TextBlock tbHiba = new TextBlock();
        private readonly Border hibaDoboz = new Border();
        private readonly EnergiaBeallitasok beallitasok;

        public EnergiaBeallitasok? Eredmeny { get; private set; }

        public BeallitasokAblak(EnergiaBeallitasok beallitasok)
        {
            this.beallitasok = beallitasok;

            Title = "Energia-beállítások";
            SotetCimsor.Alkalmaz(this);
            Width = 700;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(0x26, 0x2A, 0x32));

            int[] ertekek = Ertekek(beallitasok);

            var bal = new StackPanel();
            bal.Children.Add(Csoport("IkonAkku", "Akkumulátor", Color.FromRgb(0x98, 0xC3, 0x79), new[] { 0, 16 }, ertekek));
            bal.Children.Add(Csoport("IkonFuro", "Fúrófej", Color.FromRgb(0x61, 0xAF, 0xEF), new[] { 1, 11 }, ertekek));
            bal.Children.Add(Csoport("IkonMozgas", "Mozgás", Color.FromRgb(0xE5, 0xC0, 0x7B), new[] { 2, 3 }, ertekek));

            var jobb = new StackPanel();
            jobb.Children.Add(Csoport("IkonMunka", "Munka", Color.FromRgb(0xD1, 0x9A, 0x66), new[] { 4, 6, 7, 8, 9, 10 }, ertekek));
            jobb.Children.Add(Csoport("IkonErzekeles", "Érzékelés", Color.FromRgb(0x56, 0xB6, 0xC2), new[] { 5, 12, 13, 14, 15 }, ertekek));

            var oszlopok = new Grid();
            oszlopok.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            oszlopok.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            oszlopok.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(bal, 0);
            Grid.SetColumn(jobb, 2);
            oszlopok.Children.Add(bal);
            oszlopok.Children.Add(jobb);

            var panel = new StackPanel { Margin = new Thickness(16) };
            panel.Children.Add(Fejlec());
            panel.Children.Add(new ScrollViewer { Content = oszlopok, MaxHeight = 520, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 0, 0, 10) });

            panel.Children.Add(new TextBlock
            {
                Text = "A 0 azt jelenti, hogy a művelet nem fogyaszt energiát.",
                Foreground = Halvany,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 10)
            });

            tbHiba.Foreground = Piros;
            tbHiba.TextWrapping = TextWrapping.Wrap;
            tbHiba.VerticalAlignment = VerticalAlignment.Center;
            tbHiba.Margin = new Thickness(8, 0, 0, 0);
            var hibaSor = new StackPanel { Orientation = Orientation.Horizontal };
            hibaSor.Children.Add(Ikonok.Letrehoz("IkonFigyelmeztetes", 15, Piros));
            hibaSor.Children.Add(tbHiba);
            hibaDoboz.Child = hibaSor;
            hibaDoboz.Background = new SolidColorBrush(Color.FromArgb(0x20, 0xE0, 0x6C, 0x75));
            hibaDoboz.BorderBrush = new SolidColorBrush(Color.FromArgb(0x70, 0xE0, 0x6C, 0x75));
            hibaDoboz.BorderThickness = new Thickness(1);
            hibaDoboz.CornerRadius = new CornerRadius(6);
            hibaDoboz.Padding = new Thickness(10, 7, 10, 7);
            hibaDoboz.Margin = new Thickness(0, 0, 0, 10);
            hibaDoboz.Visibility = Visibility.Collapsed;
            panel.Children.Add(hibaDoboz);

            var gombok = new DockPanel();
            var alap = new Button { Padding = new Thickness(12, 6, 12, 6), Style = (Style)Application.Current.FindResource("DarkButton") };
            var alapTartalom = new StackPanel { Orientation = Orientation.Horizontal };
            alapTartalom.Children.Add(Ikonok.Letrehoz("IkonVisszaallit", 15, Szoveg));
            alapTartalom.Children.Add(new TextBlock { Text = "Alapértékek", Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center });
            alap.Content = alapTartalom;
            var ok = new Button { Content = "OK", IsDefault = true, Padding = new Thickness(22, 6, 22, 6), Margin = new Thickness(0, 0, 8, 0), Style = (Style)Application.Current.FindResource("RunButton") };
            var megse = new Button { Content = "Mégse", IsCancel = true, Padding = new Thickness(16, 6, 16, 6), Style = (Style)Application.Current.FindResource("DarkButton") };
            alap.Click += Alapertekek_Click;
            ok.Click += Ok_Click;
            var jobbGombok = new StackPanel { Orientation = Orientation.Horizontal };
            jobbGombok.Children.Add(ok);
            jobbGombok.Children.Add(megse);
            DockPanel.SetDock(jobbGombok, Dock.Right);
            gombok.Children.Add(jobbGombok);
            gombok.Children.Add(alap);
            alap.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(gombok);

            Content = panel;
        }

        private static UIElement Fejlec()
        {
            var ikonHatter = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(9),
                Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2E, 0x37)),
                Child = Ikonok.Letrehoz("IkonVilam", 19, new SolidColorBrush(Color.FromRgb(0xD1, 0x9A, 0x66)))
            };

            var szovegek = new StackPanel { Margin = new Thickness(12, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            szovegek.Children.Add(new TextBlock { Text = "Energia-beállítások", Foreground = Szoveg, FontSize = 15, FontWeight = FontWeights.Bold });
            szovegek.Children.Add(new TextBlock { Text = "Mennyi energiába kerül egy-egy művelet", Foreground = Halvany, FontSize = 12 });

            var sor = new StackPanel { Orientation = Orientation.Horizontal };
            sor.Children.Add(ikonHatter);
            sor.Children.Add(szovegek);

            return new Border
            {
                Child = sor,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x2F, 0x34, 0x3F)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 0, 0, 10),
                Margin = new Thickness(0, 0, 0, 12)
            };
        }

        private Border Csoport(string ikonKulcs, string nev, Color szin, int[] indexek, int[] ertekek)
        {
            var tartalom = new StackPanel();

            var cim = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            cim.Children.Add(Ikonok.Letrehoz(ikonKulcs, 17, new SolidColorBrush(szin)));
            cim.Children.Add(new TextBlock { Text = nev, Foreground = Szoveg, FontWeight = FontWeights.Bold, Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center });
            tartalom.Children.Add(cim);

            foreach (int i in indexek)
            {
                var sor = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
                var mezo = new TextBox
                {
                    Width = 76,
                    Text = ertekek[i].ToString(),
                    Background = new SolidColorBrush(Color.FromRgb(0x1B, 0x1E, 0x24)),
                    Foreground = Brushes.White,
                    BorderBrush = MezoKeret,
                    CaretBrush = Brushes.White,
                    FontFamily = new FontFamily("Consolas"),
                    Padding = new Thickness(4),
                    HorizontalContentAlignment = HorizontalAlignment.Right
                };
                mezok[i] = mezo;

                var egyseg = new TextBlock
                {
                    Text = i == 1 ? "fúrás" : "Wh",
                    Foreground = Halvany,
                    FontSize = 11,
                    Width = 34,
                    Margin = new Thickness(6, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                DockPanel.SetDock(egyseg, Dock.Right);
                DockPanel.SetDock(mezo, Dock.Right);
                sor.Children.Add(egyseg);
                sor.Children.Add(mezo);
                sor.Children.Add(new TextBlock
                {
                    Text = RovidCimkek[i],
                    Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xCE, 0xD8)),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                });
                tartalom.Children.Add(sor);
            }

            return new Border
            {
                Child = tartalom,
                Background = new SolidColorBrush(Color.FromRgb(0x21, 0x25, 0x2B)),
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(14, 10, 14, 4),
                Margin = new Thickness(0, 0, 0, 10)
            };
        }

        private static int[] Ertekek(EnergiaBeallitasok b)
        {
            return new[]
            {
                b.Kapacitas, b.FuroFejElettartam, b.LepesAra, b.FordulasAra, b.BanyaszatAra, b.IsThereAra,
                b.FurasAra, b.TamaszAra, b.TamaszFelvetelAra, b.ToltoLerakasAra, b.ToltoFelvetelAra,
                b.FuroFejAra, b.GetTypeAra, b.GetHereAra, b.GetEnergyAra, b.GetDurabilityAra, b.Toltes
            };
        }

        private void Alapertekek_Click(object sender, RoutedEventArgs e)
        {
            int[] ertekek = Ertekek(new EnergiaBeallitasok());
            for (int i = 0; i < mezok.Length; i++)
            {
                mezok[i].Text = ertekek[i].ToString();
            }
            HibaTorles();
        }

        private void HibaTorles()
        {
            tbHiba.Text = string.Empty;
            hibaDoboz.Visibility = Visibility.Collapsed;
            foreach (TextBox mezo in mezok)
            {
                mezo.BorderBrush = MezoKeret;
            }
        }

        private void HibaMutat(string uzenet, TextBox? mezo)
        {
            tbHiba.Text = uzenet;
            hibaDoboz.Visibility = Visibility.Visible;
            if (mezo != null)
            {
                mezo.BorderBrush = Piros;
                mezo.Focus();
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            HibaTorles();

            var ertekek = new int[mezok.Length];
            for (int i = 0; i < mezok.Length; i++)
            {
                if (!int.TryParse(mezok[i].Text.Trim(), out ertekek[i]))
                {
                    HibaMutat("A(z) " + Cimkek[i] + " mezőbe egész számot írj.", mezok[i]);
                    return;
                }
            }

            EnergiaBeallitasok uj = beallitasok.Masolat();
            uj.Kapacitas = ertekek[0];
            uj.FuroFejElettartam = ertekek[1];
            uj.LepesAra = ertekek[2];
            uj.FordulasAra = ertekek[3];
            uj.BanyaszatAra = ertekek[4];
            uj.IsThereAra = ertekek[5];
            uj.FurasAra = ertekek[6];
            uj.TamaszAra = ertekek[7];
            uj.TamaszFelvetelAra = ertekek[8];
            uj.ToltoLerakasAra = ertekek[9];
            uj.ToltoFelvetelAra = ertekek[10];
            uj.FuroFejAra = ertekek[11];
            uj.GetTypeAra = ertekek[12];
            uj.GetHereAra = ertekek[13];
            uj.GetEnergyAra = ertekek[14];
            uj.GetDurabilityAra = ertekek[15];
            uj.Toltes = ertekek[16];

            string? hiba = uj.Hiba();
            if (hiba != null)
            {
                HibaMutat(hiba, null);
                return;
            }

            Eredmeny = uj;
            DialogResult = true;
        }
    }
}
