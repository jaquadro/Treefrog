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
            TileWidth = tileWidth;
            TileHeight = tileHeight;

            _locations = new Dictionary<Guid, TileCoord>();
            _openLocations = new List<TileCoord>();

            _tileSource = new TextureResource(TileWidth * _initFactor, TileHeight * _initFactor);
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

        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

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

            Rectangle dest = new Rectangle(coord.X * TileWidth, coord.Y * TileHeight, TileWidth, TileHeight);
            _tileSource.Clear(dest);
            _texturePool.Invalidate(_textureId);

            _openLocations.Add(_locations[item.Uid]);
            _locations.Remove(item.Uid);

            if (ShouldReduceTexture())
                ReduceTexture();
        }

        public Tile Add (byte[] data)
        {
            if (data.Length != TileWidth * TileHeight * 4)
                throw new ArgumentException("Supplied color data is incorrect size for tile dimensions", "data");

            TextureResource texture = new TextureResource(TileWidth, TileHeight, data);
            return Add(texture);
        }

        public Tile Add (TextureResource texture)
        {
            if (texture.Width != TileWidth || texture.Height != TileHeight)
                throw new ArgumentException("Supplied texture does not have tile dimensions", "texture");

            Tile tile = new PhysicalTile() {
                Pool =_pool
            };

            if (ShouldExpandTexture())
                ExpandTexture();

            TileCoord coord = _openLocations[_openLocations.Count - 1];
            _openLocations.RemoveAt(_openLocations.Count - 1);

            _tileSource.Set(texture, new Point(coord.X * TileWidth, coord.Y * TileHeight));
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

            Rectangle src = new Rectangle(_locations[uid].X * TileWidth, _locations[uid].Y * TileHeight, TileWidth, TileHeight);
            return _tileSource.Crop(src);
        }

        public void SetTileTexture (Guid uid, TextureResource texture)
        {
            if (texture.Width != TileWidth || texture.Height != TileHeight) {
                throw new ArgumentException("Supplied texture does not match tile dimensions", "data");
            }

            if (!Contains(uid)) {
                throw new ArgumentException("No tile with the given id exists in this tile pool", "id");
            }

            Rectangle dest = new Rectangle(_locations[uid].X * TileWidth, _locations[uid].Y * TileHeight, TileWidth, TileHeight);
            _tileSource.Set(texture, new Point(_locations[uid].X * TileWidth, _locations[uid].Y * TileHeight));
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

            int factorX = newTex.Width / TileWidth;
            int factorY = newTex.Height / TileHeight;
            int threshX = _tileSource.Width / TileWidth;
            int threshY = _tileSource.Height / TileHeight;

            for (int y = 0; y < factorY; y++) {
                for (int x = 0; x < factorX; x++) {
                    if (x >= threshX || y >= threshY) {
                        _openLocations.Add(new TileCoord(x, y));
                    }
                }
            }

            _tileSource = newTex;
            _texturePool.RemoveResource(_textureId);
            _textureId = _texturePool.AddResource(_tileSource);
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

            int factorX = newTex.Width / TileWidth;
            int factorY = newTex.Height / TileHeight;

            _locations.Clear();
            _openLocations.Clear();

            for (int y = 0; y < factorY; y++) {
                for (int x = 0; x < factorX; x++) {
                    if (locs.Count == 0) {
                        _openLocations.Add(new TileCoord(x, y));
                        continue;
                    }

                    KeyValuePair<Guid, TileCoord> loc = locs.Dequeue();
                    Rectangle src = new Rectangle(loc.Value.X * TileWidth, loc.Value.Y * TileHeight, TileWidth, TileHeight);

                    newTex.Set(_tileSource.Crop(src), new Point(x * TileWidth, y * TileHeight));

                    _locations[loc.Key] = new TileCoord(x, y);
                }
            }

            _tileSource = newTex;
            _texturePool.RemoveResource(_textureId);
            _textureId = _texturePool.AddResource(_tileSource);
        }

        internal void RecalculateOpenLocations ()
        {
            _openLocations.Clear();

            int factorX = _tileSource.Width / TileWidth;
            int factorY = _tileSource.Height / TileHeight;

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

        private TileResourceCollection _tiles;

        private PropertyCollection _properties;
        private TilePoolProperties _predefinedProperties;

        protected TilePool ()
        {
            Uid = Guid.NewGuid();

            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new TilePool.TilePoolProperties(this);

            _properties.Modified += CustomProperties_Modified;
        }

        internal TilePool (TilePoolManager manager, string name, int tileWidth, int tileHeight)
            : this()
        {
            Name = name;

            _tiles = new TileResourceCollection(tileWidth, tileHeight, this, manager.TexturePool);
        }

        private TilePool (LibraryX.TilePoolX proxy, TilePoolManager manager)
            : this()
        {
            Uid = proxy.Uid;
            Name = proxy.Name;
            Tiles = TileResourceCollection.FromXmlProxy(proxy, this, manager.TexturePool);

            manager.Pools.Add(this);

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }
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
            get { return _tiles.TileWidth; }
        }

        public int TileHeight
        {
            get { return _tiles.TileHeight; }
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

        public Tile GetTile (Guid uid)
        {
            if (!_tiles.Contains(uid)) {
                return null;
            }

            return _tiles[uid];
        }

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
            int tilesWide = (image.Width - options.MarginX) / (TileWidth + options.SpaceX);
            int tilesHigh = (image.Height - options.MarginY) / (TileHeight + options.SpaceY);

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
                            options.MarginX + x * (TileWidth + options.SpaceX),
                            options.MarginY + y * (TileHeight + options.SpaceY),
                            TileWidth, TileHeight);

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
            private set { _name = value; }
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

        public static LibraryX.TilePoolX ToXProxy (TilePool pool)
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
                Texture = pool.Tiles.TextureId,
                TileWidth = pool.TileWidth,
                TileHeight = pool.TileHeight,
                TileDefinitions = tiledefs.Count > 0 ? tiledefs : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static TilePool FromXProxy (LibraryX.TilePoolX proxy, TilePoolManager manager)
        {
            if (proxy == null)
                return null;

            return new TilePool(proxy, manager);
        }
    }
}
