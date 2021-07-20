using Client.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    public partial class MainWindow : Window
    {
   
        private void Window_Loaded(object sender, EventArgs e)
        {
           
            // don't display until we have attached ourselves to OS process.
            this.Hide();

            var self = new WindowInteropHelper(this);

            var jagexLauncher = new ProcessStartInfo()
            {
                FileName = "C:\\Users\\earth\\jagexcache\\jagexlauncher\\bin\\JagexLauncher.exe",
                Arguments = "oldschool",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Verb = "runas",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // close all processes first
            var alreadyOpenedClients = Process.GetProcessesByName("JagexLauncher");
            if (alreadyOpenedClients.Count() > 1)
            {
                foreach (var client in alreadyOpenedClients)
                {
                    client.Kill();
                    // Wait a second for computer 
                    Thread.Sleep(1000);
                }

                // Wait a second for computer 
                Thread.Sleep(1000);
            }

            var oldSchoolRunescape = Process.Start(jagexLauncher);
            oldSchoolRunescape.WaitForInputIdle();

            var started = false;
            
            while (!started)
            {
                // launcher needs to launch the oficial client.
                // it's going to swap process id's now.
                oldSchoolRunescape = Process.GetProcessById(oldSchoolRunescape.Id);

                if (oldSchoolRunescape.MainWindowHandle != IntPtr.Zero)
                {
                    // Wait for client to load (the process started but the main window might not have appeared)
                    Thread.Sleep(1000);

                    // Get the oldschool window size
                    var osWindow = new System.Drawing.Rectangle();
                    ProcessHelpers.GetWindowRect(oldSchoolRunescape.MainWindowHandle, ref osWindow);

                    this.Height = (osWindow.Width - osWindow.X - 100);
                    this.Width = osWindow.Width - osWindow.X;

                    // Turn the old school client into nothing but a visible window (no borders, no menu)
                    // Set window as a child process as well, as it is about to become the child of the current window.
                    ProcessHelpers.SetWindowLong(oldSchoolRunescape.MainWindowHandle, (int)ProcessHelpers.WindowLongFlags.GWL_STYLE, ProcessHelpers.winStyle.WS_CHILD);

                    // set oldschool as active, so it appears on top of current window.
                    ProcessHelpers.SetWindowPos(oldSchoolRunescape.MainWindowHandle, (IntPtr)(0), 0, 0, osWindow.Width - osWindow.X, osWindow.Height - osWindow.Y, 0x0040 | 0x4000);

                    // set ourselves as the active window now, but old school will appear in window.
                    ProcessHelpers.SetWindowPos(self.Handle, (IntPtr)(0), 0, 0, osWindow.Width - osWindow.X, osWindow.Height - osWindow.Y, 0x0040 | 0x4000);

                    // attach ourselves to the old school window, which by now should be the active window 
                    ProcessHelpers.SetParent(oldSchoolRunescape.MainWindowHandle, self.Handle);

                    started = true;
                }
                else
                {
                    Thread.Sleep(1000);
                    oldSchoolRunescape.Refresh();
                }
            }

            this.Show();

        }
    }

}
