using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Xml;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Security.Cryptography;
using Treefrog.Framework.Compat;
using Ionic.Zlib;
using Treefrog.Framework.Model.Proxy;

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
        private Guid _textureId;
        private TextureResource _tileSource;

        private int _tileWidth;
        private int _tileHeight;

        private Dictionary<Guid, Tile> _tiles;
        private Dictionary<Guid, TileCoord> _locations;
        private List<TileCoord> _openLocations;

        //private NamedResourceCollection<Property> _properties;
        private PropertyCollection _properties;
        private TilePoolProperties _predefinedProperties;

        #endregion

        #region Constructors

        protected TilePool ()
        {
            _tiles = new Dictionary<Guid, Tile>();
            _locations = new Dictionary<Guid, TileCoord>();
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
            _textureId = _manager.TexturePool.AddResource(_tileSource);

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

        public Guid TextureId
        {
            get { return _textureId; }
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

        public Guid AddTile (TextureResource texture)
        {
            Guid uid = _manager.TakeId();
            AddTile(texture, uid);

            return uid;
        }

        public void AddTile (TextureResource texture, Guid uid)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight) {
                throw new ArgumentException("Supplied texture does not have tile dimensions", "texture");
            }

            if (_tiles.ContainsKey(uid)) {
                throw new ArgumentException("A tile with the given id already exists");
            }

            if (ShouldExpandTexture()) {
                ExpandTexture();
            }

            TileCoord coord = _openLocations[_openLocations.Count - 1];
            _openLocations.RemoveAt(_openLocations.Count - 1);

            _tileSource.Set(texture, new Point(coord.X * _tileWidth, coord.Y * _tileHeight));
            _manager.TexturePool.Invalidate(_textureId);

            _locations[uid] = coord;
            _tiles[uid] = new PhysicalTile(uid, this);
            _tiles[uid].Modified += TileModifiedHandler;

            _manager.LinkTile(uid, this);

            OnTileAdded(new TileEventArgs(_tiles[uid]));
        }

        public Guid AddTile (byte[] data)
        {
            Guid uid = _manager.TakeId();
            AddTile(data, uid);

            return uid;
        }

        public void AddTile (byte[] data, Guid uid)
        {
            if (data.Length != _tileWidth * _tileHeight * 4) {
                throw new ArgumentException("Supplied color data is incorrect size for tile dimensions", "data");
            }

            TextureResource texture = new TextureResource(_tileWidth, _tileHeight, data);
            AddTile(texture, uid);
        }

        public void RemoveTile (Guid uid)
        {
            if (!_tiles.ContainsKey(uid)) {
                return;
            }

            TileCoord coord = _locations[uid];

            Rectangle dest = new Rectangle(coord.X * _tileWidth, coord.Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.Clear(dest);
            _manager.TexturePool.Invalidate(_textureId);

            _openLocations.Add(_locations[uid]);

            Tile tile = _tiles[uid];
            tile.Modified -= TileModifiedHandler;

            _tiles.Remove(uid);
            _locations.Remove(uid);

            _manager.UnlinkTile(uid);

            if (ShouldReduceTexture()) {
                ReduceTexture();
            }

            OnTileRemoved(new TileEventArgs(tile));
        }

        public Tile GetTile (Guid uid)
        {
            if (!_tiles.ContainsKey(uid)) {
                return null;
            }

            return _tiles[uid];
        }

        public TextureResource GetTileTexture (Guid uid)
        {
            if (!_tiles.ContainsKey(uid))
                return null;

            if (_tileSource == null)
                return null;

            Rectangle src = new Rectangle(_locations[uid].X * _tileWidth, _locations[uid].Y * _tileHeight, _tileWidth, _tileHeight);
            return _tileSource.Crop(src);
        }

        public void SetTileTexture (Guid uid, TextureResource texture)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight) {
                throw new ArgumentException("Supplied texture does not match tile dimensions", "data");
            }

            if (!_tiles.ContainsKey(uid)) {
                throw new ArgumentException("No tile with the given id exists in this tile pool", "id");
            }

            Rectangle dest = new Rectangle(_locations[uid].X * _tileWidth, _locations[uid].Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.Set(texture, new Point(_locations[uid].X * _tileWidth, _locations[uid].Y * _tileHeight));
            _manager.TexturePool.Invalidate(_textureId);

            OnTileModified(new TileEventArgs(_tiles[uid]));
        }

        public TileCoord GetTileLocation (Guid uid)
        {
            if (_locations.ContainsKey(uid)) {
                return _locations[uid];
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
            _manager.TexturePool.Invalidate(_textureId);

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
            _manager.TexturePool.ReplaceResource(_textureId, _tileSource);
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

            Queue<KeyValuePair<Guid, TileCoord>> locs = new Queue<KeyValuePair<Guid, TileCoord>>();
            foreach (KeyValuePair<Guid, TileCoord> kv in _locations) {
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

                    KeyValuePair<Guid, TileCoord> loc = locs.Dequeue();
                    Rectangle src = new Rectangle(loc.Value.X * _tileWidth, loc.Value.Y * _tileHeight, _tileWidth, _tileHeight);

                    newTex.Set(_tileSource.Crop(src), new Point(x * _tileWidth, y * _tileHeight));

                    _locations[loc.Key] = new TileCoord(x, y);
                }
            }

            _tileSource = newTex;
            _manager.TexturePool.ReplaceResource(_textureId, _tileSource);
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
                    TextureResource tileTex = GetTileTexture(tile.Uid);
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

                        Guid newTileId = _manager.TakeId();
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

        public static LibraryX.TilePoolX ToXmlProxyX (TilePool pool)
        {
            if (pool == null)
                return null;

            List<LibraryX.TileDefX> tiledefs = new List<LibraryX.TileDefX>();
            foreach (Tile tile in pool._tiles.Values)
                tiledefs.Add(ToXmlProxyX(tile));

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in pool.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LibraryX.TilePoolX() {
                Name = pool.Name,
                Texture = pool._textureId,
                //Source = TextureResource.ToXmlProxy(pool._tileSource),
                TileWidth = pool._tileWidth,
                TileHeight = pool._tileHeight,
                TileDefinitions = tiledefs.Count > 0 ? tiledefs : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static TilePool FromXmlProxy (LibraryX.TilePoolX proxy, TilePoolManager manager)
        {
            if (proxy == null)
                return null;

            TilePool pool = manager.CreateTilePool(proxy.Name, proxy.TileWidth, proxy.TileHeight);
            manager.TexturePool.RemoveResource(pool._textureId);

            pool._textureId = proxy.Texture;
            pool._tileSource = manager.TexturePool.GetResource(pool._textureId);

            //pool._tileSource = TextureResource.FromXmlProxy(proxy.Source);

            if (pool._tileSource != null) {
                pool._tileSource.Apply(c => {
                    return (c.A == 0) ? Colors.Transparent : c;
                });
            }

            if (proxy.TileDefinitions != null) {
                foreach (var tiledef in proxy.TileDefinitions)
                    FromXmlProxy(tiledef, pool);
            }

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    pool.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }

            if (pool._tileSource != null)
                pool.RecalculateOpenLocations();

            return pool;
        }

        public static LibraryX.TileDefX ToXmlProxyX (Tile tile)
        {
            if (tile == null)
                return null;

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in tile.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            TileCoord loc = tile.Pool.GetTileLocation(tile.Uid);
            return new LibraryX.TileDefX() {
                Uid = tile.Uid,
                Location = loc.X + "," + loc.Y,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static Tile FromXmlProxy (LibraryX.TileDefX proxy, TilePool pool)
        {
            if (proxy == null)
                return null;

            string[] loc = proxy.Location.Split(new char[] { ',' });
            if (loc.Length != 2)
                throw new Exception("Malformed location: " + proxy.Location);

            int x = Convert.ToInt32(loc[0]);
            int y = Convert.ToInt32(loc[1]);
            if (x < 0 || y < 0)
                throw new Exception("Invalid location: " + proxy.Location);

            TileCoord coord = new TileCoord(x, y);
            Tile tile = new PhysicalTile(proxy.Uid, pool);

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    tile.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }

            pool._locations[proxy.Uid] = coord;
            pool._tiles[proxy.Uid] = tile;
            pool._tiles[proxy.Uid].Modified += pool.TileModifiedHandler;

            pool._manager.LinkTile(proxy.Uid, pool);

            return tile;
        }
    }
}
