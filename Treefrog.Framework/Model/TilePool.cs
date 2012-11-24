using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Xml;
using System.IO.Compression;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Treefrog.Framework.Model
{
    public enum TileImportPolicy
    {
        ImportAll,      // Import all tiles, including duplicate tiles in source
        SourceUnique,   // Import each unique tile in source
        SetUnique,      // Import each unique tile in source that is not already in set
    }

    public class TileEventArgs : EventArgs
    {
        public Tile Tile { get; private set; }

        public TileEventArgs (Tile tile) 
        {
            Tile = tile;
        }
    }

    public class TilePool : INamedResource, IEnumerable<Tile>, IPropertyProvider
    {
        private const int _initFactor = 4;

        private static string[] _reservedPropertyNames = new string[] { "Name" };

        #region Fields

        private string _name;

        private TilePoolManager _manager;
        private TextureResource _tileSource;

        private int _tileWidth;
        private int _tileHeight;

        private Dictionary<int, Tile> _tiles;
        private Dictionary<int, TileCoord> _locations;
        private List<TileCoord> _openLocations;

        //private NamedResourceCollection<Property> _properties;
        private PropertyCollection _properties;
        private TilePoolProperties _predefinedProperties;

        #endregion

        #region Constructors

        protected TilePool ()
        {
            _tiles = new Dictionary<int, Tile>();
            _locations = new Dictionary<int, TileCoord>();
            _openLocations = new List<TileCoord>();
            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new TilePool.TilePoolProperties(this);

            _properties.Modified += CustomProperties_Modified;
        }

        internal TilePool (TilePoolManager manager, string name, int tileWidth, int tileHeight)
            : this()
        {
            _name = name;
            _manager = manager;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            _tileSource = new TextureResource(_tileWidth * _initFactor, _tileHeight * _initFactor);

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

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public TextureResource TileSource
        {
            get { return _tileSource; }
        }

        #endregion

        /// <summary>
        /// Occurs after a new <see cref="Tile"/> has been added to the <c>TilePool</c>.
        /// </summary>
        public event EventHandler<TileEventArgs> TileAdded = (s, e) => { };

        /// <summary>
        /// Occurs after a <see cref="Tile"/> has been removed from the <c>TilePool</c>.
        /// </summary>
        public event EventHandler<TileEventArgs> TileRemoved = (s, e) => { };

        /// <summary>
        /// Occurs if a <see cref="Tile"/> or any of its underlying data has been modified.
        /// </summary>
        public event EventHandler<TileEventArgs> TileModified = (s, e) => { };

        public event EventHandler TileSourceInvalidated;

        protected virtual void OnTileSourceInvalidated (EventArgs e)
        {
            if (TileSourceInvalidated != null)
                TileSourceInvalidated(this, e);
        }

        /// <summary>
        /// Raises the <see cref="TileAdded"/> event and triggers the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">A <see cref="TileEventArgs"/> that contains the event data.</param>
        protected virtual void OnTileAdded (TileEventArgs e)
        {
            TileAdded(this, e);
            OnModified(e);
        }

        /// <summary>
        /// Raises the <see cref="TileRemoved"/> event and triggers the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">A <see cref="TileEventArgs"/> that contains the event data.</param>
        protected virtual void OnTileRemoved (TileEventArgs e)
        {
            TileRemoved(this, e);
            OnModified(e);
        }

        /// <summary>
        /// Raises the <see cref="TileModified"/> event and triggers the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">A <see cref="TileEventArgs"/> that contains the event data.</param>
        protected virtual void OnTileModified (TileEventArgs e)
        {
            TileModified(this, e);
            OnModified(e);
        }

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

        public int AddTile (TextureResource texture)
        {
            int id = _manager.TakeId();
            AddTile(texture, id);

            return id;
        }

        public void AddTile (TextureResource texture, int id)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight) {
                throw new ArgumentException("Supplied texture does not have tile dimensions", "texture");
            }

            if (_tiles.ContainsKey(id)) {
                throw new ArgumentException("A tile with the given id already exists");
            }

            if (ShouldExpandTexture()) {
                ExpandTexture();
            }

            TileCoord coord = _openLocations[_openLocations.Count - 1];
            _openLocations.RemoveAt(_openLocations.Count - 1);

            _tileSource.Set(texture, new Point(coord.X * _tileWidth, coord.Y * _tileHeight));

            _locations[id] = coord;
            _tiles[id] = new PhysicalTile(id, this);
            _tiles[id].Modified += TileModifiedHandler;

            _manager.LinkTile(id, this);
            if (_manager.LastId < id)
                _manager.LastId = id;

            OnTileAdded(new TileEventArgs(_tiles[id]));
        }

        public int AddTile (byte[] data)
        {
            int id = _manager.TakeId();
            AddTile(data, id);

            return id;
        }

        public void AddTile (byte[] data, int id)
        {
            if (data.Length != _tileWidth * _tileHeight * 4) {
                throw new ArgumentException("Supplied color data is incorrect size for tile dimensions", "data");
            }

            TextureResource texture = new TextureResource(_tileWidth, _tileHeight, data);
            AddTile(texture, id);
        }

        public void RemoveTile (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return;
            }

            TileCoord coord = _locations[id];

            Rectangle dest = new Rectangle(coord.X * _tileWidth, coord.Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.Clear(dest);

            _openLocations.Add(_locations[id]);

            Tile tile = _tiles[id];
            tile.Modified -= TileModifiedHandler;

            _tiles.Remove(id);
            _locations.Remove(id);

            _manager.UnlinkTile(id);

            if (ShouldReduceTexture()) {
                ReduceTexture();
            }

            OnTileRemoved(new TileEventArgs(tile));
        }

        public Tile GetTile (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return null;
            }

            return _tiles[id];
        }

        public TextureResource GetTileTexture (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return null;
            }

            Rectangle src = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            return _tileSource.Crop(src);
        }

        public void SetTileTexture (int id, TextureResource texture)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight) {
                throw new ArgumentException("Supplied texture does not match tile dimensions", "data");
            }

            if (!_tiles.ContainsKey(id)) {
                throw new ArgumentException("No tile with the given id exists in this tile pool", "id");
            }

            Rectangle dest = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.Set(texture, new Point(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight));

            OnTileModified(new TileEventArgs(_tiles[id]));
        }

        public TileCoord GetTileLocation (int id)
        {
            if (_locations.ContainsKey(id)) {
                return _locations[id];
            }
            return new TileCoord(0, 0);
        }

        #region Texture Management

        private const int _minTiles = 4 * 4;

        public void ReplaceTexture (TextureResource data)
        {
            if (_tileSource.Width != data.Width || _tileSource.Height != data.Height)
                throw new ArgumentException("Replacement texture has different dimensions than internal texture.");

            _tileSource.Set(data, Point.Zero);

            OnTileSourceInvalidated(EventArgs.Empty);
            OnModified(EventArgs.Empty);
        }

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
            int width = _tileSource.Width;
            int height = _tileSource.Height;

            if (width == height) {
                width *= 2;
            }
            else {
                height *= 2;
            }

            TextureResource newTex = new TextureResource(width, height);
            newTex.Set(_tileSource, new Point(0, 0));

            int factorX = newTex.Width / _tileWidth;
            int factorY = newTex.Height / _tileHeight;
            int threshX = _tileSource.Width / _tileWidth;
            int threshY = _tileSource.Height / _tileHeight;

            for (int y = 0; y < factorY; y++) {
                for (int x = 0; x < factorX; x++) {
                    if (x >= threshX || y >= threshY) {
                        _openLocations.Add(new TileCoord(x, y));
                    }
                }
            }

            _tileSource = newTex;
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

            TextureResource newTex = new TextureResource(width, height);

            int factorX = newTex.Width / _tileWidth;
            int factorY = newTex.Height / _tileHeight;

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

                    newTex.Set(_tileSource.Crop(src), new Point(x * _tileWidth, y * _tileHeight));

                    _locations[loc.Key] = new TileCoord(x, y);
                }
            }

            _tileSource = newTex;
        }

        private void RecalculateOpenLocations ()
        {
            _openLocations.Clear();

            int factorX = _tileSource.Width / _tileWidth;
            int factorY = _tileSource.Height / _tileHeight;

            HashSet<TileCoord> usedKeys = new HashSet<TileCoord>();
            foreach (TileCoord coord in _locations.Values)
                usedKeys.Add(coord);

            for (int y = 0; y < factorY; y++) {
                for (int x = 0; x < factorX; x++) {
                    if (!usedKeys.Contains(new TileCoord(x, y))) {
                        _openLocations.Add(new TileCoord(x, y));
                    }
                }
            }
        }

        #endregion

        #region Import / Export

        public class TileImportOptions
        {
            public int TileWidth { get; set; }
            public int TileHeight { get; set; }
            public int SpaceX { get; set; }
            public int SpaceY { get; set; }
            public int MarginX { get; set; }
            public int MarginY { get; set; }
            public TileImportPolicy ImportPolicty { get; set; }

            public TileImportOptions ()
            {
                ImportPolicty = TileImportPolicy.SetUnique;
            }
        }

        #region ImportMerge

        /*public void ImportMerge (Stream stream)
        {
            ImportMerge(stream, 0, 0);
        }

        public void ImportMerge (Stream stream, int spaceX, int spaceY)
        {
            ImportMerge(stream, spaceX, spaceY, 0, 0);
        }

        public void ImportMerge (Stream stream, int spaceX, int spaceY, int marginX, int marginY)
        {
            ImportMerge(stream, spaceX, spaceY, marginX, marginY, TileImportPolicy.SetUnique);
        }

        public void ImportMerge (Stream stream, int spaceX, int spaceY, int marginX, int marginY, TileImportPolicy policy)*/

        public void ImportMerge (TextureResource image, TileImportOptions options)
        {
            int tilesWide = (image.Width - options.MarginX) / (_tileWidth + options.SpaceX);
            int tilesHigh = (image.Height - options.MarginY) / (_tileHeight + options.SpaceY);

            Dictionary<string, Tile> existingHashes = new Dictionary<string, Tile>();
            Dictionary<string, Tile> newHashes = new Dictionary<string,Tile>();

            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider()) {
                foreach (Tile tile in _tiles.Values) {
                    TextureResource tileTex = GetTileTexture(tile.Id);
                    string hash = Convert.ToBase64String(sha1.ComputeHash(tileTex.RawData));
                    existingHashes[hash] = tile;
                }

                for (int y = 0; y < tilesHigh; y++) {
                    for (int x = 0; x < tilesWide; x++) {
                        Rectangle srcLoc = new Rectangle(
                            options.MarginX + x * (_tileWidth + options.SpaceX),
                            options.MarginY + y * (_tileHeight + options.SpaceY),
                            _tileWidth, _tileHeight);

                        TextureResource tileTex = image.Crop(srcLoc);
                        string hash = Convert.ToBase64String(sha1.ComputeHash(tileTex.RawData));

                        if (options.ImportPolicty == TileImportPolicy.SourceUnique && newHashes.ContainsKey(hash))
                            continue;
                        else if (options.ImportPolicty == TileImportPolicy.SetUnique && existingHashes.ContainsKey(hash))
                            continue;

                        int newTileId = _manager.TakeId();
                        AddTile(tileTex, newTileId);

                        Tile newTile = _tiles[newTileId];
                        existingHashes[hash] = newTile;
                        newHashes[hash] = newTile;
                    }
                }
            }
        }

        #endregion

        #region Export

        /*public void Export (string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            using (FileStream fs = File.OpenWrite(path)) {
                Export(fs);
            }
        }

        public void Export (Stream stream)
        {
            _tileSource.Bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        }*/

        #endregion

        #endregion

        public void ApplyTransparentColor (Color color)
        {
            _tileSource.Apply(c =>
            {
                if (color.R != c.R || color.G != c.G || color.B != c.B)
                    return c;
                else
                    return Colors.Transparent;
            });
        }

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value) {
                    NameChangingEventArgs ea = new NameChangingEventArgs(_name, value);
                    OnNameChanging(ea);
                    if (ea.Cancel)
                        return;

                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, _name));
                    OnPropertyProviderNameChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler<NameChangingEventArgs> NameChanging;

        public event EventHandler<NameChangedEventArgs> NameChanged;

        public event EventHandler Modified;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            if (NameChanging != null) {
                NameChanging(this, e);
            }
        }

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        private void TileModifiedHandler (object sender, EventArgs e)
        {
            OnTileModified(new TileEventArgs(sender as Tile));
        }

        #endregion

        #region XML Import / Export

        public static TilePool FromXml (XmlReader reader, TilePoolManager manager)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "name", "tilewidth", "tileheight",
            });

            TilePool pool = manager.CreateTilePool(attribs["name"], Convert.ToInt32(attribs["tilewidth"]), Convert.ToInt32(attribs["tileheight"]));

            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "source":
                        pool.XmlLoadSource(xmlr);
                        return false;
                    case "tiles":
                        pool.XmlLoadTiles(xmlr);
                        return false;
                    case "properties":
                        AddPropertyFromXml(xmlr, pool.CustomProperties);
                        return false;
                    default:
                        return true; // Advance reader
                }
            });

            return pool;
        }

        private void XmlLoadTiles (XmlReader reader)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "tile":
                        XmlLoadTile(xmlr);
                        break;
                }
            });

            RebuildOpenLocations();
        }

        private void XmlLoadTile (XmlReader reader)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "id", "loc",
            });

            string[] loc = attribs["loc"].Split(new char[] { ',' });

            if (loc.Length != 2) {
                throw new Exception("Malformed location: " + attribs["loc"]);
            }

            int id = Convert.ToInt32(attribs["id"]);
            int x = Convert.ToInt32(loc[0]);
            int y = Convert.ToInt32(loc[1]);

            if (x < 0 || y < 0) {
                throw new Exception("Invalid location: " + attribs["loc"]);
            }

            TileCoord coord = new TileCoord(x, y);

            _locations[id] = coord;
            _tiles[id] = new PhysicalTile(id, this);
            _tiles[id].Modified += TileModifiedHandler;

            _manager.LinkTile(id, this);

            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "properties":
                        AddPropertyFromXml(xmlr, _tiles[id].CustomProperties);
                        break;
                }
            });
        }

        private void RebuildOpenLocations ()
        {
            HashSet<TileCoord> used = new HashSet<TileCoord>();
            foreach (TileCoord coord in _locations.Values) {
                used.Add(coord);
            }

            int w = _tileSource.Width / _tileWidth;
            int h = _tileSource.Height / _tileHeight;

            _openLocations.Clear();

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    TileCoord coord = new TileCoord(x, y);
                    if (!used.Contains(coord)) {
                        _openLocations.Add(coord);
                    }
                }
            }
        }

        private void XmlLoadSource (XmlReader reader)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "width", "height", "format",
            });

            if (attribs["format"] != "Color") {
                throw new Exception("Unsupported tile source format: '" + attribs["format"] + "'");
            }

            int width = Convert.ToInt32(attribs["width"]);
            int height = Convert.ToInt32(attribs["height"]);

            if (width <= 0 || height <= 0) {
                throw new Exception("Invalid tile source dimensions: " + width + " x " + height);
            }

            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "data":
                        XmlLoadSourceData(xmlr, width, height);
                        return false;
                    default:
                        return true; // Advance reader
                }
            });
        }

        private void XmlLoadSourceData (XmlReader reader, int width, int height)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "encoding", "compression",
            });

            if (attribs["compression"] != "deflate") {
                throw new Exception("Unsupported compression option: '" + attribs["compression"] + "'");
            }

            if (attribs["encoding"] != "base64") {
                throw new Exception("Unsupported encoding option: '" + attribs["encoding"] + "'");
            }

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
                    if (czData.Length != width * height * 4) {
                        throw new Exception("Unexpected length of payload");
                    }

                    _tileSource = new TextureResource(width, height, czData);
                }
            }
        }

        private static void AddPropertyFromXml (XmlReader reader, PropertyCollection pp)
        {
            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "property":
                        pp.Add(Property.FromXml(xmlr));
                        return false;
                    default:
                        return true;
                }
            });
        }

        public void WriteXml (XmlWriter writer)
        {
            // <tileset>
            writer.WriteStartElement("tileset");
            writer.WriteAttributeString("name", _name);
            writer.WriteAttributeString("tilewidth", _tileWidth.ToString());
            writer.WriteAttributeString("tileheight", _tileHeight.ToString());

            // . <source>
            writer.WriteStartElement("source");
            writer.WriteAttributeString("width", _tileSource.Width.ToString());
            writer.WriteAttributeString("height", _tileSource.Height.ToString());
            writer.WriteAttributeString("format", "Color");

            // . . <data>
            writer.WriteStartElement("data");
            writer.WriteAttributeString("encoding", "base64");
            writer.WriteAttributeString("compression", "deflate");

            using (MemoryStream inStr = new MemoryStream(_tileSource.RawData)) {
                using (MemoryStream outStr = new MemoryStream()) {
                    using (DeflateStream zstr = new DeflateStream(outStr, CompressionMode.Compress)) {
                        inStr.CopyTo(zstr);
                    }

                    byte[] czData = outStr.GetBuffer();
                    writer.WriteBase64(czData, 0, czData.Length);
                }
            }

            writer.WriteEndElement();
            // . . </data>

            writer.WriteEndElement();
            // . </source>

            // . <tiles>
            writer.WriteStartElement("tiles");
            foreach (int id in _tiles.Keys) {
                TileCoord loc = _locations[id];
                Tile tile = _tiles[id];

                // . . <tile>
                writer.WriteStartElement("tile");
                writer.WriteAttributeString("id", id.ToString());
                writer.WriteAttributeString("loc", loc.X + "," + loc.Y);

                // . . . <properties>
                if (tile.CustomProperties.Count > 0) {
                    writer.WriteStartElement("properties");

                    foreach (Property property in tile.CustomProperties) {
                        property.WriteXml(writer);
                    }
                    writer.WriteEndElement();
                }
                // . . . </properties>

                writer.WriteEndElement();
                // . . </tile>
            }
            writer.WriteEndElement();
            // . </tiles>

            // . <properties>
            if (_properties.Count > 0) {
                writer.WriteStartElement("properties");

                foreach (Property property in _properties) {
                    property.WriteXml(writer);
                }
                writer.WriteEndElement();
            }
            // . </properties>

            writer.WriteEndElement();
        }

        #endregion

        #region IPropertyProvider Members

        private class TilePoolProperties : PredefinedPropertyCollection
        {
            private TilePool _parent;

            public TilePoolProperties (TilePool parent)
                : base(_reservedPropertyNames)
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield return _parent.LookupProperty("Name");
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public string PropertyProviderName
        {
            get { return _name; }
        }

        public PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties; }
        }

        public PropertyCollection CustomProperties
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

        #endregion

        private void CustomProperties_Modified (object sender, EventArgs e)
        {
            OnModified(e);
        }

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            Name = property.Value;
        }

        public static TilePoolXmlProxy ToXmlProxy (TilePool pool)
        {
            if (pool == null)
                return null;

            List<TileDefXmlProxy> tiledefs = new List<TileDefXmlProxy>();
            foreach (Tile tile in pool._tiles.Values)
                tiledefs.Add(ToXmlProxy(tile));

            List<PropertyXmlProxy> props = new List<PropertyXmlProxy>();
            foreach (Property prop in pool.CustomProperties)
                props.Add(Property.ToXmlProxy(prop));

            return new TilePoolXmlProxy()
            {
                Name = pool.Name,
                Source = TextureResource.ToXmlProxy(pool._tileSource),
                TileWidth = pool._tileWidth,
                TileHeight = pool._tileHeight,
                TileDefinitions = tiledefs.Count > 0 ? tiledefs : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static TilePool FromXmlProxy (TilePoolXmlProxy proxy, TilePoolManager manager)
        {
            if (proxy == null)
                return null;

            TilePool pool = manager.CreateTilePool(proxy.Name, proxy.TileWidth, proxy.TileHeight);
            pool._tileSource = TextureResource.FromXmlProxy(proxy.Source);

            pool._tileSource.Apply(c =>
            {
                return (c.A == 0) ? Colors.Transparent : c;
            });

            foreach (TileDefXmlProxy tiledef in proxy.TileDefinitions)
                FromXmlProxy(tiledef, pool);

            foreach (PropertyXmlProxy propertyProxy in proxy.Properties)
                pool.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));

            pool.RecalculateOpenLocations();

            return pool;
        }

        public static TileDefXmlProxy ToXmlProxy (Tile tile)
        {
            if (tile == null)
                return null;

            List<PropertyXmlProxy> props = new List<PropertyXmlProxy>();
            foreach (Property prop in tile.CustomProperties)
                props.Add(Property.ToXmlProxy(prop));

            TileCoord loc = tile.Pool.GetTileLocation(tile.Id);
            return new TileDefXmlProxy()
            {
                Id = tile.Id,
                Loc = loc.X + "," + loc.Y,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static Tile FromXmlProxy (TileDefXmlProxy proxy, TilePool pool)
        {
            if (proxy == null)
                return null;

            string[] loc = proxy.Loc.Split(new char[] { ',' });
            if (loc.Length != 2)
                throw new Exception("Malformed location: " + proxy.Loc);

            int x = Convert.ToInt32(loc[0]);
            int y = Convert.ToInt32(loc[1]);
            if (x < 0 || y < 0)
                throw new Exception("Invalid location: " + proxy.Loc);

            TileCoord coord = new TileCoord(x, y);
            Tile tile = new PhysicalTile(proxy.Id, pool);

            foreach (PropertyXmlProxy propertyProxy in proxy.Properties)
                tile.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));

            pool._locations[proxy.Id] = coord;
            pool._tiles[proxy.Id] = tile;
            pool._tiles[proxy.Id].Modified += pool.TileModifiedHandler;

            pool._manager.LinkTile(proxy.Id, pool);
            if (pool._manager.LastId < proxy.Id)
                pool._manager.LastId = proxy.Id;

            return tile;
        }
    }

    [XmlRoot("TilePool")]
    public class TilePoolXmlProxy
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int TileWidth { get; set; }

        [XmlAttribute]
        public int TileHeight { get; set; }

        [XmlElement]
        public TextureResource.XmlProxy Source { get; set; }

        [XmlArray]
        [XmlArrayItem("TileDef")]
        public List<TileDefXmlProxy> TileDefinitions { get; set; }

        [XmlArray]
        [XmlArrayItem("Property")]
        public List<PropertyXmlProxy> Properties { get; set; }
    }

    [XmlRoot("TileDef")]
    public class TileDefXmlProxy
    {
        [XmlAttribute]
        public int Id { get; set; }

        [XmlAttribute]
        public string Loc { get; set; }

        [XmlArray]
        [XmlArrayItem("Property")]
        public List<PropertyXmlProxy> Properties { get; set; }
    }
}
