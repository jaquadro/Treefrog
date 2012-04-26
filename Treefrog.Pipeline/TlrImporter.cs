using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Treefrog.Pipeline.Content;
using System.Xml;
using System.Diagnostics;
using Treefrog.Framework;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlr", DisplayName = "Treefrog TLR Importer", DefaultProcessor = "TlrProcessor")]
    class TlrImporter : ContentImporter<TileRegistryContent>
    {
        public override TileRegistryContent Import (string filename, ContentImporterContext context)
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

            Project project = new Project();
   //         project.Initialize(device);

            using (FileStream fs = File.OpenRead(filename)) {
                XmlReader reader = XmlTextReader.Create(fs);

                XmlHelper.SwitchAll(reader, (xmlr, s) =>
                {
                    switch (s) {
                        case "tilesets":
                            project.ReadXmlTilesets(reader);
                            break;
                    }
                });
            }

            TileRegistryContent content = new TileRegistryContent(project);

            content.Filename = filename;
            content.Directory = filename.Remove(filename.LastIndexOf('\\'));

            return content;
        }
    }
}
