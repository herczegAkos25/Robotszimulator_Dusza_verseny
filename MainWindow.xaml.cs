using Robotszimulator.Core;
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
using Point = System.Drawing.Point;

namespace Robotszimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Beolvas beolvas = new Beolvas();
        private Terkep terkep = new Terkep();
        private Futtato futtato = new Futtato();

        public MainWindow()
        {
            InitializeComponent();
            futtato.Betoltes(); 
        }
    }
}