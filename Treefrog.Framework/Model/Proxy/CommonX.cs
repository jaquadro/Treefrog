using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("Common", Namespace = CommonX.Namespace)]
    public static class CommonX
    {
        private const string Namespace = "http://jaquadro.com/schemas/treefrog/common";

        public class PropertyX
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlText]
            public string Value { get; set; }
        }

        public class TileStackX
        {
            [XmlAttribute]
            public string At { get; set; }

            [XmlText]
            public string Items { get; set; }
        }

        public class TileBlockX
        {
            [XmlAttribute]
            public int X { get; set; }

            [XmlAttribute]
            public int Y { get; set; }

            [XmlText]
            public string Data { get; set; }
        }
    }
}
