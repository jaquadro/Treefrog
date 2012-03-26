using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Treefrog.View.Controls.WinEx
{
    public partial class ComboBoxEx : ComboBox
    {
        private const int WM_PAINT = 0xF;

        public ComboBoxEx ()
            : base()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.FlatStyle = FlatStyle.Popup;
            //this.IntegralHeight = false;
            //this.DropDownStyle = ComboBoxStyle.Simple;
        }

        protected override void WndProc (ref Message m)
        {
            IntPtr hDC = IntPtr.Zero;
            Graphics gdc = null;

            switch (m.Msg) {
                case WM_PAINT:
                    base.WndProc(ref m);
                    hDC = GetWindowDC(this.Handle);
                    gdc = Graphics.FromHdc(hDC);
                    PaintFlatControlBorder(this, gdc);
                    ReleaseDC(m.HWnd, hDC);
                    gdc.Dispose();
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }            
        }

        //private void WndProcPaint ()
        //{
        //    if (!base.GetStyle(ControlStyles.UserPaint) && ((this.FlatStyle == FlatStyle.Flat) || (this.FlatStyle == FlatStyle.Popup))) {
        //        using (WindowsRegion region = new WindowsRegion(this.FlatComboBoxAdapter.dropDownRect)) {
        //            using (WindowsRegion region2 = new WindowsRegion(base.Bounds)) {
        //                IntPtr wParam;
        //                NativeMethods.RegionFlags flags = (NativeMethods.RegionFlags)SafeNativeMethods.GetUpdateRgn(new HandleRef(this, base.Handle), new HandleRef(this, region2.HRegion), true);
        //                region.CombineRegion(region2, region, RegionCombineMode.DIFF);
        //                Rectangle updateRegionBox = region2.ToRectangle();
        //                this.FlatComboBoxAdapter.ValidateOwnerDrawRegions(this, updateRegionBox);
        //                NativeMethods.PAINTSTRUCT lpPaint = new NativeMethods.PAINTSTRUCT();
        //                bool flag2 = false;
        //                if (m.WParam == IntPtr.Zero) {
        //                    wParam = UnsafeNativeMethods.BeginPaint(new HandleRef(this, base.Handle), ref lpPaint);
        //                    flag2 = true;
        //                }
        //                else {
        //                    wParam = m.WParam;
        //                }
        //                using (DeviceContext context = DeviceContext.FromHdc(wParam)) {
        //                    using (WindowsGraphics graphics = new WindowsGraphics(context)) {
        //                        if (flags != NativeMethods.RegionFlags.ERROR) {
        //                            graphics.DeviceContext.SetClip(region);
        //                        }
        //                        m.WParam = wParam;
        //                        this.DefWndProc(ref m);
        //                        if (flags != NativeMethods.RegionFlags.ERROR) {
        //                            graphics.DeviceContext.SetClip(region2);
        //                        }
        //                        using (Graphics graphics2 = Graphics.FromHdcInternal(wParam)) {
        //                            this.FlatComboBoxAdapter.DrawFlatCombo(this, graphics2);
        //                        }
        //                    }
        //                }
        //                if (flag2) {
        //                    UnsafeNativeMethods.EndPaint(new HandleRef(this, base.Handle), ref lpPaint);
        //                }
        //            }
        //            return;
        //        }
        //    }
        //}

        private void PaintFlatControlBorder (Control control, Graphics g)
        {
            Rectangle rect1 = new Rectangle(0, 0, control.Width, control.Height);

            ControlPaint.DrawBorder(g, rect1, Color.Gray, ButtonBorderStyle.Solid);
        }

        [DllImport("user32")]
        private static extern IntPtr GetWindowDC (IntPtr hWnd);

        [DllImport("user32")]
        private static extern int ReleaseDC (IntPtr hWnd, IntPtr hDC);
    }

    public partial class ComboBoxEx
    {
        internal class FlatComboAdapter
        {
            private Rectangle clientRect;
            internal Rectangle dropDownRect;
            private Rectangle innerBorder;
            private Rectangle innerInnerBorder;
            private RightToLeft origRightToLeft;
            private Rectangle outerBorder;
            private Rectangle whiteFillRect;
            private const int WhiteFillRectWidth = 5;

            public FlatComboAdapter (ComboBox comboBox, bool smallButton)
            {
                this.clientRect = comboBox.ClientRectangle;
                int horizontalScrollBarArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;
                this.outerBorder = new Rectangle(this.clientRect.Location, new Size(this.clientRect.Width - 1, this.clientRect.Height - 1));
                this.innerBorder = new Rectangle(this.outerBorder.X + 1, this.outerBorder.Y + 1, (this.outerBorder.Width - horizontalScrollBarArrowWidth) - 2, this.outerBorder.Height - 2);
                this.innerInnerBorder = new Rectangle(this.innerBorder.X + 1, this.innerBorder.Y + 1, this.innerBorder.Width - 2, this.innerBorder.Height - 2);
                this.dropDownRect = new Rectangle(this.innerBorder.Right + 1, this.innerBorder.Y, horizontalScrollBarArrowWidth, this.innerBorder.Height + 1);
                if (smallButton) {
                    this.whiteFillRect = this.dropDownRect;
                    this.whiteFillRect.Width = 5;
                    this.dropDownRect.X += 5;
                    this.dropDownRect.Width -= 5;
                }
                this.origRightToLeft = comboBox.RightToLeft;
                if (this.origRightToLeft == RightToLeft.Yes) {
                    this.innerBorder.X = this.clientRect.Width - this.innerBorder.Right;
                    this.innerInnerBorder.X = this.clientRect.Width - this.innerInnerBorder.Right;
                    this.dropDownRect.X = this.clientRect.Width - this.dropDownRect.Right;
                    this.whiteFillRect.X = (this.clientRect.Width - this.whiteFillRect.Right) + 1;
                }
            }

            public virtual void DrawFlatCombo (ComboBox comboBox, Graphics g)
            {
                if (comboBox.DropDownStyle != ComboBoxStyle.Simple) {
                    Color outerBorderColor = this.GetOuterBorderColor(comboBox);
                    Color innerBorderColor = this.GetInnerBorderColor(comboBox);
                    bool flag = comboBox.RightToLeft == RightToLeft.Yes;
                    this.DrawFlatComboDropDown(comboBox, g, this.dropDownRect);
                    if (!IsZeroWidthOrHeight(this.whiteFillRect)) {
                        using (Brush brush = new SolidBrush(innerBorderColor)) {
                            g.FillRectangle(brush, this.whiteFillRect);
                        }
                    }
                    if (outerBorderColor.IsSystemColor) {
                        Pen pen = SystemPens.FromSystemColor(outerBorderColor);
                        g.DrawRectangle(pen, this.outerBorder);
                        if (flag) {
                            g.DrawRectangle(pen, new Rectangle(this.outerBorder.X, this.outerBorder.Y, this.dropDownRect.Width + 1, this.outerBorder.Height));
                        }
                        else {
                            g.DrawRectangle(pen, new Rectangle(this.dropDownRect.X, this.outerBorder.Y, this.outerBorder.Right - this.dropDownRect.X, this.outerBorder.Height));
                        }
                    }
                    else {
                        using (Pen pen2 = new Pen(outerBorderColor)) {
                            g.DrawRectangle(pen2, this.outerBorder);
                            if (flag) {
                                g.DrawRectangle(pen2, new Rectangle(this.outerBorder.X, this.outerBorder.Y, this.dropDownRect.Width + 1, this.outerBorder.Height));
                            }
                            else {
                                g.DrawRectangle(pen2, new Rectangle(this.dropDownRect.X, this.outerBorder.Y, this.outerBorder.Right - this.dropDownRect.X, this.outerBorder.Height));
                            }
                        }
                    }
                    if (innerBorderColor.IsSystemColor) {
                        Pen pen3 = SystemPens.FromSystemColor(innerBorderColor);
                        g.DrawRectangle(pen3, this.innerBorder);
                        g.DrawRectangle(pen3, this.innerInnerBorder);
                    }
                    else {
                        using (Pen pen4 = new Pen(innerBorderColor)) {
                            g.DrawRectangle(pen4, this.innerBorder);
                            g.DrawRectangle(pen4, this.innerInnerBorder);
                        }
                    }
                    if (!comboBox.Enabled || (comboBox.FlatStyle == FlatStyle.Popup)) {
                        bool focused = comboBox.ContainsFocus; //|| comboBox.MouseIsOver;
                        using (Pen pen5 = new Pen(this.GetPopupOuterBorderColor(comboBox, focused))) {
                            Pen pen6 = comboBox.Enabled ? pen5 : SystemPens.Control;
                            if (flag) {
                                g.DrawRectangle(pen6, new Rectangle(this.outerBorder.X, this.outerBorder.Y, this.dropDownRect.Width + 1, this.outerBorder.Height));
                            }
                            else {
                                g.DrawRectangle(pen6, new Rectangle(this.dropDownRect.X, this.outerBorder.Y, this.outerBorder.Right - this.dropDownRect.X, this.outerBorder.Height));
                            }
                            g.DrawRectangle(pen5, this.outerBorder);
                        }
                    }
                }
            }

            protected virtual void DrawFlatComboDropDown (ComboBox comboBox, Graphics g, Rectangle dropDownRect)
            {
                g.FillRectangle(SystemBrushes.Control, dropDownRect);
                Brush brush = comboBox.Enabled ? SystemBrushes.ControlText : SystemBrushes.ControlDark;
                Point point = new Point(dropDownRect.Left + (dropDownRect.Width / 2), dropDownRect.Top + (dropDownRect.Height / 2));
                if (this.origRightToLeft == RightToLeft.Yes) {
                    point.X -= dropDownRect.Width % 2;
                }
                else {
                    point.X += dropDownRect.Width % 2;
                }
                Point[] points = new Point[] { new Point(point.X - 2, point.Y - 1), new Point(point.X + 3, point.Y - 1), new Point(point.X, point.Y + 2) };
                g.FillPolygon(brush, points);
            }

            protected virtual Color GetInnerBorderColor (ComboBox comboBox)
            {
                if (!comboBox.Enabled) {
                    return SystemColors.Control;
                }
                return comboBox.BackColor;
            }

            protected virtual Color GetOuterBorderColor (ComboBox comboBox)
            {
                if (!comboBox.Enabled) {
                    return SystemColors.ControlDark;
                }
                return SystemColors.Window;
            }

            protected virtual Color GetPopupOuterBorderColor (ComboBox comboBox, bool focused)
            {
                if (comboBox.Enabled && !focused) {
                    return SystemColors.Window;
                }
                return SystemColors.ControlDark;
            }

            public bool IsValid (ComboBox combo)
            {
                return ((combo.ClientRectangle == this.clientRect) && (combo.RightToLeft == this.origRightToLeft));
            }

            private static bool IsZeroWidthOrHeight (Rectangle rectangle)
            {
                if (rectangle.Width != 0) {
                    return (rectangle.Height == 0);
                }
                return true;
            }

            //public void ValidateOwnerDrawRegions (ComboBox comboBox, Rectangle updateRegionBox)
            //{
            //    if (comboBox == null) {
            //        NativeMethods.RECT rect;
            //        Rectangle r = new Rectangle(0, 0, comboBox.Width, this.innerBorder.Top);
            //        Rectangle rectangle2 = new Rectangle(0, this.innerBorder.Bottom, comboBox.Width, comboBox.Height - this.innerBorder.Bottom);
            //        Rectangle rectangle3 = new Rectangle(0, 0, this.innerBorder.Left, comboBox.Height);
            //        Rectangle rectangle4 = new Rectangle(this.innerBorder.Right, 0, comboBox.Width - this.innerBorder.Right, comboBox.Height);
            //        if (r.IntersectsWith(updateRegionBox)) {
            //            rect = new NativeMethods.RECT(r);
            //            SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
            //        }
            //        if (rectangle2.IntersectsWith(updateRegionBox)) {
            //            rect = new NativeMethods.RECT(rectangle2);
            //            SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
            //        }
            //        if (rectangle3.IntersectsWith(updateRegionBox)) {
            //            rect = new NativeMethods.RECT(rectangle3);
            //            SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
            //        }
            //        if (rectangle4.IntersectsWith(updateRegionBox)) {
            //            rect = new NativeMethods.RECT(rectangle4);
            //            SafeNativeMethods.ValidateRect(new HandleRef(comboBox, comboBox.Handle), ref rect);
            //        }
            //    }
            //}
        }
    }

    public class CustomProRenderer : ToolStripProfessionalRenderer
    {

    }
}
