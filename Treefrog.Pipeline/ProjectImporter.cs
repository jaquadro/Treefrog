using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Treefrog.Framework.Model;
using System.IO;
using Treefrog.Framework;

namespace Treefrog.Pipeline
{
    [ContentImporter(".tlpx", DisplayName = "Treefrog Project Importer", DefaultProcessor = "ProjectProcessor")]
    public class ProjectImporter : ContentImporter<Project>
    {
        public override Project Import (string filename, ContentImporterContext context)
        {
            using (FileStream fs = File.OpenRead(filename)) {
                return new Project(fs, new FileProjectResolver(filename));
            }
        }
    }
}
