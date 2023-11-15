using Contract;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Line2D;
using System.IO;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Security.Cryptography;
using System.Linq;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using Microsoft.Win32;
using Paint.Keys;
using System.Threading.Tasks;
using Circle2D;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool _isDrawing = false;
        private List<IShape> _shapes = new List<IShape>();
        private IShape? _preview;
        private string _selectedShapeName = "";
        private ToggleButton? _selectedShapeBtn;
        private ShapeFactory _shapeFactory = ShapeFactory.Instance;
        private Color _colorStroke;
        private Color _colorFill;
        private bool _isFilled = false;
        private bool _hasStroke = true;
        private int _strokeSize = 5;
        private Stack<IShape> _undoStack = new Stack<IShape>();
        private bool _isSelecting = false;
        private Rectangle _selectionFrame;

        public MainWindow()
        {
            InitializeComponent();
            HotkeysManager.SetupSystemHook();

            // Save drawn objects hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.D, saveObjects));

            // Save as JPG picture hotkey
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.S, saveImage));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
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

                // Basic shapes: line, rectangle, ellipse, square, circle
                if (isBasicShape(shape))
                {
                    button = new ToggleButton()
                    {
                        ToolTip = shape.Name,
                        Style = Resources["ToggleButtonDisableStyle"] as Style,
                        Margin = new Thickness(15, 0, 0, 0),
                        Height = 35,
                        Width = 35,
                        Content = new Image()
                        {
                            Source = new BitmapImage(new Uri($"./Resources/{shape.Name.ToLower()}.png", UriKind.Relative)),
                            Width = 23,
                            Height = 23,
                        },
                        Tag = shape
                    };
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
            Circle = 5
        }
        private void CreateSelectionFrame(Point position)
        {
            if (_selectionFrame != null)
            {
                canvas.Children.Remove(_selectionFrame);
                _selectionFrame = null;
            }

            foreach (IShape shape in _shapes)
            {
                if (shape.ContainsPoint(position.X, position.Y))
                {
                    _selectionFrame = new Rectangle()
                    {
                        Stroke = Brushes.Black,
                        StrokeDashArray = new DoubleCollection() { 2, 2 },
                        StrokeThickness = 1,
                        StrokeDashCap = PenLineCap.Round, 
                        Width = shape.GetWidth(), 
                        Height = shape.GetHeight(), 
                    };

                    Canvas.SetLeft(_selectionFrame, shape.GetLeft());
                    Canvas.SetTop(_selectionFrame, shape.GetTop());

                    canvas.Children.Add(_selectionFrame);
                    break;
                }
            }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isSelecting)
            {
                Point pos = e.GetPosition(canvas);
                CreateSelectionFrame(pos);
            }
            else
            {
                if (_preview != null)
                {
                    Point pos = e.GetPosition(canvas);
                    _isDrawing = true;
                    _preview.HandleStart(pos.X, pos.Y);

                }
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                Point pos = e.GetPosition(canvas);
                _preview.HandleEnd(pos.X, pos.Y);

                // Remove all objects
                canvas.Children.Clear();
                // Draw all old objects
                foreach (var shape in _shapes)
                {
                    UIElement element = shape.Draw();
                    canvas.Children.Add(element);
                }

                // Draw preview object
                canvas.Children.Add(_preview.Draw());
                //clear undo stack
                _undoStack.Clear();
                undoButton.IsEnabled = true;
                redoButton.IsEnabled = false;
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_preview == null)
            {
                return;
            }

            _isDrawing = false;

            // Add last object to list
            Point pos = e.GetPosition(canvas);
            _preview.HandleEnd(pos.X, pos.Y);
            _shapes.Add(_preview);

            // Generate next object (same shape)
            createPreviewShape();

            redrawCanvas();
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

            _preview = _shapeFactory.Create(_selectedShapeName, _colorStroke, colorFill, strokeSize);
        }

        private void redrawCanvas()
        {
            // Remove all objects
            canvas.Children.Clear();

            // Draw all objects in list
            foreach (var shape in _shapes)
            {
                var element = shape.Draw();
                canvas.Children.Add(element);
            }
        }

        private bool isBasicShape(IShape s)
        {
            if (s.Name == (nameof(Shapes.Line)))
            {
                return true;
            }
            if (s.Name == (nameof(Shapes.Rectangle)))
            {
                return true;
            }
            if (s.Name == (nameof(Shapes.Ellipse)))
            {
                return true;
            }
            if (s.Name == (nameof(Shapes.Square)))
            {
                return true;
            }
            if (s.Name == (nameof(Shapes.Circle)))
            {
                return true;
            }

            return false;
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
            popupFile.IsOpen = true;
            popupFile.Closed += (senderClosed, eClosed) =>
            {
                fileButton.IsChecked = false;
            };
        }

        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            popupRotate.IsOpen = true;
            popupRotate.Closed += (senderClosed, eClosed) =>
            {
                rotateButton.IsChecked = false;
            };
        }

        private void flipButton_Click(object sender, RoutedEventArgs e)
        {
            popupFlip.IsOpen = true;
            popupFlip.Closed += (senderClosed, eClosed) =>
            {
                flipButton.IsChecked = false;
            };
        }

        private void strokeTypeButton_Click(object sender, RoutedEventArgs e)
        {
            popupStrokeType.IsOpen = true;
            popupStrokeType.Closed += (senderClosed, eClosed) =>
            {
                strokeTypeButton.IsChecked = false;
            };
        }

        private void strokeSizeButton_Click(object sender, RoutedEventArgs e)
        {
            popupStrokeSize.IsOpen = true;
            popupStrokeSize.Closed += (senderClosed, eClosed) =>
            {
                strokeSizeButton.IsChecked = false;
            };
        }

        private void ColorPickerStroke_ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Color selectedColor)
            {
                _colorStroke = selectedColor;
                createPreviewShape();
            }
        }

        private void ColorPickerFill_ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Color selectedColor)
            {
                _colorFill = selectedColor;
                createPreviewShape();
            }
        }

        private void undoButton_Click(object sender, RoutedEventArgs e)
        {
            if(_shapes.Count > 0 && canvas.Children.Count>0)
            {
                var lastIndex = _shapes.Count - 1;
                _undoStack.Push(_shapes[lastIndex]);
                canvas.Children.RemoveAt(lastIndex);
                _shapes.RemoveAt(lastIndex);
                redoButton.IsEnabled = true;
            }
        }

        private void redoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                //var nextIndex = _shapes.Count - _undoNum;
                IShape shape = _undoStack.Pop();
                UIElement redoItem = shape.Draw();
                canvas.Children.Add(redoItem);
                _shapes.Add(shape);
            }
        }

        private void strokeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // User choose to remove stroke
            var strokeBtn = sender as ToggleButton;
            if (strokeBtn != null)
            {
                strokeBtn.Style = Resources["ToggleButtonActiveStyle"] as Style;
                strokeBtn.ToolTip = "Remove stroke";
            }
        }

        private void strokeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // User choose to add stroke
            var strokeBtn = sender as ToggleButton;
            if (strokeBtn != null)
            {
                strokeBtn.Style = Resources["ToggleButtonDisableStyle"] as Style;
                strokeBtn.ToolTip = "Add stroke";
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
            var serializedShapeList = JsonConvert.SerializeObject(_shapes, settings);
            var dialog = new SaveFileDialog();

            dialog.Filter = "JSON (*.json)|*.json";
            bool? result = dialog.ShowDialog();
            if (result ?? true)
            {
                string filename = dialog.FileName;
                File.WriteAllText(filename, serializedShapeList);
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

            dialog.Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp";
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
            //loadImage();
        }

        private async void loadImage()
        {
            await Task.Delay(100);
            var dialog = new OpenFileDialog();
            dialog.Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp";
            bool? result = dialog.ShowDialog();
            if (result ?? true)
            {
                string filename = dialog.FileName;
                //ImageBrush brush = new ImageBrush();
                //brush.ImageSource = new BitmapImage(new Uri(filename, UriKind.Absolute));
                //canvas.Background = brush;

                //Image img;
                //img = new Image();
                //img.Source = new BitmapImage(new Uri(filename, UriKind.Absolute));
                //Canvas.SetLeft(img, 150);
                //Canvas.SetTop(img, 130);
                //canvas.Children.Add(img);
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
                var deserializedShapeList = JsonConvert.DeserializeObject<List<IShape>>(json, settings);
                if (deserializedShapeList != null)
                {
                    _shapes.AddRange(deserializedShapeList);
                    redrawCanvas();
                }
            }
        }
    }
}
