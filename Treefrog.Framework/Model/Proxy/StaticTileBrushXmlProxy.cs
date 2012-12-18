using System.Xml.Serialization;
using System.Collections.Generic;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("StaticBrush")]
    public class StaticTileBrushXmlProxy : TileBrushXmlProxy
    {
        [XmlArray]
        [XmlArrayItem("Tile")]
        public List<TileStackXmlProxy> Tiles { get; set; }
    }
}
