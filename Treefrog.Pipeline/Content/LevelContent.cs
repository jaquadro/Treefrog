using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Treefrog.Pipeline.Content
{
    class LevelContent
    {
        public LevelContent (Project project)
        {
            Project = project;
        }

        public short Version
        {
            get { return 0; }
        }

        public Project Project { get; private set; }
        public Level Level { get; set; }

        public List<string> TilesetAssets { get; set; }

        public string Filename { get; set; }
        public string Directory { get; set; }
    }
}
