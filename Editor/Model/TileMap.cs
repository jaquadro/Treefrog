using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Editor.Model
{
    public class Level : INamedResource
    {
        private static int _lastId = 0;

        #region Fields

        private string _name;

        private int _tileWidth;
        private int _tileHeight;
        private int _tilesWide;
        private int _tilesHigh;

        //private List<Layer> _layers;
        private OrderedResourceCollection<Layer> _layers;

        #endregion

        #region Constructors

        public Level (string name)
        {
            _name = name;

            _layers = new OrderedResourceCollection<Layer>();
        }

        public Level (string name, int tileWidth, int tileHeight, int width, int height)
            : this(name)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _tilesWide = width;
            _tilesHigh = height;
        }

        #endregion

        #region Properties

        public int PixelsHigh
        {
            get { return _tilesHigh * _tileHeight; }
        }

        public int PixelsWide
        {
            get { return _tilesWide * _tileWidth; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
            set { _tileHeight = value; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
            set { _tileWidth = value; }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
            set { _tilesHigh = value; }
        }

        public int TilesWide
        {
            get { return _tilesWide; }
            set { _tilesWide = value; }
        }

        public OrderedResourceCollection<Layer> Layers
        {
            get { return _layers; }
        }

        #endregion

        #region XML Import / Export

        public static Level FromXml (XmlReader reader)
        {
            List<string> reqAttrib = new List<string> {
                "name", "width", "height", "tilewidth", "tileheight",
            };

            Dictionary<string, string> attribs = new Dictionary<string, string>();

            if (reader.HasAttributes) {
                while (reader.MoveToNextAttribute()) {
                    attribs[reader.Name] = reader.Value;
                }
                reader.MoveToElement();
            }

            foreach (string name in reqAttrib) {
                if (!attribs.ContainsKey(name)) {
                    throw new Exception("Required attribute '" + name + "' missing in tag 'level'");
                }
            }

            Level level = new Level(attribs["name"], Convert.ToInt32(attribs["tilewidth"]), Convert.ToInt32(attribs["tileheight"]),
                Convert.ToInt32(attribs["width"]), Convert.ToInt32(attribs["height"]));

            using (XmlReader subReader = reader.ReadSubtree()) {
                while (subReader.Read()) {
                    if (subReader.IsStartElement()) {
                        switch (subReader.Name) {
                            case "layers":
                                AddLayerFromXml(subReader, level);
                                break;
                        }
                    }
                }
            }

            return level;
        }

        private static void AddLayerFromXml (XmlReader reader, Level level)
        {
            using (XmlReader subReader = reader.ReadSubtree()) {
                while (subReader.Read()) {
                    if (subReader.IsStartElement()) {
                        switch (subReader.Name) {
                            case "layer":
                                level.Layers.Add(Layer.FromXml(subReader, level));
                                break;
                        }
                    }
                }
            }
        }

        public void WriteXml (XmlWriter writer)
        {
            // <level name="" height="" width="">
            writer.WriteStartElement("level");
            writer.WriteAttributeString("name", _name);
            writer.WriteAttributeString("width", _tilesWide.ToString());
            writer.WriteAttributeString("height", _tilesHigh.ToString());
            writer.WriteAttributeString("tilewidth", _tileWidth.ToString());
            writer.WriteAttributeString("tileheight", _tileHeight.ToString());

            //   <layers>
            writer.WriteStartElement("layers");

            foreach (Layer layer in _layers) {
                layer.WriteXml(writer);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        #endregion

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        #endregion
    }

    public class TileMap : INamedResource, ITileSource2D
    {
        private static int _lastId = 0;

        private int _id;
        private string _name;

        private int _tileWidth;
        private int _tileHeight;
        private int _roomWidth;     // in tiles
        private int _roomHeight;    // in tiles
        private int _mapWidth;      // in rooms
        private int _mapHeight;     // in rooms

        private List<Layer> _layers;

        public TileMap (string name)
            : this(++_lastId, name)
        {
        }

        public TileMap (int id, string name)
        {
            _id = id;
            _name = name;

            _tileWidth = 16;
            _tileHeight = 16;
            _roomWidth = 15;
            _roomHeight = 10;
            _mapWidth = 2;
            _mapHeight = 2;

            _layers = new List<Layer>();
            _layers.Add(new MultiTileGridLayer(name, TileWidth, TileHeight, TilesWide, TilesHigh));
        }

        public int MapHeight
        {
            get { return _mapHeight; }
        }

        public int MapWidth
        {
            get { return _mapWidth; }
        }

        public int PixelsHigh
        {
            get { return _mapHeight * _roomHeight * _tileHeight; }
        }

        public int PixelsWide
        {
            get { return _mapWidth * _roomWidth * _tileWidth; }
        }

        public int RoomHeight
        {
            get { return _roomHeight; }
        }

        public int RoomWidth
        {
            get { return _roomWidth; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public int TilesHigh
        {
            get { return _mapHeight * _roomHeight; }
        }

        public int TilesWide
        {
            get { return _mapWidth * _roomWidth; }
        }

        #region INamedResource Members

        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public event EventHandler<IdChangedEventArgs> IdChanged;

        public event EventHandler<NameChangedEventArgs> NameChanged;

        #endregion

        #region ITileSource2D Members


        public Tile this[TileCoord coord]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<KeyValuePair<TileCoord, Tile>> Region (Microsoft.Xna.Framework.Rectangle rect)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITileSource Members

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEnumerable<Tile> Members

        public IEnumerator<Tile> GetEnumerator ()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
