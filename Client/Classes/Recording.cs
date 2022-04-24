using Client.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Winook;

namespace Client.Classes
{
    public class Recording
    {
        //private int _processId { get; set; }
        private MouseHook _mouseHook;
        private KeyboardHook _keyboardHook;
        private MainWindow _window;
        private Process _currentProcess;
        private Point _test;
        private long _currentTicks { get; set; }
        public List<Frame> Frames { get; set; }

        public Recording(MouseHook mouseHook, KeyboardHook keyboardHook, MainWindow window)
        {
            this._mouseHook = mouseHook;
            this._keyboardHook = keyboardHook;
            this._window = window;
        }
        
        public Recording(List<Frame> Frames,MainWindow window, Process process)
        {
            this.Frames = Frames;
            this._window = window;
            this._currentProcess = process;
        }

        public void Start()
        {
            _currentTicks = DateTime.Now.Ticks;
            Frames = new List<Frame>();

            _mouseHook.LeftButtonDown += LeftClick;
            _mouseHook.RightButtonDown += RightClick;

            // todo
            //_mouseHook.MouseWheel += MouseWheel;

            _keyboardHook.MessageReceived += InputReceived;
        }
        public Recording Stop()
        {
            UpdateTicks();

            if (_mouseHook != null)
                _mouseHook.RemoveAllHandlers();

            if (_keyboardHook != null)
                _keyboardHook.RemoveAllHandlers();

            return this;
        }
        public async void Play()
        {
            if (!Frames.Any())
                return;

            var hwnd = new WindowInteropHelper(this._window);

            var tasks = new List<Task>();
            foreach (var frame in Frames)
            {
                var type = frame.InputData.GetType();

                //if (type == typeof(Idle))
                //{
                //    var data = (Idle)frame.InputData;
                //    Thread.Sleep((int)data.Ticks / 10000);
                //}

                if (type == typeof(KeyStroke))
                {
                  
                    var data = (KeyStroke)frame.InputData;
                    tasks.Add(Task.Factory.StartNew(() => SendKey(data)));
                    //ProcessHelpers.sendKeystroke((ushort)data.Code, _currentProcess.MainWindowHandle);
                }

                if (type == typeof(MouseClick))
                {
                    var data = (MouseClick)frame.InputData;
                    tasks.Add(Task.Factory.StartNew(() => Click(data)));
                }
             
            }

            Task.WaitAll(tasks.ToArray()); 
        }
        public void InputReceived(object sender, KeyboardMessageEventArgs e)
        {
            UpdateTicks();
            Frames.Add(new Frame(new KeyStroke(e.KeyValue, e.Shift)));
            //Debug.Write($"Code: {e.KeyValue}; Modifiers: {e.Modifiers:x}; Flags: {e.Flags:x}; ");
            //Debug.WriteLine($"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}");
        }

        public void LeftClick(object sender, MouseMessageEventArgs e)
        {
            UpdateTicks();
            Frames.Add(new Frame(new MouseClick(e.X, e.Y, Button.Left)));
            //Debug.Write($"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Modifiers: {e.Modifiers:x}; ");
            //Debug.WriteLine($"Delta: {e.Delta}; XButtons: {e.XButtons}");

        }

        public void RightClick(object sender, MouseMessageEventArgs e)
        {
            UpdateTicks();
            Frames.Add(new Frame(new MouseClick(e.X, e.Y, Button.Right)));
            //Debug.Write($"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Modifiers: {e.Modifiers:x}; ");
            //Debug.WriteLine($"Delta: {e.Delta}; XButtons: {e.XButtons}");
        }

        //public void MouseWheel(object sender, MouseMessageEventArgs e)
        //{
        //    UpdateTicks();

        //    Debug.Write($"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Modifiers: {e.Modifiers:x}; ");
        //    Debug.WriteLine($"Delta: {e.Delta}; XButtons: {e.XButtons}");
            
            
        //    ProcessHelpers.SetCursorPos((int)_test.X, (int)_test.Y);
        //    ProcessHelpers.mouse_event(ProcessHelpers.MouseEvent.MOUSEEVENTF_LEFTDOWN, 460, 342, 0, 0);
        //    ProcessHelpers.mouse_event(ProcessHelpers.MouseEvent.MOUSEEVENTF_LEFTUP, 460, 342, 0, 0);
        //}

        private void UpdateTicks()
        {
            //if (Frames != null && Frames.Any())
            //{
            //    var timeIdle = DateTime.Now.Ticks - _currentTicks;
            //    _currentTicks = DateTime.Now.Ticks;
            //    // only care if idled longer than 1 second
            //    if (timeIdle > 10000000)
            //        Frames.Add(new Frame(new Idle(timeIdle)));
            //}
        }

        private void Click(MouseClick data)
        {
            var thread = new Thread(() =>
            {
                var eventTypeDown = data.Button == Button.Left ? ProcessHelpers.MouseEvent.LEFTDOWN :
                                                                    ProcessHelpers.MouseEvent.RIGHTDOWN;

                var eventTypeUp = data.Button == Button.Left ? ProcessHelpers.MouseEvent.LEFTUP :
                                                                    ProcessHelpers.MouseEvent.RIGHTUP;
                ProcessHelpers.SetCursorPos(data.X, data.Y);
                ProcessHelpers.mouse_event((int)eventTypeDown, data.X, data.Y, 0, 0);
                ProcessHelpers.mouse_event((int)eventTypeUp, data.X, data.Y, 0, 0);
            });

            thread.Start();
        }

        private void SendKey(KeyStroke data)
        {
            // sleep a random amount of time between 100 milliseconds to 500
            var sleepTimeGenerator = new Random();
            int sleepTime = sleepTimeGenerator.Next(100, 500);
            ProcessHelpers.SetForegroundWindow(_currentProcess.MainWindowHandle);
            KeyBoardInput.SendString(data.ToString());
            
            Thread.Sleep(sleepTime);

        }
    }
}
