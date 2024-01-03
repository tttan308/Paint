using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using MyLib;

namespace Paint
{
    public class DrawCommand : ICommand
    {
        private Canvas _canvas;
        private UIElement _element;
        private IShape _shape;

        public DrawCommand(Canvas canvas, IShape shape, UIElement element)
        {
            _canvas = canvas;
            _element = element;
            _shape = shape;
        }

        public void Execute()
        {
            _canvas.Children.Add(_element);
        }

        public void Unexecute()
        {
            _canvas.Children.Remove(_element);
        }
    }
}
