using System;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;

namespace Treefrog.Render.Layers
{
    public class TileSetRenderLayer : RenderLayer
    {
        public TileSetRenderLayer (TileSetLayerPresenter model)
            : base(model)
        { }

        protected new TileSetLayerPresenter Model
        {
            get { return ModelCore as TileSetLayerPresenter; }
        }

        protected override void RenderContent (SpriteBatch spriteBatch)
        {
            if (Model != null && TextureCache != null)
                RenderCommands(spriteBatch, TextureCache, Model.RenderCommands);
        }

        public override int DependentHeight
        {
            get
            {
                int width = LevelGeometry.ViewportBounds.Width;
                int tilesWide = Math.Max(1, width / Model.TileWidth);
                return ((Model.Layer.Count + tilesWide - 1) / tilesWide) * Model.TileHeight;
            }
        }

        public override int DependentWidth
        {
            get
            {
                int height = LevelGeometry.ViewportBounds.Height;
                int tilesHigh = Math.Max(1, height / Model.TileHeight);
                return ((Model.Layer.Count + tilesHigh - 1) / tilesHigh) * Model.TileWidth;
            }
        }
    }
}
