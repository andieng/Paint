using System;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Contract;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Line2D
{
    public class Line2D : IShape
    {
        private UIElement _line;
        private Point2D _start = new Point2D();
        private Point2D _end = new Point2D();

        public Point2D Start
        {
            get => _start;
            set
            {
                if (_start != value)
                {
                    _start = value;
                }
            }
        }

        public Point2D End
        {
            get => _end;
            set
            {
                if (_end != value)
                {
                    _end = value;
                }
            }
        }

        public SolidColorBrush ColorStroke { get; set; }
        public SolidColorBrush ColorFill { get; set; }

        public void UpdateColorStroke(SolidColorBrush colorStroke)
        {
            if (colorStroke != null)
            {
                ColorStroke = colorStroke;
                if (_line != null)
                {
                    (_line as Line).Stroke = colorStroke;
                }
            }
        }

        public void UpdateColorFill(SolidColorBrush colorFill)
        {
            if (colorFill != null)
            {
                ColorFill = colorFill;
                if (_line != null)
                {
                    (_line as Line).Fill = colorFill;
                }
            }
        }

        public int StrokeSize { get; set; }
        public double[] StrokeDashArray { get; set; }

        public void UpdateStrokeDashArray(double[] dashArray)
        {
            StrokeDashArray = dashArray;
            if (_line != null)
            {
                (_line as Line).StrokeDashArray = dashArray != null ? new DoubleCollection(dashArray) : null;
            }
        }

        public void UpdateStrokeSize(int strokeSize)
        {
            StrokeSize = strokeSize;
            if (_line != null)
            {
                (_line as Line).StrokeThickness = strokeSize;
            }
        }

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
            _line = new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = StrokeSize,
                Stroke = ColorStroke,
                StrokeDashArray = StrokeDashArray != null ? new DoubleCollection(StrokeDashArray) : null,
            };
            return _line;
        }

        public IShape Clone()
        {
            return new Line2D();
        }

        public bool ContainsPoint(double x, double y)
        {
            double distance = Math.Abs((_end.Y - _start.Y) * x - (_end.X - _start.X) * y + _end.X * _start.Y - _end.Y * _start.X)
                             / Math.Sqrt(Math.Pow(_end.Y - _start.Y, 2) + Math.Pow(_end.X - _start.X, 2));

            return distance <= StrokeSize;
        }

        public double GetTop()
        {
            return Math.Min(_start.Y, _end.Y) - StrokeSize / 2;
        }

        public double GetLeft()
        {
            return Math.Min(_start.X, _end.X) - StrokeSize / 2;
        }

        public double GetWidth()
        {
            return Math.Abs(_end.X - _start.X) + StrokeSize;
        }

        public double GetHeight()
        {
            return Math.Abs(_end.Y - _start.Y) + StrokeSize;
        }

        public void ChangePosition(double x, double y)
        {
        }
    }
}
