using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Treefrog.Windows.Controls;

using TFImaging = Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Layers
{
    public class ObjectControlLayer : BaseControlLayer
    {
        private ObjectLayer _layer;

        public override int VirtualHeight
        {
            get 
            { 
                if (_layer == null)
                    return 0;
                return _layer.LayerHeight;
            }
        }

        public override int VirtualWidth
        {
            get 
            {
                if (_layer == null)
                    return 0; 
                return _layer.LayerWidth;
            }
        }

        public ObjectControlLayer (LayerControl control)
            : base(control)
        {
        }

        public ObjectControlLayer (LayerControl control, ObjectLayer layer)
            : this(control)
        {
            Layer = layer;
        }

        public new ObjectLayer Layer
        {
            get { return _layer; }
            protected set
            {
                _layer = value;
                base.Layer = value;
            }
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch)
        {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            return offset;
        }

        protected void EndDraw (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        protected override void DrawContentImpl (SpriteBatch spriteBatch)
        {
            if (_layer == null || Visible == false)
                return;

            Rectangle region = Control.VisibleRegion;

            TFImaging.Rectangle castRegion = new TFImaging.Rectangle(
                    region.X, region.Y, region.Width, region.Height
                    );

            DrawObjects(spriteBatch, castRegion);
        }

        protected virtual void DrawObjects (SpriteBatch spriteBatch, TFImaging.Rectangle region)
        {
            ObjectTextureService textureService = Control.Services.GetService<ObjectTextureService>();
            if (textureService == null)
                return;

            Vector2 offset = BeginDraw(spriteBatch);

            foreach (ObjectInstance inst in Layer.ObjectsInRegion(region, ObjectRegionTest.PartialImage)) {
                TFImaging.Rectangle srcRect = inst.ObjectClass.ImageBounds;
                TFImaging.Rectangle dstRect = inst.ImageBounds;

                Texture2D texture = textureService.GetTexture(inst.ObjectClass.Id);
                if (texture == null)
                    continue;
                
                Rectangle sourceRect = new Rectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height);
                Rectangle destRect = new Rectangle(
                        (int)(dstRect.X * Control.Zoom),
                        (int)(dstRect.Y * Control.Zoom),
                        (int)(dstRect.Width * Control.Zoom),
                        (int)(dstRect.Height * Control.Zoom)
                        );

                spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
            }

            EndDraw(spriteBatch);
        }
    }
}
