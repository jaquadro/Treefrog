using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Treefrog.Pipeline.Content;
using System.IO;

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog TLV Processor")]
    internal class TlvProcessor : ContentProcessor<LevelContent, LevelContent>
    {
        [DisplayName("Project Key")]
        [Description("The name of a Treefrog project namespace to associate this resource with.")]
        public string ProjectKey { get; set; }

        [DisplayName("Level Key")]
        [Description("The name of a Treefrog project level.")]
        public string LevelKey { get; set; }

        [DisplayName("Tileset Assets")]
        [Description("A semicolon-delimited list of Tileset assets used by this level.")]
        public string TilesetAssets { get; set; }

        public override LevelContent Process (LevelContent input, ContentProcessorContext context)
        {
            if (!Directory.Exists("build")) {
                Directory.CreateDirectory("build");
            }

            string assets = TilesetAssets ?? string.Empty;

            input.Level = input.Project.Levels[LevelKey];
            input.TilesetAssets = new List<string>(assets.Split(new char[] { ';' }));

            return input;
        }
    }
}
