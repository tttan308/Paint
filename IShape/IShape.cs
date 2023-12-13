using System.Collections.Generic;
using System.Windows;

namespace MyLib
{
    public abstract class IShape
    {
        public abstract string Name { get; }
        public abstract string Icon { get; }
        public List<Point> Points { get; set; } = new List<Point>();

        public abstract UIElement Draw();
        public abstract IShape Clone();
    }
}
