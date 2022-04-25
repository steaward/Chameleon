
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Winook;
using Button = System.Windows.Controls.Button;
using Client.Classes;
using Client.Helpers;
using Newtonsoft.Json;

namespace Client
{
    public partial class MainWindow : Window
    {
        private Button recordBtn = new Button();
        private Button playBtn = new Button();

        private Process oldschoolRunescape = new Process();
        
        private MouseHook _mouseHook;
        private KeyboardHook _keyboardHook;
        private Recording _currentRecording;
        
        private IntPtr _startUpWindowHandle;
        private IntPtr _oldschoolRunescape;
        /// <summary>
        /// Once window has loaded, hide it from view.
        /// Attach window to a process
        /// Show the window once this has completed
        /// Process is now this window.
        /// Startup global hooks on the thread of this window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, EventArgs e)
        {

            // don't display until we have attached ourselves to OS process.
            this.Hide();

            var self = new WindowInteropHelper(this);
            _startUpWindowHandle = self.Handle;
            
            var currentPos = new System.Drawing.Rectangle();
            ProcessHelpers.GetWindowRect(_startUpWindowHandle, ref currentPos);

            var jagexLauncher = new ProcessStartInfo()
            {
                FileName = "C:\\Users\\steve\\jagexcache\\jagexlauncher\\bin\\JagexLauncher.exe",
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

            oldschoolRunescape = Process.Start(jagexLauncher);
            oldschoolRunescape.WaitForInputIdle();

            var started = false;

            while (!started)
            {
                // launcher needs to launch the oficial client.
                // it's going to swap process id's now.
                oldschoolRunescape = Process.GetProcessById(oldschoolRunescape.Id);

                if (oldschoolRunescape.MainWindowHandle != IntPtr.Zero)
                {
                    // used for recording global hooks
                    _mouseHook = new MouseHook(oldschoolRunescape.Id);
                    _mouseHook.InstallAsync();

                    _keyboardHook = new KeyboardHook(oldschoolRunescape.Id);
                    _keyboardHook.InstallAsync();

                    // Get the oldschool window size
                    var osWindow = new System.Drawing.Rectangle();
                    ProcessHelpers.GetWindowRect(oldschoolRunescape.MainWindowHandle, ref osWindow);

                    this.Height = osWindow.Width - osWindow.X - 100;
                    this.Width = osWindow.Width - osWindow.X;

                    //Turn the old school client into nothing but a visible window(no borders, no menu)
                    //Set window as a child process as well, as it is about to become the child of the current window.
                    ProcessHelpers.SetWindowLong(oldschoolRunescape.MainWindowHandle, (int)ProcessHelpers.WindowLongFlags.GWL_STYLE, ProcessHelpers.winStyle.WS_CHILD);

                    //set oldschool as active, so it appears on top of current window.
                    ProcessHelpers.SetWindowPos(oldschoolRunescape.MainWindowHandle, (IntPtr)(0), 0, 0, osWindow.Width - osWindow.X, osWindow.Height - osWindow.Y, 0x0040 | 0x4000);

                    // set ourselves as the active window now, but old school will appear in window.
                    ProcessHelpers.SetWindowPos(_startUpWindowHandle, (IntPtr)(0), 0, 0, osWindow.Width - osWindow.X, osWindow.Height - osWindow.Y, 0x0040 | 0x4000);

                    // attach ourselves to the old school window, which by now should be the active window 
                    ProcessHelpers.SetParent(oldschoolRunescape.MainWindowHandle, _startUpWindowHandle);
                    InitializeControls();

                    // Determine the Oldschool window handle to send messages to:
                    // it spawns a lot of windows...
                    _startUpWindowHandle = ProcessHelpers.FindWindow(null, "Chameleon");
                    var allChildWindows = new WindowHandleInfo(_startUpWindowHandle).GetAllChildHandles();
                    foreach (var childWindow in allChildWindows)
                    {
                        var ancestors = new WindowHandleInfo(childWindow).GetAllChildHandles();
                        if (!ancestors.Any())
                        {
                            _oldschoolRunescape = childWindow;
                            break;
                        }
                    }
                    started = true;
                }
                else
                {
                    Thread.Sleep(1000);
                    oldschoolRunescape.Refresh();
                }
            }
            this.Show();
        }

        // All controls need to be inialized after osrs process has attached to this window.
        private void InitializeControls()
        {
            // stack panel full of buttons for now?
            // to-do allow users to select their own screen recordings to play back.
            var stackPanel = new StackPanel();
            stackPanel.VerticalAlignment = VerticalAlignment.Top;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
            // Record Button:
            recordBtn.Content = "Start Recording";
            recordBtn.Height = 30;
            recordBtn.Width = 150;
            recordBtn.Tag = "Stopped";
            recordBtn.Click += btnRecord;

            playBtn.Height = 30;
            playBtn.Width = 150;
            playBtn.Content = "Play";
            playBtn.Click += btnPlay;

            stackPanel.Children.Add(recordBtn);
            stackPanel.Children.Add(playBtn);
            
            this.AddChild(stackPanel);
        }

        private void StartRecording()
        {
            _currentRecording = new Recording(_mouseHook, _keyboardHook);
            _currentRecording.Start();
        }

        private void btnPlay(object sender, RoutedEventArgs e)
        {
            var framesJson = File.ReadAllText("Recording.json");
            var frames = JsonConvert.DeserializeObject<List<Classes.Frame>>(framesJson, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            
            var recording = new Recording(frames, _oldschoolRunescape, _startUpWindowHandle);
            recording.Play();
        }

        private void StopRecording()
        {
            Recording finishedRecording = null;
            if (_currentRecording != null)
                finishedRecording = _currentRecording.Stop();

            // save as json to load and replay later?
            if(finishedRecording != null)
            {
                var json = JsonConvert.SerializeObject(finishedRecording.Frames, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                if (File.Exists("Recording.json"))
                    File.Delete("Recording.json");

                File.WriteAllTextAsync("Recording.json", json);
            }
        }

        private void btnRecord(object sender, RoutedEventArgs e)
        {
            bool startedRecording = false;
            if (recordBtn.Tag.ToString().Equals("Stopped", StringComparison.OrdinalIgnoreCase))
            {
                recordBtn.Tag = "Start";
                startedRecording = true;
            }
            else
                recordBtn.Tag = "Stopped";

            recordBtn.Content = startedRecording ? "Stop Recording" : "Start Recording";

            if (startedRecording)
                StartRecording();
            else
                StopRecording();
        }

    }

}

