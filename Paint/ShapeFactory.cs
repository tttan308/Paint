using MyLib;
using System;
using System.Collections.Generic;

namespace Paint
{
    public class ShapeFactory
    {
        public Dictionary<string, IShape> Prototypes =
            new Dictionary<string, IShape>();

        public IShape Create(String choice)
        {
            IShape shape = Prototypes[choice].Clone();
            return shape;
        }
    }
}
