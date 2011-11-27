using System;
using System.Drawing;
using System.Windows.Forms;

namespace Treefrog.View.Controls.WinEx
{
    public class ColorButton : Button
    {
        private Color _color = SystemColors.Control;

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                Invalidate();
            }
        }

        protected override void OnPaint (PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            Graphics g = pevent.Graphics;
            Rectangle boxRect = new Rectangle(
                ClientRectangle.X + 4, ClientRectangle.Y + 4,
                ClientRectangle.Width - 9, ClientRectangle.Height - 9
                );

            g.FillRectangle(new SolidBrush(_color), boxRect);
            g.DrawRectangle(new Pen(Color.Black, 1), boxRect);
        }
    }
}
