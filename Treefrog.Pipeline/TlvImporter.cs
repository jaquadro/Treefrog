using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Pipeline.Content;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework.Model;
using System.IO;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlv", DisplayName = "Treefrog TLV Importer", DefaultProcessor = "TlvProcessor")]
    class TlvImporter : ContentImporter<LevelContent>
    {
        public override LevelContent Import (string filename, ContentImporterContext context)
        {
            Form form = new Form();

            PresentationParameters presentation = new PresentationParameters();
            presentation.DeviceWindowHandle = form.Handle;

            GraphicsAdapter.UseReferenceDevice = true;
            GraphicsAdapter.UseNullDevice = true;

            GraphicsDevice device = new GraphicsDevice(
                GraphicsAdapter.DefaultAdapter,
                GraphicsProfile.Reach,
                presentation
                );

            Project project;
            using (FileStream fs = File.OpenRead(filename)) {
                project = Project.Open(fs, device);
            }

            LevelContent content = new LevelContent(project);
            content.Filename = filename;
            content.Directory = filename.Remove(filename.LastIndexOf('\\'));

            return content;
        }
    }
}
