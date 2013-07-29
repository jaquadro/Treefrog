using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using Treefrog.Pipeline.Content;
using System.Diagnostics;

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
            string projectPath = Path.Combine(BuildPath, "Levels");
            string levelPath = Path.Combine(projectPath, input.Name);

            try {
                if (Directory.Exists(projectPath))
                    Directory.Delete(projectPath, true);

                Directory.CreateDirectory(projectPath);
                Directory.CreateDirectory(levelPath);
            }
            catch { }

            string assetName = context.OutputFilename.Remove(context.OutputFilename.LastIndexOf('.')).Substring(context.OutputDirectory.Length);
            string assetPath = Path.GetDirectoryName(context.OutputFilename).Substring(context.OutputDirectory.Length);

            foreach (Level level in input.Levels) {
                string levelAsset = Path.Combine(assetPath, input.Name, level.Name);

                OpaqueDataDictionary data = new OpaqueDataDictionary() {
                    { "LevelUid", level.Uid.ToString() },
                };

                context.BuildAsset<Project, LevelContent>(
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
