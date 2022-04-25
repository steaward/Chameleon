using Client.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Client.Classes
{
    public class Frame 
    {
        public InputData InputData { get; set; }
        
        public Frame (InputData data)
        {
            this.InputData = data;
        }

        public async Task<Frame> Play(IntPtr targetWindowHandle, IntPtr currentWindowHandle)
        {
            var type = this.InputData.GetType();

            if (type == typeof(Idle)) 
                await Task.Delay((int)((Idle)this.InputData).Ticks / 10000);

            else if (type == typeof(KeyStroke))
                // send key to current window handle
                await SendKey((KeyStroke)this.InputData, currentWindowHandle);
            else if (type == typeof(MouseClick))
                // post message to target window handle
                await Click((MouseClick)this.InputData, targetWindowHandle);

            return this;
        }

        private async Task Click(MouseClick data, IntPtr targetWindowHandle)
        {
            var point = data.Point;
            uint rightClick = 0x0204;
            uint leftClick = 0x0201;
            uint clickType = data.Button == Button.Left ? leftClick : rightClick;
            // clicking needs to be on it's own thread.
            // when thread returns, mouse_event message is sent?
            // can't get this to work on main thread when looping through frames...
            await Task.Run(async () =>
            {
                // new way: post click message...
                ProcessHelpers.ScreenToClient(targetWindowHandle, ref point);
                var intPtr99 = ProcessHelpers.PostMessage(targetWindowHandle, clickType, 0x0001, MAKELPARAM((int)point.X, (int)point.Y));
                var intPtr9 = ProcessHelpers.PostMessage(targetWindowHandle, clickType, 0x0001, MAKELPARAM((int)point.X, (int)point.Y));
              
                await Task.Delay(300);

                // old way:
                //var sleepTimeGenerator = new Random();
                //int sleepTime = sleepTimeGenerator.Next(300, 500);

                //var eventTypeDown = data.Button == Button.Left ? ProcessHelpers.MouseEvent.LEFTDOWN :
                //                                                 ProcessHelpers.MouseEvent.RIGHTDOWN;

                //var eventTypeUp = data.Button == Button.Left ? ProcessHelpers.MouseEvent.LEFTUP :
                //                                               ProcessHelpers.MouseEvent.RIGHTUP;
                //ProcessHelpers.SetCursorPos(data.X, data.Y);
                //await Task.Delay(sleepTime); // allow cursor to move into position

                //ProcessHelpers.mouse_event((int)eventTypeDown, data.X, data.Y, 0, 0);
                //ProcessHelpers.mouse_event((int)eventTypeUp, data.X, data.Y, 0, 0);
            });

        }
 
        private async Task SendKey(KeyStroke data, IntPtr _startupWindow)
        {
            // fake user delay? 
            var sleepTimeGenerator = new Random();
            int sleepTime = sleepTimeGenerator.Next(100, 500);
            await Task.Delay(sleepTime);

            uint WM_KEYDOWN = 0x0100;
            uint WM_KEYUP = 0x0101;
            uint WM_CHAR = 0x0102;

            // we have to process post WM_KEYDOWN For enter, tab ect.
            if (data.Shift)
                ProcessHelpers.PostMessage(_startupWindow, WM_KEYDOWN, 0x10, 0);
            // ProcessHelpers.VkKeyScan(data.ToChar()) <-- doesn't get us @
            
            // use unicode for all characters...
            ProcessHelpers.PostMessage(_startupWindow, WM_CHAR, data.ToChar(), 0) ;
            await Task.Delay(100);

            //ProcessHelpers.SetForegroundWindow(_startupWindow);
            //KeyBoardInput.SendString(data.ToString());
            //// add "user" delay to typing...
            //await Task.Delay(sleepTime);


            //var current = ProcessHelpers.FindWindow(null, "Chameleon");
            //var allChildWindows = new WindowHandleInfo(current).GetAllChildHandles();

            //foreach (var window in allChildWindows)
            //{
            //    ProcessHelpers.SetForegroundWindow(window);
            //    await Task.Delay(300);

            //    KeyBoardInput.SendString(data.ToString());
            //    // add "user" delay to typing...
            //    await Task.Delay(300);
            //}

        }

        private int MAKELPARAM(int p, int p_2)
        {
            return ((p_2 << 16) | (p & 0xFFFF));
        }
    }
}
