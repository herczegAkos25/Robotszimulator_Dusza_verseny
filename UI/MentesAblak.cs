using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator.UI
{
    public class MentesAblak : Window
    {
        private readonly CheckBox cbProgram = new CheckBox { IsChecked = true, VerticalAlignment = VerticalAlignment.Center };
        private readonly CheckBox cbNaplo = new CheckBox { IsChecked = true, VerticalAlignment = VerticalAlignment.Center };

        public bool ProgramMentes => cbProgram.IsChecked == true;
        public bool NaploMentes => cbNaplo.IsChecked == true;

        public MentesAblak(bool vanNaplo)
        {
            Title = "Mentés";
            SotetCimsor.Alkalmaz(this);
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
            Background = new SolidColorBrush(Color.FromRgb(0x21, 0x25, 0x2B));
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));

            cbProgram.Style = (Style)Application.Current.FindResource("Pipa");
            cbNaplo.Style = (Style)Application.Current.FindResource("Pipa");

            if (!vanNaplo)
            {
                cbNaplo.IsChecked = false;
                cbNaplo.IsEnabled = false;
            }

            var cim = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
            cim.Children.Add(Ikonok.Letrehoz("IkonMentes", 20, new SolidColorBrush(Color.FromRgb(0x61, 0xAF, 0xEF))));
            cim.Children.Add(new TextBlock
            {
                Text = "Mit szeretnél menteni?",
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3))
            });

            Border programKartya = Kartya("IkonLista", Color.FromRgb(0x98, 0xC3, 0x79), "Program", "Bemeneti fájl: a térkép és a program", cbProgram);
            Border naploKartya = Kartya("IkonMappa", Color.FromRgb(0xE5, 0xC0, 0x7B), "Napló", vanNaplo ? "Kimeneti fájl: a futás naplója" : "Még nincs napló, előbb futtasd le a programot", cbNaplo);
            naploKartya.Margin = new Thickness(0, 0, 0, 14);

            // MENTÉS GOMB (Sárga stílus)
            var mentes = new Button
            {
                Content = "Mentés",
                IsDefault = true,
                Padding = new Thickness(20, 10, 20, 10),
                Width = 75,
                Height = 30,
                FontSize = 14,
                Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush(Color.FromRgb(0xD1, 0x9A, 0x66)), // Sárga
                Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E)), // Sötét szöveg
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand,
                Template = CreateButtonTemplate(Color.FromRgb(0xD1, 0x9A, 0x66), Color.FromRgb(0xE5, 0xC0, 0x7B))
            };
            mentes.Click += (kuldo, esemeny) => DialogResult = true;

            // MÉGSE GOMB (Sötét szürke stílus - a kért szürke hoverrel)
            var megse = new Button
            {
                Content = "Mégse",
                IsCancel = true,
                Padding = new Thickness(20, 10, 20, 10),
                Width = 75,
                Height = 30,
                FontSize = 14,
                Background = new SolidColorBrush(Color.FromRgb(0x28, 0x2C, 0x34)), // Alap sötét
                Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)), // Fehér szöveg
                Cursor = System.Windows.Input.Cursors.Hand,
                Template = CreateButtonTemplate(Color.FromRgb(0x28, 0x2C, 0x34), Color.FromRgb(0x3A, 0x3F, 0x4B)) // Hover szürke
            };

            var gombok = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            gombok.Children.Add(mentes);
            gombok.Children.Add(megse);

            var tartalom = new StackPanel { Margin = new Thickness(18), MinWidth = 380 };
            tartalom.Children.Add(cim);
            tartalom.Children.Add(programKartya);
            tartalom.Children.Add(naploKartya);
            tartalom.Children.Add(gombok);

            Content = tartalom;
        }

        private static Border Kartya(string ikonKulcs, Color szin, string cim, string alcim, CheckBox pipa)
        {
            var ikonHatter = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(9),
                Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2E, 0x37)),
                Child = Ikonok.Letrehoz(ikonKulcs, 19, new SolidColorBrush(szin))
            };

            var szovegek = new StackPanel { Margin = new Thickness(12, 0, 12, 0), VerticalAlignment = VerticalAlignment.Center };
            szovegek.Children.Add(new TextBlock { Text = cim, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)) });
            szovegek.Children.Add(new TextBlock { Text = alcim, FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(0x7D, 0x85, 0x94)) });

            var sor = new Grid();
            sor.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            sor.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sor.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(szovegek, 1);
            Grid.SetColumn(pipa, 2);
            sor.Children.Add(ikonHatter);
            sor.Children.Add(szovegek);
            sor.Children.Add(pipa);

            var kartya = new Border
            {
                Child = sor,
                Background = new SolidColorBrush(Color.FromRgb(0x26, 0x2A, 0x32)),
                CornerRadius = new CornerRadius(9),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = System.Windows.Input.Cursors.Hand,
                Opacity = pipa.IsEnabled ? 1 : 0.45
            };
            kartya.MouseLeftButtonUp += (kuldo, esemeny) =>
            {
                if (pipa.IsEnabled)
                {
                    pipa.IsChecked = pipa.IsChecked != true;
                }
            };

            return kartya;
        }

        private ControlTemplate CreateButtonTemplate(Color normalColor, Color hoverColor)
        {
            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border), "Border");
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(normalColor));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentPresenter);

            template.VisualTree = borderFactory;

            var trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(hoverColor), "Border"));
            template.Triggers.Add(trigger);

            return template;
        }
    }
}