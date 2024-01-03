using System.Windows.Media;

namespace Paint.Model
{
    public class DrawingAttributes
    {
        public Color SelectedColor { get; set; }
        public double PenWidth { get; set; }
        public DoubleCollection StrokeStyle { get; set; }

        public DrawingAttributes()
        {
            // Set default values
            SelectedColor = Colors.Black;
            PenWidth = 1.0;
            StrokeStyle = new DoubleCollection();
        }
    }
}
