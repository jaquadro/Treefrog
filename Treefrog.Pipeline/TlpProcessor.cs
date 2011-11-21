using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using System.IO;
using System.Xml;
using Treefrog.Pipeline.Content;

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

            // Stage and build Tile Registry

            using (FileStream fs = new FileStream("build\\registry.tlr", FileMode.OpenOrCreate, FileAccess.Write)) {
                XmlWriter writer = XmlTextWriter.Create(fs);
                input.WriteXmlTilesets(writer);
                writer.Close();
            }

            string asset = context.OutputFilename.Remove(context.OutputFilename.LastIndexOf('.')).Substring(context.OutputDirectory.Length);
            string asset_reg = asset + "_registry";

            OpaqueDataDictionary data = new OpaqueDataDictionary();
            data.Add("ProjectKey", asset);

            context.BuildAsset<TileRegistryContent, TileRegistryContent>(
                new ExternalReference<TileRegistryContent>("build\\registry.tlr"),
                "TlrProcessor",
                data,
                "TlrImporter",
                asset_reg);

            // Stage and build levels

            LevelIndexContent content = new LevelIndexContent();

            int id = 0;
            foreach (Level level in input.Levels) {
                string build_level = "build\\level_" + id + ".tlv";
                string asset_level = asset + "_level_" + id;

                using (FileStream fs = new FileStream(build_level, FileMode.OpenOrCreate, FileAccess.Write)) {
                    XmlWriter writer = XmlTextWriter.Create(fs);
                    input.WriteXml(writer);
                    writer.Close();
                }

                data = new OpaqueDataDictionary();
                data.Add("ProjectKey", asset);
                data.Add("LevelKey", level.Name);

                context.BuildAsset<LevelContent, LevelContent>(
                    new ExternalReference<LevelContent>(build_level),
                    "TlvProcessor",
                    data,
                    "TlvImporter",
                    asset_level);

                LevelIndexEntry entry = new LevelIndexEntry(id, level.Name);
                entry.Asset = asset_level;

                foreach (Property prop in level.Properties) {
                    entry.Properties.Add(prop);
                }

                content.Levels.Add(entry);
                id++;
            }

            return content;
        }
    }
}
