using Bot.Imports;
using Bot.Models;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using System.Drawing.Imaging;
using static Bot.Enums;

namespace Bot
{
    public class RunescapeBot
    {
        private Routine? _routine { get; set; }
        private IntPtr _hWnd { get; set; }

        public RunescapeBot(IntPtr windowHandle)
        {
            this._hWnd = windowHandle;
        }

        public void Setup()
        {
            if (this._hWnd == IntPtr.Zero)
                throw new Exception("Failure to bind bot to a proper window.");

            // anything else this class needs? not sure yet.
        }

        public void Run(Routine routine)
        {
            _routine = routine;

            if (_routine == null)
                throw new Exception("No routine found for bot.");

            // always want to see the current window to process the routine against
            while (true)
            {
               var currentMat = UpdateWindow();

               // determine what to do from routine:

            }
        }

        // display window handle in open CV
        private Mat UpdateWindow()
        {
            var mat = BitmapExtension.ToMat(_hWnd.GetWindow());

            CvInvoke.Imshow(String.Empty, mat);
            CvInvoke.WaitKey(10); // wait for key events indefinately

            return mat;
        }

      
}