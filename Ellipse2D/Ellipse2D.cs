using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Contract;

namespace Ellipse2D
{
    public class Ellipse2D : IShape
    {
        private UIElement _ellipse;
        private Point2D _leftTop = new Point2D();
        private Point2D _rightBottom = new Point2D();

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

        public SolidColorBrush ColorStroke { get; set; }
        public SolidColorBrush ColorFill { get; set; }
        public int StrokeSize { get; set; }
        public double[] StrokeDashArray { get; set; }

        public void UpdateStrokeDashArray(double[] dashArray)
        {
            StrokeDashArray = dashArray;
            if (_ellipse != null)
            {
                (_ellipse as Ellipse).StrokeDashArray = dashArray != null ? new DoubleCollection(dashArray) : null;
            }
        }

        public void UpdateStrokeSize(int strokeSize)
        {
            StrokeSize = strokeSize;
            if (_ellipse != null)
            {
                (_ellipse as Ellipse).StrokeThickness = strokeSize;
            }
        }

        public string Name => "Ellipse";

        public UIElement Draw()
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);

            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            var width = right - left;
            var height = bottom - top;

            _ellipse = new Ellipse()
            {
                Width = width,
                Height = height,
                StrokeThickness = StrokeSize,
                Stroke = ColorStroke,
                Fill = ColorFill,
                StrokeDashArray = StrokeDashArray != null ? new DoubleCollection(StrokeDashArray) : null,
            };
            Canvas.SetLeft(_ellipse, left);
            Canvas.SetTop(_ellipse, top);

            return _ellipse;
        }

        public void HandleEnd(double x, double y)
        {
            _rightBottom.X = x;
            _rightBottom.Y = y;
        }

        public void HandleStart(double x, double y)
        {
            _leftTop.X = x;
            _leftTop.Y = y;
        }

        public IShape Clone()
        {
            return new Ellipse2D();
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

            double radiusX = width / 2;
            double radiusY = height / 2;

            return (Math.Pow((x - centerX) / radiusX, 2) + Math.Pow((y - centerY) / radiusY, 2)) <= 1;
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
            if (_ellipse != null)
            {
                double width = GetWidth();
                double height = GetHeight();

                double newLeft = x + 2.5;
                double newTop = y + 2.5;

                Canvas.SetLeft(_ellipse, newLeft);
                Canvas.SetTop(_ellipse, newTop);

                _leftTop.X = newLeft;
                _leftTop.Y = newTop;
                _rightBottom.X = newLeft + width;
                _rightBottom.Y = newTop + height;
            }
        }
    }
}
