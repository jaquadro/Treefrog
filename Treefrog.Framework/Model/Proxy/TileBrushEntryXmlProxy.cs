using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("BrushEntry")]
    public class TileBrushEntryXmlProxy
    {
        [XmlAttribute]
        public int Slot { get; set; }

        [XmlAttribute]
        public int TileId { get; set; }
    }
}
