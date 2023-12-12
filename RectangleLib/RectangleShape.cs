using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using MyLib;
using System;

namespace RectangleLib
{
        public class RectangleShape : IShape
        {
            public override UIElement Draw()
            {
            // TODO: can dam bao Diem 0 < Diem 1
            double width = Math.Abs(Points[1].X - Points[0].X);
            double height = Math.Abs(Points[1].Y - Points[0].Y);

            double left = Math.Min(Points[0].X, Points[1].X);
            double top = Math.Min(Points[0].Y, Points[1].Y);

            var element = new System.Windows.Shapes.Rectangle()
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
                return new RectangleShape();
            }

            public override string Name => "Rectangle";
        }   
}
