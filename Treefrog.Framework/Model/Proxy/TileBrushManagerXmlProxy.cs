using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("TileBrushes")]
    public class TileBrushManagerXmlProxy
    {
        [XmlAttribute]
        public int LastKey { get; set; }

        [XmlElement]
        public TileBrushCollectionXmlProxy<StaticTileBrushXmlProxy> StaticBrushes { get; set; }

        [XmlElement]
        public TileBrushCollectionXmlProxy<DynamicTileBrushXmlProxy> DynamicBrushes { get; set; }
    }
}
