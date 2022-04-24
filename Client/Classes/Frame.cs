using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Classes
{
    public class Frame 
    {
        public InputData InputData { get; set; }
        
        public Frame (InputData data)
        {
            this.InputData = data;
        }
    }
}
