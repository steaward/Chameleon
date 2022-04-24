using Client.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Classes
{
    public class KeyStroke : InputData
    {
        public int Code { get; set; }
        public bool Shift { get; set; }
        public string Value { get; set; }
        public KeyStroke(int code, bool shift)
        {
            this.Code = code;
            this.Shift = shift;
        }

        public override string ToString()
        {
            StringBuilder charPressed = new StringBuilder(256);
            ProcessHelpers.ToUnicode((uint)Code, 0, new byte[256], charPressed, charPressed.Capacity, 0);
            return charPressed.ToString();
        }
    }
}
