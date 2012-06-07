using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows;
using System.Runtime;

namespace Treefrog.Controls.Hooks
{
    public class SystemMouseButtonEventArgs : EventArgs
    {
        public MouseButtonState ButtonState { get; private set; }
        public MouseButton ChangedButton { get; private set; }
        public Point ScreenLocation { get; private set; }

        public SystemMouseButtonEventArgs (MouseButton button, MouseButtonState state, Point location)
        {
            ButtonState = state;
            ChangedButton = button;
            ScreenLocation = location;
        }
    }

    public static class MouseHook
    {
        private const int WH_MOUSE_LL = 14;

        private delegate IntPtr LowLevelMouseProc (int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelMouseProc _proc;
        private static IntPtr _hookId = IntPtr.Zero;

        public static void Attach ()
        {
            _proc = HookCallback;
            _hookId = SetHook(_proc);
        }

        public static void Detach ()
        {
            UnhookWindowsHookEx(_hookId);
        }

        public static event EventHandler<SystemMouseButtonEventArgs> MouseDown;

        public static event EventHandler<SystemMouseButtonEventArgs> MouseUp;

        private enum MouseMessages
        {
            WM_MOUSEMOVE = 0x200,
            WM_LBUTTONDOWN = 0x201,
            WM_LBUTTONUP = 0x202,
            WM_LBUTTONDBCLK = 0x203,
            WM_RBUTTONDOWN = 0x204,
            WM_RBUTTONUP = 0x205,
            WM_RBUTTONDBCLK = 0x206,
            WM_MBUTTONDOWN = 0x207,
            WM_MBUTTONUP = 0x208,
            WM_MBUTTONDBCLK = 0x209,
            WM_MOUSEWHEEL = 0x20A,
            WM_XBUTTONDOWN = 0x20B,
            WM_XBUTTONUP = 0x20C,
            WM_XBUTTONDBCLK = 0x20D,
        }

        private static IntPtr SetHook (LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private struct HookMessage
        {
            public IntPtr Message;
            public MSLLHOOKSTRUCT HookStruct;
        }

        private static void HandleHookMessage (HookMessage hMsg)
        {
            switch ((MouseMessages)hMsg.Message) {
                case MouseMessages.WM_LBUTTONDOWN:
                    OnLeftMouseDown(hMsg.HookStruct);
                    break;
                case MouseMessages.WM_RBUTTONDOWN:
                    OnRightMouseDown(hMsg.HookStruct);
                    break;
                case MouseMessages.WM_MBUTTONDOWN:
                    OnMiddleMouseDown(hMsg.HookStruct);
                    break;
                case MouseMessages.WM_LBUTTONUP:
                    OnLeftMouseUp(hMsg.HookStruct);
                    break;
                case MouseMessages.WM_RBUTTONUP:
                    OnRightMouseUp(hMsg.HookStruct);
                    break;
                case MouseMessages.WM_MBUTTONUP:
                    OnMiddleMouseUp(hMsg.HookStruct);
                    break;
            }
        }

        private static IntPtr HookCallback (int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && Enum.IsDefined(typeof(MouseMessages), (int)wParam)) {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                Window window = Application.Current.MainWindow;
                if (window != null)
                    window.Dispatcher.BeginInvoke(new Action<HookMessage>(HandleHookMessage), new HookMessage()
                    {
                        Message = wParam,
                        HookStruct = hookStruct,
                    });
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static void OnMouseDown (SystemMouseButtonEventArgs e)
        {
            var ev = MouseDown;
            if (ev != null)
                ev(null, e);
        }

        private static void OnLeftMouseDown (MSLLHOOKSTRUCT info)
        {
            var e = new SystemMouseButtonEventArgs(MouseButton.Left, MouseButtonState.Pressed, new Point(info.pt.X, info.pt.Y));
            OnMouseDown(e);
        }

        private static void OnRightMouseDown (MSLLHOOKSTRUCT info)
        {
            var e = new SystemMouseButtonEventArgs(MouseButton.Right, MouseButtonState.Pressed, new Point(info.pt.X, info.pt.Y));
            OnMouseDown(e);
        }

        private static void OnMiddleMouseDown (MSLLHOOKSTRUCT info)
        {
            var e = new SystemMouseButtonEventArgs(MouseButton.Middle, MouseButtonState.Pressed, new Point(info.pt.X, info.pt.Y));
            OnMouseDown(e);
        }

        private static void OnMouseUp (SystemMouseButtonEventArgs e)
        {
            var ev = MouseUp;
            if (ev != null)
                ev(null, e);
        }

        private static void OnLeftMouseUp (MSLLHOOKSTRUCT info)
        {
            var e = new SystemMouseButtonEventArgs(MouseButton.Left, MouseButtonState.Released, new Point(info.pt.X, info.pt.Y));
            OnMouseUp(e);
        }

        private static void OnRightMouseUp (MSLLHOOKSTRUCT info)
        {
            var e = new SystemMouseButtonEventArgs(MouseButton.Right, MouseButtonState.Released, new Point(info.pt.X, info.pt.Y));
            OnMouseUp(e);
        }

        private static void OnMiddleMouseUp (MSLLHOOKSTRUCT info)
        {
            var e = new SystemMouseButtonEventArgs(MouseButton.Middle, MouseButtonState.Released, new Point(info.pt.X, info.pt.Y));
            OnMouseUp(e);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx (int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx (IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx (IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle (string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}
