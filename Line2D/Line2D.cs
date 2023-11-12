using System;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Contract;

namespace Line2D
{
    public class Line2D : IShape
    {
        private Point2D _start = new Point2D();
        private Point2D _end = new Point2D();
        public SolidColorBrush ColorStroke { get; set; }
        public SolidColorBrush ColorFill { get; set; }
        public string Name => "Line";

        public void HandleEnd(double x, double y)
        {
            _end.X = x;
            _end.Y = y;
        }

        public void HandleStart(double x, double y)
        {
            _start.X = x;
            _start.Y = y;
        }
        public UIElement Draw()
        {
            return new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = 1,
                Stroke = ColorStroke,
            };
        }

        public IShape Clone()
        {
            return new Line2D();
        }
    }
}
