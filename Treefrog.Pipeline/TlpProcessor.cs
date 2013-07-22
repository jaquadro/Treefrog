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
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;

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

            // Stage and bulid Object Pools

            Dictionary<ObjectPool, string> objectPoolAssetIndex = new Dictionary<ObjectPool, string>();

            int id = 0;
            foreach (ObjectPool pool in input.ObjectPoolManager.Pools) {
                string asset_reg = asset + "_objectpool_" + id;
                string build_reg = "build\\" + asset_reg + ".tlo";

                Project poolContent = new Project();
                poolContent.ObjectPoolManager.Pools.Add(pool);

                using (FileStream fs = File.Create(build_reg)) {
                    XmlWriter writer = XmlTextWriter.Create(fs);
                    //XmlSerializer ser = new XmlSerializer(typeof(ProjectXmlProxy));
                    //ser.Serialize(writer, Project.ToXmlProxy(poolContent));
                    XmlSerializer ser = new XmlSerializer(typeof(LibraryX));
                    ser.Serialize(writer, Library.ToXProxy(poolContent.DefaultLibrary));
                    writer.Close();
                }

                OpaqueDataDictionary data = new OpaqueDataDictionary();
                data.Add("ProjectKey", asset);
                data.Add("ObjectPoolKey", pool.Name);
                data.Add("ObjectPoolId", id);

                context.BuildAsset<ObjectRegistryContent, ObjectRegistryContent>(
                    new ExternalReference<ObjectRegistryContent>(build_reg),
                    "TloProcessor",
                    data,
                    "TloImporter",
                    asset_reg);

                objectPoolAssetIndex[pool] = asset_reg;
                id++;
            }

            // Stage and build Tile Pools

            Dictionary<TilePool, string> tilesetAssetIndex = new Dictionary<TilePool, string>();

            id = 0;
            foreach (TilePool pool in input.TilePoolManager.Pools) {
                string asset_reg = asset + "_tileset_map_" + id;
                string build_reg = "build\\" + asset_reg + ".tlr";

                using (FileStream fs = File.Create(build_reg)) {
                    XmlWriter writer = XmlTextWriter.Create(fs);
                    //input.WriteXmlTilesets(writer);
                    XmlSerializer ser = new XmlSerializer(typeof(LibraryX.TilePoolX));
                    ser.Serialize(writer, TilePool.ToXProxy(pool));
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

                List<ObjectPool> objectPools = ObjectPoolsByLevel(level);
                List<string> objectPoolAssets = new List<string>();
                foreach (ObjectPool pool in objectPools) {
                    objectPoolAssets.Add(objectPoolAssetIndex[pool]);
                }

                using (FileStream fs = File.Create(build_level)) {
                    XmlWriter writer = XmlTextWriter.Create(fs);
                    XmlSerializer ser = new XmlSerializer(typeof(LevelX));
                    ser.Serialize(writer, Level.ToXProxy(level));
                    //input.WriteXml(writer);
                    writer.Close();
                }

                OpaqueDataDictionary data = new OpaqueDataDictionary();
                data.Add("ProjectKey", asset);
                data.Add("LevelKey", level.Name);
                data.Add("TilesetAssets", string.Join(";", poolAssets));
                data.Add("ObjectPoolAssets", string.Join(";", objectPoolAssets));

                context.BuildAsset<LevelContent, LevelContent>(
                    new ExternalReference<LevelContent>(build_level),
                    "TlvProcessor",
                    data,
                    "TlvImporter",
                    asset_level);

                LevelIndexEntry entry = new LevelIndexEntry(id, level.Name);
                entry.Asset = asset_level;

                foreach (Property prop in level.PropertyManager.CustomProperties) {
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

        private List<ObjectPool> ObjectPoolsByLevel (Level level)
        {
            List<ObjectPool> pools = new List<ObjectPool>();

            foreach (Layer layer in level.Layers) {
                ObjectLayer objLayer = layer as ObjectLayer;
                if (objLayer != null) {
                    foreach (ObjectInstance inst in objLayer.Objects) {
                        ObjectPool pool = level.Project.ObjectPoolManager.PoolFromItemKey(inst.ObjectClass.Uid);
                        if (!pools.Contains(pool)) {
                            pools.Add(pool);
                        }
                    }
                }
            }

            return pools;
        }
    }
}
