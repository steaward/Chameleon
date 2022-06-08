
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
using System.Threading.Tasks;

namespace Client
{
    public partial class MainWindow : Window
    {
        private Button recordBtn = new Button();
        private Button playBtn = new Button();
        private Button stopBtn = new Button();
        private Button loginRecord = new Button();
        private Button loginBtn = new Button();

        private bool loggedIn;
        private bool stop;
        public Process oldschoolRunescape = new Process();
        public System.Drawing.Rectangle osWindow;
        public MouseHook _mouseHook;
        public KeyboardHook _keyboardHook;
        public Recording _currentRecording;

        public IntPtr _startUpWindowHandle;
        public IntPtr _oldschoolRunescape;
        public IntPtr _oldschoolWindow;
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
                    _oldschoolWindow = ProcessHelpers.FindWindow(null, "Old School Runescape");
                    // used for recording global hooks
                    _mouseHook = new MouseHook(oldschoolRunescape.Id);
                    _mouseHook.InstallAsync();

                    _keyboardHook = new KeyboardHook(oldschoolRunescape.Id);
                    _keyboardHook.InstallAsync();

                    // Get the oldschool window size
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

                    //var controls = new Windows.Controls(this);
                    //controls.Show();
                    InitializeControls();

                    // Determine the Oldschool window handle to send click event messages to:
                    // it spawns a lot of windows...
                    checkForProcessUpdates();

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

            loginBtn.Height = 30;
            loginBtn.Width = 150;
            loginBtn.Content = "Login";
            loginBtn.Click += btnLogin;

            playBtn.Height = 30;
            playBtn.Width = 150;
            playBtn.Content = "Play";
            playBtn.Click += btnPlay;

            stopBtn.Height = 30;
            stopBtn.Width = 150;
            stopBtn.Content = "Stop";
            stopBtn.Click += btnStop;

            // Record Button:
            loginRecord.Content = "Start Login Recording";
            loginRecord.Height = 30;
            loginRecord.Width = 150;
            loginRecord.Tag = "Stopped";
            loginRecord.Click += btnLoginRecord;


            stackPanel.Children.Add(recordBtn);
            stackPanel.Children.Add(playBtn);
            stackPanel.Children.Add(stopBtn);
            stackPanel.Children.Add(loginRecord);
            stackPanel.Children.Add(loginBtn);
            
            this.AddChild(stackPanel);
        }

        private void StartRecording()
        {
            _currentRecording = new Recording(_mouseHook, _keyboardHook);
            _currentRecording.Start();
        }

        private void btnStop(object sender, RoutedEventArgs e)
        {
            if (_currentRecording != null)
                _currentRecording.Stop();

            stop = true;
        }
        private async void btnPlay(object sender, RoutedEventArgs e)
        {
            //await login();
            stop = false;

            while (!stop)
            {
                checkForProcessUpdates();
                var framesJson = File.ReadAllText("Recording.json");
                var frames = JsonConvert.DeserializeObject<List<Classes.Frame>>(framesJson, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });

                try
                {
                    _currentRecording = new Recording(frames, _oldschoolRunescape, _oldschoolWindow);
                    await _currentRecording.Play();
                }
                catch(Exception ex) { }
            }
        }


        private async void btnLogin(object sender, RoutedEventArgs e)
        {
            var framesJson = File.ReadAllText("Login.json");
            var frames = JsonConvert.DeserializeObject<List<Classes.Frame>>(framesJson, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });

            try
            {
                _currentRecording = new Recording(frames, _oldschoolRunescape, _oldschoolWindow);
                await _currentRecording.Play();
                checkForProcessUpdates();
                loggedIn = true;
            }
            catch (Exception ex) { }
        }

        private void StopRecording(string fileName)
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

                if (File.Exists(fileName))
                    File.Delete(fileName);

                File.WriteAllTextAsync(fileName, json);
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
                StopRecording("Recording.json");
        }

        private void btnLoginRecord(object sender, RoutedEventArgs e)
        {
            bool startedRecording = false;
            if (loginRecord.Tag.ToString().Equals("Stopped", StringComparison.OrdinalIgnoreCase))
            {
                loginRecord.Tag = "Start";
                startedRecording = true;
            }
            else
                loginRecord.Tag = "Stopped";

            loginRecord.Content = startedRecording ? "Stop Login Recording" : "Start Login Recording";

            if (startedRecording)
                StartRecording();
            else
                StopRecording("Login.json");
        }

        private void checkForProcessUpdates()
        {

            // Determine the Oldschool window handle to send click event messages to:
            // it spawns a lot of windows...
            var allChildWindows = new WindowHandleInfo(_oldschoolWindow).GetAllChildHandles();
            foreach (var childWindow in allChildWindows)
            {
                var ancestors = new WindowHandleInfo(childWindow).GetAllChildHandles();
                if (!ancestors.Any())
                {
                    _oldschoolRunescape = childWindow;
                    break;
                }
            }
        }

        private async Task login()
        {
            if (loggedIn)
                return;

            var loginJson = File.ReadAllText("Login.json");
            if (loginJson != null)
            {
                var loginFrames = JsonConvert.DeserializeObject<List<Classes.Frame>>(loginJson, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                var recording = new Recording(loginFrames, _oldschoolRunescape, _oldschoolWindow);
                await recording.Play();
            }

            checkForProcessUpdates();
            loggedIn = true;
            return;
        }
    }

}

