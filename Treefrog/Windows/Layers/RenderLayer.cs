using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;
using Amphibian.Drawing;
using Microsoft.Xna.Framework;

namespace Treefrog.Windows.Layers
{
    public class RenderLayer : CanvasLayer
    {
        [Flags]
        protected enum RenderMode
        {
            Sprite,
            Drawing,
        }

        private SpriteBatch _spriteBatch;
        private RenderTarget2D _target;

        public RenderLayer ()
        {
            Mode = RenderMode.Sprite;
        }

        protected override void DisposeManaged ()
        {
            if (_spriteBatch != null)
                _spriteBatch.Dispose();
            if (_target != null)
                _target.Dispose();

            base.DisposeManaged();
        }

        public RenderLayerPresenter Model { get; set; }

        protected RenderMode Mode { get; set; }

        private float LayerOpacity 
        {
            get { return 1f; }
        }

        protected override void RenderCore (GraphicsDevice device)
        {
            if (Mode.HasFlag(RenderMode.Sprite)) {
                if (_spriteBatch == null || _spriteBatch.GraphicsDevice != device)
                    _spriteBatch = new SpriteBatch(device);

                RenderCore(_spriteBatch);
            }
        }

        protected virtual void RenderCore (SpriteBatch spriteBatch)
        {
            Vector2 offset = BeginDraw(spriteBatch);
            RenderContent(spriteBatch);
            EndDraw(spriteBatch, offset);
        }

        protected virtual void RenderCore (DrawBatch drawBatch)
        {

        }

        protected virtual void RenderContent (SpriteBatch spriteBatch)
        {
            if (Model != null)
                RenderCommands(spriteBatch, Model.RenderCommands);
        }

        protected virtual void RenderCommands (SpriteBatch spriteBatch, IEnumerable<DrawCommand> drawList)
        {
            foreach (DrawCommand command in drawList) {
                Texture2D texture;
                if (_xnaTextures.TryGetValue(command.Texture, out texture)) {
                    if (texture == null) {
                        TextureResource texRef = _textures[command.Texture];
                        texture = texRef.CreateTexture(spriteBatch.GraphicsDevice);
                        _xnaTextures[command.Texture] = texture;
                    }
                    spriteBatch.Draw(texture, ToXnaRectangle(command.DestRect), ToXnaRectangle(command.SourceRect), ToXnaColor(command.BlendColor));
                }
            }
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch)
        {
            return BeginDraw(spriteBatch, null);
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch, Effect effect)
        {
            _target = null;
            if (LayerOpacity < 1f) {
                if (_target == null
                    || _target.GraphicsDevice != spriteBatch.GraphicsDevice
                    || _target.Width != spriteBatch.GraphicsDevice.Viewport.Width
                    || _target.Height != spriteBatch.GraphicsDevice.Viewport.Height) {
                    _target = new RenderTarget2D(spriteBatch.GraphicsDevice,
                        spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height,
                        false, SurfaceFormat.Color, DepthFormat.None);
                }

                spriteBatch.GraphicsDevice.SetRenderTarget(_target);
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            }

            return BeginDrawInner(spriteBatch, effect);
        }

        private Vector2 BeginDrawInner (SpriteBatch spriteBatch, Effect effect)
        {
            Vector2 offset = Vector2.Zero;
            RasterizerState rasterState = null;

            if (LevelGeometry != null) {
                offset = LevelGeometry.CanvasBounds.Location.ToXnaVector2();

                spriteBatch.GraphicsDevice.ScissorRectangle = LevelGeometry.ViewportBounds.ToXnaRectangle();
                RasterizerState state = new RasterizerState() {
                    ScissorTestEnable = true,
                };

                if (spriteBatch.GraphicsDevice.ScissorRectangle.IsEmpty)
                    state.ScissorTestEnable = false;
            }

            Matrix transform = Matrix.CreateTranslation(offset.X, offset.Y, 0);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, rasterState, effect, transform);

            return offset;
        }

        protected void EndDraw (SpriteBatch spriteBatch, Vector2 offset)
        {
            EndDrawInner(spriteBatch);

            if (_target != null) {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);

                BeginDrawInner(spriteBatch, null);
                spriteBatch.Draw(_target, new Vector2((float)-offset.X, (float)-offset.Y), new Color(1f, 1f, 1f, LayerOpacity));
                EndDrawInner(spriteBatch);
            }
        }

        private void EndDrawInner (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }
    }
}
