using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using System.IO;
using System.Xml;
using Treefrog.Pipeline.Content;
using Treefrog.Framework.Model.Support;

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog Project Processor")]
    class TlpProcessor : ContentProcessor<Project, LevelIndexContent>
    {
        public override LevelIndexContent Process (Project input, ContentProcessorContext context)
        {
            if (!Directory.Exists("build")) {
                Directory.CreateDirectory("build");
            }

            string asset = context.OutputFilename.Remove(context.OutputFilename.LastIndexOf('.')).Substring(context.OutputDirectory.Length);

            // Stage and build Tile Pools

            Dictionary<TilePool, string> tilesetAssetIndex = new Dictionary<TilePool, string>();

            int id = 0;
            foreach (TilePool pool in input.TilePools) {
                string asset_reg = asset + "_tileset_map_" + id;
                string build_reg = "build\\" + asset_reg + ".tlr";

                using (FileStream fs = new FileStream(build_reg, FileMode.OpenOrCreate, FileAccess.Write)) {
                    XmlWriter writer = XmlTextWriter.Create(fs);
                    input.WriteXmlTilesets(writer);
                    writer.Close();
                }

                OpaqueDataDictionary data = new OpaqueDataDictionary();
                data.Add("ProjectKey", asset);
                data.Add("TilesetKey", pool.Name);
                data.Add("TilesetId", id);

                context.BuildAsset<TileRegistryContent, TileRegistryContent>(
                    new ExternalReference<TileRegistryContent>(build_reg),
                    "TlrProcessor",
                    data,
                    "TlrImporter",
                    asset_reg);

                tilesetAssetIndex[pool] = asset_reg;
                id++;
            }

            // Stage and build levels

            LevelIndexContent content = new LevelIndexContent();

            id = 0;
            foreach (Level level in input.Levels) {
                string asset_level = asset + "_level_" + id;
                string build_level = "build\\" + asset_level + ".tlv";

                List<TilePool> pools = TilePoolsByLevel(level);
                List<string> poolAssets = new List<string>();
                foreach (TilePool pool in pools) {
                    poolAssets.Add(tilesetAssetIndex[pool]);
                }

                using (FileStream fs = new FileStream(build_level, FileMode.OpenOrCreate, FileAccess.Write)) {
                    XmlWriter writer = XmlTextWriter.Create(fs);
                    input.WriteXml(writer);
                    writer.Close();
                }

                OpaqueDataDictionary data = new OpaqueDataDictionary();
                data.Add("ProjectKey", asset);
                data.Add("LevelKey", level.Name);
                data.Add("TilesetAssets", string.Join(";", poolAssets));

                context.BuildAsset<LevelContent, LevelContent>(
                    new ExternalReference<LevelContent>(build_level),
                    "TlvProcessor",
                    data,
                    "TlvImporter",
                    asset_level);

                LevelIndexEntry entry = new LevelIndexEntry(id, level.Name);
                entry.Asset = asset_level;

                foreach (Property prop in level.CustomProperties) {
                    entry.Properties.Add(prop);
                }

                content.Levels.Add(entry);
                id++;
            }

            return content;
        }

        private List<TilePool> TilePoolsByLevel (Level level)
        {
            List<TilePool> pools = new List<TilePool>();

            foreach (Layer layer in level.Layers) {
                MultiTileGridLayer tileLayer = layer as MultiTileGridLayer;
                if (tileLayer != null) {
                    foreach (LocatedTile t in tileLayer.Tiles) {
                        if (!pools.Contains(t.Tile.Pool)) {
                            pools.Add(t.Tile.Pool);
                        }
                    }
                }
            }

            return pools;
        }
    }
}
