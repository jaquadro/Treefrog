using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("Project", Namespace = ProjectX.Namespace)]
    public class ProjectX
    {
        private const string Namespace = "http://jaquadro.com/schemas/treefrog/project";

        public class PropertyGroupX
        {
            [XmlElement]
            public Guid ProjectGuid { get; set; }

            [XmlElement]
            public string ProjectName { get; set; }

            [XmlElement]
            public Guid DefaultLibrary { get; set; }

            [XmlAnyElement]
            public List<XmlElement> Extra { get; set; }
        }

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
        public PropertyGroupX PropertyGroup { get; set; }

        [XmlElement("ItemGroup")]
        public List<ItemGroupX> ItemGroups { get; set; }
    }
}
