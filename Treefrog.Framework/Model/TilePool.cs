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

    public class TileResourceCollection : ResourceCollection<Tile>
    {
        private const int _initFactor = 4;
        private const int _minTiles = _initFactor * _initFactor;

        private int _tileWidth;
        private int _tileHeight;

        private TextureResource _tileSource;
        private Dictionary<Guid, TileCoord> _locations;
        private List<TileCoord> _openLocations;

        private TilePool _pool;
        private TexturePool _texturePool;
        private Guid _textureId;

        public TileResourceCollection (int tileWidth, int tileHeight, TilePool pool, TexturePool texturePool)
        {
            _pool = pool;
            _texturePool = texturePool;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            _locations = new Dictionary<Guid, TileCoord>();
            _openLocations = new List<TileCoord>();

            _tileSource = new TextureResource(_tileWidth * _initFactor, _tileHeight * _initFactor);
            _textureId = _texturePool.AddResource(_tileSource);

            for (int x = 0; x < _initFactor; x++) {
                for (int y = 0; y < _initFactor; y++)
                    _openLocations.Add(new TileCoord(x, y));
            }
        }

        public TextureResource TextureResource
        {
            get { return _tileSource; }
            internal set { _tileSource = value; }
        }

        internal Guid TextureId
        {
            get { return _textureId; }
            set { _textureId = value; }
        }

        public int Capacity
        {
            get { return _openLocations.Count + _locations.Count; }
        }

        protected override void AddCore (Tile item)
        {
            base.AddCore(item);
        }

        protected override void RemoveCore (Tile item)
        {
            base.RemoveCore(item);

            TileCoord coord = _locations[item.Uid];

            Rectangle dest = new Rectangle(coord.X * _tileWidth, coord.Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.Clear(dest);
            _texturePool.Invalidate(_textureId);

            _openLocations.Add(_locations[item.Uid]);
            _locations.Remove(item.Uid);

            if (ShouldReduceTexture())
                ReduceTexture();
        }

        public Tile Add (byte[] data)
        {
            if (data.Length != _tileWidth * _tileHeight * 4)
                throw new ArgumentException("Supplied color data is incorrect size for tile dimensions", "data");

            TextureResource texture = new TextureResource(_tileWidth, _tileHeight, data);
            return Add(texture);
        }

        public Tile Add (TextureResource texture)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight)
                throw new ArgumentException("Supplied texture does not have tile dimensions", "texture");

            Tile tile = new PhysicalTile() {
                Pool =_pool
            };

            if (ShouldExpandTexture())
                ExpandTexture();

            TileCoord coord = _openLocations[_openLocations.Count - 1];
            _openLocations.RemoveAt(_openLocations.Count - 1);

            _tileSource.Set(texture, new Point(coord.X * _tileWidth, coord.Y * _tileHeight));
            _texturePool.Invalidate(_textureId);

            _locations[tile.Uid] = coord;

            Add(tile);

            return tile;
        }

        public TextureResource GetTileTexture (Guid uid)
        {
            if (!Contains(uid))
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

            if (!Contains(uid)) {
                throw new ArgumentException("No tile with the given id exists in this tile pool", "id");
            }

            Rectangle dest = new Rectangle(_locations[uid].X * _tileWidth, _locations[uid].Y * _tileHeight, _tileWidth, _tileHeight);
            _tileSource.Set(texture, new Point(_locations[uid].X * _tileWidth, _locations[uid].Y * _tileHeight));
            _texturePool.Invalidate(_textureId);

            OnResourceModified(new ResourceEventArgs<Tile>(this[uid]));
        }

        public TileCoord GetTileLocation (Guid uid)
        {
            if (_locations.ContainsKey(uid)) {
                return _locations[uid];
            }
            return new TileCoord(0, 0);
        }

        public void ReplaceTexture (TextureResource data)
        {
            if (_tileSource.Width != data.Width || _tileSource.Height != data.Height)
                throw new ArgumentException("Replacement texture has different dimensions than internal texture.");

            _tileSource.Set(data, Point.Zero);
            _texturePool.Invalidate(_textureId);

            //OnTileSourceInvalidated(EventArgs.Empty);
            OnModified(EventArgs.Empty);
        }

        private bool ShouldExpandTexture ()
        {
            return _openLocations.Count == 0;
        }

        private bool ShouldReduceTexture ()
        {
            return _openLocations.Count >= Count && (_openLocations.Count + Count) > _minTiles;
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
            _texturePool.ReplaceResource(_textureId, _tileSource);
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
            _texturePool.ReplaceResource(_textureId, _tileSource);
        }

        internal void RecalculateOpenLocations ()
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

        public static LibraryX.TileDefX ToXmlProxyX (Tile tile)
        {
            if (tile == null)
                return null;

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in tile.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            TileCoord loc = tile.Pool.Tiles.GetTileLocation(tile.Uid);
            return new LibraryX.TileDefX() {
                Uid = tile.Uid,
                Location = loc.X + "," + loc.Y,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static Tile FromXmlProxy (LibraryX.TileDefX proxy, TileResourceCollection tileCollection)
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
            Tile tile = new PhysicalTile(proxy.Uid) {
                Pool = tileCollection._pool,
            };

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    tile.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }

            tileCollection._locations[tile.Uid] = coord;
            tileCollection.Add(tile);

            return tile;
        }

        public static TileResourceCollection FromXmlProxy (LibraryX.TilePoolX proxy, TilePool pool, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            TileResourceCollection collection = new TileResourceCollection(proxy.TileWidth, proxy.TileHeight, pool, texturePool);

            texturePool.RemoveResource(collection._textureId);

            collection._textureId = proxy.Texture;
            collection._tileSource = texturePool.GetResource(collection._textureId);

            if (collection._tileSource != null) {
                collection._tileSource.Apply(c => {
                    return (c.A == 0) ? Colors.Transparent : c;
                });
            }

            if (proxy.TileDefinitions != null) {
                foreach (var tiledef in proxy.TileDefinitions)
                    TileResourceCollection.FromXmlProxy(tiledef, collection);
            }

            if (collection.TextureResource != null)
                collection.RecalculateOpenLocations();

            return collection;
        }
    }

    public class TilePool : IResource, IResourceManager<Tile>, IPropertyProvider, INotifyPropertyChanged
    {
        private const int _initFactor = 4;

        private static string[] _reservedPropertyNames = new string[] { "Name" };

        private string _name;

        private TilePoolManager _manager;
        private Guid _textureId;
        private TextureResource _tileSource;

        private int _tileWidth;
        private int _tileHeight;

        //private Dictionary<Guid, Tile> _tiles;
        //private ResourceCollection<Tile> _tiles;
        private TileResourceCollection _tiles;
        //private Dictionary<Guid, TileCoord> _locations;
        //private List<TileCoord> _openLocations;

        //private NamedResourceCollection<Property> _properties;
        private PropertyCollection _properties;
        private TilePoolProperties _predefinedProperties;

        protected TilePool ()
        {
            Uid = Guid.NewGuid();

            //_tiles = new Dictionary<Guid, Tile>();
            //_locations = new Dictionary<Guid, TileCoord>();
            //_openLocations = new List<TileCoord>();
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

            _tiles = new TileResourceCollection(tileWidth, tileHeight, this, manager.TexturePool);

            /*for (int x = 0; x < _initFactor; x++) {
                for (int y = 0; y < _initFactor; y++) {
                    _openLocations.Add(new TileCoord(x, y));
                }
            }*/
        }

        public TileResourceCollection Tiles
        {
            get { return _tiles; }
            private set { _tiles = value; }
        }

        IResourceCollection<Tile> IResourceManager<Tile>.Items
        {
            get { return _tiles; }
        }

        #region Properties

        public Guid Uid { get; private set; }

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
            get { return _tiles.TextureId; }
        }

        public TextureResource TileSource
        {
            get { return _tiles.TextureResource; }
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

        /*IEnumerator IEnumerable.GetEnumerator ()
        {
            return _tiles.GetEnumerator();
        }

        public IEnumerator<Tile> GetEnumerator ()
        {
            return _tiles.GetEnumerator();
        }*/

        /*public Guid AddTile (TextureResource texture)
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

            //_manager.LinkItemKey(uid, this);

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

        public void Add (Tile tile)
        {
            throw new NotImplementedException();
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

            //_manager.UnlinkItemKey(uid);

            if (ShouldReduceTexture()) {
                ReduceTexture();
            }

            OnTileRemoved(new TileEventArgs(tile));
        }*/

        public Tile GetTile (Guid uid)
        {
            if (!_tiles.Contains(uid)) {
                return null;
            }

            return _tiles[uid];
        }

        /*public TextureResource GetTileTexture (Guid uid)
        {
            if (!_tiles.Contains(uid))
                return null;

            if (_tileSource == null)
                return null;

            Rectangle src = new Rectangle(_locations[uid].X * _tileWidth, _locations[uid].Y * _tileHeight, _tileWidth, _tileHeight);
            return _tileSource.Crop(src);
        }*/

        /*public void SetTileTexture (Guid uid, TextureResource texture)
        {
            if (texture.Width != _tileWidth || texture.Height != _tileHeight) {
                throw new ArgumentException("Supplied texture does not match tile dimensions", "data");
            }

            if (!_tiles.Contains(uid)) {
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
        }*/

        #region Texture Management

        /*private const int _minTiles = 4 * 4;

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
        }*/

        /*private void RecalculateOpenLocations ()
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
        }*/

        #endregion

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

        public void ImportMerge (TextureResource image, TileImportOptions options)
        {
            int tilesWide = (image.Width - options.MarginX) / (_tileWidth + options.SpaceX);
            int tilesHigh = (image.Height - options.MarginY) / (_tileHeight + options.SpaceY);

            Dictionary<string, Tile> existingHashes = new Dictionary<string, Tile>();
            Dictionary<string, Tile> newHashes = new Dictionary<string,Tile>();

            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider()) {
                foreach (Tile tile in _tiles) {
                    TextureResource tileTex = Tiles.GetTileTexture(tile.Uid);
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

                        Tile newTile = _tiles.Add(tileTex);

                        existingHashes[hash] = newTile;
                        newHashes[hash] = newTile;
                    }
                }
            }
        }

        /*public void ApplyTransparentColor (Color color)
        {
            _tileSource.Apply(c =>
            {
                if (color.R != c.R || color.G != c.G || color.B != c.B)
                    return c;
                else
                    return Colors.Transparent;
            });
        }*/

        /*public event EventHandler<KeyProviderEventArgs<string>> KeyChanging;
        public event EventHandler<KeyProviderEventArgs<string>> KeyChanged;

        protected virtual void OnKeyChanging (KeyProviderEventArgs<string> e)
        {
            if (KeyChanging != null)
                KeyChanging(this, e);
        }

        protected virtual void OnKeyChanged (KeyProviderEventArgs<string> e)
        {
            if (KeyChanged != null)
                KeyChanged(this, e);
        }

        public string GetKey ()
        {
            return Name;
        }*/

        public string Name
        {
            get { return _name; }
        }

        /*public bool SetName (string name)
        {
            if (_name != name) {
                KeyProviderEventArgs<string> e = new KeyProviderEventArgs<string>(_name, name);
                try {
                    OnKeyChanging(e);
                }
                catch (KeyProviderException) {
                    return false;
                }

                _name = name;
                OnKeyChanged(e);
                OnPropertyProviderNameChanged(EventArgs.Empty);
                RaisePropertyChanged("Name");
            }

            return true;
        }*/

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void RaisePropertyChanged (string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        #endregion

        #region INamedResource Members

        /*public string Name
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
        */

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            var ev = Modified;
            if (ev != null)
                ev(this, e);
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
            //SetName(property.Value);
        }

        public static LibraryX.TilePoolX ToXmlProxyX (TilePool pool)
        {
            if (pool == null)
                return null;

            List<LibraryX.TileDefX> tiledefs = new List<LibraryX.TileDefX>();
            foreach (Tile tile in pool._tiles)
                tiledefs.Add(TileResourceCollection.ToXmlProxyX(tile));

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in pool.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LibraryX.TilePoolX() {
                Uid = pool.Uid,
                Name = pool.Name,
                Texture = pool._textureId,
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

            //TilePool pool = manager.CreatePool(proxy.Name, proxy.TileWidth, proxy.TileHeight);
            //manager.TexturePool.RemoveResource(pool._textureId);
            TilePool pool = new TilePool(manager, proxy.Name, proxy.TileWidth, proxy.TileHeight) {
                Uid = proxy.Uid,
            };
            pool.Tiles = TileResourceCollection.FromXmlProxy(proxy, pool, manager.TexturePool);
            manager.Pools.Add(pool);

            //pool.Uid = proxy.Uid;
            //pool._textureId = proxy.Texture;
            //pool._tileSource = manager.TexturePool.GetResource(pool._textureId);

            /*if (pool._tileSource != null) {
                pool._tileSource.Apply(c => {
                    return (c.A == 0) ? Colors.Transparent : c;
                });
            }

            if (proxy.TileDefinitions != null) {
                foreach (var tiledef in proxy.TileDefinitions)
                    TileResourceCollection.FromXmlProxy(tiledef, pool._tiles);
            }*/

            //pool._tiles = TileResourceCollection.FromXmlProxy(proxy, pool, manager.TexturePool);

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    pool.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }

            //if (pool.Tiles.TextureResource != null)
            //    pool.Tiles.RecalculateOpenLocations();

            return pool;
        }

        /*public static LibraryX.TileDefX ToXmlProxyX (Tile tile)
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

            pool._manager.LinkItemKey(proxy.Uid, pool);

            return tile;
        }*/
    }
}
