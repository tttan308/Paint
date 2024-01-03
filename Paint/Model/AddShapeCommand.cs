using MyLib;
using System.Collections.Generic;

namespace Paint.Model
{
    public class AddShapeCommand : ICommand
    {
        private readonly IShape _shape;
        private readonly List<IShape> _shapes;

        public AddShapeCommand(IShape shape, List<IShape> shapes)
        {
            _shape = shape;
            _shapes = shapes;
        }

        public void Execute()
        {
            _shapes.Add(_shape);
        }

        public void Unexecute()
        {
            _shapes.Remove(_shape);
        }
    }
}
