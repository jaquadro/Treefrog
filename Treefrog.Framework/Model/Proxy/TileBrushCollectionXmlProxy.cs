using System.Collections.Generic;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("TileBrushCollection")]
    public class TileBrushCollectionXmlProxy<TProxy>
        where TProxy : TileBrushXmlProxy
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlArray]
        [XmlArrayItem("Brush")]
        public List<TProxy> Brushes { get; set; }
    }
}
