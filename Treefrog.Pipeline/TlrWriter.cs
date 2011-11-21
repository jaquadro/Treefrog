using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Treefrog.Pipeline.Content;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework;

namespace Treefrog.Pipeline
{
    [ContentTypeWriter]
    public class TlrWriter : ContentTypeWriter<TileRegistryContent>
    {
        protected override void Write (ContentWriter output, TileRegistryContent value)
        {
            output.Write(value.Version);
            output.Write((short)value.Project.TilePools.Count);

            Dictionary<int, int> tilePoolMap = new Dictionary<int, int>();
            int id = 0;

            foreach (TilePool pool in value.Project.TilePools) {
                output.Write((short)id);
                output.Write((short)pool.TileWidth);
                output.Write((short)pool.TileHeight);
                output.Write((short)pool.Count);

                foreach (Tile tile in pool) {
                    tilePoolMap[tile.Id] = id;
                    TileCoord coord = pool.GetTileLocation(tile.Id);

                    output.Write((short)tile.Id);
                    output.Write((short)coord.X);
                    output.Write((short)coord.Y);

                    WritePropertyBlock(output, tile.Properties);
                }

                id++;
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
            return "Treefrog.Runtime.TilesetReader, Treefrog.Runtime";
        }

        public override string GetRuntimeType (TargetPlatform targetPlatform)
        {
            return "Treefrog.Runtime.Tileset, Treefrog.Runtime";
        }
    }
}
