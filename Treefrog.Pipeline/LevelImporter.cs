using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlpx", DisplayName = "Treefrog Level Importer", DefaultProcessor = "LevelProcessor")]
    public class LevelImporter : ContentImporter<Project>
    {
        public override Project Import (string filename, ContentImporterContext context)
        {
            using (FileStream fs = File.OpenRead(filename)) {
                return new Project(fs, new FileProjectResolver(filename));
            }
        }
    }
}
