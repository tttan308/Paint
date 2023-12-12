using MyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        ShapeFactory _factory;
        private Button _selectedButton = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var abilities = new List<IShape>();

            // Do tim cac kha nang
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = (new DirectoryInfo(folder)).GetFiles("*.dll");

            foreach (var fi in fis)
            {
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass & (!type.IsAbstract))
                    {
                        if (typeof(IShape).IsAssignableFrom(type))
                        {
                            var shape = Activator.CreateInstance(type) as IShape;
                            abilities.Add(shape!);
                        }
                    }
                }
            }

            _factory = new ShapeFactory();
            /*foreach (var ability in abilities)
            {
                _factory.Prototypes.Add(
                    ability.Name, ability
                );

                var button = new Button()
                {
                    Width = 80,
                    Height = 35,
                    Content = ability.Name,
                    Tag = ability.Name
                };
                button.Click += (sender, args) =>
                {
                    var control = (Button)sender;
                    _choice = (string)control.Tag;
                };
                actionsStackPanel.Children.Add(button);
            };*/

            foreach (var ability in abilities)
            {
                _factory.Prototypes.Add(
                    ability.Name, ability
                );
                var button = new Button()
                {
                    Width = 50,
                    Height = 50,
                    Tag = ability.Name
                };

                // Tạo Canvas để chứa shape
                var canvas = new Canvas()
                {
                    Width = 80,
                    Height = 50
                };

                // Tạo và cấu hình shape dựa trên loại shape
                UIElement shapeElement;
                switch (ability.Name)
                {
                    case "Line":
                        shapeElement = new Line
                        {
                            X1 = 0,
                            Y1 = 0,
                            X2 = 60,
                            Y2 = 60,
                            Stroke = Brushes.Red,
                            StrokeThickness = 3
                        };
                        break;
                    case "Rectangle":
                        shapeElement = new Rectangle
                        {
                            Width = 40,
                            Height = 30,
                            Stroke = Brushes.Red,
                            StrokeThickness = 3
                        };
                        break;
                    case "Ellipse":
                        shapeElement = new Ellipse
                        {
                            Width = 40,
                            Height = 30,
                            Stroke = Brushes.Red,
                            StrokeThickness = 3
                        };
                        break;
                    default:
                        throw new NotImplementedException("Shape not implemented");
                }

                canvas.Children.Add(shapeElement);

                // Đặt canvas làm content của button
                button.Content = canvas;

                button.Click += (sender, args) =>
                {
                    if (_selectedButton != null)
                    {
                        _selectedButton.Background = Brushes.LightGray;
                    }
                    var control = (Button)sender;
                    _choice = (string)control.Tag;
                    control.Background = Brushes.LightGray; 
                    _selectedButton = control;
                };

                actionsStackPanel.Children.Add(button);
            };

            if (abilities.Count > 0)
            {
                _choice = abilities[0].Name;
            }
        }

        bool isDrawing = false;
        Point _start;
        Point _end;
        string _choice; // Line
        List<IShape> _shapes = new List<IShape>();

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            _start = e.GetPosition(drawingCanvas);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                _end = e.GetPosition(drawingCanvas);

                Title = $"{_start.X}, {_start.Y} => {_end.X}, {_end.Y}";

                IShape preview = _factory.Create(_choice);
                preview.Points.Add(_start);
                preview.Points.Add(_end);

                drawingCanvas.Children.Clear();

                foreach (var shape in _shapes)
                {
                    drawingCanvas.Children.Add(shape.Draw());
                }

                drawingCanvas.Children.Add(preview.Draw());
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IShape shape = _factory.Create(_choice);
            shape.Points.Add(_start);
            shape.Points.Add(_end);

            _shapes.Add(shape);

            isDrawing = false;
            if (_selectedButton != null)
            {
                _selectedButton.Background = Brushes.LightGray; // Giả sử màu nền ban đầu là trắng
                _selectedButton = null;
            }
        }
    }
}
