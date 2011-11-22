using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Treefrog.Pipeline.Content
{
    public class TileRegistryContent
    {
        public TileRegistryContent (Project project)
        {
            Project = project;
        }

        public short Version
        {
            get { return 0; }
        }

        public Project Project { get; private set; }
        public TilePool TilePool { get; set; }
        public int Id { get; set; }
        public string TextureAsset { get; set; }

        public string Filename { get; set; }
        public string Directory { get; set; }
    }
}
