using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using Treefrog.Pipeline.Content;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlo", DisplayName = "Treefrog TLO Importer", DefaultProcessor = "TloProcessor")]
    class TloImporter : ContentImporter<ObjectRegistryContent>
    {
        public override ObjectRegistryContent Import (string filename, ContentImporterContext context)
        {
            using (FileStream fs = File.OpenRead(filename)) {
                XmlReader reader = XmlTextReader.Create(fs);
                XmlSerializer serializer = new XmlSerializer(typeof(ProjectXmlProxy));

                ProjectXmlProxy proxy = serializer.Deserialize(reader) as ProjectXmlProxy;
                Project project = Project.FromXmlProxy(proxy);

                ObjectRegistryContent content = new ObjectRegistryContent(project);

                content.Filename = filename;
                content.Directory = filename.Remove(filename.LastIndexOf('\\'));

                return content;
            }
        }
    }
}
