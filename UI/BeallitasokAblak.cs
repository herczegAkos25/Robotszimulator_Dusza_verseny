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

        private readonly TextBox[] mezok = new TextBox[Cimkek.Length];
        private readonly TextBlock tbHiba = new TextBlock();
        private readonly EnergiaBeallitasok beallitasok;

        public EnergiaBeallitasok? Eredmeny { get; private set; }

        public BeallitasokAblak(EnergiaBeallitasok beallitasok)
        {
            this.beallitasok = beallitasok;

            Title = "Energia-beállítások";
            SotetCimsor.Alkalmaz(this);
            Width = 420;
            SizeToContent = SizeToContent.Height;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(0x21, 0x25, 0x2B));

            int[] ertekek = Ertekek(beallitasok);
            var lista = new StackPanel();
            for (int i = 0; i < Cimkek.Length; i++)
            {
                var sor = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
                var mezo = new TextBox
                {
                    Width = 80,
                    Text = ertekek[i].ToString(),
                    Background = new SolidColorBrush(Color.FromRgb(0x28, 0x2C, 0x34)),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0x3F, 0x4B)),
                    CaretBrush = Brushes.White,
                    Padding = new Thickness(4),
                    HorizontalContentAlignment = HorizontalAlignment.Right
                };
                DockPanel.SetDock(mezo, Dock.Right);
                mezok[i] = mezo;

                sor.Children.Add(mezo);
                sor.Children.Add(new TextBlock
                {
                    Text = Cimkek[i],
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)),
                    VerticalAlignment = VerticalAlignment.Center
                });
                lista.Children.Add(sor);
            }

            var panel = new StackPanel { Margin = new Thickness(14) };
            panel.Children.Add(new ScrollViewer { Content = lista, MaxHeight = 480, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 0, 0, 10) });

            tbHiba.Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75));
            tbHiba.TextWrapping = TextWrapping.Wrap;
            tbHiba.Margin = new Thickness(0, 0, 0, 10);
            panel.Children.Add(tbHiba);

            var gombok = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var alap = new Button { Content = "Alapértékek", Padding = new Thickness(14, 6, 14, 6), Margin = new Thickness(0, 0, 8, 0), Style = (Style)Application.Current.FindResource("DarkButton") };
            var ok = new Button { Content = "OK", IsDefault = true, Padding = new Thickness(18, 6, 18, 6), Margin = new Thickness(0, 0, 8, 0), Style = (Style)Application.Current.FindResource("RunButton") };
            var megse = new Button { Content = "Mégse", IsCancel = true, Padding = new Thickness(14, 6, 14, 6), Style = (Style)Application.Current.FindResource("DarkButton") };
            alap.Click += Alapertekek_Click;
            ok.Click += Ok_Click;
            gombok.Children.Add(alap);
            gombok.Children.Add(ok);
            gombok.Children.Add(megse);
            panel.Children.Add(gombok);

            Content = panel;
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
            tbHiba.Text = string.Empty;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var ertekek = new int[mezok.Length];
            for (int i = 0; i < mezok.Length; i++)
            {
                if (!int.TryParse(mezok[i].Text.Trim(), out ertekek[i]))
                {
                    tbHiba.Text = "A(z) " + Cimkek[i] + " mezőbe egész számot írj.";
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
                tbHiba.Text = hiba;
                return;
            }

            Eredmeny = uj;
            DialogResult = true;
        }
    }
}
