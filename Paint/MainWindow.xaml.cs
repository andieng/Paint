using Contract;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json;
using Microsoft.Win32;
using Paint.Keys;
using System.Threading.Tasks;
using Image = System.Windows.Controls.Image;
using System.Xml.Linq;
using System.Reflection;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isDrawing = false;
        private List<object> _canvasObjects = new List<object>();
        private IShape? _preview;
        private string _selectedShapeName = "";
        private ToggleButton? _selectedShapeBtn;
        private ShapeFactory _shapeFactory = ShapeFactory.Instance;
        private Color _colorStroke;
        private Color _colorFill;
        private Color _colorText;
        private bool _isFilled = false;
        private bool _hasStroke = true;
        private int _strokeSize = 1;
        private double[] _strokeDashArray;
        private bool _isSelecting = false;
        private bool _isPreviewAdded = false;
        private Rectangle _selectionFrame;
        private IShape _selectedShape;
        private int _selectedIndex;
        private Image _selectedImg;
        private object _cloneSelected;
        private bool isDragging = false;
        private Point offset;
        private Point originalPosition;
        private IShape _clipboardShape;
        private Image _clipboardImage;
        private int distance = 10;
        private bool isText = false;
        private UIElement _textShape;
        private Stack<(object?, int)> _undoStack = new Stack<(object?, int)>();
        private Stack<(object, int)> _redoStack = new Stack<(object, int)>();

        public MainWindow()
        {
            InitializeComponent();
            HotkeysManager.SetupSystemHook();

            // Save drawn objects hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.D, saveObjects));

            // Save as JPG picture hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.S, saveImage));

            // New hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.N, resetCanvas));

            // Undo hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Z, undo));

            // Redo hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Y, redo));

            // Import objects hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.O, loadObjects));

            // Import image hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.I, loadImage));

            // Copy hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.C, copy));

            // Paste hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.V, paste));

            // Cut hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.X, cut));

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            createShapeButtons();
        }

        private void createShapeButtons()
        {
            var prototypes = _shapeFactory.GetPrototypes();
            List<ToggleButton> btnList = new List<ToggleButton>();
            foreach (var item in prototypes)
            {
                var shape = item.Value as IShape;
                ToggleButton button;

                // Basic shapes: line, rectangle, ellipse, square, circle, text
                if (isBasicShape(shape))
                {
                    button = new ToggleButton()
                    {
                        ToolTip = shape.Name,
                        Style = Resources["ToggleButtonDisableStyle"] as Style,
                        Margin = new Thickness(15, 0, 0, 0),
                        Height = 35,
                        Width = 35,
                        Tag = shape
                    };

                    if (shape.Name == nameof(Shapes.Text))
                    {
                        button.Content = new Image()
                        {
                            Source = new BitmapImage(new Uri($"./Resources/{shape.Name.ToLower()}.png", UriKind.Relative)),
                            Width = 20,
                            Height = 20,
                        };
                    } else
                    {
                        button.Content = new Image()
                        {
                            Source = new BitmapImage(new Uri($"./Resources/{shape.Name.ToLower()}.png", UriKind.Relative)),
                            Width = 23,
                            Height = 23,
                        };
                    }
                }
                else
                {
                    button = new ToggleButton()
                    {
                        ToolTip = shape.Name,
                        Style = Resources["ToggleButtonPluginDisableStyle"] as Style,
                        Margin = new Thickness(23, 0, 0, 0),
                        Height = 28,
                        Content = new TextBlock()
                        {
                            Text = shape.Name,
                            Margin = new Thickness(12, 0, 12, 0),
                        },
                        Tag = shape
                    };
                }

                // Make rounded button
                var style = new Style
                {
                    TargetType = typeof(Border),
                    Setters = { new Setter { Property = Border.CornerRadiusProperty, Value = new CornerRadius(5) } }
                };
                button.Resources.Add(style.TargetType, style);
                button.Checked += prototypeButton_Checked;
                button.Unchecked += prototypeButton_Unchecked;
                btnList.Add(button);
            }
            shapesItemsControl.ItemsSource = btnList;
        }

        private void prototypeButton_Checked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button != null)
            {
                if (_selectedShapeBtn != null)
                {
                    _selectedShapeBtn.IsChecked = false;
                }
                _selectedShapeBtn = button;
                selectToggleButton.IsChecked = false;

                var shape = (IShape)button.Tag;
                _selectedShapeName = shape.Name;
                createPreviewShape();
                if (isBasicShape(shape))
                {
                    button.Style = Resources["ToggleButtonActiveStyle"] as Style;
                }
                else
                {
                    button.Style = Resources["ToggleButtonPluginActiveStyle"] as Style;
                }
            }
        }

        private void prototypeButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button != null)
            {
                _selectedShapeName = "";
                _preview = null;

                var shape = (IShape)button.Tag;
                if (isBasicShape(shape))
                {
                    button.Style = Resources["ToggleButtonDisableStyle"] as Style;
                }
                else
                {
                    button.Style = Resources["ToggleButtonPluginDisableStyle"] as Style;
                }

                _selectedShapeBtn = null;
            }
        }

        enum Shapes
        {
            Line = 1,
            Rectangle = 2,
            Ellipse = 3,
            Square = 4,
            Circle = 5,
            Text = 6
        }

        private void deleteAllSelectionFrame()
        {
            if (_selectionFrame != null)
            {
                canvas.Children.Remove(_selectionFrame);
                _selectionFrame = null;
                _selectedImg = null;
                _selectedShape = null;
                _selectedIndex = -1;
            }
        }

        private Image findImageFromName(string name)
        {
            foreach (var child in canvas.Children)
            {
                if (child is Image image && image.Name == name)
                {
                    return image;
                }
            }
            return null;
        }

        private bool IsPointInsideImage(Point point, string imgName)
        {
            Image img = findImageFromName(imgName);
            var left = Canvas.GetLeft(img);
            var top = Canvas.GetTop(img);
            var right = left + img.Width;
            var bottom = top + img.Height;
            return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
        }

        private void CreateSelectionFrame(Point position)
        {
            for (int i = _canvasObjects.Count - 1; i >= 0; i--)
            {
                var obj = _canvasObjects[i];
                if (obj.GetType() == typeof(BitmapImage) || obj.GetType() == typeof(BitmapSource))
                {
                    Image curImage = (Image)canvas.Children[i];
                    if (IsPointInsideImage(position, curImage.Name))
                    {
                        Image img = findImageFromName(curImage.Name);
                        double imageWidth = img.Width;
                        double imageHeight = img.Height;
                        _selectedImg = img;
                        _selectedIndex = i;

                        _selectionFrame = new Rectangle()
                        {
                            Stroke = Brushes.Blue,
                            StrokeDashArray = new DoubleCollection() { 4, 4 },
                            StrokeThickness = 1,
                            StrokeDashCap = PenLineCap.Round,
                            Width = imageWidth + 10,
                            Height = imageHeight + 10,
                        };
                        addEventsToSelectionFrame();

                        Canvas.SetLeft(_selectionFrame, Canvas.GetLeft(img) - 5);
                        Canvas.SetTop(_selectionFrame, Canvas.GetTop(img) - 5);

                        canvas.Children.Insert(i, _selectionFrame);
                        return;
                    }
                } 
                else if (obj is IShape shape)
                {
                    if (shape.ContainsPoint(position.X, position.Y))
                    {
                        _selectedShape = shape;
                        _selectedIndex = i;
                        _selectionFrame = new Rectangle()
                        {
                            Stroke = Brushes.Blue,
                            StrokeDashArray = new DoubleCollection() { 4, 4 },
                            StrokeThickness = 1,
                            StrokeDashCap = PenLineCap.Round,
                            Width = shape.GetWidth() + 5,
                            Height = shape.GetHeight() + 5,
                        };
                        addEventsToSelectionFrame();

                        Canvas.SetLeft(_selectionFrame, shape.GetLeft() - 2.5);
                        Canvas.SetTop(_selectionFrame, shape.GetTop() - 2.5);

                        originalPosition = new Point(Canvas.GetLeft(_selectionFrame), Canvas.GetTop(_selectionFrame));

                        canvas.Children.Insert(i, _selectionFrame);
                        break;
                    }
                }
            }
        }

        private void addEventsToSelectionFrame()
        {
            if ((_selectionFrame != null && _selectedShape != null) || (_selectionFrame != null && _selectedImg != null))
            {
                cloneSelected();

                _selectionFrame.MouseDown += (sender, e) =>
                {
                    isDragging = true;
                    offset = e.GetPosition(_selectionFrame);
                    _selectionFrame.CaptureMouse();
                };

                _selectionFrame.MouseUp += (sender, e) =>
                {
                    if (isDragging && _selectedShape != null)
                    {
                        pushUndoClearRedo(_cloneSelected);
                        Point newPosition = e.GetPosition(canvas);
                        double newX = newPosition.X - offset.X;
                        double newY = newPosition.Y - offset.Y;
                        if (newX < 0 || newY < 0 || newX > canvas.ActualWidth || newY > canvas.ActualHeight)
                        {
                            Canvas.SetLeft(_selectionFrame, originalPosition.X);
                            Canvas.SetTop(_selectionFrame, originalPosition.Y);

                            _selectedShape.ChangePosition(originalPosition.X, originalPosition.Y);
                        }
                        else
                        {
                            Canvas.SetLeft(_selectionFrame, newX);
                            Canvas.SetTop(_selectionFrame, newY);

                            _selectedShape.ChangePosition(newX, newY);

                        }
                        isDragging = false;
                        _selectionFrame.ReleaseMouseCapture();
                    }
                    else if (isDragging && _selectedImg != null)
                    {
                        pushUndoClearRedo(_cloneSelected);
                        Point newPosition = e.GetPosition(canvas);
                        double newX = newPosition.X - offset.X;
                        double newY = newPosition.Y - offset.Y;
                        double oldWidth = _selectedImg.Width + 10;
                        ChangeImagePosition(newX, newY);

                        if (_selectionFrame.Width == oldWidth)
                        {
                            Canvas.SetLeft(_selectionFrame, newX);
                            Canvas.SetTop(_selectionFrame, newY);
                        }
                        else
                        {
                            double width = _selectedImg.RenderSize.Width;
                            double height = _selectedImg.RenderSize.Height;
                            double newLeft, newTop;
                            newLeft = Canvas.GetLeft(_selectedImg) - (height - width) / 2;
                            newTop = Canvas.GetTop(_selectedImg) + (height - width) / 2;
                            Canvas.SetLeft(_selectionFrame, newLeft - 5);
                            Canvas.SetTop(_selectionFrame, newTop - 5);
                        }

                        isDragging = false;
                        _selectionFrame.ReleaseMouseCapture();
                    }
                };

                _selectionFrame.MouseMove += (sender, e) =>
                {
                    if (isDragging && _selectedShape != null)
                    {
                        Point newPosition = e.GetPosition(canvas);
                        double newX = newPosition.X - offset.X;
                        double newY = newPosition.Y - offset.Y;

                        Canvas.SetLeft(_selectionFrame, newX);
                        Canvas.SetTop(_selectionFrame, newY);

                        _selectedShape.ChangePosition(newX, newY);
                    }
                    else if (isDragging && _selectedImg != null)
                    {
                        Point newPosition = e.GetPosition(canvas);
                        double newX = newPosition.X - offset.X;
                        double newY = newPosition.Y - offset.Y;

                        double oldWidth = _selectedImg.Width + 10;
                        ChangeImagePosition(newX, newY);

                        if (_selectionFrame.Width == oldWidth)
                        {
                            Canvas.SetLeft(_selectionFrame, newX);
                            Canvas.SetTop(_selectionFrame, newY);
                        }
                        else
                        {
                            double width = _selectedImg.RenderSize.Width;
                            double height = _selectedImg.RenderSize.Height;
                            double newLeft, newTop;
                            newLeft = Canvas.GetLeft(_selectedImg) - (height - width) / 2;
                            newTop = Canvas.GetTop(_selectedImg) + (height - width) / 2;
                            Canvas.SetLeft(_selectionFrame, newLeft - 5);
                            Canvas.SetTop(_selectionFrame, newTop - 5);
                        }
                    }
                };
            }
        }

        private void ChangeImagePosition(double x, double y)
        {
            if (_selectedImg != null)
            {
                double newLeft = x + 5;
                double newTop = y + 5;

                Canvas.SetLeft(_selectedImg, newLeft);
                Canvas.SetTop(_selectedImg, newTop);
            }
        }

        private bool IsPointInsideSelectionFrame(Point point)
        {
            if (_selectionFrame == null) return false;

            if (_selectionFrame == null) return false;
            var left = Canvas.GetLeft(_selectionFrame);
            var top = Canvas.GetTop(_selectionFrame);
            var right = left + _selectionFrame.Width;
            var bottom = top + _selectionFrame.Height;
            return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool isMouseOverSelectionFrame = IsPointInsideSelectionFrame(e.GetPosition(canvas));
            if (isMouseOverSelectionFrame)
            {
                originalPosition = new Point(Canvas.GetLeft(_selectionFrame), Canvas.GetTop(_selectionFrame));
                isDragging = true;
                offset = e.GetPosition(_selectionFrame);
                _selectionFrame.CaptureMouse();
            }
            else
            {
                deleteAllSelectionFrame();
                if (_isSelecting)
                {
                    Point pos = e.GetPosition(canvas);
                    CreateSelectionFrame(pos);
                }
                else if (isText)
                {
                    if (_textShape is TextBox textBox)
                    {
                        Keyboard.ClearFocus();
                        isText = false;
                        textBox.BorderThickness = new Thickness(0);
                    }
                }
                else
                {
                    if (_preview != null)
                    {
                        Point pos = e.GetPosition(canvas);
                        _isDrawing = true;
                        _preview.StrokeSize = _strokeSize;
                        if (_hasStroke)
                        _preview.StrokeDashArray = _strokeDashArray;
                        _preview.HandleStart(pos.X, pos.Y);
                    }
                }
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing && _preview != null)
            {
                Point pos = e.GetPosition(canvas);
                _preview.HandleEnd(pos.X, pos.Y);

                if (_isPreviewAdded)
                {
                    // Remove previous preview drawing on canvas
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);
                }

                // Add preview object
                canvas.Children.Add(_preview.Draw());
                _isPreviewAdded = true;

                // Clear redo stack
                _redoStack.Clear();
                undoButton.IsEnabled = true;
                redoButton.IsEnabled = false;
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            if (_preview == null || !_isPreviewAdded)
            {
                return;
            }

            // Add last object to list
            Point pos = e.GetPosition(canvas);
            _preview.HandleEnd(pos.X, pos.Y);
            _canvasObjects.Add(_preview);
            _undoStack.Push((null, _canvasObjects.Count - 1));

            // remove preview shape
            canvas.Children.RemoveAt(canvas.Children.Count - 1);

            // draw final shape
            if (_selectedShapeName == "Text")
            {
                isText = true;
                var left = _preview.GetLeft();
                var top = _preview.GetTop();
                _textShape = _preview.Draw();
                if (_textShape is TextBox textBox)
                {
                    canvas.Children.Add(_textShape);
                    textBox.Focus();
                    textBox.Foreground = new SolidColorBrush(_colorText);

                    textBox.LostFocus += (s, args) =>
                    {
                        Keyboard.ClearFocus();
                        isText = false;
                        textBox.BorderThickness = new Thickness(0);
                        updateActualSizeForTextBox(textBox,left,top);
                        return;
                    };
                }
            }
            else
            {
                addObjectToCanvas(_preview);
            }

            // Generate next object
            createPreviewShape();
            _isPreviewAdded = false;
        }

        private void updateActualSizeForTextBox(TextBox textBox,double left, double top)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = textBox.Text,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = textBox.FontFamily,
                FontStyle = textBox.FontStyle,
                FontWeight = textBox.FontWeight,
                FontStretch = textBox.FontStretch,
                FontSize = textBox.FontSize,
            };

            textBlock.Measure(new Size(textBox.ActualWidth, double.PositiveInfinity));
            textBlock.Arrange(new Rect(new Point(0, 0), textBlock.DesiredSize));

            var X = left + textBlock.ActualWidth;
            var Y = top + textBlock.ActualHeight;

            Object m = _canvasObjects[_canvasObjects.Count - 1];
            
            if( m is IShape shape)
            {
                shape.HandleStart(left, top);
                shape.HandleEnd(X+7, Y + 5);
                shape.TextContent = textBox.Text;
                shape.ColorFill = new SolidColorBrush(_colorText);
                shape.ColorStroke = new SolidColorBrush(Colors.Transparent);
                _canvasObjects[_canvasObjects.Count - 1] = m;
            }
        }
        private void addObjectToCanvas(object obj)
        {
            UIElement element;
            if (obj.GetType() == typeof(BitmapImage))
            {
                Image img = new Image();
                img.Source = (BitmapImage) obj;
                element = img;
            }
            else if (obj.GetType() == typeof(BitmapSource))
            {
                Image img = new Image();
                img.Source = (BitmapSource)obj;
                element = img;
            }
            else
            {
                var shape = (IShape)obj;
                element = shape.Draw();
            }
            canvas.Children.Add(element);
        }

        private void createPreviewShape()
        {
            Color colorFill = Colors.Transparent;
            int strokeSize = 0;
            if (_hasStroke)
            {
                strokeSize = _strokeSize;
            }
            if (_isFilled)
            {
                colorFill = _colorFill;
            }
            if (_selectedShapeName == "Text")
            {
                _preview = _shapeFactory.Create(_selectedShapeName, _colorText, colorFill, strokeSize, _strokeDashArray);
            } else
            {
                _preview = _shapeFactory.Create(_selectedShapeName, _colorStroke, colorFill, strokeSize, _strokeDashArray);
            }
        }

        private bool isBasicShape(IShape s)
        {
            switch(s.Name)
            {
                case nameof(Shapes.Line):
                case nameof(Shapes.Rectangle):
                case nameof(Shapes.Ellipse):
                case nameof(Shapes.Square):
                case nameof(Shapes.Circle):
                case nameof(Shapes.Text):
                    return true;
                default:
                    return false;
            }
        }

        private void popup_Click(object sender, RoutedEventArgs e) 
        {
            var popup = sender as Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }

        private void fileButton_Click(object sender, RoutedEventArgs e)
        {
            fileButton.Style = Resources["ToggleButtonActiveStyle"] as Style;
            fileButton.IsChecked = true;
            popupFile.IsOpen = true;
            popupFile.Closed += (senderClosed, eClosed) =>
            {
                fileButton.Style = Resources["TransparentToggleButtonStyle"] as Style;
                fileButton.IsChecked = false;
            };
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            editButton.Style = Resources["ToggleButtonActiveStyle"] as Style;
            editButton.IsChecked = true;
            popupEdit.IsOpen = true;
            popupEdit.Closed += (senderClosed, eClosed) =>
            {
                editButton.Style = Resources["TransparentToggleButtonStyle"] as Style;
                editButton.IsChecked = false;
            };
        }

        private void cutButton_Click(object sender, RoutedEventArgs e)
        {
            cut();
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            copy();
        }

        private void pasteButton_Click(object sender, RoutedEventArgs e)
        {
            paste();
        }

        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            rotateButton.Style = Resources["ToggleButtonActiveStyle"] as Style;
            rotateButton.IsChecked = true;
            popupRotate.IsOpen = true;
            popupRotate.Closed += (senderClosed, eClosed) =>
            {
                rotateButton.Style = Resources["TransparentToggleButtonStyle"] as Style;
                rotateButton.IsChecked = false;
            };
        }

        private void flipButton_Click(object sender, RoutedEventArgs e)
        {
            flipButton.Style = Resources["ToggleButtonActiveStyle"] as Style;
            flipButton.IsChecked = true;
            popupFlip.IsOpen = true;
            popupFlip.Closed += (senderClosed, eClosed) =>
            {
                flipButton.Style = Resources["TransparentToggleButtonStyle"] as Style;
                flipButton.IsChecked = false;
            };
        }

        private void strokeTypeButton_Click(object sender, RoutedEventArgs e)
        {
            strokeTypeButton.Style = Resources["ToggleButtonActiveStyle"] as Style;
            strokeTypeButton.IsChecked = true;
            popupStrokeType.IsOpen = true;
            popupStrokeType.Closed += (senderClosed, eClosed) =>
            {
                strokeTypeButton.Style = Resources["TransparentToggleButtonStyle"] as Style;
                strokeTypeButton.IsChecked = false;
            };
        }

        private void strokeType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton strokeTypeRadioButton)
            {
                if (strokeTypeRadioButton.Name == "solidStrokeButton")
                    _strokeDashArray = null;
                else if (strokeTypeRadioButton.Name == "dashedStrokeButton")
                {
                    switch (_strokeSize)
                    {
                        case 1:
                            _strokeDashArray = new double[] { 12, 8 };
                            break;
                        case 3:
                            _strokeDashArray = new double[] { 10, 6 };
                            break;
                        case 5:
                            _strokeDashArray = new double[] { 8, 4 };
                            break;
                        case 8:
                            _strokeDashArray = new double[] { 7, 3 };
                            break;
                    }
                }
                else if (strokeTypeRadioButton.Name == "dottedStrokeButton")
                {
                    switch (_strokeSize)
                    {
                        case 1:
                            _strokeDashArray = new double[] { 1, 3 };
                            break;
                        case 3:
                            _strokeDashArray = new double[] { 1, 2 };
                            break;
                        case 5:
                            _strokeDashArray = new double[] { 1, 1.5 };
                            break;
                        case 8:
                            _strokeDashArray = new double[] { 1, 1.25 };
                            break;
                    }
                }
                else
                {
                    switch (_strokeSize)
                    {
                        case 1:
                            _strokeDashArray = new double[] { 9, 6, 1, 6 };
                            break;
                        case 3:
                            _strokeDashArray = new double[] { 8, 4, 1, 4 };
                            break;
                        case 5:
                            _strokeDashArray = new double[] { 7, 3, 1, 3 };
                            break;
                        case 8:
                            _strokeDashArray = new double[] { 6, 2.4, 1, 2.4 };
                            break;
                    }
                }

                if (_isSelecting)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateStrokeDashArray(_strokeDashArray);
                    cloneSelected();
                }
                createPreviewShape();
            }
        }

        private void strokeSizeButton_Click(object sender, RoutedEventArgs e)
        {
            strokeSizeButton.Style = Resources["ToggleButtonActiveStyle"] as Style;
            strokeSizeButton.IsChecked = true;
            popupStrokeSize.IsOpen = true;
            popupStrokeSize.Closed += (senderClosed, eClosed) =>
            {
                strokeSizeButton.Style = Resources["TransparentToggleButtonStyle"] as Style;
                strokeSizeButton.IsChecked = false;
            };
        }

        private void strokeSize_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton strokeSizeRadioButton)
            {
                if (strokeSizeRadioButton.Name == "oneThicknessButton")
                    _strokeSize = 1; 
                else if (strokeSizeRadioButton.Name == "threeThicknessButton")
                    _strokeSize = 3;
                else if (strokeSizeRadioButton.Name == "fiveThicknessButton")
                    _strokeSize = 5;
                else _strokeSize = 8;

                if (_isSelecting)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateStrokeSize(_strokeSize);
                    cloneSelected();
                }
                createPreviewShape();
            }
        }

        private void ColorPickerStroke_ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Color selectedColor)
            {
                _colorStroke = selectedColor;
                if (_isSelecting && _selectedShape != null && _hasStroke)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateColorStroke(new SolidColorBrush(selectedColor));
                    cloneSelected();
                }
                createPreviewShape();
            }
        }

        private void ColorPickerFill_ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Color selectedColor)
            {
                _colorFill = selectedColor;
                if (_isSelecting && _selectedShape != null && _isFilled)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateColorFill(new SolidColorBrush(selectedColor));
                    cloneSelected();
                }
                createPreviewShape();
            }
        }

        private void ColorPickerText_ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Color selectedColor)
            {
                _colorText = selectedColor;
                if (_isSelecting && _selectedShape != null)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateColorStroke(new SolidColorBrush(selectedColor));
                    cloneSelected();
                }
                createPreviewShape();
            }
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            undo();
        }

        private async void undo()
        {
            await Task.Delay(100);
            if (_undoStack.Count > 0)
            {
                if (_isSelecting)
                {
                    _isSelecting = false;
                    selectToggleButton.IsChecked = false;
                    deleteAllSelectionFrame();
                }

                var undoItem = _undoStack.Pop();

                if (canvas.Children[undoItem.Item2].GetType() == typeof(Image))
                {
                    Image img = (Image)canvas.Children[undoItem.Item2];
                    _redoStack.Push((img, undoItem.Item2));
                    canvas.Children.RemoveAt(undoItem.Item2);
                    _canvasObjects.RemoveAt(undoItem.Item2);

                    if (undoItem.Item1 != null)
                    {

                        _canvasObjects.Insert(undoItem.Item2, (BitmapSource)img.Source);
                        var newImg = cloneImage(img);
                        canvas.Children.Insert(undoItem.Item2, newImg);
                    }
                } else
                {
                    IShape cloneShape = cloneShapeWithPosition((IShape)_canvasObjects[undoItem.Item2]);
                    _redoStack.Push((cloneShape, undoItem.Item2));

                    canvas.Children.RemoveAt(undoItem.Item2);
                    _canvasObjects.RemoveAt(undoItem.Item2);

                    if (undoItem.Item1 != null)
                    {
                        _canvasObjects.Insert(undoItem.Item2, undoItem.Item1);
                        var element = ((IShape)undoItem.Item1).Draw();
                        canvas.Children.Insert(undoItem.Item2, element);
                    }
                }

                redoButton.IsEnabled = true;

                // Disable when not able to undo
                if (_undoStack.Count == 0)
                {
                    undoButton.IsEnabled = false;
                }
            }
        }

        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            redo();
        }

        private async void redo()
        {
            await Task.Delay(100);
            if (_redoStack.Count > 0)
            {
                if (_isSelecting)
                {
                    _isSelecting = false;
                    selectToggleButton.IsChecked = false;
                    deleteAllSelectionFrame();
                }

                var redoItem = _redoStack.Pop();
                var obj = redoItem.Item1;

                if (_canvasObjects.Count == redoItem.Item2)
                {
                    _undoStack.Push((null, redoItem.Item2));

                    if (obj.GetType() == typeof(Image))
                    {
                        Image img = (Image)obj;
                        _canvasObjects.Add((BitmapSource)img.Source);
                        Image newImg = new Image();
                        var bitmap = (BitmapSource)img.Source;
                        newImg.Source = bitmap;
                        newImg.Width = bitmap.PixelWidth;
                        newImg.Height = bitmap.PixelHeight;
                        newImg.Name = GenerateUniqueImageName();

                        Canvas.SetLeft(newImg, Canvas.GetLeft(img));
                        Canvas.SetTop(newImg, Canvas.GetTop(img));
                        canvas.Children.Insert(redoItem.Item2, newImg);
                    } else
                    {
                        _canvasObjects.Add(redoItem.Item1);
                        addObjectToCanvas(redoItem.Item1);
                    }
                } else
                {
                    if (obj.GetType() == typeof(Image))
                    {
                        Image img = (Image)obj;
                        _canvasObjects.Insert(redoItem.Item2, (BitmapSource)img.Source);

                        Image newImg = new Image();
                        var bitmap = (BitmapSource)img.Source;
                        newImg.Source = bitmap;
                        newImg.Width = bitmap.PixelWidth;
                        newImg.Height = bitmap.PixelHeight;
                        newImg.Name = GenerateUniqueImageName();

                        Canvas.SetLeft(newImg, Canvas.GetLeft(img));
                        Canvas.SetTop(newImg, Canvas.GetTop(img));
                        canvas.Children.Insert(redoItem.Item2, newImg);
                    } else
                    {
                        IShape cloneShape = cloneShapeWithPosition((IShape)_canvasObjects[redoItem.Item2]);
                        _undoStack.Push((cloneShape, redoItem.Item2));
                        canvas.Children.RemoveAt(redoItem.Item2);
                        _canvasObjects.RemoveAt(redoItem.Item2);

                        _canvasObjects.Insert(redoItem.Item2, redoItem.Item1);
                        var element = ((IShape)redoItem.Item1).Draw();
                        canvas.Children.Insert(redoItem.Item2, element);
                    }
                }
                undoButton.IsEnabled = true;

                // Disable when not able to redo
                if (_redoStack.Count == 0)
                {
                    redoButton.IsEnabled = false;
                }
            }
        }


        private void UncheckAllRadioButtons()
        {
            if (_hasStroke && oneThicknessButton != null)
            {
                oneThicknessButton.IsChecked = false;
                threeThicknessButton.IsChecked = false;
                fiveThicknessButton.IsChecked = false;
                eightThicknessButton.IsChecked = false;

                solidStrokeButton.IsChecked = false;
                dashedStrokeButton.IsChecked = false;
                dottedStrokeButton.IsChecked = false;
                dashedDottedStrokeButton.IsChecked = false;
            }
        }

        private void strokeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var strokeBtn = sender as ToggleButton;
            if (strokeBtn != null)
            {
                strokeBtn.Style = Resources["ToggleButtonActiveStyle"] as Style;
                strokeBtn.ToolTip = "Remove stroke";

                if (_isSelecting)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateStrokeSize(1);
                    _selectedShape.UpdateStrokeDashArray(null);
                }
                else
                {
                    _strokeDashArray = null;
                    _strokeSize = 1;
                    if (!_hasStroke && oneThicknessButton != null)
                    {
                        oneThicknessButton.IsChecked = true;
                        solidStrokeButton.IsChecked = true;
                    }
                }
            }
        }

        private void strokeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var strokeBtn = sender as ToggleButton;
            if (strokeBtn != null)
            {
                strokeBtn.Style = Resources["ToggleButtonDisableStyle"] as Style;
                strokeBtn.ToolTip = "Add stroke";

                if (_isSelecting)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateStrokeSize(0);
                    _selectedShape.UpdateStrokeDashArray(null);
                }
                else
                {
                    _strokeDashArray = null;
                    _strokeSize = 0;
                    UncheckAllRadioButtons();
                }
            }
        }

        private void strokeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _hasStroke = !_hasStroke;
            createPreviewShape();
        }

        private void fillToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // User choose to remove stroke
            var fillBtn = sender as ToggleButton;
            if (fillBtn != null)
            {
                fillBtn.Style = Resources["ToggleButtonActiveStyle"] as Style;
                fillBtn.ToolTip = "Remove fill";

                if (_isSelecting && _selectedShape != null)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateColorFill(new SolidColorBrush(_colorFill));
                    cloneSelected();
                }
            }
        }

        private void fillToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // User choose to add stroke
            var fillBtn = sender as ToggleButton;
            if (fillBtn != null)
            {
                fillBtn.Style = Resources["ToggleButtonDisableStyle"] as Style;
                fillBtn.ToolTip = "Fill shape";

                if (_isSelecting)
                {
                    pushUndoClearRedo(_selectedShape);
                    _selectedShape.UpdateColorFill(new SolidColorBrush(Colors.Transparent));
                    cloneSelected();
                }
            }
        }
        private void fillToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isFilled = !_isFilled;
            createPreviewShape();
        }

        private void selectToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectToggleBtn = sender as ToggleButton;
            if (selectToggleBtn != null)
            {
                // Uncheck current shape button -> change focus to selection button
                if (_selectedShapeBtn != null)
                {
                    _selectedShapeBtn.IsChecked = false;
                }
                _isSelecting = true;
                _isDrawing = false;
                selectToggleBtn.Style = Resources["ToggleButtonActiveStyle"] as Style;
            }
        }

        private void selectToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var selectToggleBtn = sender as ToggleButton;
            if (selectToggleBtn != null)
            {
                _isSelecting = false;
                selectToggleBtn.Style = Resources["ToggleButtonDisableStyle"] as Style;
            }
        }
        
        private void saveObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            saveObjects();
        }

        private async void saveObjects()
        {
            await Task.Delay(100);
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var serializedObjectList = JsonConvert.SerializeObject(_canvasObjects, settings);
            var dialog = new SaveFileDialog();

            dialog.Filter = "JSON (*.json)|*.json";
            bool? result = dialog.ShowDialog();
            if (result ?? true)
            {
                string filename = dialog.FileName;
                File.WriteAllText(filename, serializedObjectList);
            }
        }

        private void saveImageButton_Click(object sender, RoutedEventArgs e)
        {
            saveImage();
        }

        private async void saveImage()
        {
            await Task.Delay(100);
            var dialog = new SaveFileDialog();

            dialog.Filter = "Image Files |*.jpg;*.jpeg;*.png;*.bmp;";
            bool? result = dialog.ShowDialog();
            if (result ?? true)
            {
                string filename = dialog.FileName;
                string extension = System.IO.Path.GetExtension(filename);
                var bitmapSrc = GetRenderTargetBitmapFromControl(canvas);

                switch(extension) 
                {
                    case ".png":
                        PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                        pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSrc));

                        using (FileStream file = File.Create(filename))
                        {
                            pngEncoder.Save(file);
                        }
                        break;
                    case ".jpeg":
                    case ".jpg":
                        JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                        jpegEncoder.Frames.Add(BitmapFrame.Create(bitmapSrc));

                        using (FileStream file = File.Create(filename))
                        {
                            jpegEncoder.Save(file);
                        }
                        break;
                    case ".bmp":
                        BmpBitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                        bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSrc));

                        using (FileStream file = File.Create(filename))
                        {
                            bitmapEncoder.Save(file);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static BitmapSource GetRenderTargetBitmapFromControl(Visual targetControl, double dpi = 96)
        {
            if (targetControl == null) return null;

            var bounds = VisualTreeHelper.GetDescendantBounds(targetControl);
            var renderTargetBitmap = new RenderTargetBitmap((int)(bounds.Width * dpi / 96.0),
                                                            (int)(bounds.Height * dpi / 96.0),
                                                            dpi,
                                                            dpi,
                                                            PixelFormats.Pbgra32);

            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(targetControl);
                drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }

            renderTargetBitmap.Render(drawingVisual);
            return renderTargetBitmap;
        }

        private void importImageButton_Click(object sender, RoutedEventArgs e)
        {
            loadImage();
        }

        private string GenerateUniqueImageName()
        {
            string name;
            Random random = new Random();
            do
            {
                name = "Image_" + random.Next(1000, 9999); 
            } while (ImageNameExists(name)); 
            return name;
        }

        private bool ImageNameExists(string name)
        {
            foreach (var child in canvas.Children)
            {
                if (child is Image image && image.Name == name)
                {
                    return true; 
                }
            }
            return false;
        }

        private async void loadImage()
        {
            await Task.Delay(100);
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image Files |*.jpg;*.jpeg;*.png;*.bmp;";
            bool? result = dialog.ShowDialog();
            if (result ?? true)
            {
                string filename = dialog.FileName;
                var bitmap = new BitmapImage(new Uri(filename, UriKind.Absolute));
                Image img = new Image();
                img.Source = bitmap;
                img.Name = GenerateUniqueImageName();
                _canvasObjects.Add(bitmap);
                canvas.Children.Add(img);

                _undoStack.Push((null, _canvasObjects.Count - 1));
                undoButton.IsEnabled = true;
                _redoStack.Clear();
                redoButton.IsEnabled = false;

                img.Width = bitmap.PixelWidth;
                img.Height = bitmap.PixelHeight;

                Canvas.SetLeft(img, 0);
                Canvas.SetTop(img, 0);
            }
        }

        private void importObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            loadObjects();
        }

        private async void loadObjects()
        {
            await Task.Delay(100);
            var dialog = new OpenFileDialog();
            dialog.Filter = "JSON (*.json)|*.json";
            bool? result = dialog.ShowDialog();

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
            if (result ?? true)
            {
                string json = File.ReadAllText(dialog.FileName);
                var deserializedObjectList = JsonConvert.DeserializeObject<List<object>>(json, settings);
                if (deserializedObjectList != null)
                {
                    for (int i = 0; i < deserializedObjectList.Count; i++)
                    {
                        if (deserializedObjectList[i].GetType() == typeof(string))
                        {
                            try
                            {
                                var uri = new Uri((string)deserializedObjectList[i], UriKind.Absolute);
                                deserializedObjectList[i] = new BitmapImage(uri);
                            }
                            catch (Exception e)
                            {
                                deserializedObjectList[i] = null;
                            }
                        }
                        if (deserializedObjectList[i] != null)
                        {
                            addObjectToCanvas(deserializedObjectList[i]);
                        }
                    }
                    deserializedObjectList.RemoveAll(item => item == null);
                    if (deserializedObjectList.Count > 0)
                    {
                        undoButton.IsEnabled = true;
                        _redoStack.Clear();
                        redoButton.IsEnabled = false;
                    }
                    for (int i = 0; i < deserializedObjectList.Count; i++)
                    {
                        _undoStack.Push((null, _canvasObjects.Count + i));
                    }
                    _canvasObjects.AddRange(deserializedObjectList);
                }
            }
        }

        private void newButton_Click(object sender, RoutedEventArgs e)
        {
            resetCanvas();
        }

        private void resetCanvas()
        {
            _isDrawing = false;
            _canvasObjects.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            undoButton.IsEnabled = false;
            redoButton.IsEnabled = false;
            _isSelecting = false;
            selectToggleButton.IsEnabled = false;
            canvas.Children.Clear();
        }

        private void FlipHorizontally_Click(object sender, RoutedEventArgs e)
        {
            if (_isSelecting && _selectedShape != null)
            {
                pushUndoClearRedo(_selectedShape);
                _selectedShape.FlipHorizontally();
                cloneSelected();
                updateSelectionFrame();
            }
            else if (_isSelecting && _selectedImg != null)
            {
                pushUndoClearRedo(_selectedImg);
                BitmapSource bmpSource = (BitmapSource)_selectedImg.Source;

                TransformedBitmap transformedBitmap = new TransformedBitmap(
                    bmpSource,
                    new ScaleTransform(-1, 1, _selectedImg.Width / 2.0, 0));

                _selectedImg.Source = transformedBitmap;
            }
        }

        private void FlipVertically_Click(object sender, RoutedEventArgs e)
        {
            if (_isSelecting && _selectedShape != null)
            {
                pushUndoClearRedo(_selectedShape);
                _selectedShape.FlipHorizontally();
                cloneSelected();
                updateSelectionFrame();
            }
            else if (_isSelecting && _selectedImg != null)
            {
                pushUndoClearRedo(_selectedImg);
                BitmapSource bmpSource = (BitmapSource)_selectedImg.Source;

                TransformedBitmap transformedBitmap = new TransformedBitmap(
                    bmpSource,
                    new ScaleTransform(1, -1, _selectedImg.Width / 2.0, 0));

                _selectedImg.Source = transformedBitmap;
            }
        }

        private void RotateRight90Degrees_Click(object sender, RoutedEventArgs e)
        {
            if (_isSelecting && _selectedShape != null)
            {
                pushUndoClearRedo(_selectedShape);
                _selectedShape.RotateRight90Degrees();
                cloneSelected();
                updateSelectionFrame();
            }
            else if (_isSelecting && _selectedImg != null)
            {
                pushUndoClearRedo(_selectedImg);

                double oldWidth = _selectedImg.Width;
                double oldHeight = _selectedImg.Height;

                BitmapSource bmpSource = (BitmapSource)_selectedImg.Source; 

                TransformedBitmap transformedBitmap = new TransformedBitmap(
                    bmpSource,
                    new RotateTransform(90));

                _selectedImg.Source = transformedBitmap;
                _selectedImg.Width = oldHeight; _selectedImg.Height = oldWidth;
                updateSelectionFrame();
            }
        }

        private void RotateLeft90Degrees_Click(object sender, RoutedEventArgs e)
        {
            
            if (_isSelecting && _selectedShape != null)
            {
                pushUndoClearRedo(_selectedShape);
                _selectedShape.RotateLeft90Degrees();
                cloneSelected();
                updateSelectionFrame();
            }
            else if (_isSelecting && _selectedImg != null)
            {
                pushUndoClearRedo(_selectedImg);

                double oldWidth = _selectedImg.Width;
                double oldHeight = _selectedImg.Height;

                BitmapSource bmpSource = (BitmapSource)_selectedImg.Source;

                TransformedBitmap transformedBitmap = new TransformedBitmap(
                    bmpSource,
                    new RotateTransform(-90));

                _selectedImg.Source = transformedBitmap;
                _selectedImg.Width = oldHeight; _selectedImg.Height = oldWidth;

                updateSelectionFrame();
            }
        }

        private IShape cloneShapeWithPosition(IShape shape)
        {
            IShape cloneShape = _shapeFactory.Create(shape.Name, shape.ColorStroke.Color, 
                shape.ColorFill.Color, shape.StrokeSize, shape.StrokeDashArray);

            cloneShape.HandleStart(shape.GetStart().X, shape.GetStart().Y);
            cloneShape.HandleEnd(shape.GetEnd().X, shape.GetEnd().Y);
            cloneShape.TextContent = shape.TextContent;
            
            return cloneShape;
        }

        private void updateSelectionFrame()
        {
            if (_selectedShape != null)
            {
                canvas.Children.Remove(_selectionFrame);
                _selectionFrame = null;
                _selectionFrame = new Rectangle()
                {
                    Stroke = Brushes.Blue,
                    StrokeDashArray = new DoubleCollection() { 4, 4 },
                    StrokeThickness = 1,
                    StrokeDashCap = PenLineCap.Round,
                    Width = _selectedShape.GetWidth() + 5,
                    Height = _selectedShape.GetHeight() + 5,
                };
                addEventsToSelectionFrame();
                Canvas.SetLeft(_selectionFrame, _selectedShape.GetLeft() - 2.5);
                Canvas.SetTop(_selectionFrame, _selectedShape.GetTop() - 2.5);
                originalPosition = new Point(Canvas.GetLeft(_selectionFrame), Canvas.GetTop(_selectionFrame));

                canvas.Children.Add(_selectionFrame);
            }
            if (_selectedImg != null)
            {
                double oldWidth = _selectedImg.Width + 10;
                double oldHeight = _selectedImg.Height + 10;
                _selectionFrame.Width = _selectionFrame.Width == oldWidth ? oldHeight : oldWidth;
                _selectionFrame.Height = _selectionFrame.Height == oldHeight ? oldWidth : oldHeight;

                if(_selectionFrame.Width == oldWidth)
                {
                    Canvas.SetLeft(_selectionFrame, Canvas.GetLeft(_selectedImg) - 5);
                    Canvas.SetTop(_selectionFrame, Canvas.GetTop(_selectedImg) - 5);
                }
            }
        }

        private void copy()
        {
            distance = 10;
            if(_isSelecting && _selectedImg != null)
            {
                BitmapSource source = (BitmapSource)_selectedImg.Source;
                if (source != null)
                {
                    _clipboardImage = new Image()
                    {
                        Source = source.Clone(),
                        Width = _selectedImg.Width,
                        Height = _selectedImg.Height,
                    };
                }

                _clipboardShape = null;
            }
            else if (_isSelecting && _selectedShape != null)
            {
                _clipboardShape = _selectedShape;
                _clipboardImage = null;
            }
        }

        private async void paste()
        {
            await Task.Delay(100);
            if (_clipboardImage != null)
            {
                Image pastedImage = new Image()
                {
                    Source = _clipboardImage.Source.Clone(),
                    Width = _clipboardImage.Width,
                    Height = _clipboardImage.Height,
                    Name = GenerateUniqueImageName()
                };

                Canvas.SetLeft(pastedImage, 0);
                Canvas.SetTop(pastedImage, 0);

                canvas.Children.Add(pastedImage);
                if (pastedImage.Source is BitmapSource bitmapSource)
                {
                    _canvasObjects.Add(bitmapSource);
                    _undoStack.Push((null, _canvasObjects.Count - 1));
                    _redoStack.Clear();
                }
            }
            else if (_clipboardShape != null)
            {
                IShape shape = (IShape)_clipboardShape;
                IShape pastedShape = _shapeFactory.Create(shape.Name, shape.ColorStroke.Color, shape.ColorFill.Color, shape.StrokeSize, shape.StrokeDashArray);
                pastedShape.UpdateStrokeDashArray(shape.StrokeDashArray);
                pastedShape.HandleStart(_clipboardShape.GetStart().X - distance, _clipboardShape.GetStart().Y - distance);
                pastedShape.HandleEnd(_clipboardShape.GetEnd().X - distance, _clipboardShape.GetEnd().Y - distance);
                distance += 10;

                UIElement pastedShapeView = pastedShape.Draw();
                pastedShape.SetInCanvas();

                canvas.Children.Add(pastedShapeView);
                _canvasObjects.Add(pastedShape);
                _undoStack.Push((null, _canvasObjects.Count - 1));
                _redoStack.Clear();
            }
        }

        private void cut()
        {
            distance = 10;
            if (_isSelecting && _selectedImg != null)
            {
                BitmapSource source = (BitmapSource)_selectedImg.Source;
                if (source != null)
                {
                    _clipboardImage = new Image()
                    {
                        Source = source.Clone(),
                        Width = _selectedImg.Width,
                        Height = _selectedImg.Height,
                    };
                }
                foreach (object obj in canvas.Children)
                {
                    if (obj.GetType() == typeof(Image))
                    {
                        Image curImage = (Image)obj;
                        if (curImage.Name == _selectedImg.Name)
                        {
                            canvas.Children.Remove(curImage);
                            canvas.InvalidateVisual();
                            break;
                        }
                    }
                }

                if (_clipboardImage.Source is BitmapSource bitmapSource)
                {
                    _canvasObjects.Add(bitmapSource);
                }

                _clipboardShape = null;

            }
            else if (_isSelecting && _selectedShape != null)
            {
                _clipboardShape = _selectedShape;
                _clipboardImage = null;

                int index = 0;
                foreach (object obj in _canvasObjects)
                {
                    if (obj.GetType().ToString() == "Circle2D.Circle2D"
                        || obj.GetType().ToString() == "Ellipse2D.Ellipse2D"
                        || obj.GetType().ToString() == "Rectangle2D.Rectangle2D"
                        || obj.GetType().ToString() == "Square2D.Square2D"
                        || obj.GetType().ToString() == "Line2D.Line2D")
                    {
                        IShape shape = (IShape)obj;
                        {
                            if (shape == _clipboardShape)
                            {
                                _canvasObjects.RemoveAt(index);
                                break;
                            }
                        }
                        index++;
                    }
                }            
                canvas.Children.RemoveAt(index);
            }
            deleteAllSelectionFrame();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var temp = slider.Value / 100.0;
            if (temp > 0.0 && canvas != null)
            {
                ScaleTransform scaleTransform = new ScaleTransform(temp, temp);
                canvas.LayoutTransform = scaleTransform;
                border.LayoutTransform = scaleTransform;

                if (scrollViewer != null)
                {
                    // center the Scroll Viewer
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight / 2.0);
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.ScrollableWidth / 2.0);
                }
            }
        }

        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (slider.Value + 10 > 1000)
            {
                slider.Value = 1000;
            } else
            {
                slider.Value += 10;
            }
        }

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (slider.Value - 10 < 10)
            {
                slider.Value = 10;
            }
            else
            {
                slider.Value -= 10;
            }
        }

        private void pushUndoClearRedo(object obj)
        {
            if (obj.GetType() == typeof (Image))
            {
                Image img = (Image)obj;
                Image cloneImg = cloneImage(img);
                _undoStack.Push((cloneImg, _selectedIndex));
            } else
            {
                IShape shape = (IShape)obj;
                IShape cloneShape = cloneShapeWithPosition(shape);
                _undoStack.Push((cloneShape, _selectedIndex));
            }
            _redoStack.Clear();
            undoButton.IsEnabled = true;
            redoButton.IsEnabled = false;
        }

        private Image cloneImage(Image img)
        {
            Image newImg = new Image();
            var bitmap = (BitmapSource)img.Source.Clone();
            newImg.Source = bitmap;
            newImg.Width = bitmap.PixelWidth;
            newImg.Height = bitmap.PixelHeight;
            newImg.Name = GenerateUniqueImageName();

            Canvas.SetLeft(newImg, Canvas.GetLeft(img));
            Canvas.SetTop(newImg, Canvas.GetTop(img));

            return newImg;
        }

        private void cloneSelected()
        {
            if (_selectedShape != null)
            {
                _cloneSelected = cloneShapeWithPosition(_selectedShape);
            }
            if (_selectedImg != null)
            {
                _cloneSelected = cloneImage(_selectedImg);
            }
        }
    }

}
