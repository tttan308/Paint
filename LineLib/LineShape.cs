using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using MyLib;

namespace LineLib
{
    public class LineShape : IShape
    {
        public override UIElement Draw()
        {
            if (Points.Count < 2)
            {
                return new Line();
            }

            var element = new Line()
            {
                X1 = Points[0].X,
                Y1 = Points[0].Y,
                X2 = Points[1].X,
                Y2 = Points[1].Y,
                Stroke = new SolidColorBrush(ShapeColor),
                StrokeThickness = PenWidth,
                StrokeDashArray = StrokeStyle
            };

            return element;
        }

        public override IShape Clone()
        {
            return new LineShape();
        }

        public override bool IsPointInside(Point point)
        {
            if (Points.Count < 2)
            {
                return false;
            }

            // Calculate the distance from the point to the line
            double numerator = Math.Abs(
                (Points[1].Y - Points[0].Y) * point.X
                    - (Points[1].X - Points[0].X) * point.Y
                    + Points[1].X * Points[0].Y
                    - Points[1].Y * Points[0].X
            );
            double denominator = Math.Sqrt(
                Math.Pow(Points[1].Y - Points[0].Y, 2) + Math.Pow(Points[1].X - Points[0].X, 2)
            );
            double distance = numerator / denominator;

            return Math.Abs(distance) < 10;
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
                return new Line();
            }

            double length = Math.Sqrt(
                Math.Pow(Points[1].X - Points[0].X, 2) + Math.Pow(Points[1].Y - Points[0].Y, 2)
            );
            double angle = Math.Atan2(Points[1].Y - Points[0].Y, Points[1].X - Points[0].X);

            var line = new Line
            {
                X1 = mousePosition.X,
                Y1 = mousePosition.Y,
                X2 = mousePosition.X + length * Math.Cos(angle),
                Y2 = mousePosition.Y + length * Math.Sin(angle),
                Stroke = new SolidColorBrush(ShapeColor),
                StrokeThickness = PenWidth,
                StrokeDashArray = StrokeStyle
            };

            Points.Clear();
            Points.Add(mousePosition);
            Points.Add(
                new Point(
                    mousePosition.X + length * Math.Cos(angle),
                    mousePosition.Y + length * Math.Sin(angle)
                )
            );

            return line;
        }

        public override string Name => "Line";

        public override string Icon => "imgs/Line.png";
    }
}
