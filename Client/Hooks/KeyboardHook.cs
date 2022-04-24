using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Winook;

namespace Client.Hooks
{
    public static class KeyboardHook
    {
        public static void InputReceived(object sender, KeyboardMessageEventArgs e)
        {
            Debug.Write($"Code: {e.KeyValue}; Modifiers: {e.Modifiers:x}; Flags: {e.Flags:x}; ");
            Debug.WriteLine($"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}");
        }
    }
}
