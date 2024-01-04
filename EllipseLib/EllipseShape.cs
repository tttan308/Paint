using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MyLib;

namespace EllipseLib
{
    public class EllipseShape : IShape
    {
        public override UIElement Draw()
        {
            if (Points.Count < 2)
            {
                return new Ellipse();
            }

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

            element.Stroke = new SolidColorBrush(ShapeColor);
            element.StrokeThickness = PenWidth;
            element.StrokeDashArray = StrokeStyle;

            return element;
        }

        public override IShape Clone()
        {
            return new EllipseShape();
        }

        public override bool IsPointInside(Point point)
        {
            double width = Math.Abs(Points[1].X - Points[0].X);
            double height = Math.Abs(Points[1].Y - Points[0].Y);

            double left = Math.Min(Points[0].X, Points[1].X);
            double top = Math.Min(Points[0].Y, Points[1].Y);

            // bán kính
            double a = width / 2;
            double b = height / 2;

            // tọa độ tâm
            double x0 = left + a;
            double y0 = top + b;

            double x = point.X;
            double y = point.Y;

            return Math.Pow((x - x0) / a, 2) + Math.Pow((y - y0) / b, 2) <= 1;
        }

        public override void Move(double dx, double dy)
        {
            Points[0] = new Point(Points[0].X + dx, Points[0].Y + dy);
            Points[1] = new Point(Points[1].X + dx, Points[1].Y + dy);
        }

        public override UIElement DrawWithNewPosition(Point mousePosition)
        {
            if (Points.Count < 2)
            {
                return new Ellipse();
            }

            double width = Math.Abs(Points[1].X - Points[0].X);
            double height = Math.Abs(Points[1].Y - Points[0].Y);

            var ellipse = new Ellipse { Width = width, Height = height };

            Canvas.SetLeft(ellipse, mousePosition.X - Points[0].X);
            Canvas.SetTop(ellipse, mousePosition.Y - Points[0].Y);

            ellipse.Stroke = new SolidColorBrush(ShapeColor);
            ellipse.StrokeThickness = PenWidth;
            ellipse.StrokeDashArray = StrokeStyle;

            Points.Clear();

            Points.Add(mousePosition);
            Points.Add(new Point(mousePosition.X + width, mousePosition.Y + height));
            return ellipse;
        }

        public override string Name => "Ellipse";

        public override string Icon => "imgs/Ellipse.png";
    }
}
