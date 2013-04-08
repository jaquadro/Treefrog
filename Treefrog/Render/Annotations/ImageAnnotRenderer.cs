using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Aux;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Render.Annotations
{
    public class ImageAnnotRenderer : AnnotationRenderer
    {
        private ImageAnnot _data;
        private Texture2D _tex;

        public ImageAnnotRenderer (ImageAnnot data)
        {
            _data = data;
        }

        protected override void DisposeManaged ()
        {
            if (_data != null) {
                _data.ImageInvalidated -= HandleImageInvalidated;
                _data = null;
            }

            if (_tex != null) {
                _tex.Dispose();
                _tex = null;
            }

            base.DisposeManaged();
        }

        public override void Render (SpriteBatch spriteBatch, float zoomFactor)
        {
            if (IsDisposed)
                return;

            InitializeResources(spriteBatch.GraphicsDevice);
            if (_tex == null)
                return;

            Rectangle rect = new Rectangle(
                (int)(_data.Position.X * zoomFactor),
                (int)(_data.Position.Y * zoomFactor),
                (int)(_data.Image.Width * zoomFactor),
                (int)(_data.Image.Height * zoomFactor)
                );

            spriteBatch.Draw(_tex, rect, _data.BlendColor.ToXnaColor());
        }

        private void InitializeResources (GraphicsDevice device)
        {
            if (_tex == null && _data.Image != null)
                _tex = _data.Image.CreateTexture(device);
        }

        private void HandleImageInvalidated (object sender, EventArgs e)
        {
            if (_tex != null) {
                _tex.Dispose();
                _tex = null;
            }
        }
    }
}
