using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Client.Helpers
{
   public class ProcessHelpers
   {
        [Flags]
        public enum winStyle : int
        {
            WS_VISIBLE = 0x10000000,
            WS_CHILD = 0x40000000, //child window
            WS_BORDER = 0x00800000, //window with border
            WS_DLGFRAME = 0x00400000, //window with double border but no title
            WS_CAPTION = WS_BORDER | WS_DLGFRAME, //window with a title bar

        }
        [Flags]
        public enum pos : uint
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_SHOWWINDOW = 0x0040
        }

        [Flags]
        public enum key : uint
        {
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101
        }

        [Flags]
        public enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }

        [Flags]
        public enum MouseEvent : int
        {
            MOUSEEVENTF_LEFTDOWN = 0x02,
            MOUSEEVENTF_LEFTUP = 0x04,
            MOUSEEVENTF_RIGHTDOWN = 0x08,
            MOUSEEVENTF_RIGHTUP = 0x10
        }

        public static int GWL_STYLE = -16;

        //Sets a window to be a child window of another window
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        //Sets window attributes
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, winStyle dwNewLong);

        [DllImport("user32.dll")]
        public static extern long GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, long uFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        // Get a handle to an application window.
        [DllImport("user32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int ShowWindow(IntPtr hWnd, short cmdShow);

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

        [DllImport("user32.dll")]
        public static extern uint SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(MouseEvent dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public static void sendKeystroke(ushort k, IntPtr hwnd)
        {
            const uint WM_KEYDOWN = 0x100;
            const uint WM_SYSCOMMAND = 0x018;
            const uint SC_CLOSE = 0x053;


            IntPtr result3 = SendMessage(hwnd, WM_KEYDOWN, ((IntPtr)k), (IntPtr)0);
            System.Threading.Thread.Sleep(3000);

        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    }
}
