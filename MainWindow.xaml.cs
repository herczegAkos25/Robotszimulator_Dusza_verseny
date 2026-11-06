using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Robotszimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Beolvas beolvas = new Beolvas();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            beolvas.Beolvasas();
            foreach (var item in beolvas.TerkepLista)
            {
                InputTextBox.Text += item + "\n";
            }
            foreach (var item in beolvas.ProgramLista)
            {
                InputTextBox.Text += item + "\n";
            }
        }
    }
}