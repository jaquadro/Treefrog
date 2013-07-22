using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Pipeline.Content;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlr", DisplayName = "Treefrog TLR Importer", DefaultProcessor = "TlrProcessor")]
    class TlrImporter : ContentImporter<TileRegistryContent>
    {
        public override TileRegistryContent Import (string filename, ContentImporterContext context)
        {
            //Form form = new Form();

            //PresentationParameters presentation = new PresentationParameters();
            //presentation.DeviceWindowHandle = form.Handle;

            //GraphicsAdapter.UseReferenceDevice = true;
            //GraphicsAdapter.UseNullDevice = true;

            //GraphicsDevice device = new GraphicsDevice(
            //    GraphicsAdapter.DefaultAdapter,
            //    GraphicsProfile.Reach,
            //    presentation
            //    );

            Project project = new Project();
   //         project.Initialize(device);

            using (FileStream fs = File.OpenRead(filename)) {
                XmlReaderSettings settings = new XmlReaderSettings() {
                    CloseInput = true,
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                };

                using (XmlReader reader = XmlTextReader.Create(fs, settings)) {
                    XmlSerializer serializer = new XmlSerializer(typeof(LibraryX.TilePoolX));
                    LibraryX.TilePoolX proxy = serializer.Deserialize(reader) as LibraryX.TilePoolX;

                    TilePool.FromXProxy(proxy, (TilePoolManager)project.TilePoolManager);
                }

                /*XmlReader reader = XmlTextReader.Create(fs);

                XmlHelper.SwitchAll(reader, (xmlr, s) =>
                {
                    switch (s) {
                        case "tilesets":
                            
                            project.ReadXmlTilesets(reader);
                            break;
                    }
                });*/
            }

            TileRegistryContent content = new TileRegistryContent(project);

            content.Filename = filename;
            content.Directory = filename.Remove(filename.LastIndexOf('\\'));

            return content;
        }
    }
}
