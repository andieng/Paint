using Contract;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace Text2D
{
    public class Text2D : IShape
    {
        private UIElement _text;
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

        public void UpdateColorStroke(SolidColorBrush colorStroke)
        {
        }

        public void UpdateColorFill(SolidColorBrush colorFill)
        {
            if (colorFill != null)
            {
                ColorFill = colorFill;
                if (_text != null)
                {
                    (_text as TextBox).Foreground = colorFill;
                }
            }
        }

        public int StrokeSize { get; set; }
        public double[] StrokeDashArray { get; set; }

        public void UpdateStrokeDashArray(double[] dashArray)
        {
        }

        public void UpdateStrokeSize(int strokeSize)
        {
        }

        public string Name => "Text";
        public string TextContent {get;set;}

 
        public UIElement Draw()
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);

            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            var width = right - left;
            var height = bottom - top;

            _text = new TextBox()
            {
                Width = width,
                Foreground = ColorFill,
                IsReadOnly = false, // Allow user input
                //BorderBrush = ColorFill, // Add a border for better visibility
                BorderThickness = new Thickness(1),
                Text = TextContent,
                TextWrapping = TextWrapping.Wrap,
                Height = double.NaN,
                Background = Brushes.Transparent
            };
            _text.LostFocus += (s, args) =>
            {
                (_text as TextBox).BorderBrush = Brushes.Transparent;
                TextContent = "";
            };
            Canvas.SetLeft(_text, left);
            Canvas.SetTop(_text, top);
            _text.Focus();
            return _text;
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

        public IShape Clone()
        {
            return new Text2D();
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
            if (_text != null)
            {
                double width = GetWidth();
                double height = GetHeight();

                double newLeft = x + 2.5;
                double newTop = y + 2.5;

                Canvas.SetLeft(_text, newLeft);
                Canvas.SetTop(_text, newTop);

                _leftTop.X = newLeft;
                _leftTop.Y = newTop;
                _rightBottom.X = newLeft + width;
                _rightBottom.Y = newTop + height;
            }
        }

    }
}
