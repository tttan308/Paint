using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fluent;
using Microsoft.Win32;
using MyLib;
using Newtonsoft.Json;
using Paint.Converter;
using Paint.Model;


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
        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();
        private Model.DrawingAttributes _drawingAttributes = new Model.DrawingAttributes();
        IShape _selectedShape = null;
        ScaleTransform scale = new ScaleTransform();
        private Transform _renderTransform;

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

            scale = new ScaleTransform();
            drawingCanvas.LayoutTransform = scale;
        }

        private Point _lastMousePosition;

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isTextMode)
            {
                Point position = e.GetPosition(canvas);
                ShowTextInputDialog(position);
            }
            else if (_isEditMode)
            {
                Point position = e.GetPosition(canvas);

                foreach (IShape shape in _shapes)
                {
                    if (shape.IsPointInside(position))
                    {
                        _selectedShape = shape;
                        _lastMousePosition = position;
                        break;
                    }
                }
            }
            else
            {
                isDrawing = true;
                _start = e.GetPosition(drawingCanvas);

                IShape newShape = _factory.Create(_choice);
                ExecuteCommand(new AddShapeCommand(newShape, _shapes));

                newShape.ShapeColor = _drawingAttributes.SelectedColor;
                newShape.PenWidth = _drawingAttributes.PenWidth;
                newShape.StrokeStyle = _drawingAttributes.StrokeStyle;
                newShape.Points.Add(_start);

                isSaved = false;
            }
        }

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

            if (_selectedShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(canvas);

                double dx = position.X - _lastMousePosition.X;
                double dy = position.Y - _lastMousePosition.Y;
                _selectedShape.Move(dx, dy);

                _lastMousePosition = position;

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
            openFileDialog.Filter =
                "PNG Files (*.png)|*.png|Binary Files (*.bin)|*.bin|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!isSaved)
                    doYouWantToSave(sender, e);
                if (openFileDialog.FileName.EndsWith(".png"))
                {
                    LoadImageToCanvas(openFileDialog.FileName);
                }
                else if (openFileDialog.FileName.EndsWith(".bin"))
                {
                    _shapes = LoadShapes(openFileDialog.FileName);
                    RedrawCanvas();
                }
            }
            else
            {
                MessageBox.Show("The file has been saved!", "Save File", MessageBoxButton.OK);
            }
            // if (openFileDialog.ShowDialog() == true)
            // {
            //     if (!isSaved)
            //         doYouWantToSave(sender, e);
            //     LoadImageToCanvas(openFileDialog.FileName);
            // }
            // else
            // {
            //     MessageBox.Show("The file has been saved!", "Save File", MessageBoxButton.OK);
            // }
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
                doYouWantToSave(sender, e);
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
            textButton.Background = new SolidColorBrush(Colors.LightBlue);
        }

        private void ShowTextInputDialog(Point position)
        {
            var inputDialog = new TextInputDialog("");
            if (inputDialog.ShowDialog() == true)
            {
                string text = inputDialog.EnteredText;
                AddTextToCanvas(text, position);
            }
            textButton.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void AddTextToCanvas(string text, Point position)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 25
            };

            Canvas.SetLeft(textBlock, position.X);
            Canvas.SetTop(textBlock, position.Y);
            canvas.Children.Add(textBlock);

            _isTextMode = false;
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
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        private void Undo()
        {
            if (_undoStack.Any())
            {
                ICommand command = _undoStack.Pop();
                command.Unexecute();
                _redoStack.Push(command);

                RedrawCanvas();
            }
        }

        private void Redo()
        {
            if (_redoStack.Any())
            {
                ICommand command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);

                RedrawCanvas();
            }
        }

        private void ZoomSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e
        )
        {
            double zoomFactor = 1.0;
            if (e != null)
            {
                zoomFactor = e.NewValue;
            }

            scale.ScaleX = zoomFactor;
            scale.ScaleY = zoomFactor;

            _drawingAttributes.zoomFactor = zoomFactor;
        }

        private bool _isEditMode = false;

        private void EditMode_Click(object sender, RoutedEventArgs e)
        {
            _isEditMode = !_isEditMode;
            if (_isEditMode)
            {
                editMode.Background = new SolidColorBrush(Colors.LightBlue);
            }
            else
            {
                editMode.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private IShape _clipboardShape;

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _selectedShape != null)
            {
                _clipboardShape = _selectedShape;

                _shapes.Remove(_selectedShape);

                RedrawCanvas();

                _selectedShape = null;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode && _selectedShape != null)
            {
                // Copy the selected shape to the clipboard
                _clipboardShape = _selectedShape;
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_clipboardShape != null)
            {
                Point mousePosition = Mouse.GetPosition(drawingCanvas);

                drawingCanvas.Children.Add(_clipboardShape.DrawWithNewPosition(mousePosition));

                _shapes.Add(_clipboardShape);

                RedrawCanvas();
            }
        }

        public void SaveShapes(List<IShape> shapes, string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(shapes, options);
            File.WriteAllText(filePath, jsonString);
        }

        public List<IShape> LoadShapes(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<IShape>>(
                jsonString,
                new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>
                    {
                        new ShapeConverter(),
                        new PointConverterJson(),
                        new ColorConverterJson()
                    }
                }
            );
        }

        private void saveBinaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSaved)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Binary Files (*.bin)|*.bin";
                if (saveFileDialog.ShowDialog() == true)
                {
                    SaveShapes(_shapes, saveFileDialog.FileName);
                    isSaved = true;
                }
            }
            else
            {
                MessageBox.Show("The file has been saved!", "Save File", MessageBoxButton.OK);
            }
        }
    }
}
