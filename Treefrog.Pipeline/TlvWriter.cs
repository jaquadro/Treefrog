using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Pipeline.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Pipeline
{
    internal enum LayerType
    {
        Unknown = 0,
        Tiles = 1,
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

            WritePropertyBlock(output, value.Level.Properties);

            output.Write((short)value.Level.Layers.Count);

            int id = 0;
            foreach (Layer layer in value.Level.Layers) {
                output.Write((short)id);
                output.Write(layer.Name);
                output.Write(layer.IsVisible);
                output.Write(layer.Opacity);

                WritePropertyBlock(output, layer.Properties);

                switch (LayerType(layer)) {
                    case Pipeline.LayerType.Tiles:
                        WriteTileLayerBlock(output, layer as MultiTileGridLayer);
                        break;
                }

                id++;
            }
        }

        private LayerType LayerType (Layer layer)
        {
            if (layer is MultiTileGridLayer)
                return Pipeline.LayerType.Tiles;

            return Pipeline.LayerType.Unknown;
        }

        private void WriteTileLayerBlock (ContentWriter output, MultiTileGridLayer layer)
        {
            output.Write((short)layer.TileWidth);
            output.Write((short)layer.TileHeight);
            output.Write((short)layer.LayerWidth);
            output.Write((short)layer.LayerHeight);

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
