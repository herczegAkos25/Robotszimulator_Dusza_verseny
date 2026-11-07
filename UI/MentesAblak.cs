using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator.UI
{
    public class MentesAblak : Window
    {
        private readonly CheckBox cbProgram = new CheckBox { Content = "Program (bemeneti fájl: térkép + program)", IsChecked = true, Margin = new Thickness(0, 0, 0, 8), Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)) };
        private readonly CheckBox cbNaplo = new CheckBox { Content = "Napló (kimeneti fájl)", IsChecked = true, Margin = new Thickness(0, 0, 0, 14), Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)) };

        public bool ProgramMentes => cbProgram.IsChecked == true;
        public bool NaploMentes => cbNaplo.IsChecked == true;

        public MentesAblak(bool vanNaplo)
        {
            Title = "Mentés";
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowInTaskbar = false;
            Background = new SolidColorBrush(Color.FromRgb(0x21, 0x25, 0x2B));
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));

            if (!vanNaplo)
            {
                cbNaplo.IsChecked = false;
                cbNaplo.IsEnabled = false;
                cbNaplo.Content = "Napló (még nincs, előbb futtasd le a programot)";
            }

            var cim = new TextBlock
            {
                Text = "Mit szeretnél menteni?",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3))
            };

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

            var tartalom = new StackPanel { Margin = new Thickness(18) };
            tartalom.Children.Add(cim);
            tartalom.Children.Add(cbProgram);
            tartalom.Children.Add(cbNaplo);
            tartalom.Children.Add(gombok);

            Content = tartalom;
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