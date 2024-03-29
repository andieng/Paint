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
        public string TextContent { get; set; }
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

        public Point2D GetStart()
        {
            return _start;
        }

        public Point2D GetEnd()
        {
            return _end;
        }

        public void SetInCanvas()
        {
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
                    (_line as Line).Stroke = colorFill;
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

        public Line2D()
        {
        }

        public IShape Clone()
        {
            return (Line2D)this.MemberwiseClone();
        }

        public IShape Create()
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

        public enum LineDirection
        {
            None,
            TopLeftToBottomRight,
            BottomLeftToTopRight,
            TopRightToBottomLeft,
            BottomRightToTopLeft
        }

        public LineDirection direction()
        {
            if (_start.X < _end.X && _start.Y < _end.Y)
            {
                return LineDirection.TopLeftToBottomRight;
            }
            else if (_start.X < _end.X && _start.Y > _end.Y)
            {
                return LineDirection.BottomLeftToTopRight;
            }
            else if (_start.X > _end.X && _start.Y < _end.Y)
            {
                return LineDirection.TopRightToBottomLeft;
            }
            else if (_start.X > _end.X && _start.Y > _end.Y)
            {
                return LineDirection.BottomRightToTopLeft;
            }
            else
            {
                return LineDirection.None;
            }
        }

        public void ChangePosition(double x, double y)
        {
            double halfStroke = StrokeSize / 2.0;
            double newX = x + 2.5 + halfStroke;
            double newY = y + 2.5 + halfStroke;

            LineDirection lineDir = direction();

            double width = Math.Abs(_end.X - _start.X);
            double height = Math.Abs(_end.Y - _start.Y);

            switch (lineDir)
            {
                case LineDirection.TopLeftToBottomRight:
                    _start.X = newX;
                    _start.Y = newY;
                    _end.X = newX + width;
                    _end.Y = newY + height;
                    break;
                case LineDirection.BottomLeftToTopRight:
                    _start.X = newX;
                    _end.X = newX + width;
                    _end.Y = newY;
                    _start.Y = newY + height;
                    break;
                case LineDirection.TopRightToBottomLeft:
                    _start.X = newX + width;
                    _start.Y = newY;
                    _end.X = newX;
                    _end.Y = newY + height;
                    break;
                case LineDirection.BottomRightToTopLeft:
                    _end.X = newX;
                    _end.Y = newY;
                    _start.X = newX + width;
                    _start.Y = newY + height;
                    break;
                default:
                    break;
            }

            if (_line != null && _line is Line lineElement)
            {
                lineElement.X1 = _start.X;
                lineElement.Y1 = _start.Y;
                lineElement.X2 = _end.X;
                lineElement.Y2 = _end.Y;
            }
        }

        public void FlipHorizontally()
        {
            double newStartX = _end.X;
            double newEndX = _start.X;

            _start.X = newStartX;
            _end.X = newEndX;

            if (_line != null && _line is Line lineElement)
            {
                lineElement.X1 = _start.X;
                lineElement.X2 = _end.X;
            }
        }

        public void FlipVertically()
        {
            double newStartY = _end.Y;
            double newEndY = _start.Y;

            _start.Y = newStartY;
            _end.Y = newEndY;

            if (_line != null && _line is Line lineElement)
            {
                lineElement.Y1 = _start.Y;
                lineElement.Y2 = _end.Y;
            }
        }

        public void RotateRight90Degrees()
        {
            int angleDegrees = 90;
            double midX = (_start.X + _end.X) / 2;
            double midY = (_start.Y + _end.Y) / 2;

            double startXTranslated = _start.X - midX;
            double startYTranslated = _start.Y - midY;
            double endXTranslated = _end.X - midX;
            double endYTranslated = _end.Y - midY;

            double angleRadians = angleDegrees * (Math.PI / 180.0);

            double startXNew = startXTranslated * Math.Cos(angleRadians) - startYTranslated * Math.Sin(angleRadians);
            double startYNew = startXTranslated * Math.Sin(angleRadians) + startYTranslated * Math.Cos(angleRadians);
            double endXNew = endXTranslated * Math.Cos(angleRadians) - endYTranslated * Math.Sin(angleRadians);
            double endYNew = endXTranslated * Math.Sin(angleRadians) + endYTranslated * Math.Cos(angleRadians);

            _start.X = startXNew + midX;
            _start.Y = startYNew + midY;
            _end.X = endXNew + midX;
            _end.Y = endYNew + midY;

            if (_line != null && _line is Line lineElement)
            {
                lineElement.X1 = _start.X;
                lineElement.Y1 = _start.Y;
                lineElement.X2 = _end.X;
                lineElement.Y2 = _end.Y;
            }
        }

        public void RotateLeft90Degrees()
        {
            int angleDegrees = -90;
            double midX = (_start.X + _end.X) / 2;
            double midY = (_start.Y + _end.Y) / 2;

            double startXTranslated = _start.X - midX;
            double startYTranslated = _start.Y - midY;
            double endXTranslated = _end.X - midX;
            double endYTranslated = _end.Y - midY;

            double angleRadians = angleDegrees * (Math.PI / 180.0);

            double startXNew = startXTranslated * Math.Cos(angleRadians) - startYTranslated * Math.Sin(angleRadians);
            double startYNew = startXTranslated * Math.Sin(angleRadians) + startYTranslated * Math.Cos(angleRadians);
            double endXNew = endXTranslated * Math.Cos(angleRadians) - endYTranslated * Math.Sin(angleRadians);
            double endYNew = endXTranslated * Math.Sin(angleRadians) + endYTranslated * Math.Cos(angleRadians);

            _start.X = startXNew + midX;
            _start.Y = startYNew + midY;
            _end.X = endXNew + midX;
            _end.Y = endYNew + midY;

            if (_line != null && _line is Line lineElement)
            {
                lineElement.X1 = _start.X;
                lineElement.Y1 = _start.Y;
                lineElement.X2 = _end.X;
                lineElement.Y2 = _end.Y;
            }
        }
    }
}
