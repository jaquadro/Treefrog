using System;
using System.Drawing;
using System.Windows.Forms;

namespace Treefrog.Windows.Controls.WinEx
{
    public class LayerListView : ListView
    {
        private const int WM_LBUTTONDBLCLK = 0x203;

        protected override void WndProc (ref Message m)
        {
            if (m.Msg >= 0x201 && m.Msg <= 0x209) {
                if (m.Msg == WM_LBUTTONDBLCLK) {
                    OnDoubleClick(EventArgs.Empty);
                    return;
                }

                Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                ListViewHitTestInfo hit = this.HitTest(pos);

                switch (hit.Location) {
                    case ListViewHitTestLocations.AboveClientArea:
                    case ListViewHitTestLocations.BelowClientArea:
                    case ListViewHitTestLocations.LeftOfClientArea:
                    case ListViewHitTestLocations.RightOfClientArea:
                    case ListViewHitTestLocations.None:
                        return;
                }
            }

            base.WndProc(ref m);
        }
    }
}
