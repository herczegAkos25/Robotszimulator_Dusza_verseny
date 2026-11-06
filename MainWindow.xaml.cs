using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator
{
    public partial class MainWindow : Window
    {
        private Beolvas beolvas = new Beolvas();
        private const double CellaMeret = 40; // Egy mező mérete pixelben

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                if (!File.Exists(AppSettings.InputFilePath))
                {
                    MessageBox.Show($"A fájl nem található: {Path.GetFullPath(AppSettings.InputFilePath)}");
                    return;
                }

                // Fájl beolvasása és értékek feldolgozása
                beolvas.Beolvasas();

                // Térkép kirajzolása a Canvas-re
                KirajzolTerkepCanvasen();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a beolvasás vagy rajzolás során: {ex.Message}");
            }
        }

        private void KirajzolTerkepCanvasen()
        {
            var terkep = beolvas.Terkep;
            if (terkep == null || terkep.Grid == null) return;

            TerkepCanvas.Children.Clear();

            // Canvas méretezése a térkép sor- és oszlopszáma szerint
            TerkepCanvas.Width = terkep.TerkepOszlop * CellaMeret;
            TerkepCanvas.Height = terkep.TerkepSor * CellaMeret;

            for (int r = 0; r < terkep.TerkepSor; r++)
            {
                for (int c = 0; c < terkep.TerkepOszlop; c++)
                {
                    string ertek = terkep.Grid[r][c];

                    // Cella kerete
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

                    // Színezés és karakter kiírás típus alapján
                    switch (ertek)
                    {
                        case "#": // Akadály
                            cella.Background = Brushes.DarkGray;
                            break;
                        case "@": // Arany
                            cella.Background = Brushes.Gold;
                            szoveg.Text = "★";
                            break;
                        case "<":
                        case ">":
                        case "^":
                        case "v": // Robot
                            cella.Background = Brushes.LightSkyBlue;
                            szoveg.Text = ertek;
                            break;
                        default:
                            szoveg.Text = "";
                            break;
                    }

                    cella.Child = szoveg;

                    // Koordináta szerinti elhelyezés a Canvasen
                    Canvas.SetLeft(cella, c * CellaMeret);
                    Canvas.SetTop(cella, r * CellaMeret);

                    TerkepCanvas.Children.Add(cella);
                }
            }
        }
    }
}