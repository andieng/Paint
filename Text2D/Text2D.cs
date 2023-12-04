using Contract;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Text2D
{
    public class Text2D : IShape
    {
        private UIElement _text;
        public string TextContent { get; set; }

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

            Canvas.SetLeft(_text, left);
            Canvas.SetTop(_text, top);
        }

        public SolidColorBrush ColorStroke  { get; set; }
        public SolidColorBrush ColorFill { get; set; }

        public void UpdateColorStroke(SolidColorBrush colorStroke)
        {
            if (colorStroke != null)
            {
                ColorStroke = colorStroke;
                if (_text != null)
                {
                    (_text as TextBox).Foreground = colorStroke;
                }
            }
        }

        public void UpdateColorFill(SolidColorBrush colorFill)
        {

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

 
        public UIElement Draw()
        {
            var left = Math.Min(_rightBottom.X, _leftTop.X);
            var top = Math.Min(_rightBottom.Y, _leftTop.Y);

            var right = Math.Max(_rightBottom.X, _leftTop.X);
            var bottom = Math.Max(_rightBottom.Y, _leftTop.Y);

            var width = right - left;
            var height = bottom - top;

            var textBox = new TextBox()
            {
                Width = width,
                Foreground = ColorFill,
                IsReadOnly = false,
                BorderThickness = new Thickness(1),
                Text = TextContent,
                TextWrapping = TextWrapping.Wrap,
                Height = height,
                Background = Brushes.Transparent,
                AcceptsReturn = true,
                AcceptsTab = true,
                BorderBrush = ColorStroke
            };

            textBox.TextChanged += (s, args) =>
            {
                TextContent = textBox.Text;
                int lineCount = textBox.LineCount;
                if (lineCount * 18 > textBox.Height)
                {
                    textBox.Height = double.NaN;
                    return;
                };
            };

            Canvas.SetLeft(textBox, left);
            Canvas.SetTop(textBox, top);

            _text = textBox;
            return _text;
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

        public void FlipHorizontally()
        {
            // unsupported error
        }

        public void FlipVertically()
        {
            // unsupported error
        }

        public void RotateLeft90Degrees()
        {
            
        }

        public void RotateRight90Degrees()
        {

        }
    }
}
