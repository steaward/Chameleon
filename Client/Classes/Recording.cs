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
        private IntPtr _startupWindowHandle;
        private IntPtr _targetWindowHandle;
        private long _currentTicks { get; set; }
        public List<Frame> Frames { get; set; }
        private Frame _previousFrame { get; set; }
        // used to know if we are entering string.
        // easier to enter string all at once at computer than to send 1 char at a time...?
        // maybe bot prevention will see this tho.

        public Recording(MouseHook mouseHook, KeyboardHook keyboardHook)
        {
            this._mouseHook = mouseHook;
            this._keyboardHook = keyboardHook;
        }
        
        public Recording(List<Frame> Frames, IntPtr _targetWindowHandle, IntPtr _startupWindowHandle)
        {
            this.Frames = Frames;
            this._targetWindowHandle = _targetWindowHandle;
            this._startupWindowHandle = _startupWindowHandle;
        }

        /// <summary>
        /// Setup global hooks for events to record.
        /// </summary>
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

        /// <summary>
        /// Clear all global hooks for events
        /// </summary>
        /// <returns></returns>
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

            foreach (var frame in Frames)
            {
                await frame.Play(_oldSchoolRunescapeHandle, _startUpWindowHandle);
                _previousFrame = frame;
            }
        }

        public string Save()
        {
            return "";
        }
       
        private void InputReceived(object sender, KeyboardMessageEventArgs e)
        {
            // for now - don't capture just shift key...
            if (e.KeyValue == 16 && e.Shift)
                return;

            if (e.Direction != KeyDirection.Down)
                return;
            
            UpdateTicks();
            var keyStroke = new KeyStroke(e.KeyValue, e.Shift);
            Frames.Add(new Frame(keyStroke));
            Debug.WriteLine($"Captured input: {keyStroke}");
            
            
            //Debug.Write($"Code: {e.KeyValue}; Modifiers: {e.Modifiers:x}; Flags: {e.Flags:x}; ");
            //Debug.WriteLine($"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}");
        }

        private void LeftClick(object sender, MouseMessageEventArgs e)
        {
            UpdateTicks();
            Frames.Add(new Frame(new MouseClick(e.X, e.Y, Button.Left)));
            //Debug.Write($"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Modifiers: {e.Modifiers:x}; ");
            //Debug.WriteLine($"Delta: {e.Delta}; XButtons: {e.XButtons}");

        }

        private void RightClick(object sender, MouseMessageEventArgs e)
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
            if (Frames != null && Frames.Any())
            {
                var timeIdle = DateTime.Now.Ticks - _currentTicks;
                _currentTicks = DateTime.Now.Ticks;
                // only care if idled longer than 1 second
                if (timeIdle > 10000000)
                    Frames.Add(new Frame(new Idle(timeIdle)));
            }
        }

    }
}
