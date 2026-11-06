using Robotszimulator.Core;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator
{
    public partial class MainWindow : Window
    {
        private Beolvas beolvas = new Beolvas();
        private const double CellaMeret = 20;
        private Terkep terkep = new Terkep();
        private Futtato futtato = new Futtato();

        public MainWindow()
        {
            InitializeComponent();
            futtato.Betoltes();
            futtato.Inditas();         
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

                    Border cella = new Border
                    {
                        Width = CellaMeret,
                        Height = CellaMeret,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0.5),
                        Background = Brushes.White
                    };

                    TextBlock szoveg = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 18,
                        FontWeight = FontWeights.Bold
                    };

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
    }
}