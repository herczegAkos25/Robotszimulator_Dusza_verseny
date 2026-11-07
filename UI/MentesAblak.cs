using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Robotszimulator.UI
{
    public class MentesAblak : Window
    {
        private readonly CheckBox cbProgram = new CheckBox { Content = "Program (bemeneti fájl: térkép + program)", IsChecked = true, Margin = new Thickness(0, 0, 0, 8) };
        private readonly CheckBox cbNaplo = new CheckBox { Content = "Napló (kimeneti fájl)", IsChecked = true, Margin = new Thickness(0, 0, 0, 14) };

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

            var cim = new TextBlock { Text = "Mit szeretnél menteni?", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) };

            var mentes = new Button { Content = "Mentés", IsDefault = true, Padding = new Thickness(14, 4, 14, 4), Margin = new Thickness(0, 0, 8, 0) };
            mentes.Click += (kuldo, esemeny) => DialogResult = true;

            var megse = new Button { Content = "Mégse", IsCancel = true, Padding = new Thickness(14, 4, 14, 4) };

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
    }
}
