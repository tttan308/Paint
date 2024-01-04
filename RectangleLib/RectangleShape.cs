using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyLib;

namespace RectangleLib
{
    public class RectangleShape : IShape
    {
        public override UIElement Draw()
        {
            if(Points.Count < 2)
            {
                return new System.Windows.Shapes.Rectangle();
            }

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

            element.Stroke = new SolidColorBrush(ShapeColor);
            element.StrokeThickness = PenWidth;
            element.StrokeDashArray = StrokeStyle;

            return element;
        }

        public override IShape Clone()
        {
            return new RectangleShape();
        }

        public override bool IsPointInside(Point point)
        {
            double width = Math.Abs(Points[1].X - Points[0].X);
            double height = Math.Abs(Points[1].Y - Points[0].Y);

            double left = Math.Min(Points[0].X, Points[1].X);
            double top = Math.Min(Points[0].Y, Points[1].Y);

            if (
                point.X >= left
                && point.X <= left + width
                && point.Y >= top
                && point.Y <= top + height
            )
            {
                return true;
            }
            return false;
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
                return new System.Windows.Shapes.Rectangle();

            }

            double width = Math.Abs(Points[1].X - Points[0].X);
            double height = Math.Abs(Points[1].Y - Points[0].Y);

            var rectangle = new System.Windows.Shapes.Rectangle()
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(ShapeColor),
                StrokeThickness = PenWidth,
                StrokeDashArray = StrokeStyle
            };

            Canvas.SetLeft(rectangle, mousePosition.X);
            Canvas.SetTop(rectangle, mousePosition.Y);

            Points.Clear();
            Points.Add(mousePosition);
            Points.Add(new Point(mousePosition.X + width, mousePosition.Y + height));

            return rectangle;
        }

        public override string Name => "Rectangle";

        public override string Icon => "imgs/Rectangle.png";

        private Transform _renderTransform;
    }
}
