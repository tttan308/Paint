using MyLib;
using Fluent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Win32;

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
        private System.Windows.Controls.Button _selectedButton = null;
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
                _factory.Prototypes.Add(
                     ability.Name, ability
                 );
            }
            iconListView.ItemsSource = _factory.Prototypes.Values.ToList();

            if (abilities.Count > 0)
            {
                _choice = abilities[0].Name;
            }

        }

        bool isDrawing = false;
        Point _start;
        Point _end;
        string _choice;
        List<IShape> _shapes = new List<IShape>();
        List<IShape> _shapesBefore = new List<IShape>();
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            _start = e.GetPosition(drawingCanvas);

            IShape newShape = _factory.Create(_choice);
            newShape.Points.Add(_start);
            _shapes.Add(newShape);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && _shapes.Any())
            {
                _end = e.GetPosition(drawingCanvas);

                Title = $"{_start.X}, {_start.Y} => {_end.X}, {_end.Y}";

                var currentShape = _shapes.Last();
                if (currentShape.Points.Count == 2)
                {
                    currentShape.Points[1] = _end;
                }
                else
                {
                    currentShape.Points.Add(_end);
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
            if (_selectedButton != null)
            {
                _selectedButton.Background = Brushes.LightGray; 
                _selectedButton = null;
            }
        }

        private bool IsCanvasEmpty()
        {
            return drawingCanvas.Children.Count == 0 || (drawingCanvas.Children.Count == 1 && importedImage != null);
        }

        private void doYouWantToSave(object sender, RoutedEventArgs e)
        {
            if (IsCanvasEmpty() || isSaved == true)
            {
                importedImage = null;
                _shapes.Clear();
                drawingCanvas.Children.Clear();
                isSaved= false;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save the current file?", "Save File", MessageBoxButton.YesNoCancel);
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


        private Image importedImage = null;

        
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
                doYouWantToSave(sender, e);
                LoadImageToCanvas(openFileDialog.FileName);
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
                PixelFormats.Pbgra32);

            // Tạo một VisualBrush từ Canvas
            VisualBrush visualBrush = new VisualBrush(canvas);

            // Tạo một DrawingVisual và dùng VisualBrush để vẽ nội dung của Canvas
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), new Size(canvas.ActualWidth, canvas.ActualHeight)));
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

        private bool isSaved = false;
        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Files (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveCanvasAsPng(drawingCanvas, saveFileDialog.FileName);
                isSaved = true;
            }
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void iconListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
    }
}
