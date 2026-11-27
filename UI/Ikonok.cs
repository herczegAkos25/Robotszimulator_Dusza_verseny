using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Robotszimulator.UI
{
    public static class Ikonok
    {
        public static FrameworkElement Letrehoz(string kulcs, double meret, Brush szin, bool kitoltott = false)
        {
            var vonal = new Path
            {
                Data = (Geometry)Application.Current.FindResource(kulcs),
                Stroke = szin,
                Fill = kitoltott ? szin : null,
                StrokeThickness = 1.8,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };
            var rajz = new Canvas { Width = 24, Height = 24 };
            rajz.Children.Add(vonal);
            return new Viewbox { Width = meret, Height = meret, Child = rajz };
        }
    }
}
