using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
        string Name { get; }
        int StrokeSize { get; set; }
        double[] StrokeDashArray { get; set; }
        SolidColorBrush ColorStroke { get; set; }
        SolidColorBrush ColorFill { get; set; }
        string TextContent { get; set; }

        void UpdateColorStroke(SolidColorBrush colorStroke);
        void UpdateColorFill(SolidColorBrush colorFill);
        void UpdateStrokeDashArray(double[] dashArray);
        void UpdateStrokeSize(int strokeSize);
        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);
        bool ContainsPoint(double x, double y);
        double GetTop();
        double GetLeft();
        double GetWidth();
        double GetHeight();
        void ChangePosition(double x, double y);
        void FlipHorizontally();
        void FlipVertically();
        void RotateRight90Degrees();
        void RotateLeft90Degrees();
        Point2D GetStart();
        Point2D GetEnd();
        void SetInCanvas();
        UIElement Draw();
        IShape Clone();

    }
}
