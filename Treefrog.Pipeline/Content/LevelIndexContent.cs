using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Treefrog.Pipeline.Content
{
    public class LevelIndexEntry 
    {
        public LevelIndexEntry (int id, string name) {
            Id = id;
            Name = name;
            Properties = new List<Property>();
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Asset { get; set; }
        public List<Property> Properties { get; private set; }
    }

    public class LevelIndexContent
    {
        public LevelIndexContent ()
        {
            Levels = new List<LevelIndexEntry>();
        }

        public List<LevelIndexEntry> Levels { get; private set; }
    }
}
