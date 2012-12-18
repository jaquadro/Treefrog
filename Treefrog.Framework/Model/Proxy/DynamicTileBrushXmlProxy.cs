using System.Collections.Generic;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("DynamicBrush")]
    public class DynamicTileBrushXmlProxy : TileBrushXmlProxy
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlArray]
        [XmlArrayItem("Entry")]
        public List<TileBrushEntryXmlProxy> BrushEntries { get; set; }
    }
}
