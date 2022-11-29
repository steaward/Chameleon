using System;
using System.Collections.Generic;
using System.Text;

namespace Chameleon.Classes
{
    public enum Button
    {
        Left,
        Right
    }

    public class MouseClick : InputData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Button Button { get; set; }
        //public System.Windows.Point Point { get; set; }
        //public MouseClick(int x, int y, Button btn)
        //{
        //    this.X = x;
        //    this.Y = y;
        //    this.Button = btn;

        //    Point = new System.Windows.Point(X, Y);
        //}
    }
}
