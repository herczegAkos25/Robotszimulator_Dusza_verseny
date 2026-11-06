using Robotszimulator.Core;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator
{
    public partial class MainWindow : Window
    {
        private const double CellaMeret = 20;
        private Futtato futtato = new Futtato();

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
                KirajzolTerkepCanvasen();
                UIFrissites();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltéskor: " + ex.Message);
            }
        }

        private void UIFrissites()
        {
            var robot = futtato.Robot;
            if (robot == null) return;

            tbKibanyaszottArany.Text = robot.KibanyaszottAranySzam.ToString();

            tbIrány.Text = robot.AktualisIrany.ToString();

            try
            {
                int szenzorErtek = robot.Szenzor();
                tbSzenzor.Text = szenzorErtek switch
                {
                    0 => "Üres (.)",
                    1 => "Fal (#)",
                    2 => "Arany (@)",
                    _ => "Ismeretlen"
                };
            }
            catch
            {
                tbSzenzor.Text = "-";
            }

            if (futtato.Interpreter != null)
            {
                tbNapló.Text = string.Join(Environment.NewLine, futtato.Interpreter.Naplo);
                tbNapló.ScrollToEnd();
            }

            if (futtato.Interpreter != null)
            {
                tbKonzol.Text = string.Join(Environment.NewLine, futtato.Interpreter.Konzol);
                tbKonzol.ScrollToEnd();
            }

            KirajzolTerkepCanvasen();
        }

        private void KirajzolTerkepCanvasen()
        {
            var terkep = futtato.Terkep;
            var robot = futtato.Robot;
            if (terkep == null || terkep.Grid == null || robot == null) return;

            TerkepCanvas.Children.Clear();
            TerkepCanvas.Width = terkep.TerkepOszlop * CellaMeret;
            TerkepCanvas.Height = terkep.TerkepSor * CellaMeret;

            for (int r = 0; r < terkep.TerkepSor; r++)
            {
                for (int c = 0; c < terkep.TerkepOszlop; c++)
                {
                    string ertek = terkep.Grid[r][c];
                    bool felfedezve = terkep.Felfedezett != null && terkep.Felfedezett[r, c];

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

                    if (r == robot.Sor && c == robot.Oszlop)
                    {
                        cella.Background = Brushes.LightSkyBlue;
                        szoveg.Text = robot.AktualisIrany switch
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
            try
            {
                futtato.Inditas();
                KirajzolTerkepCanvasen();
                UIFrissites();

                foreach (var sorIndex in futtato.Interpreter.FuttatoIterator())
                {
                    UIFrissites();
                    int csuszoErtek = slSebesseg != null ? (int)slSebesseg.Value : 5;
                    int kesleltetes = Math.Max(10, 1000 - (csuszoErtek * 100));
                    await Task.Delay(kesleltetes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Futási hiba történt: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                UIFrissites();
            }
        }
    }
}