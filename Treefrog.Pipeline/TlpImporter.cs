using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlp", DisplayName = "Treefrog Project Importer", DefaultProcessor = "TlpProcessor")]
    class TlpImporter : ContentImporter<Project>
    {
        public override Project Import (string filename, ContentImporterContext context)
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

            //project.Filename = filename;
            //project.Direcotry = filename.Remove(filename.LastIndexOf('\\'));

            return project;
        }
    }
}
