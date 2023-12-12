using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using MyLib;

namespace LineLib
{
    public class LineShape : IShape
    {
        public override UIElement Draw()
        {
            return new Line()
            {
                X1 = Points[0].X,
                Y1 = Points[0].Y,
                X2 = Points[1].X,
                Y2 = Points[1].Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };
        }

        public override IShape Clone()
        {
            return new LineShape();
        }

        public override string Name => "Line";

    }
}
