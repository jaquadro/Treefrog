using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("TileBrush")]
    public class TileBrushXmlProxy
    {
        [XmlAttribute]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int TileWidth { get; set; }

        [XmlAttribute]
        public int TileHeight { get; set; }
    }
}
