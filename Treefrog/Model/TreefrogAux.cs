using System;
using Microsoft.Xna.Framework.Graphics;
using TextureResource = Treefrog.Framework.Imaging.TextureResource;

namespace Treefrog.Aux
{
    public static class TextureResourceXnaExt
    {
        public static TextureResource CreateTextureResource (Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (texture.Format != SurfaceFormat.Color)
                throw new ArgumentException("texture must be in Color format");

            TextureResource tex = new TextureResource(texture.Width, texture.Height);
            texture.GetData(tex.RawData);

            return tex;
        }

        public static Texture2D CreateTexture (this TextureResource self, GraphicsDevice device)
        {
            Texture2D texture = new Texture2D(device, self.Width, self.Height, false, SurfaceFormat.Color);
            texture.SetData(self.RawData);

            return texture;
        }
    }
}
