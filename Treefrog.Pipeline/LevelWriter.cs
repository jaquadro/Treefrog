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
using Treefrog.Pipeline.ImagePacker;

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
            WriteObjectSets(output, value);
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

        private void WriteObjectSets (ContentWriter output, LevelContent content)
        {
            IObjectPoolManager mgr = content.Level.Project.ObjectPoolManager;

            output.Write(mgr.Pools.Count);
            foreach (ObjectPool pool in mgr.Pools)
                WriteObjectSet(output, content, pool);
        }

        private void WriteObjectSet (ContentWriter output, LevelContent content, ObjectPool pool)
        {
            output.Write(content.Translate(pool.Uid));
            output.Write(content.AssetMap[pool.Uid]);

            Common.WritePropertyBlock(output, pool.PropertyManager.CustomProperties);

            output.Write(pool.Count);
            foreach (ObjectClass obj in pool.Objects) {
                Rect objRect = FindRectByName(content.AtlasPages, obj.Uid.ToString());
                if (objRect == null)
                    continue;

                output.Write(content.Translate(obj.Uid));
                output.Write(obj.Name);
                output.Write(obj.Origin.X);
                output.Write(obj.Origin.Y);
                output.Write(obj.MaskBounds.Left);
                output.Write(obj.MaskBounds.Top);
                output.Write(obj.MaskBounds.Width);
                output.Write(obj.MaskBounds.Height);

                output.Write(objRect.Rotated);
                output.Write(objRect.X);
                output.Write(objRect.Y);
                output.Write(objRect.Width);
                output.Write(objRect.Height);
                output.Write(objRect.OriginalWidth);
                output.Write(objRect.OriginalHeight);
                output.Write(objRect.OffsetX);
                output.Write(objRect.OffsetY);
                //output.Write(objRect.Index);

                Common.WritePropertyBlock(output, obj.PropertyManager.CustomProperties);
            }
        }

        private Rect FindRectByName (List<Page> pages, string name)
        {
            foreach (Page page in pages) {
                foreach (Rect rect in page.OutputRects) {
                    if (rect.Name == name)
                        return rect;
                }
            }

            return null;
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
            int objCount = 0;
            foreach (ObjectInstance obj in layer.Objects)
                objCount++;

            output.Write(objCount);
            foreach (ObjectInstance obj in layer.Objects) {
                output.Write(content.Translate(obj.ObjectClass.Uid));
                output.Write(obj.Position.X);
                output.Write(obj.Position.Y);
                output.Write(obj.Rotation);
                output.Write(obj.ScaleX);
                output.Write(obj.ScaleY);

                Common.WritePropertyBlock(output, obj.PropertyManager.CustomProperties);
            }
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
