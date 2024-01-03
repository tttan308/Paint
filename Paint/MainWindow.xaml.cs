using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fluent;
using Microsoft.Win32;
using MyLib;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        ShapeFactory _factory;
        bool isDrawing = false;
        Point _start;
        Point _end;
        string _choice;
        List<IShape> _shapes = new List<IShape>();
        System.Windows.Controls.Button _selectedButton = null;
        private Image importedImage = null;
        private bool isSaved = false;
        private Stack<ICommand> _undoCommands = new Stack<ICommand>();
        private Stack<ICommand> _redoCommands = new Stack<ICommand>();
        private Model.DrawingAttributes _drawingAttributes = new Model.DrawingAttributes();

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

            foreach (var ability in abilities)
            {
                _factory.Prototypes.Add(ability.Name, ability);
            }
            iconListView.ItemsSource = _factory.Prototypes.Values.ToList();

            if (abilities.Count > 0)
            {
                _choice = abilities[0].Name;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isTextMode)
            {
                Point position = e.GetPosition(canvas);
                ShowTextInputDialog(position);
            }
            else
            {
                isDrawing = true;
                _start = e.GetPosition(drawingCanvas);

                IShape newShape = _factory.Create(_choice);

                newShape.ShapeColor = _drawingAttributes.SelectedColor;
                newShape.PenWidth = _drawingAttributes.PenWidth;
                newShape.StrokeStyle = _drawingAttributes.StrokeStyle;
                newShape.Points.Add(_start);
                _shapes.Add(newShape);

                isSaved = false;
            }
        }

        // Create a WriteableBitmap with the same size as the canvas
        WriteableBitmap buffer = new WriteableBitmap(670, 1200, 96, 96, PixelFormats.Pbgra32, null);

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && _shapes.Any())
            {
                _end = e.GetPosition(drawingCanvas);

                Title = $"{_start.X}, {_start.Y} => {_end.X}, {_end.Y}";

                var currentShape = _shapes.Last();
                if (currentShape.Points.Count < 2)
                {
                    currentShape.Points.Add(_end);
                }
                else
                {
                    currentShape.Points[currentShape.Points.Count - 1] = _end;
                }

                RedrawCanvas();
            }
            
        }

        private void RedrawCanvas()
        {
            // Xóa tất cả ngoại trừ hình ảnh đã import
            drawingCanvas.Children.Clear();
            if (importedImage != null)
            {
                drawingCanvas.Children.Add(importedImage);
            }

            // Vẽ lại các hình dạng
            foreach (var shape in _shapes)
            {
                drawingCanvas.Children.Add(shape.Draw());
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
            drawingCanvas.ReleaseMouseCapture();
            RedrawCanvas();
            if (_selectedButton != null)
            {
                _selectedButton.Background = Brushes.LightGray;
                _selectedButton = null;
            }
        }

        private bool IsCanvasEmpty()
        {
            return drawingCanvas.Children.Count == 0
                || (drawingCanvas.Children.Count == 1 && importedImage != null);
        }

        private void doYouWantToSave(object sender, RoutedEventArgs e)
        {
            if (IsCanvasEmpty() || isSaved)
            {
                importedImage = null;
                _shapes.Clear();
                drawingCanvas.Children.Clear();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "Do you want to save the current file?",
                    "Save File",
                    MessageBoxButton.YesNoCancel
                );
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        saveFileButton_Click(sender, e);
                        _shapes.Clear();
                        drawingCanvas.Children.Clear();
                        break;
                    case MessageBoxResult.No:
                        importedImage = null;
                        _shapes.Clear();
                        drawingCanvas.Children.Clear();
                        break;
                    case MessageBoxResult.Cancel:
                        break;
                }
            }
        }

        private void createNewButton_Click(object sender, RoutedEventArgs e)
        {
            doYouWantToSave(sender, e);
        }

        private void LoadImageToCanvas(string filePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(filePath);
            bitmapImage.EndInit();

            importedImage = new Image();
            importedImage.Source = bitmapImage;

            drawingCanvas.Children.Clear();
            drawingCanvas.Children.Add(importedImage);

            var window = GetWindow(this);
            canvas.Width = window.Width;
            canvas.Height = window.Height;
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!isSaved)
                    doYouWantToSave(sender, e);
                LoadImageToCanvas(openFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("The file has been saved!", "Save File", MessageBoxButton.OK);
            }
        }

        private void SaveCanvasAsPng(Canvas canvas, string filePath)
        {
            // Tạo một RenderTargetBitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth,
                (int)canvas.ActualHeight,
                96d,
                96d,
                PixelFormats.Pbgra32
            );

            // Tạo một VisualBrush từ Canvas
            VisualBrush visualBrush = new VisualBrush(canvas);

            // Tạo một DrawingVisual và dùng VisualBrush để vẽ nội dung của Canvas
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(
                    visualBrush,
                    null,
                    new Rect(new Point(), new Size(canvas.ActualWidth, canvas.ActualHeight))
                );
            }

            // Render DrawingVisual vào RenderTargetBitmap
            renderBitmap.Render(drawingVisual);

            // Lưu RenderTargetBitmap dưới dạng PNG
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filePath))
            {
                encoder.Save(file);
            }
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSaved)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Files (*.png)|*.png";
                if (saveFileDialog.ShowDialog() == true)
                {
                    SaveCanvasAsPng(drawingCanvas, saveFileDialog.FileName);
                    isSaved = true;
                }
            }
            else
            {
                MessageBox.Show("The file has been saved!", "Save File", MessageBoxButton.OK);
            }
        }

        private void importImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter =
                "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                doYouWantToSave(sender, e); // Optional: Check if the user wants to save current work
                LoadImageToCanvas(openFileDialog.FileName);
            }
        }

        private void iconListView_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e
        )
        {
            var shape = iconListView.SelectedItem as IShape;
            if (shape != null)
            {
                _choice = shape.Name;
            }
        }

        private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (canvas != null)
            {
                canvas.Width = e.NewSize.Width;
                canvas.Height = e.NewSize.Height;
            }
        }

        private void layersButton_Click(object sender, RoutedEventArgs e) { }

        private void ColorPicker_SelectedColorChanged(
            object sender,
            RoutedPropertyChangedEventArgs<Color?> e
        )
        {
            if (e.NewValue.HasValue)
            {
                _drawingAttributes.SelectedColor = e.NewValue.Value;
            }
        }

        private void StrokeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (e.AddedItems[0] as ComboBoxItem)?.Content.ToString();
            switch (selectedItem)
            {
                case "Dash":
                    _drawingAttributes.StrokeStyle = new DoubleCollection { 4, 2 };
                    break;
                case "Dot":
                    _drawingAttributes.StrokeStyle = new DoubleCollection { 1, 2 };
                    break;
                case "Dash Dot":
                    _drawingAttributes.StrokeStyle = new DoubleCollection { 4, 2, 1, 2 };
                    break;
                case "Dash Dot Dot":
                    _drawingAttributes.StrokeStyle = new DoubleCollection { 4, 2, 1, 2, 1, 2 };
                    break;
                default:
                    _drawingAttributes.StrokeStyle = null;
                    break;
            }
        }

        private void PenWidthSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e
        )
        {
            _drawingAttributes.PenWidth = e.NewValue;
        }

        private bool _isTextMode = false;

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            _isTextMode = true;
        }

        private void ShowTextInputDialog(Point position)
        {
            // Hiển thị hộp thoại để người dùng nhập văn bản
            var inputDialog = new TextInputDialog("");
            if (inputDialog.ShowDialog() == true)
            {
                string text = inputDialog.EnteredText;
                AddTextToCanvas(text, position);
            }
        }

        private void AddTextToCanvas(string text, Point position)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Colors.Black), // Hoặc màu tự chọn
                FontSize = 25 // Hoặc kích thước tự chọn
            };

            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);
            canvas.Children.Add(textBlock);

            _isTextMode = false; // Tắt chế độ vẽ chữ sau khi thêm văn bản
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoCommands.Push(command);
            _redoCommands.Clear();
        }

        private void Undo()
        {
            if (_undoCommands.Count > 0)
            {
                var command = _undoCommands.Pop();
                command.Unexecute();
                _redoCommands.Push(command);
            }
        }

        private void Redo()
        {
            if (_redoCommands.Count > 0)
            {
                var command = _redoCommands.Pop();
                command.Execute();
                _undoCommands.Push(command);
            }
        }
    }
}
