using Contract;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace Circle2D
{
    public class Circle2D : IShape
    {
        private UIElement _circle;
        private Point2D _leftTop = new Point2D();
        private Point2D _rightBottom = new Point2D();
        public string TextContent { get; set; }
        public Point2D LeftTop
        {
            get => _leftTop;
            set
            {
                if (_leftTop != value)
                {
                    _leftTop = value;
                }
            }
        }

        public Point2D RightBottom
        {
            get => _rightBottom;
            set
            {
                if (_rightBottom != value)
                {
                    _rightBottom = value;
                }
            }
        }

        public Point2D GetStart()
        {
            return _leftTop;
        }

        public Point2D GetEnd()
        {
            return _rightBottom;
        }

        public void SetInCanvas()
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);

            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            var width = right - left;
            var height = bottom - top;

            Canvas.SetLeft(_circle, left);
            Canvas.SetTop(_circle, top);
        }

        public SolidColorBrush ColorStroke { get; set; }
        public SolidColorBrush ColorFill { get; set; }

        public void UpdateColorStroke(SolidColorBrush colorStroke)
        {
            if (colorStroke != null)
            {

                ColorStroke = colorStroke;
                if (_circle != null)
                {
                    (_circle as Ellipse).Stroke = colorStroke;
                }
            }
        }

        public void UpdateColorFill(SolidColorBrush colorFill)
        {
            if (colorFill != null)
            {
                ColorFill = colorFill;
                if (_circle != null)
                {
                    (_circle as Ellipse).Fill = colorFill;
                }
            }
        }

        public int StrokeSize { get; set; }

        public void UpdateStrokeSize(int strokeSize)
        {
            StrokeSize = strokeSize;
            if (_circle != null)
            {
                (_circle as Ellipse).StrokeThickness = strokeSize;
            }
        }

        public double[] StrokeDashArray { get; set; }

        public void UpdateStrokeDashArray(double[] dashArray)
        {
            StrokeDashArray = dashArray;
            if (_circle != null)
            {
                (_circle as Ellipse).StrokeDashArray = dashArray != null ? new DoubleCollection(dashArray) : null;
            }
        }

        public string Name => "Circle";

        public UIElement Draw()
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);

            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            var width = right - left;
            var height = bottom - top;

            _circle = new Ellipse()
            {
                Width = width,
                Height = height,
                StrokeThickness = StrokeSize,
                Stroke = ColorStroke,
                Fill = ColorFill,
                StrokeDashArray = StrokeDashArray != null ? new DoubleCollection(StrokeDashArray) : null,
            };
            Canvas.SetLeft(_circle, left);
            Canvas.SetTop(_circle, top);

            return _circle;
        }

        public void HandleEnd(double x, double y)
        {
            _rightBottom.X = x;
            _rightBottom.Y = y;
            double width = Math.Abs(_rightBottom.X - _leftTop.X);
            double height = Math.Abs(_rightBottom.Y - _leftTop.Y);
            if (width < height)
            {
                if (_rightBottom.Y < _leftTop.Y)
                    _rightBottom.Y = _leftTop.Y - width;
                else
                    _rightBottom.Y = _leftTop.Y + width;
            }
            else if (width > height)
            {
                if (_rightBottom.X < _leftTop.X)
                    _rightBottom.X = _leftTop.X - height;
                else _rightBottom.X = _leftTop.X + height;
            }
        }

        public void HandleStart(double x, double y)
        {
            _leftTop.X = x;
            _leftTop.Y = y;
        }

        public Circle2D()
        {
        }

        public IShape Clone()
        {
            return (Circle2D)this.MemberwiseClone();
        }

        public IShape Create()
        {
            return new Circle2D();
        }

        public bool ContainsPoint(double x, double y)
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);

            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            var width = right - left;
            var height = bottom - top;

            double centerX = left + width / 2;
            double centerY = top + height / 2;

            double distance = Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2));
            double radius = Math.Min((_rightBottom.X - _leftTop.X) / 2, (_rightBottom.Y - _leftTop.Y) / 2);

            return distance <= radius;
        }
        public double GetTop()
        {
            return Math.Min(_rightBottom.Y, _leftTop.Y); ;
        }
        public double GetLeft()
        {
            return Math.Min(_rightBottom.X, _leftTop.X);
        }
        public double GetWidth()
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var right = Math.Max(_rightBottom.X, _leftTop.X);

            return right - left;
        }
        public double GetHeight()
        {
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            return bottom - top;
        }

        public void ChangePosition(double x, double y)
        {
            if (_circle != null)
            {
                double width = GetWidth();
                double height = GetHeight();

                double newLeft = x + 2.5;
                double newTop = y + 2.5;

                Canvas.SetLeft(_circle, newLeft);
                Canvas.SetTop(_circle, newTop);

                _leftTop.X = newLeft;
                _leftTop.Y = newTop;
                _rightBottom.X = newLeft + width;
                _rightBottom.Y = newTop + height;
            }
        }

        public void FlipHorizontally()
        {
        }

        public void FlipVertically()
        {
        }

        public void RotateRight90Degrees()
        {
        }

        public void RotateLeft90Degrees()
        {
        }
    }
}
