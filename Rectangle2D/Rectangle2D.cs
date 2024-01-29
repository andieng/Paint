using Contract;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace Rectangle2D
{
    public class Rectangle2D : IShape
    {
        private UIElement _rectangle;
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

            Canvas.SetLeft(_rectangle, left);
            Canvas.SetTop(_rectangle, top);
        }

        public SolidColorBrush ColorStroke { get; set; }
        public SolidColorBrush ColorFill { get; set; }

        public void UpdateColorStroke(SolidColorBrush colorStroke)
        {
            if (colorStroke != null)
            {
                ColorStroke = colorStroke;
                if (_rectangle != null)
                {
                    (_rectangle as Rectangle).Stroke = colorStroke;
                }
            }
        }

        public void UpdateColorFill(SolidColorBrush colorFill)
        {
            if (colorFill != null)
            {
                ColorFill = colorFill;
                if (_rectangle != null)
                {
                    (_rectangle as Rectangle).Fill = colorFill;
                }
            }
        }

        public int StrokeSize { get; set; }
        public double[] StrokeDashArray { get; set; }

        public void UpdateStrokeDashArray(double[] dashArray)
        {
            StrokeDashArray = dashArray;
            if (_rectangle != null)
            {
                (_rectangle as Rectangle).StrokeDashArray = dashArray != null ? new DoubleCollection(dashArray) : null;
            }
        }

        public void UpdateStrokeSize(int strokeSize)
        {
            StrokeSize = strokeSize;
            if (_rectangle != null)
            {
                (_rectangle as Rectangle).StrokeThickness = strokeSize;
            }
        }

        public string Name => "Rectangle";
        public UIElement Draw()
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);
            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            var width = right - left;
            var height = bottom - top;

            _rectangle = new Rectangle()
            {
                Width = width,
                Height = height,
                StrokeThickness = StrokeSize,
                Stroke = ColorStroke,
                Fill = ColorFill,
                StrokeDashArray = StrokeDashArray != null ? new DoubleCollection(StrokeDashArray) : null,
            };
            Canvas.SetLeft(_rectangle, left);
            Canvas.SetTop(_rectangle, top);
            return _rectangle;
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

        public Rectangle2D()
        {
        }

        public IShape Clone()
        {
            return (Rectangle2D)this.MemberwiseClone();
        }

        public IShape Create()
        {
            return new Rectangle2D();
        }

        public bool ContainsPoint(double x, double y)
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);

            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            return x >= left && x <= right && y >= top && y <= bottom;
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
            if (_rectangle != null)
            {
                double width = GetWidth();
                double height = GetHeight();

                double newLeft = x + 2.5;
                double newTop = y + 2.5;

                Canvas.SetLeft(_rectangle, newLeft);
                Canvas.SetTop(_rectangle, newTop);

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
            double centerX = (_leftTop.X + _rightBottom.X) / 2;
            double centerY = (_leftTop.Y + _rightBottom.Y) / 2;

            double width = GetWidth();
            double height = GetHeight();

            double newLeft = centerX - height / 2;
            double newTop = centerY - width / 2;
            double newRight = centerX + height / 2;
            double newBottom = centerY + width / 2;

            _leftTop.X = newLeft;
            _leftTop.Y = newTop;
            _rightBottom.X = newRight;
            _rightBottom.Y = newBottom;

            if (_rectangle != null && _rectangle is Rectangle rectangleElement)
            {
                rectangleElement.Width = height;
                rectangleElement.Height = width;

                Canvas.SetLeft(_rectangle, newLeft);
                Canvas.SetTop(_rectangle, newTop);
            }
        }

        public void RotateLeft90Degrees()
        {
            double centerX = (_leftTop.X + _rightBottom.X) / 2;
            double centerY = (_leftTop.Y + _rightBottom.Y) / 2;

            double width = GetWidth();
            double height = GetHeight();

            double newLeft = centerX - height / 2;
            double newTop = centerY - width / 2;
            double newRight = centerX + height / 2;
            double newBottom = centerY + width / 2;

            _leftTop.X = newLeft;
            _leftTop.Y = newTop;
            _rightBottom.X = newRight;
            _rightBottom.Y = newBottom;

            if (_rectangle != null && _rectangle is Rectangle rectangleElement)
            {
                rectangleElement.Width = height;
                rectangleElement.Height = width;

                Canvas.SetLeft(_rectangle, newLeft);
                Canvas.SetTop(_rectangle, newTop);
            }
        }
    }
}
