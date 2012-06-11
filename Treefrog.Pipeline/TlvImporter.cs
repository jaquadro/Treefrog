using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework.Model;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlv", DisplayName = "Treefrog TLV Importer", DefaultProcessor = "TlvProcessor")]
    class TlvImporter : ContentImporter<LevelContent>
    {
        public override LevelContent Import (string filename, ContentImporterContext context)
        {
            Project project;
            using (FileStream fs = File.OpenRead(filename)) {
                project = Project.Open(fs);
            }

            LevelContent content = new LevelContent(project);
            content.Filename = filename;
            content.Directory = filename.Remove(filename.LastIndexOf('\\'));

            return content;
        }
    }
}
