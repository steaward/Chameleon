using Bot.Imports;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class Extensions
    {
        public static Bitmap GetWindow(this IntPtr windowHandle)
        {
            RECT rc;
            Win32Api.GetWindowRect(windowHandle, out rc);
            Bitmap b = new Bitmap(rc.Width, rc.Height);

            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(new System.Drawing.Point(rc.Left, rc.Top), System.Drawing.Point.Empty, rc.Size);
            }

            return b;
        }
    }
}

