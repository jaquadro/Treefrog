using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Pipeline.Content;

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

        [DisplayName("Object Pool Assets")]
        [Description("A semicolon-delimited list of Object Pool assets used by this level.")]
        public string ObjectPoolAssets { get; set; }

        public override LevelContent Process (LevelContent input, ContentProcessorContext context)
        {
            if (!Directory.Exists("build")) {
                Directory.CreateDirectory("build");
            }

            string assets = TilesetAssets ?? string.Empty;
            string objectPoolAssets = ObjectPoolAssets ?? string.Empty;

            input.Level = input.Project.Levels[LevelKey];
            input.TilesetAssets = new List<string>(assets.Split(new char[] { ';' }));
            input.ObjectPoolAssets = new List<string>(assets.Split(new char[] { ';' }));

            return input;
        }
    }
}
