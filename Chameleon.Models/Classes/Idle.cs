using System;
using System.Collections.Generic;
using System.Text;

namespace Chameleon.Classes
{
    public class Idle : InputData
    {
        // 10,000,000 ticks = 1 second;
        public long Ticks { get; set; }

        public Idle (long ticks)
        {
            Ticks = ticks;
        }
    }
}
