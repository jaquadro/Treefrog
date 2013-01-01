using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Amphibian.Drawing;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;

namespace Treefrog.Windows.Layers
{
    public class WorkspaceLayer : RenderLayer
    {
        private Texture2D _pattern;

        public WorkspaceLayer (WorkspaceLayerPresenter model)
            : base(model)
        {
            Mode = RenderMode.Sprite | RenderMode.Drawing;
        }

        protected new WorkspaceLayerPresenter Model
        {
            get { return ModelCore as WorkspaceLayerPresenter; }
        }

        protected override void DisposeManaged ()
        {
            if (_pattern != null)
                _pattern.Dispose();

            base.DisposeManaged();
        }

        protected override void RenderCore (SpriteBatch spriteBatch)
        {
            Scissor = true;
            Vector2 offset = BeginDraw(spriteBatch, SamplerState.PointWrap);
            RenderContent(spriteBatch);
            EndDraw(spriteBatch, offset);
        }

        protected override void RenderCore (DrawBatch drawBatch)
        {
            Scissor = false;
            base.RenderCore(drawBatch);
        }

        protected override void RenderContent (SpriteBatch spriteBatch)
        {
            if (_pattern == null)
                _pattern = BuildCanvasPattern(spriteBatch.GraphicsDevice);

            ILevelGeometry geometry = LevelGeometry;
            Rectangle bounds = geometry.VisibleBounds.ToXnaRectangle();

            Rectangle dest = new Rectangle(
                (int)Math.Ceiling(bounds.X * geometry.ZoomFactor),
                (int)Math.Ceiling(bounds.Y * geometry.ZoomFactor),
                (int)(bounds.Width * geometry.ZoomFactor),
                (int)(bounds.Height * geometry.ZoomFactor)
                );

            spriteBatch.Draw(_pattern, dest, dest, Color.White);
        }

        protected override void RenderContent (DrawBatch drawBatch)
        {
            ILevelGeometry geometry = LevelGeometry;
            Rectangle levelBounds = geometry.LevelBounds.ToXnaRectangle();

            Rectangle bounds = new Rectangle(
                (int)Math.Ceiling(levelBounds.X * geometry.ZoomFactor),
                (int)Math.Ceiling(levelBounds.Y * geometry.ZoomFactor),
                (int)(levelBounds.Width * geometry.ZoomFactor),
                (int)(levelBounds.Height * geometry.ZoomFactor)
                );
            drawBatch.DrawRectangle(bounds, Pens.Black);

            if (levelBounds.X != 0)
                drawBatch.DrawLine(new Point(0, bounds.Top), new Point(0, bounds.Bottom), Pens.Gray);
            if (levelBounds.Y != 0)
                drawBatch.DrawLine(new Point(bounds.Left, 0), new Point(bounds.Right, 0), Pens.Gray);
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
