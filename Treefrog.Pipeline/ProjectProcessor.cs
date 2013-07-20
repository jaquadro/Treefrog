using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;

namespace Treefrog.Pipeline
{
    [ContentProcessor(DisplayName = "Treefrog Project Processor")]
    public class ProjectProcessor : ContentProcessor<Project, Project>
    {
        [DisplayName("Build Path")]
        [Description("The location to place intermediate build files")]
        [DefaultValue("obj\\treefrog")]
        public string BuildPath { get; set; }

        public ProjectProcessor ()
        {
            BuildPath = "obj\\treefrog";
        }

        public override Project Process (Project input, ContentProcessorContext context)
        {
            if (!Directory.Exists(BuildPath))
                Directory.CreateDirectory(BuildPath);

            string assetName = context.OutputFilename.Remove(context.OutputFilename.LastIndexOf('.')).Substring(context.OutputDirectory.Length);

            foreach (Level level in input.Levels) {
                string levelAsset = "level_" + level.Uid.ToString();

                OpaqueDataDictionary data = new OpaqueDataDictionary() {
                    { "LevelUid", level.Uid.ToString() },
                };

                context.BuildAsset<Project, Level>(
                    new ExternalReference<Project>(assetName + ".tlpx"),
                    "LevelProcessor",
                    data,
                    "LevelImporter",
                    levelAsset);
            }

            return input;
        }
    }
}
