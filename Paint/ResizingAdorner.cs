using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Paint
{
    public class ResizingAdorner : Adorner
    {
        private VisualCollection visualChildren;
        private Thumb topLeft, topRight, bottomLeft, bottomRight, rotateThumb;
        Rectangle Rec;

        public ResizingAdorner(UIElement adornedElement) : base(adornedElement)
        {
            visualChildren = new VisualCollection(this);
            
            BuildAdornerCorner(ref topLeft, Cursors.SizeNWSE);
            BuildAdornerCorner(ref topRight, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomLeft, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomRight, Cursors.SizeNWSE);

            BuildAdornerRotateHandle(ref rotateThumb, Cursors.Hand);

            Rec = new Rectangle()
            {
                Stroke = Brushes.Blue,
                StrokeDashArray = new DoubleCollection() { 5, 5 },
                StrokeThickness = 1,
                StrokeDashCap = PenLineCap.Round,
            };
            visualChildren.Add(Rec);

            topLeft.DragDelta += HandleTopLeft;
            topRight.DragDelta += HandleTopRight;
            bottomLeft.DragDelta += HandleBottomLeft;
            bottomRight.DragDelta += HandleBottomRight;
            rotateThumb.DragDelta += HandleRotate;

            topLeft.DragCompleted += HandleDragCompleted;
            topRight.DragCompleted += HandleDragCompleted;
            bottomLeft.DragCompleted += HandleDragCompleted;
            bottomRight.DragCompleted += HandleDragCompleted;
            rotateThumb.DragCompleted += HandleDragCompleted;
        }

        private void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
        {
            cornerThumb = new Thumb() { Background = Brushes.Blue, Height = 7, Width = 7 };
            cornerThumb.Cursor = customizedCursor;
            cornerThumb.Height = cornerThumb.Width = 10;
            cornerThumb.Opacity = 0.5;
            visualChildren.Add(cornerThumb);
        }

        private void BuildAdornerRotateHandle(ref Thumb rotateThumb, Cursor customizedCursor)
        {
            rotateThumb = new Thumb();
            rotateThumb.Cursor = customizedCursor;
            rotateThumb.Height = rotateThumb.Width = 10;
            rotateThumb.Opacity = 0.5;
            visualChildren.Add(rotateThumb);
        }

        private void HandleTopLeft(object sender, DragDeltaEventArgs e)
        {
            if (AdornedElement is Line line)
            {
                
            }
            else
            {
                var element = (FrameworkElement)AdornedElement;
                double oldWidth = element.ActualWidth;
                double oldHeight = element.ActualHeight;

                Canvas.SetLeft(element, Canvas.GetLeft(element) + e.HorizontalChange);
                Canvas.SetTop(element, Canvas.GetTop(element) + e.VerticalChange);

                double newWidth = oldWidth - e.HorizontalChange;
                double newHeight = oldHeight - e.VerticalChange;

                if (newWidth > 0 && newHeight > 0)
                {
                    element.Width = newWidth;
                    element.Height = newHeight;
                }
            }
        }

        private void HandleTopRight(object sender, DragDeltaEventArgs e)
        {
            var element = (FrameworkElement)AdornedElement;
            double oldWidth = element.ActualWidth;
            double oldHeight = element.ActualHeight;

            Canvas.SetTop(element, Canvas.GetTop(element) + e.VerticalChange);

            double newWidth = oldWidth + e.HorizontalChange;
            double newHeight = oldHeight - e.VerticalChange;

            if (newWidth > 0 && newHeight > 0)
            {
                element.Width = newWidth;
                element.Height = newHeight;
            }
        }

        private void HandleBottomLeft(object sender, DragDeltaEventArgs e)
        {
            var element = (FrameworkElement)AdornedElement;
            double oldWidth = element.ActualWidth;
            double oldHeight = element.ActualHeight;

            Canvas.SetLeft(element, Canvas.GetLeft(element) + e.HorizontalChange);

            double newWidth = oldWidth - e.HorizontalChange;
            double newHeight = oldHeight + e.VerticalChange;

            if (newWidth > 0 && newHeight > 0)
            {
                element.Width = newWidth;
                element.Height = newHeight;
            }
        }

        private void HandleBottomRight(object sender, DragDeltaEventArgs e)
        {
            var element = (FrameworkElement)AdornedElement;
            double oldWidth = element.ActualWidth;
            double oldHeight = element.ActualHeight;

            double newWidth = oldWidth + e.HorizontalChange;
            double newHeight = oldHeight + e.VerticalChange;

            if (newWidth > 0 && newHeight > 0)
            {
                element.Width = newWidth;
                element.Height = newHeight;
            }
        }

        private void HandleRotate(object sender, DragDeltaEventArgs e)
        {
            var element = (FrameworkElement)AdornedElement;
            double deltaAngle = Math.Atan2(e.VerticalChange, e.HorizontalChange) * (180.0 / Math.PI);

            RotateTransform rotateTransform = new RotateTransform(deltaAngle, element.ActualWidth / 2, element.ActualHeight / 2);
            element.RenderTransform = rotateTransform;
        }

        private void HandleDragCompleted(object sender, DragCompletedEventArgs e)
        {

        }


        protected override int VisualChildrenCount
        {
            get { return visualChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualChildren[index];
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double adornerWidth = finalSize.Width;
            double adornerHeight = finalSize.Height;

            if (AdornedElement is Line line)
            {
                double lineX1 = line.X1;
                double lineY1 = line.Y1;
                double lineX2 = line.X2;
                double lineY2 = line.Y2;

                double left = Math.Min(lineX1, lineX2) ;
                double top = Math.Min(lineY1, lineY2)   ;
                double width = Math.Abs(lineX2 - lineX1) ;
                double height = Math.Abs(lineY2 - lineY1);

                Rec.Arrange(new Rect(left, top, width, height));
                topLeft.Arrange(new Rect(left - 5, top - 5, 10, 10));
                topRight.Arrange(new Rect(left + width - 5, top - 5, 10, 10));
                bottomLeft.Arrange(new Rect(left - 5, top + height - 5, 10, 10));
                bottomRight.Arrange(new Rect(left + width - 5, top + height - 5, 10, 10));
                rotateThumb.Arrange(new Rect(left + width / 2 - 5, top - 15, 10, 10));
            }
            else
            {
                Rec.Arrange(new Rect(-1.5, -1.5, adornerWidth + 3, adornerHeight + 3));

                topLeft.Arrange(new Rect(-5, -5, 10, 10));
                topRight.Arrange(new Rect(adornerWidth - 5, -5, 10, 10));
                bottomLeft.Arrange(new Rect(-5, adornerHeight - 5, 10, 10));
                bottomRight.Arrange(new Rect(adornerWidth - 5, adornerHeight - 5, 10, 10));
                rotateThumb.Arrange(new Rect(adornerWidth / 2 - 5, -15, 10, 10));
            }
            return finalSize;
        }
    }
}
