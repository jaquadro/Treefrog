using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    internal enum LayerType
    {
        Unknown = 0,
        Tiles = 1,
        Objects = 2,
    }

    [ContentTypeWriter]
    class TlvWriter : ContentTypeWriter<LevelContent>
    {
        protected override void Write (ContentWriter output, LevelContent value)
        {
            output.Write(value.Version);
            output.Write((short)value.Level.TileWidth);
            output.Write((short)value.Level.TileHeight);
            output.Write((short)value.Level.TilesWide);
            output.Write((short)value.Level.TilesHigh);

            WritePropertyBlock(output, value.Level.CustomProperties);

            output.Write(value.TilesetAssets.Count);
            foreach (string asset in value.TilesetAssets) {
                output.Write(asset);
            }

            output.Write(value.ObjectPoolAssets.Count);
            foreach (string asset in value.ObjectPoolAssets) {
                output.Write(asset);
            }

            output.Write((short)value.Level.Layers.Count);

            int id = 0;
            foreach (Layer layer in value.Level.Layers) {
                switch (LayerType(layer)) {
                    case Pipeline.LayerType.Tiles:
                        output.Write("TILE");
                        break;
                    case Pipeline.LayerType.Objects:
                        output.Write("OBJE");
                        break;
                }

                output.Write((short)id);
                output.Write(layer.Name);
                output.Write(layer.IsVisible);
                output.Write(layer.Opacity);

                WritePropertyBlock(output, layer.CustomProperties);

                switch (LayerType(layer)) {
                    case Pipeline.LayerType.Tiles:
                        WriteTileLayerBlock(output, layer as MultiTileGridLayer);
                        break;
                    case Pipeline.LayerType.Objects:
                        WriteObjectLayerBlock(output, layer as ObjectLayer);
                        break;
                }

                id++;
            }
        }

        private LayerType LayerType (Layer layer)
        {
            if (layer is MultiTileGridLayer)
                return Pipeline.LayerType.Tiles;
            if (layer is ObjectLayer)
                return Pipeline.LayerType.Objects;

            return Pipeline.LayerType.Unknown;
        }

        private void WriteTileLayerBlock (ContentWriter output, MultiTileGridLayer layer)
        {
            output.Write((short)layer.TileWidth);
            output.Write((short)layer.TileHeight);
            output.Write((short)layer.TilesWide);
            output.Write((short)layer.TilesHigh);

            int tscount = 0;
            foreach (LocatedTileStack stack in layer.TileStacks) {
                tscount++;
            }

            output.Write(tscount);
            foreach (LocatedTileStack stack in layer.TileStacks) {
                output.Write((short)stack.X);
                output.Write((short)stack.Y);
                output.Write((short)stack.Stack.Count);

                foreach (Tile tile in stack.Stack) {
                    output.Write((short)tile.Id);
                }
            }
        }

        private void WriteObjectLayerBlock (ContentWriter output, ObjectLayer layer)
        {
            int objcount = 0;
            foreach (ObjectInstance inst in layer.Objects)
                objcount++;

            output.Write(objcount);
            foreach (ObjectInstance inst in layer.Objects) {
                output.Write(inst.X);
                output.Write(inst.Y);
                output.Write((short)inst.ObjectClass.Id);

                //WritePropertyBlock(output, inst.CustomProperties);
            }
        }

        private void WritePropertyBlock (ContentWriter output, IEnumerable<Property> properties)
        {
            short pcount = 0;
            foreach (Property p in properties) {
                pcount++;
            }

            output.Write((short)pcount);
            foreach (Property p in properties) {
                output.Write(p.Name);
                output.Write(p.ToString());
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
    }
}
