using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("Project", Namespace = ProjectX.Namespace)]
    public class ProjectX
    {
        private const string Namespace = "http://jaquadro.com/schemas/treefrog/project";

        public class ItemGroupX
        {
            [XmlElement("Library")]
            public List<LibraryX> Libraries { get; set; }

            [XmlElement("Level")]
            public List<LevelX> Levels { get; set; }
        }

        public class LibraryX
        {
            [XmlAttribute]
            public string Include { get; set; }
        }

        public class LevelX
        {
            [XmlAttribute]
            public string Include { get; set; }
        }

        [XmlElement]
        public CommonX.PropertyGroupX PropertyGroup { get; set; }

        [XmlElement("ItemGroup")]
        public List<ItemGroupX> ItemGroups { get; set; }
    }
}
