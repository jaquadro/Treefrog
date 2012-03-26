using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Collections;
using System.Xml;
using System.IO.Compression;
using Treefrog.Framework.Model.Collections;

using Bitmap = System.Drawing.Bitmap;
using BitmapData = System.Drawing.Imaging.BitmapData;
using ImageLockMode = System.Drawing.Imaging.ImageLockMode;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using SysRectangle = System.Drawing.Rectangle;

namespace Treefrog.Framework.Model
{
    public enum TileImportPolicy
    {
        ImprotAll,      // Import all tiles, including duplicate tiles in source
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

        private TileRegistry _registry;
        //private Texture2D _tileSource;
        private TextureSource _tileSource;

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
            //_properties.ResourceAdded += PropertyResourceAddedHandler;
            //_properties.ResourceRemoved += PropertyResourceRemovedHandler;
            //_properties.ResourceRemapped += PropertyResourceRenamedHandler;
        }

        public TilePool (string name, TileRegistry registry, int tileWidth, int tileHeight)
            : this()
        {
            _name = name;
            _registry = registry;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            if (registry.GraphicsDevice != null) {
                _tileSource = new TextureSource(_tileWidth * _initFactor, _tileHeight * _initFactor, _registry.GraphicsDevice);
            }
            else {
                _tileSource = new TextureSource(_tileWidth * _initFactor, _tileHeight * _initFactor);
                registry.GraphicsDeviceInitialized += TileRegistry_GraphicsDeviceInitialized;
            }

            for (int x = 0; x < _initFactor; x++) {
                for (int y = 0; y < _initFactor; y++) {
                    _openLocations.Add(new TileCoord(x, y));
                }
            }
        }

        #endregion

        private void TileRegistry_GraphicsDeviceInitialized (object sender, EventArgs e)
        {
            _tileSource.Initialize(_registry.GraphicsDevice);
            _registry.GraphicsDeviceInitialized -= TileRegistry_GraphicsDeviceInitialized;
        }

        #region Properties

        public int Capacity
        {
            get { return _openLocations.Count + _locations.Count; }
        }

        public int Count
        {
            get { return _tiles.Count; }
        }

        //private PropertyCollection Properties
        //{
        //    get { return _properties; }
        //}

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
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
            //_tileSource.SetData(0, dest, data, 0, data.Length);
            _tileSource.SetData(data, dest);

            _locations[id] = coord;
            _tiles[id] = new PhysicalTile(id, this);
            _tiles[id].Modified += TileModifiedHandler;

            _registry.LinkTile(id, this);

            OnTileAdded(new TileEventArgs(_tiles[id]));
        }

        public void RemoveTile (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return;
            }

            TileCoord coord = _locations[id];

            Rectangle dest = new Rectangle(coord.X * _tileWidth, coord.Y * _tileHeight, _tileWidth, _tileHeight);

            byte[] data = new byte[_tileWidth * _tileHeight * 4];
            //_tileSource.SetData(0, dest, data, 0, data.Length);
            _tileSource.SetData(data, dest);

            _openLocations.Add(_locations[id]);

            Tile tile = _tiles[id];
            tile.Modified -= TileModifiedHandler;

            _tiles.Remove(id);
            _locations.Remove(id);

            _registry.UnlinkTile(id);

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

        public byte[] GetTileTextureData (int id)
        {
            if (!_tiles.ContainsKey(id)) {
                return null;
            }

            Rectangle src = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            byte[] data = new byte[_tileWidth * _tileHeight * 4];

            //_tileSource.GetData(0, src, data, 0, data.Length);
            _tileSource.GetData(data, src);

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
            //_tileSource.SetData(0, dest, data, 0, data.Length);
            _tileSource.SetData(data, dest);

            OnTileModified(new TileEventArgs(_tiles[id]));
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

        public TileCoord GetTileLocation (int id)
        {
            if (_locations.ContainsKey(id)) {
                return _locations[id];
            }
            return new TileCoord(0, 0);
        }

        public void DrawTile (SpriteBatch spriteBatch, int id, Rectangle dest)
        {
            DrawTile(spriteBatch, id, dest, Color.White);
        }

        public void DrawTile (SpriteBatch spritebatch, int id, Rectangle dest, Color color)
        {
            if (!_tiles.ContainsKey(id) || _tileSource.Texture == null) {
                return;
            }

            Rectangle src = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            //spritebatch.Draw(_tileSource, dest, src, Color.White);
            spritebatch.Draw(_tileSource.Texture, dest, src, color);
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

            //Texture2D next = new Texture2D(_registry.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            TextureSource next = new TextureSource(width, height, _registry.GraphicsDevice);
            Rectangle dest = new Rectangle(0, 0, _tileSource.Width, _tileSource.Height);
            //next.SetData(0, dest, data, 0, data.Length);
            next.SetData(data, dest);

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
                    //_tileSource.GetData(0, src, data, 0, data.Length);
                    _tileSource.GetData(data, src);

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
            return Import(name, registry, stream, tileWidth, tileHeight, spaceX, spaceY, marginX, marginY, TileImportPolicy.SetUnique);
        }

        public static TilePool Import (string name, TileRegistry registry, Stream stream, int tileWidth, int tileHeight, int spaceX, int spaceY, int marginX, int marginY, TileImportPolicy policy)
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
            ImportMerge(stream, spaceX, spaceY, marginX, marginY, TileImportPolicy.SetUnique);
        }

        public void ImportMerge (Stream stream, int spaceX, int spaceY, int marginX, int marginY, TileImportPolicy policy)
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
            if (!Directory.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            using (FileStream fs = File.OpenWrite(path)) {
                Export(fs);
            }
        }

        public void Export (Stream stream)
        {
            _tileSource.CopyBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            //SaveAsPng(stream, _tileSource.Width, _tileSource.Height);
        }

        #endregion

        #endregion

        public void ApplyTransparentColor (Color color)
        {
            byte[] data = new byte[_tileSource.Width * _tileSource.Height * 4];
            _tileSource.GetData(data);

            for (int i = 0; i < data.Length; i++) {
                byte a = data[4 * i + 0];
                byte r = data[4 * i + 1];
                byte g = data[4 * i + 2];
                byte b = data[4 * i + 3];

                if (color.R == r && color.G == g && color.B == b) {
                    data[4 * i + 0] = 0;
                }
            }

            _tileSource.SetData(data);
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

            _registry.LinkTile(id, this);

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
                        XmlLoadSourceData(xmlr, width, height, SurfaceFormat.Color);
                        return false;
                    default:
                        return true; // Advance reader
                }
            });
        }

        private void XmlLoadSourceData (XmlReader reader, int width, int height, SurfaceFormat format)
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

                    //_tileSource = new Texture2D(_registry.GraphicsDevice, width, height, false, format);
                    _tileSource = new TextureSource(width, height, _registry.GraphicsDevice);
                    _tileSource.SetData(czData);
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

            byte[] data = new byte[_tileSource.Width * _tileSource.Height * 4];
            _tileSource.GetData(data);

            using (MemoryStream inStr = new MemoryStream(data)) {
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
    }
}
