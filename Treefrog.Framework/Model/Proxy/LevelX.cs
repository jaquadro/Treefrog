using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model.Proxy
{
    [XmlRoot("Level", Namespace = LevelX.Namespace)]
    public class LevelX
    {
        private const string Namespace = "http://jaquadro.com/schemas/treefrog/level";

        public class ItemGroupX
        {
            [XmlElement("Library")]
            public LibraryX[] Libraries { get; set; }
        }

        public abstract class LayerX
        {
            protected LayerX ()
            {
                Opacity = 1.0f;
                Visible = true;
                RasterMode = RasterMode.Point;
            }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public float Opacity { get; set; }

            [XmlAttribute]
            public bool Visible { get; set; }

            [XmlAttribute]
            public RasterMode RasterMode { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public List<CommonX.PropertyX> Properties { get; set; }
        }

        [XmlRoot("LayerData", Namespace = "http://jaquadro.com/schemas/treefrog/level")]
        public class MultiTileGridLayerX : LayerX
        {
            [XmlAttribute]
            public int TileWidth { get; set; }

            [XmlAttribute]
            public int TileHeight { get; set; }

            [XmlArray]
            [XmlArrayItem("Tile")]
            public List<CommonX.TileStackX> Tiles { get; set; }
        }

        [XmlRoot("LayerData", Namespace = "http://jaquadro.com/schemas/treefrog/level")]
        public class ObjectLayerX : LayerX
        {
            [XmlArray]
            [XmlArrayItem("Object")]
            public List<ObjectInstanceX> Objects { get; set; }
        }

        public class ObjectInstanceX
        {
            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public Guid Class { get; set; }

            [XmlAttribute]
            public string At { get; set; }

            [XmlAttribute]
            public float Rotation { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public List<CommonX.PropertyX> Properties { get; set; }
        }

        public class TileIndexEntryX
        {
            [XmlAttribute]
            public int Id { get; set; }

            [XmlAttribute]
            public Guid Uid { get; set; }
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int OriginX { get; set; }

        [XmlAttribute]
        public int OriginY { get; set; }

        [XmlAttribute]
        public int Width { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlElement]
        public CommonX.PropertyGroupX PropertyGroup { get; set; }

        [XmlElement("ItemGroup")]
        public ItemGroupX[] ItemGroups { get; set; }

        [XmlArray]
        [XmlArrayItem("Entry")]
        public List<TileIndexEntryX> TileIndex { get; set; }

        [XmlArray]
        [XmlArrayItem("Layer", Type = typeof(AbstractXmlSerializer<LayerX>))]
        public List<AbstractXmlSerializer<LayerX>> Layers { get; set; }

        [XmlArray]
        [XmlArrayItem("Property")]
        public List<CommonX.PropertyX> Properties { get; set; }
    }
}
