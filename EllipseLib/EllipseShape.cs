using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System;
using MyLib;

namespace EllipseLib
{
    public class EllipseShape : IShape
    {
        public override UIElement Draw()
        {
            // TODO: can dam bao Diem 0 < Diem 1
            double width = Math.Abs(Points[1].X - Points[0].X);
            double height = Math.Abs(Points[1].Y - Points[0].Y);

            double left = Math.Min(Points[0].X, Points[1].X);
            double top = Math.Min(Points[0].Y, Points[1].Y);

            var element = new Ellipse()
            {
                Width = width,
                Height = height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);

            return element;
        }

        public override IShape Clone()
        {
            return new EllipseShape();
        }

        public override string Name => "Ellipse";

        public override string Icon => "imgs/Ellipse.png";
    }
}
