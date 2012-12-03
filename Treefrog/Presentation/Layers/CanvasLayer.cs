using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Windows.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Treefrog.Presentation.Layers
{
    public class CanvasLayer : BaseControlLayer
    {
        private Texture2D _pattern;

        public CanvasLayer (LayerControl control)
            : base(control)
        {
        }

        public override int VirtualHeight
        {
            get { return 0; }
        }

        public override int VirtualWidth
        {
            get { return 0; }
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch)
        {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));

            return offset;
        }

        protected void EndDraw (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        public new void DrawContent (SpriteBatch spriteBatch)
        {
            if (Visible == false)
                return;

            if (_pattern == null)
                _pattern = BuildCanvasPattern(spriteBatch.GraphicsDevice);

            Vector2 offset = BeginDraw(spriteBatch);

            Rectangle dest = new Rectangle(0, 0, (int)(Control.VirtualWidth * Control.Zoom), (int)(Control.VirtualHeight * Control.Zoom));

            spriteBatch.Draw(_pattern, dest, dest, Color.White);

            EndDraw(spriteBatch);
        }

        private Texture2D BuildCanvasPattern (GraphicsDevice device)
        {
            Color color1 = new Color(1f, 1f, 1f);
            Color color2 = new Color(.95f, .95f, .95f);

            byte[] data = new byte[16 * 16 * 4];
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    int index1 = (y * 16 + x) * 4;
                    int index2 = (y * 16 + x + 8) * 4;
                    int index3 = ((y + 8) * 16 + x) * 4;
                    int index4 = ((y + 8) * 16 + x + 8) * 4;

                    SetPixel(data, index1, color1);
                    SetPixel(data, index2, color2);
                    SetPixel(data, index3, color2);
                    SetPixel(data, index4, color1);
                }
            }

            Texture2D tex = new Texture2D(device, 16, 16);
            tex.SetData<byte>(data);

            return tex;
        }

        private void SetPixel (byte[] data, int index, Color color)
        {
            data[index + 0] = color.R;
            data[index + 1] = color.G;
            data[index + 2] = color.B;
            data[index + 3] = color.A;
        }
    }
}
