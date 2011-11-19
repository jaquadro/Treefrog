using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Collections;
using System.Xml;
using System.IO.Compression;

namespace Treefrog.Framework.Model
{
    public enum ImportPolicy
    {
        ImprotAll,      // Import all tiles, including duplicate tiles in source
        SourceUnique,   // Import each unique tile in source
        SetUnique,      // Import each unique tile in source that is not already in set
    }

    public class TilePool : INamedResource, IEnumerable<Tile>, IPropertyProvider
    {
        private const int _initFactor = 4;

        #region Fields

        private string _name;

        private TileRegistry _registry;
        private Texture2D _tileSource;

        private int _tileWidth;
        private int _tileHeight;

        private Dictionary<int, Tile> _tiles;
        private Dictionary<int, TileCoord> _locations;
        private List<TileCoord> _openLocations;

        private NamedResourceCollection<Property> _properties;

        #endregion

        #region Constructors

        protected TilePool ()
        {
            _tiles = new Dictionary<int, Tile>();
            _locations = new Dictionary<int, TileCoord>();
            _openLocations = new List<TileCoord>();
            _properties = new NamedResourceCollection<Property>();

            _properties.Modified += PropertiesModifiedHandler;
        }

        public TilePool (string name, TileRegistry registry, int tileWidth, int tileHeight)
            : this()
        {
            _name = name;
            _registry = registry;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            _tileSource = new Texture2D(_registry.GraphicsDevice, _tileWidth * _initFactor, _tileHeight * _initFactor, false, SurfaceFormat.Color);
            for (int x = 0; x < _initFactor; x++) {
                for (int y = 0; y < _initFactor; y++) {
                    _openLocations.Add(new TileCoord(x, y));
                }
            }
        }

        #endregion

        #region Properties

        public int Capacity
        {
            get { return _openLocations.Count + _locations.Count; }
        }

        public int Count
        {
            get { return _tiles.Count; }
        }

        public NamedResourceCollection<Property> Properties
        {
            get { return _properties; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        #endregion

        private void PropertiesModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return _tiles.Values.GetEnumerator();
        }

        public IEnumerator<Tile> GetEnumerator ()
        {
            return _tiles.Values.GetEnumerator();
        }

        public int AddTile (Texture2D texture)
        {
            int id = _registry.TakeId();
            AddTile(texture, id);

            return id;
        }

        public void AddTile (Texture2D texture, int id)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight) {
                throw new ArgumentException("Supplied texture does not have tile dimensions", "texture");
            }

            byte[] data = new byte[_tileWidth * _tileHeight * 4];
            texture.GetData(data);

            AddTile(data, id);
        }

        public int AddTile (byte[] data)
        {
            int id = _registry.TakeId();
            AddTile(data, id);

            return id;
        }

        public void AddTile (byte[] data, int id)
        {
            if (data.Length != _tileWidth * _tileHeight * 4) {
                throw new ArgumentException("Supplied color data is incorrect size for tile dimensions", "data");
            }

            if (_tiles.ContainsKey(id)) {
                throw new ArgumentException("A tile with the given id already exists");
            }

            if (ShouldExpandTexture()) {
                ExpandTexture();
            }

            TileCoord coord = _openLocations[_openLocations.Count - 1];
            _openLocations.RemoveAt(_openLocations.Count - 1);

            Rectangle dest = new Rectangle(coord.X * _tileWidth, coord.Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.SetData(0, dest, data, 0, data.Length);

            _locations[id] = coord;
            _tiles[id] = new PhysicalTile(id, this);
            _tiles[id].Modified += TileModifiedHandler;

            _registry.LinkTile(id, this);
        }

        public void RemoveTile (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return;
            }

            TileCoord coord = _locations[id];

            Rectangle dest = new Rectangle(coord.X * _tileWidth, coord.Y * _tileHeight, _tileWidth, _tileHeight);

            Color[] data = new Color[_tileWidth * _tileHeight];
            _tileSource.SetData(0, dest, data, 0, data.Length);

            _openLocations.Add(_locations[id]);

            _tiles.Remove(id);
            _locations.Remove(id);

            if (ShouldReduceTexture()) {
                ReduceTexture();
            }
        }

        public Tile GetTile (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return null;
            }

            return _tiles[id];
        }

        public byte[] GetTileTextureData (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return null;
            }

            Rectangle src = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            byte[] data = new byte[_tileWidth * _tileHeight * 4];

            _tileSource.GetData(0, src, data, 0, data.Length);

            return data;
        }

        public Texture2D GetTileTexture (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return null;
            }

            byte[] data = GetTileTextureData(id);

            Texture2D texture = new Texture2D(_registry.GraphicsDevice, _tileWidth, _tileHeight);
            texture.SetData(data);

            return texture;
        }

        public void SetTileTextureData (int id, byte[] data)
        {
            if (data.Length != _tileWidth * _tileHeight * 4) {
                throw new ArgumentException("Supplied texture data does not match tile dimensions", "data");
            }

            if (!_tiles.ContainsKey(id)) {
                throw new ArgumentException("No tile with the given id exists in this tile pool", "id");
            }

            Rectangle dest = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.SetData(0, dest, data, 0, data.Length);
        }

        public void SetTileTexture (int id, Texture2D texture)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight) {
                throw new ArgumentException("Supplied texture does not match tile dimensions", "data");
            }

            byte[] data = new byte[_tileWidth * _tileHeight * 4];
            texture.GetData(data);

            SetTileTextureData(id, data); 
        }

        public void DrawTile (SpriteBatch spriteBatch, int id, Rectangle dest)
        {
            DrawTile(spriteBatch, id, dest, Color.White);
        }

        public void DrawTile (SpriteBatch spritebatch, int id, Rectangle dest, Color color)
        {
            if (!_tiles.ContainsKey(id)) {
                return;
            }

            Rectangle src = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            //spritebatch.Draw(_tileSource, dest, src, Color.White);
            spritebatch.Draw(_tileSource, dest, src, color);
        }

        #region Texture Management

        private const int _minTiles = 4 * 4;

        private bool ShouldExpandTexture ()
        {
            return _openLocations.Count == 0;
        }

        private bool ShouldReduceTexture ()
        {
            return _openLocations.Count >= _tiles.Count && (_openLocations.Count + _tiles.Count) > _minTiles;
        }

        private void ExpandTexture ()
        {
            byte[] data = new byte[_tileSource.Width * _tileSource.Height * 4];
            _tileSource.GetData(data);

            int width = _tileSource.Width;
            int height = _tileSource.Height;

            if (width == height) {
                width *= 2;
            }
            else {
                height *= 2;
            }

            Texture2D next = new Texture2D(_registry.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            Rectangle dest = new Rectangle(0, 0, _tileSource.Width, _tileSource.Height);
            next.SetData(0, dest, data, 0, data.Length);

            int factorX = next.Width / _tileWidth;
            int factorY = next.Height / _tileHeight;
            int threshX = _tileSource.Width / _tileWidth;
            int threshY = _tileSource.Height / _tileHeight;

            for (int y = 0; y < factorY; y++) {
                for (int x = 0; x < factorX; x++) {
                    if (x >= threshX || y >= threshY) {
                        _openLocations.Add(new TileCoord(x, y));
                    }
                }
            }

            _tileSource = next;
        }

        private void ReduceTexture ()
        {
            int width = _tileSource.Width;
            int height = _tileSource.Height;

            if (width == height) {
                height /= 2;
            }
            else {
                width /= 2;
            }

            Queue<KeyValuePair<int, TileCoord>> locs = new Queue<KeyValuePair<int, TileCoord>>();
            foreach (KeyValuePair<int, TileCoord> kv in _locations) {
                locs.Enqueue(kv);
            }

            Texture2D next = new Texture2D(_registry.GraphicsDevice, width, height, false, SurfaceFormat.Color);

            int factorX = next.Width / _tileWidth;
            int factorY = next.Height / _tileHeight;

            byte[] data = new byte[_tileWidth * _tileHeight * 4];

            _locations.Clear();
            _openLocations.Clear();

            for (int y = 0; y < factorY; y++) {
                for (int x = 0; x < factorX; x++) {
                    if (locs.Count == 0) {
                        _openLocations.Add(new TileCoord(x, y));
                        continue;
                    }

                    KeyValuePair<int, TileCoord> loc = locs.Dequeue();
                    Rectangle src = new Rectangle(loc.Value.X * _tileWidth, loc.Value.Y * _tileHeight, _tileWidth, _tileHeight);
                    _tileSource.GetData(0, src, data, 0, data.Length);

                    Rectangle dst = new Rectangle(x * _tileWidth, y * _tileHeight, _tileWidth, _tileHeight);
                    next.SetData(0, dst, data, 0, data.Length);

                    _locations[loc.Key] = new TileCoord(x, y);
                }
            }
        }

        #endregion

        #region Import / Export

        #region Import

        public static TilePool Import (string name, TileRegistry registry, Stream stream, int tileWidth, int tileHeight)
        {
            return Import(name, registry, stream, tileWidth, tileHeight, 0, 0);
        }

        public static TilePool Import (string name, TileRegistry registry, Stream stream, int tileWidth, int tileHeight, int spaceX, int spaceY)
        {
            return Import(name, registry, stream, tileWidth, tileHeight, spaceX, spaceY, 0, 0);
        }

        public static TilePool Import (string name, TileRegistry registry, Stream stream, int tileWidth, int tileHeight, int spaceX, int spaceY, int marginX, int marginY)
        {
            return Import(name, registry, stream, tileWidth, tileHeight, spaceX, spaceY, marginX, marginY, ImportPolicy.SetUnique);
        }

        public static TilePool Import (string name, TileRegistry registry, Stream stream, int tileWidth, int tileHeight, int spaceX, int spaceY, int marginX, int marginY, ImportPolicy policy)
        {
            TilePool pool = new TilePool(name, registry, tileWidth, tileHeight);
            pool.ImportMerge(stream, spaceX, spaceY, marginX, marginY, policy);

            return pool;
        }

        #endregion

        #region ImportMerge

        public void ImportMerge (Stream stream)
        {
            ImportMerge(stream, 0, 0);
        }

        public void ImportMerge (Stream stream, int spaceX, int spaceY)
        {
            ImportMerge(stream, spaceX, spaceY, 0, 0);
        }

        public void ImportMerge (Stream stream, int spaceX, int spaceY, int marginX, int marginY)
        {
            ImportMerge(stream, spaceX, spaceY, marginX, marginY, ImportPolicy.SetUnique);
        }

        public void ImportMerge (Stream stream, int spaceX, int spaceY, int marginX, int marginY, ImportPolicy policy)
        {
            try {
                Texture2D source = Texture2D.FromStream(_registry.GraphicsDevice, stream);

                int tilesWide = (source.Width - marginX) / (_tileWidth + spaceX);
                int tilesHigh = (source.Height - marginY) / (_tileHeight + spaceY);

                for (int y = 0; y < tilesHigh; y++) {
                    for (int x = 0; x < tilesWide; x++) {
                        Rectangle srcLoc = new Rectangle(marginX + x * (_tileWidth + spaceX), marginY + y * (_tileHeight + spaceY), _tileWidth, _tileHeight);

                        byte[] data = new byte[_tileWidth * _tileHeight * 4];
                        source.GetData(0, srcLoc, data, 0, data.Length);

                        AddTile(data, _registry.TakeId());
                    }
                }
            }
            catch (Exception e) {
                // TODO: Proper exception handling
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
        }

        #endregion

        #region Export

        public void Export (string path)
        {
            using (FileStream fs = File.OpenWrite(path)) {
                Export(fs);
            }
        }

        public void Export (Stream stream)
        {
            _tileSource.SaveAsPng(stream, _tileSource.Width, _tileSource.Height);
        }

        #endregion

        #endregion

        public void ApplyTransparentColor (Color color)
        {
            Color[] data = new Color[_tileSource.Width * _tileSource.Height];
            _tileSource.GetData(data);

            for (int i = 0; i < data.Length; i++) {
                if (color.R == data[i].R && color.G == data[i].G && color.B == data[i].B) {
                    data[i].A = 0;
                }
            }

            _tileSource.SetData(data);
        }

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
            private set
            {
                if (_name != value) {
                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, _name));
                }
            }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged;

        public event EventHandler Modified;

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        private void TileModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #endregion

        #region XML Import / Export

        public static TilePool FromXml (XmlReader reader, IServiceProvider services)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "name", "tilewidth", "tileheight",
            });

            TileRegistry registry = services.GetService(typeof(TileRegistry)) as TileRegistry;

            TilePool pool = new TilePool(attribs["name"], registry, Convert.ToInt32(attribs["tilewidth"]), Convert.ToInt32(attribs["tileheight"]));

            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "tile":
                        pool.AddTileFromXml(xmlr);
                        return false;
                    default:
                        return true; // Advance reader
                }
            });

            return pool;
        }

        public void WriteXml (XmlWriter writer)
        {
            // <tileset>
            writer.WriteStartElement("tileset");
            writer.WriteAttributeString("name", _name);
            writer.WriteAttributeString("tilewidth", _tileWidth.ToString());
            writer.WriteAttributeString("tileheight", _tileHeight.ToString());

            foreach (int id in _tiles.Keys) {
                TileCoord loc = _locations[id];
                Tile tile = _tiles[id];

                Rectangle texRec = new Rectangle(loc.X * _tileWidth, loc.Y * _tileHeight, _tileWidth, _tileHeight);
                byte[] texData = new byte[TileWidth * TileHeight * 4];
                _tileSource.GetData(0, texRec, texData, 0, texData.Length);

                writer.WriteStartElement("tile");
                writer.WriteAttributeString("id", id.ToString());

                using (MemoryStream inStr = new MemoryStream(texData)) {
                    using (MemoryStream outStr = new MemoryStream()) {
                        using (DeflateStream zstr = new DeflateStream(outStr, CompressionMode.Compress)) {
                            inStr.CopyTo(zstr);
                        }

                        byte[] czData = outStr.GetBuffer();
                        writer.WriteBase64(czData, 0, czData.Length);
                    }
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void AddTileFromXml (XmlReader reader)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "id",
            });

            using (MemoryStream inStr = new MemoryStream()) {
                byte[] buffer = new byte[1000];
                int count = 0;

                while ((count = reader.ReadElementContentAsBase64(buffer, 0, buffer.Length)) > 0) {
                    inStr.Write(buffer, 0, count);
                }

                inStr.Position = 0;
                using (MemoryStream outStr = new MemoryStream(TileWidth * TileHeight * 4)) {
                    using (DeflateStream zstr = new DeflateStream(inStr, CompressionMode.Decompress)) {
                        zstr.CopyTo(outStr);
                    }

                    byte[] czData = outStr.GetBuffer();
                    if (czData.Length != TileWidth * TileHeight * 4) {
                        throw new Exception("Unexpected length of Tile payload");
                    }

                    AddTile(czData, Convert.ToInt32(attribs["id"]));
                }
            }
        }

        #endregion

        #region IPropertyProvider Members

        public string PropertyProviderName
        {
            get { return _name; }
        }

        public IEnumerable<Property> PredefinedProperties
        {
            get 
            {
                yield return LookupProperty("Name");
            }
        }

        public IEnumerable<Property> CustomProperties
        {
            get { return _properties; }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            switch (name) {
                case "Name":
                    return PropertyCategory.Predefined;
                default:
                    return _properties.Contains(name) ? PropertyCategory.Custom : PropertyCategory.None;
            }
        }

        public Property LookupProperty (string name)
        {
            Property prop;

            switch (name) {
                case "Name":
                    prop = new StringProperty("Name", _name);
                    prop.ValueChanged += NamePropertyChangedHandler;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        public void AddCustomProperty (Property property)
        {
            if (property == null) {
                throw new ArgumentNullException("The property is null.");
            }
            if (_properties.Contains(property.Name)) {
                throw new ArgumentException("A property with the same name already exists.");
            }

            _properties.Add(property);
        }

        public void RemoveCustomProperty (string name)
        {
            if (name == null) {
                throw new ArgumentNullException("The name is null.");
            }

            _properties.Remove(name);
        }

        #endregion

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            Name = property.Value;
        }
    }
}
