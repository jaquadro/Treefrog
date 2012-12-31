using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Presentation.Layers
{
    public class TileGridLayerPresenter : TileLayerPresenter
    {
        private TileGridLayer _layer;

        public TileGridLayerPresenter (LevelPresenter2 levelPresenter, TileGridLayer layer)
            : base(levelPresenter, layer)
        {
            _layer = layer;
        }

        protected new TileGridLayer Layer
        {
            get { return _layer; }
        }

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get
            {
                if (Layer == null || LevelPresenter.LevelGeometry == null)
                    yield break;

                ILevelGeometry geometry = LevelPresenter.LevelGeometry;

                Rectangle tileRegion = ComputeTileRegion();
                foreach (LocatedTile tile in Layer.TilesAt(tileRegion)) {
                    TileCoord scoord = tile.Tile.Pool.GetTileLocation(tile.Tile.Id);

                    yield return new DrawCommand() {
                        Texture = tile.Tile.Pool.TextureId,
                        SourceRect = new Rectangle(scoord.X * tile.Tile.Width, scoord.Y * tile.Tile.Height, tile.Tile.Width, tile.Tile.Height),
                        DestRect = new Rectangle(
                            (int)(tile.X * tile.Tile.Width * geometry.ZoomFactor),
                            (int)(tile.Y * tile.Tile.Height * geometry.ZoomFactor),
                            (int)(tile.Tile.Width * geometry.ZoomFactor),
                            (int)(tile.Tile.Height * geometry.ZoomFactor)),
                        BlendColor = Colors.White,
                    };
                }

                if (TileSelection != null && TileSelection.Floating) {
                    foreach (KeyValuePair<TileCoord, TileStack> item in TileSelection.Tiles) {
                        foreach (Tile tile in item.Value) {
                            TileCoord scoord = tile.Pool.GetTileLocation(tile.Id);
                            TileCoord dcoord = item.Key;

                            yield return new DrawCommand() {
                                Texture = tile.Pool.TextureId,
                                SourceRect = new Rectangle(scoord.X * tile.Width, scoord.Y * tile.Height, tile.Width, tile.Height),
                                DestRect = new Rectangle(
                                    (int)((dcoord.X + TileSelection.Offset.X) * tile.Width * geometry.ZoomFactor),
                                    (int)((dcoord.Y + TileSelection.Offset.Y) * tile.Height * geometry.ZoomFactor),
                                    (int)(tile.Width * geometry.ZoomFactor),
                                    (int)(tile.Height * geometry.ZoomFactor)),
                                BlendColor = Colors.White,
                            };
                        }
                    }
                }
            }
        }
    }
}
