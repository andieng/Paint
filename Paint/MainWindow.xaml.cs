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

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        bool _isDrawing = false;
        List<IShape> _shapes = new List<IShape>();
        IShape? _preview;
        string _selectedShapeName = "";
        //Dictionary<string, IShape> _prototypes = new Dictionary<string, IShape>();
        private ShapeFactory _shapeFactory = ShapeFactory.Instance;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            createShapeButtons();
            //var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            //var dlls = new DirectoryInfo(exeFolder).GetFiles("*.dll");

            //foreach(var dll in dlls) 
            //{
            //    var assembly = Assembly.LoadFile(dll.FullName);
            //    var types = assembly.GetTypes();
                
            //    foreach(var type in types)
            //    {
            //        if (type.IsClass)
            //        {
            //            if (typeof(IShape).IsAssignableFrom(type))
            //            {
            //                var shape = Activator.CreateInstance(type) as IShape;
            //                if (shape != null)
            //                {
            //                    if (!_prototypes.ContainsKey(shape.Name))
            //                    {
            //                        _prototypes.Add(shape.Name, shape);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //// Create buttons for selecting shapes
            //foreach(var item in _prototypes)
            //{
            //    var shape = item.Value as IShape;
            //    Button button;

            //    // Basic shapes: line, rectangle, ellipse, square, circle
            //    if (isBasicShape(shape))
            //    {
            //        button = new Button()
            //        {
            //            ToolTip = shape.Name,
            //            Style = Resources["TransparentFocusStyle"] as Style,
            //            Margin = new Thickness(10, 0, 0, 0),
            //            Height = 35,
            //            Width = 35,
            //            Content = new Image()
            //            {
            //                Source = new BitmapImage(new Uri($"./Resources/{shape.Name.ToLower()}.png", UriKind.Relative)),
            //                Width = 23,
            //                Height = 23,
            //            },
            //            Tag = shape.Name
            //        };
            //    }
            //    else
            //    {
            //        button = new Button()
            //        {
            //            ToolTip = shape.Name,
            //            Style = Resources["TransparentPluginStyle"] as Style,
            //            Margin = new Thickness(18, 0, 0, 0),
            //            Height = 28,
            //            Content = new TextBlock()
            //            {
            //                Text = shape.Name,
            //                Margin = new Thickness(12, 0, 12, 0),
            //            },
            //            Tag = shape.Name
            //        };
            //    }

            //    // Make rounded button
            //    var style = new Style
            //    {
            //        TargetType = typeof(Border),
            //        Setters = { new Setter { Property = Border.CornerRadiusProperty, Value = new CornerRadius(5) } }
            //    };
            //    button.Resources.Add(style.TargetType, style);

            //    button.Click += prototypeButton_Click;
            //    shapes_StackPanel.Children.Add(button);
            //}
        }

        private void createShapeButtons()
        {
            var prototypes = _shapeFactory.GetPrototypes();
            foreach (var item in prototypes)
            {
                var shape = item.Value as IShape;
                Button button;

                // Basic shapes: line, rectangle, ellipse, square, circle
                if (isBasicShape(shape))
                {
                    button = new Button()
                    {
                        ToolTip = shape.Name,
                        Style = Resources["TransparentFocusStyle"] as Style,
                        Margin = new Thickness(10, 0, 0, 0),
                        Height = 35,
                        Width = 35,
                        Content = new Image()
                        {
                            Source = new BitmapImage(new Uri($"./Resources/{shape.Name.ToLower()}.png", UriKind.Relative)),
                            Width = 23,
                            Height = 23,
                        },
                        Tag = shape.Name
                    };
                }
                else
                {
                    button = new Button()
                    {
                        ToolTip = shape.Name,
                        Style = Resources["TransparentPluginStyle"] as Style,
                        Margin = new Thickness(18, 0, 0, 0),
                        Height = 28,
                        Content = new TextBlock()
                        {
                            Text = shape.Name,
                            Margin = new Thickness(12, 0, 12, 0),
                        },
                        Tag = shape.Name
                    };
                }

                // Make rounded button
                var style = new Style
                {
                    TargetType = typeof(Border),
                    Setters = { new Setter { Property = Border.CornerRadiusProperty, Value = new CornerRadius(5) } }
                };
                button.Resources.Add(style.TargetType, style);

                button.Click += prototypeButton_Click;
                shapes_StackPanel.Children.Add(button);
            }
        }

        private void prototypeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string name = (string)button.Tag;
                _selectedShapeName = name;
                _preview = _shapeFactory.Create(_selectedShapeName);
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

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e) 
        {
            if (_preview != null)
            {
                _isDrawing = true;
                Point pos = e.GetPosition(canvas);

                _preview.HandleStart(pos.X, pos.Y);
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
            _preview = _shapeFactory.Create(_selectedShapeName);

            // Remove all objects
            canvas.Children.Clear();

            // Draw all objects in list
            foreach(var shape in _shapes)
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
    }
}
