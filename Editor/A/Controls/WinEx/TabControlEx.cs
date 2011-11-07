using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Editor.A.Controls.WinEx
{
    public class TabControlEx : TabControl
    {
        private const int TCM_FIRST = 0x1300;
        private const int TCM_ADJUSTRECT = TCM_FIRST + 40;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        protected override void WndProc (ref Message m)
        {
            if (m.Msg == TCM_ADJUSTRECT) {
                RECT rc = (RECT)m.GetLParam(typeof(RECT));
                rc.Left -= 3;
                rc.Right += 0;
                rc.Top -= 1;
                rc.Bottom += 2;
                Marshal.StructureToPtr(rc, m.LParam, true);
            }

            base.WndProc(ref m);
        }
    }
}
