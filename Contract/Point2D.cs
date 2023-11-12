using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Contract
{
    public class Point2D : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }

        public SolidColorBrush ColorStroke { get; set; }
        public SolidColorBrush ColorFill { get; set; }

        public string Name => "Point";

        public void HandleEnd(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void HandleStart(double x, double y)
        {
            X = x;
            Y = y;
        }

        public UIElement Draw()
        {
            return new Line()
            {
                X1 = X,
                Y1 = Y,
                X2 = X,
                Y2 = Y,
                StrokeThickness = 1,
                Stroke = ColorStroke,
            };
        }

        public IShape Clone()
        {
            return new Point2D();
        }
    }
}
