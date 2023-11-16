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
using System.Windows.Documents;
using System.Windows.Media.Converters;

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
        private bool _isFilled = false;
        private bool _hasStroke = true;
        private int _strokeSize = 1;
        private Stack<object> _undoStack = new Stack<object>();
        private bool _isSelecting = false;
        private Rectangle _selectionFrame;
        private bool isPreviewAdded = false;

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
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            int index = 0;

            foreach (object obj in _canvasObjects)
            {
                UIElement selectedElement = canvas.Children[index];
                Adorner[] toRemoveArray = adornerLayer.GetAdorners(selectedElement);
                if (toRemoveArray != null)
                {
                    for (int x = 0; x < toRemoveArray.Length; x++)
                    {
                        adornerLayer.Remove(toRemoveArray[x]);
                    }
                }
                index++;
            }

            index = 0;

            foreach (object obj in _canvasObjects)
            {
                if (obj.GetType() != typeof(BitmapImage)) 
                {
                    IShape shape = (IShape)obj;
                    if (shape.ContainsPoint(position.X, position.Y))
                    {
                        if (index >= 0 && index < canvas.Children.Count)
                        {
                            UIElement selectedElement = canvas.Children[index];
                            if (selectedElement != null)
                            {
                                adornerLayer.Add(new ResizingAdorner(selectedElement));
                            }
                        }
                        break;
                    }
                }
                index++;
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
                    _preview.StrokeSize = _strokeSize;
                    _preview.HandleStart(pos.X, pos.Y);

                }
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing && _preview != null)
            {
                Point pos = e.GetPosition(canvas);
                _preview.HandleEnd(pos.X, pos.Y);

                if (isPreviewAdded)
                {
                    // Remove previous preview drawing on canvas
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);
                }

                // Add preview object
                canvas.Children.Add(_preview.Draw());
                isPreviewAdded = true;

                // Clear undo stack
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
            _canvasObjects.Add(_preview);
            canvas.Children.RemoveAt(canvas.Children.Count - 1);
            addObjectToCanvas(_preview);

            // Generate next object
            createPreviewShape();
            isPreviewAdded = false;
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

            _preview = _shapeFactory.Create(_selectedShapeName, _colorStroke, colorFill, strokeSize);
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
            if (sender is RadioButton strokeSizeRadioButton && strokeSizeRadioButton.IsChecked == true)
            {
                if (strokeSizeRadioButton.Name == "oneThicknessButton")
                    _strokeSize = 1; 
                else if (strokeSizeRadioButton.Name == "threeThicknessButton")
                    _strokeSize = 3;
                else if (strokeSizeRadioButton.Name == "fiveThicknessButton")
                    _strokeSize = 5;
                else _strokeSize = 8;
            }
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
            undo();
        }

        private async void undo()
        {
            await Task.Delay(100);
            if (_canvasObjects.Count > 0 && canvas.Children.Count > 0)
            {
                var lastIndex = _canvasObjects.Count - 1;
                _undoStack.Push(_canvasObjects[lastIndex]);
                canvas.Children.RemoveAt(lastIndex);
                _canvasObjects.RemoveAt(lastIndex);
                redoButton.IsEnabled = true;

                // Disable when not able to undo
                if (_canvasObjects.Count == 0)
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
            if (_undoStack.Count > 0)
            {
                object obj = _undoStack.Pop();
                _canvasObjects.Add(obj);
                addObjectToCanvas(obj);
                undoButton.IsEnabled = true;

                // Disable when not able to redo
                if (_undoStack.Count == 0)
                {
                    redoButton.IsEnabled = false;
                }
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
            loadImage();
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
                var bitmap = new BitmapImage(new Uri(filename, UriKind.Absolute));
                Image img = new Image();
                img.Source = bitmap;
                _canvasObjects.Add(bitmap);
                canvas.Children.Add(img);
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
                    _canvasObjects.AddRange(deserializedObjectList);
                    undoButton.IsEnabled = true;
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
            canvas.Children.Clear();
        }
    }
}
