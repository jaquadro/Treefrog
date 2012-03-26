using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows;

namespace Treefrog.V2.Controls.Layers
{
    public class RenderLayer : XnaCanvasLayer
    {
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _target;

        public static readonly DependencyProperty OpacityProperty;

        static RenderLayer ()
        {
            OpacityProperty = DependencyProperty.Register("Opacity",
                typeof(float), typeof(RenderLayer), new PropertyMetadata(1.0f));
        }

        public float Opacity
        {
            get { return (float)this.GetValue(OpacityProperty); }
            set { this.SetValue(OpacityProperty, value); }
        }

        protected override void RenderCore (GraphicsDevice device)
        {
            if (_spriteBatch == null || _spriteBatch.GraphicsDevice != device)
                _spriteBatch = new SpriteBatch(device);

            RenderCore(_spriteBatch);
        }

        private static Random rand = new Random();

        protected virtual void RenderCore (SpriteBatch spriteBatch)
        {
            Texture2D tex = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            tex.SetData(new Color[] { Color.White });

            int x = 40;
            int y = 40;
            int w = 40;

            Vector offset = BeginDraw(spriteBatch);

            //Matrix mat = Matrix.CreateTranslation(-(float)HorizontalOffset, -(float)VerticalOffset, 0);
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, mat);
            spriteBatch.Draw(tex, new Microsoft.Xna.Framework.Rectangle(x, y, w, w), Color.White);
            //spriteBatch.End();

            EndDraw(spriteBatch, offset);
        }

        protected Rect VisibleRegion
        {
            get
            {
                return new Rect(HorizontalOffset, VerticalOffset,
                    Math.Ceiling(Math.Min(ViewportWidth / ZoomFactor, VirtualWidth)),
                    Math.Ceiling(Math.Min(ViewportHeight / ZoomFactor, VirtualHeight)));
            }
        }

        protected Vector VirtualSurfaceOffset
        {
            get
            {
                return new Vector(0, 0);
            }
        }

        private Vector BeginDrawInner (SpriteBatch spriteBatch)
        {
            Rect region = VisibleRegion;
            Vector offset = VirtualSurfaceOffset;

            offset.X = Math.Ceiling(offset.X - region.X * ZoomFactor);
            offset.Y = Math.Ceiling(offset.Y - region.Y * ZoomFactor);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation((float)offset.X, (float)offset.Y, 0));
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            return offset;
        }

        private void EndDrawInner (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        protected Vector BeginDraw (SpriteBatch spriteBatch)
        {
            _target = null;
            if (Opacity < 1f) {
                _target = new RenderTarget2D(spriteBatch.GraphicsDevice,
                    spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height,
                    false, SurfaceFormat.Color, DepthFormat.None);

                spriteBatch.GraphicsDevice.SetRenderTarget(_target);
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            }

            return BeginDrawInner(spriteBatch);
        }

        protected void EndDraw (SpriteBatch spriteBatch, Vector offset)
        {
            EndDrawInner(spriteBatch);

            if (_target != null) {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);

                BeginDrawInner(spriteBatch);
                spriteBatch.Draw(_target, new Vector2((float)-offset.X, (float)-offset.Y), new Color(1f, 1f, 1f, Opacity));
                EndDrawInner(spriteBatch);

                _target = null;
            }
        }

        public override double VirtualHeight
        {
            get
            {
                return 1000;
            }
        }

        public override double VirtualWidth
        {
            get
            {
                return 1000;
            }
        }
    }
}
