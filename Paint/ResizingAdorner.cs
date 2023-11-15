using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System;

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
                Stroke = Brushes.Black,
                StrokeDashArray = new DoubleCollection() { 2, 2 },
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
            cornerThumb = new Thumb() { Background = Brushes.Black, Height = 10, Width = 10 };
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
            // Handle resizing logic for the top-left corner
        }

        private void HandleTopRight(object sender, DragDeltaEventArgs e)
        {
            // Handle resizing logic for the top-right corner
        }

        private void HandleBottomLeft(object sender, DragDeltaEventArgs e)
        {
            // Handle resizing logic for the bottom-left corner
        }

        private void HandleBottomRight(object sender, DragDeltaEventArgs e)
        {
            // Handle resizing logic for the bottom-right corner
        }

        private void HandleRotate(object sender, DragDeltaEventArgs e)
        {
            // Handle rotation logic
        }

        private void HandleDragCompleted(object sender, DragCompletedEventArgs e)
        {
            // Handle logic after dragging is completed
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
