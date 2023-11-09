using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Contract
{
    public class Point2D : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }
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
                Stroke = new SolidColorBrush(Colors.Red)
            };
        }

        public IShape Clone()
        {
            return new Point2D();
        }
    }
}
