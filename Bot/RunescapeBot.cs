
using Bot.Models;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using System.Drawing.Imaging;
using static Bot.Enums;
using System.IO.Pipes;
using Bot.Imports;
using System;

namespace Bot
{
    public class RunescapeBot
    {
        private int _globalClickCount = 0;
        private bool _running = true;
        private int x_offset = 20;
        private int y_offset = 15;

        private Routine? _routine { get; set; }
        private IntPtr _hWnd { get; set; }
        private double ThreshHold = 0.75;
        public RunescapeBot(IntPtr windowHandle)
        {
            this._hWnd = windowHandle;
        }
        public DateTime endDate = new DateTime(2022, 11, 29).AddHours(2);
        public void Setup()
        {
            if (this._hWnd == IntPtr.Zero)
                throw new Exception("Failure to bind bot to a proper window.");

            // anything else this class needs? not sure yet.
        }

        public async void Run(Routine routine)
        {
            _routine = routine;

            if (_routine == null)
                throw new Exception("No routine found for bot.");

            Mat? window;
            //x_offset = 50;
            //y_offset = 10;
            // always want to see the current window to process the routine against
            while (_running)
            {
                var rnd = new Random();

                //_running = DateTime.Now <= endDate;

                Mat result = new Mat();

                window = UpdateWindow();

                if (_globalClickCount >= 35)
                {
                    var knife = ConvertToBitmapMat("C:\\Repos\\Chameleon\\Bot\\Screenshots\\knife.jpg");
                    CvInvoke.MatchTemplate(window, knife, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                    result.MinMax(out var minVal, out var maxVal, out var minLoc, out var maxLoc);
                    await Click(new MouseClick(maxLoc[0].X + 25, maxLoc[0].Y + 15, true), _hWnd);
                    Thread.Sleep(1000);
                    await Click(new MouseClick(maxLoc[0].X + 100, maxLoc[0].Y + 15, true), _hWnd);
                    Thread.Sleep(28900);

                    _globalClickCount = 0;
                }

                var picToSearch = ConvertToBitmapMat("C:\\Repos\\Chameleon\\Bot\\Screenshots\\perl_fishing3.jpg");
                var picToSearch2 = ConvertToBitmapMat("C:\\Repos\\Chameleon\\Bot\\Screenshots\\perl_fishing4.jpg");

                #region Fishing
                try
                {
                    window = UpdateWindow();

                    result = new Mat();
                    CvInvoke.MatchTemplate(window, picToSearch2, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                    result.MinMax(out var minVal2, out var maxVal2, out var minLoc2, out var maxLoc2);

                    if (maxVal2[0] >= ThreshHold)
                    {
                        var timeToClick = rnd.Next(1000, 2500);
                        //Rectangle match = new Rectangle(maxLoc2[0], picToSearch2.Size);
                        //CvInvoke.Rectangle(window, match, new Emgu.CV.Structure.MCvScalar());
                        await Click(new MouseClick(maxLoc2[0].X + x_offset, maxLoc2[0].Y + y_offset, true), _hWnd);
                        Thread.Sleep(timeToClick);
                    }
                    else
                    {
                        window = UpdateWindow();

                        CvInvoke.MatchTemplate(window, picToSearch, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                        result.MinMax(out var minVal, out var maxVal, out var minLoc, out var maxLoc);

                        if (maxVal[0] >= ThreshHold)
                        {
                            var timeToClick = rnd.Next(1000, 2500);

                            //Rectangle match = new Rectangle(maxLoc[0], picToSearch.Size);
                            //CvInvoke.Rectangle(window, match, new Emgu.CV.Structure.MCvScalar());


                            await Click(new MouseClick(maxLoc[0].X + 25, maxLoc[0].Y + 15, true), _hWnd);
                            Thread.Sleep(timeToClick);
                        }
                    }

                }
                catch (Exception e)
                {

                }

                //try
                //{
                //    window = UpdateWindow();

                //    CvInvoke.MatchTemplate(window, picToSearch, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                //    result.MinMax(out var minVal, out var maxVal, out var minLoc, out var maxLoc);

                //    if (maxVal[0] >= ThreshHold)
                //    {
                //        var timeToClick = rnd.Next(1000, 2500);

                //        //Rectangle match = new Rectangle(maxLoc[0], picToSearch.Size);
                //        //CvInvoke.Rectangle(window, match, new Emgu.CV.Structure.MCvScalar());


                //        await Click(new MouseClick(maxLoc[0].X + 25, maxLoc[0].Y + 15, true), _hWnd);
                //        Thread.Sleep(timeToClick);
                //    }
                   
                //}
                //catch (Exception e)
                //{

                //}
              

                //CvInvoke.Imshow(String.Empty, window);
                //CvInvoke.WaitKey(10); // wait for key events indefinately
                #endregion
            }
        }

        public void Stop()
        {
            _running = false;
        }

        // display window handle in open CV
        private Mat UpdateWindow()
        {
            var mat = BitmapExtension.ToMat(_hWnd.GetWindow());

            //CvInvoke.Imshow(String.Empty, mat);
            //CvInvoke.WaitKey(10); // wait for key events indefinately

            return mat;
        }


        private Mat ConvertToBitmapMat(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);
            }

            return BitmapExtension.ToMat(bitmap);
        }

        private async Task Click(MouseClick data, IntPtr targetWindowHandle)
        {
            var point = data.Point;
            uint rightClick = 0x0204;
            uint leftClick = 0x0201;
            uint clickTypeUp = data.Left ? (uint)0x0202 : (uint)0x0201;
            uint clickTypeDown = data.Left ? (uint)0x0201 : (uint)0x0204;
            uint clickType = data.Left ? leftClick : rightClick;
            // clicking needs to be on it's own thread.
            // when thread returns, mouse_event message is sent?
            // can't get this to work on main thread when looping through frames...
            await Task.Run(async () =>
            {
                //var children = Win32Api.GetChildWindows(targetWindowHandle);

                //var last = children.Last();
                //Win32Api.SendMessage(last, 0x84, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //Win32Api.PostMessage(last, 0x200, 0x0000, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);


                //Win32Api.SendMessage(last, 0x84, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);

                //foreach (var child in children)
                //{
                //    //Win32Api.ScreenToClient(child, ref point);
                //    //var clickUp = Win32Api.SendMessage(child, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //    //await Task.Delay(10);
                //    //var sendDown = Win32Api.SendMessage(child, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //    //await Task.Delay(10);

                //    //var sendUp = Win32Api.SendMessage(child, clickTypeUp, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //    var postUp = Win32Api.PostMessage(child, clickTypeUp, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //    await Task.Delay(10);
                //    var postDown = Win32Api.PostMessage(child, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //    await Task.Delay(10);
                //}

                //var lsendDown = Win32Api.SendMessage(last, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //var lsendUp = Win32Api.SendMessage(last, clickTypeUp, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //var lpostDown = Win32Api.PostMessage(last, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //var lpostUp = Win32Api.PostMessage(last, clickTypeUp, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);


                //var first = children.First();
                //var sendDown = Win32Api.SendMessage(first, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //var sendUp = Win32Api.SendMessage(first, clickTypeUp, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //var postDown = Win32Api.PostMessage(first, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //var postUp = Win32Api.PostMessage(first, clickTypeUp, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);
                //Win32Api.SendMessage(first, clickTypeDown, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(100);
                //Win32Api.SendMessage(first, clickTypeUp, 0x0001, MAKELPARAM((int)data.X, (int)data.Y));
                //Win32Api.SendMessage(first, 0x0210, 0x0201, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);

               // await Task.Delay(100);
                //Win32Api.SendMessage(targetWindowHandle, 0x0210, 0x0201, MAKELPARAM((int)data.X, (int)data.Y));
                //await Task.Delay(10);


                //old way:
                var sleepTimeGenerator = new Random();
                int sleepTime = sleepTimeGenerator.Next(300, 500);

                var eventTypeDown = data.Left ? Win32Api.MouseEvent.LEFTDOWN :
                                               Win32Api.MouseEvent.RIGHTDOWN;

                var eventTypeUp = data.Left ? Win32Api.MouseEvent.LEFTUP :
                                              Win32Api.MouseEvent.RIGHTUP;
                Win32Api.SetCursorPos(data.X, data.Y);
                await Task.Delay(sleepTime); // allow cursor to move into position

                Win32Api.mouse_event((int)eventTypeDown, data.X, data.Y, 0, 0);
                Win32Api.mouse_event((int)eventTypeUp, data.X, data.Y, 0, 0);

                _globalClickCount++;
            });

        }

        private int MAKELPARAM(int p, int p_2)
        {
            return ((p_2 << 16) | (p & 0xFFFF));
        }

    }

    public class MouseClick 
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Left { get; set; }
        public Point Point { get; set; }
        public MouseClick(int x, int y, bool left)
        {
            this.X = x;
            this.Y = y;
            this.Left = left;

            Point = new Point(X, Y);
        }
    }

  
        
}