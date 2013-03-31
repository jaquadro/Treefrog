using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using LilyPath;

namespace Treefrog.Windows.Controls
{
    public class LilyPathControl : GraphicsDeviceControl
    {
        private DrawBatch _drawBatch;

        public Color ClearColor { get; set; }

        public Action<DrawBatch> DrawAction { get; set; }

        protected override void Dispose (bool disposing)
        {
            if (disposing)
                Application.Idle -= IdleHandler;

            base.Dispose(disposing);
        }

        protected override void Initialize ()
        {
            _drawBatch = new DrawBatch(GraphicsDevice);

            ClearColor = Color.DarkGray;

            Application.Idle += IdleHandler;
        }

        protected override void Draw ()
        {
            GraphicsDevice.Clear(ClearColor);

             if (DrawAction != null)
                DrawAction(_drawBatch);
        }

        private void IdleHandler (object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
