using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvalonDock.Controls
{
    class FocusChangeEventArgs : EventArgs
    {
        public FocusChangeEventArgs(IntPtr gotFocusWinHandle, IntPtr lostFocusWinHandle)
        {
            GotFocusWinHandle = gotFocusWinHandle;
            LostFocusWinHandle = lostFocusWinHandle;
        }

        public IntPtr GotFocusWinHandle
        {
            get;
            private set;
        }
        public IntPtr LostFocusWinHandle
        {
            get;
            private set;
        }
    }

    class FocusHookHandler
    {
        public FocusHookHandler()
        { 
        
        }
        
        IntPtr _focusHook;
        Win32Helper.HookProc _hookProc;
        public void Attach()
        {
            _hookProc = new Win32Helper.HookProc(this.HookProc);
            _focusHook = Win32Helper.SetWindowsHookEx(
                Win32Helper.HookType.WH_CBT,
                _hookProc,
                IntPtr.Zero,
                (int)Win32Helper.GetCurrentThreadId());
        }


        public void Detach()
        {
            Win32Helper.UnhookWindowsHookEx(_focusHook);
        }   

        public int HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code == Win32Helper.HCBT_SETFOCUS)
            {
                if (FocusChanged != null)
                    FocusChanged(this, new FocusChangeEventArgs(wParam, lParam));
            }
            

            return Win32Helper.CallNextHookEx(_focusHook, code, wParam, lParam);
        }

        public event EventHandler<FocusChangeEventArgs> FocusChanged;

    }
}
