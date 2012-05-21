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
            Project project;
            using (FileStream fs = File.OpenRead(filename)) {
                project = Project.Open(fs);
            }

            //project.Filename = filename;
            //project.Direcotry = filename.Remove(filename.LastIndexOf('\\'));

            return project;
        }
    }
}
