using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework;
using Treefrog.Pipeline.Content;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Pipeline
{
    [ContentTypeWriter]
    public class LevelWriter : ContentTypeWriter<LevelContent>
    {
        protected override void Write (ContentWriter output, LevelContent value)
        {
            output.Write(value.Level.Name);
            output.Write(value.Level.OriginX);
            output.Write(value.Level.OriginY);
            output.Write(value.Level.Width);
            output.Write(value.Level.Height);

            Common.WritePropertyBlock(output, value.Level.PropertyManager.CustomProperties);

            WriteTileSets(output, value);
            WriteLayers(output, value);
        }

        private void WriteTileSets (ContentWriter output, LevelContent content)
        {
            ITilePoolManager mgr = content.Level.Project.TilePoolManager;

            output.Write(mgr.Pools.Count);
            foreach (TilePool pool in mgr.Pools)
                WriteTileSet(output, content, pool);
        }

        private void WriteTileSet (ContentWriter output, LevelContent content, TilePool pool)
        {
            output.Write(content.Translate(pool.Uid));
            output.Write((short)pool.TileWidth);
            output.Write((short)pool.TileHeight);
            output.Write(content.AssetMap[pool.Uid]);

            Common.WritePropertyBlock(output, pool.PropertyManager.CustomProperties);

            output.Write(pool.Count);
            foreach (Tile tile in pool.Tiles) {
                TileCoord coord = pool.Tiles.GetTileLocation(tile.Uid);

                output.Write(content.Translate(tile.Uid));
                output.Write((short)coord.X);
                output.Write((short)coord.Y);

                Common.WritePropertyBlock(output, tile.PropertyManager.CustomProperties);
            }
        }

        private void WriteLayers (ContentWriter output, LevelContent content)
        {
            output.Write(content.Level.Layers.Count);
            foreach (Layer layer in content.Level.Layers)
                WriteLayer(output, content, layer);
        }

        private void WriteLayer (ContentWriter output, LevelContent content, Layer layer)
        {
            switch (GetLayerType(layer)) {
                case LayerType.Tiles:
                    output.Write("TILE");
                    break;
                case LayerType.Objects:
                    output.Write("OBJE");
                    break;
            }

            output.Write(content.Translate(layer.Uid));
            output.Write(layer.Name);
            output.Write(layer.IsVisible);
            output.Write(layer.Opacity);
            output.Write((short)layer.RasterMode);

            Common.WritePropertyBlock(output, layer.PropertyManager.CustomProperties);

            switch (GetLayerType(layer)) {
                case LayerType.Tiles:
                    WriteTileLayer(output, content, layer as TileGridLayer);
                    break;
                case LayerType.Objects:
                    WriteObjectLayer(output, content, layer as ObjectLayer);
                    break;
            }
        }

        private void WriteTileLayer (ContentWriter output, LevelContent content, TileGridLayer layer)
        {
            output.Write((short)layer.TileWidth);
            output.Write((short)layer.TileHeight);
            output.Write((short)layer.TilesWide);
            output.Write((short)layer.TilesHigh);

            int tileCount = 0;
            foreach (LocatedTile tile in layer.Tiles)
                tileCount++;

            output.Write(tileCount);
            foreach (LocatedTile tile in layer.Tiles) {
                output.Write((short)tile.X);
                output.Write((short)tile.Y);
                output.Write(content.Translate(tile.Tile.Uid));
            }
        }

        private void WriteObjectLayer (ContentWriter output, LevelContent content, ObjectLayer layer)
        {

        }

        public override string GetRuntimeReader (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.LevelReader, Treefrog.Runtime";
        }

        public override string GetRuntimeType (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.Level, Treefrog.Runtime";
        }

        private LayerType GetLayerType (Layer layer)
        {
            if (layer is TileGridLayer)
                return LayerType.Tiles;
            if (layer is ObjectLayer)
                return LayerType.Objects;

            return LayerType.Unknown;
        }

        private enum LayerType
        {
            Unknown = 0,
            Tiles = 1,
            Objects = 2,
        }
    }
}
