using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MyLib
{
    public abstract class IShape
    {
        public abstract string Name { get; }
        public abstract string Icon { get; }

        public List<Point> Points { get; set; } = new List<Point>();

        public abstract UIElement Draw();
        public abstract IShape Clone();
        public Color ShapeColor { get; set; }

        public double PenWidth { get; set; }
        public DoubleCollection StrokeStyle { get; set; }

        public abstract bool IsPointInside(Point point);

        public abstract void Move(double dx, double dy);

        public abstract UIElement DrawWithNewPosition(Point mousePosition);
    }
}
