using Client.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chameleon.Classes
{
    public class KeyStroke : InputData
    {
        public int Code { get; set; }
        public bool Shift { get; set; }
        public char Value { get; set; }
        public KeyStroke(int code, bool shift)
        {
            this.Code = code;
            this.Shift = shift;
        }

        public char ToChar()
        {
            StringBuilder charPressed = new StringBuilder(256);

            // are we shift?
            // handle special casse
            if (this.Shift)
            {
                switch (Code)
                {
                    case 48:
                        return ')';
                    case 49:
                        return '!';
                    case 50:
                        return '@';
                    case 51:
                        return '#';
                    case 52:
                        return '$';
                    case 53:
                        return '%';
                    case 54:
                        return '^';
                    case 55:
                        return '&';
                    case 56:
                        return '*';
                    case 57:
                        return '(';
                }
            }
                else
                    ProcessHelpers.ToUnicode((uint)Code, 0, new byte[256], charPressed, charPressed.Capacity, 0);
            
            return charPressed.ToString().ToCharArray()[0];
        }
    }
}
