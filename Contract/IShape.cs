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
        SolidColorBrush ColorStroke { get; set; }
        SolidColorBrush ColorFill { get; set; }
        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);
        UIElement Draw();
        IShape Clone();
    }
}
