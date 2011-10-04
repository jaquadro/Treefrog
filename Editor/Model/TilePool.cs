﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Collections;

namespace Editor.Model
{
    public enum ImportPolicy
    {
        ImprotAll,      // Import all tiles, including duplicate tiles in source
        SourceUnique,   // Import each unique tile in source
        SetUnique,      // Import each unique tile in source that is not already in set
    }

    public class TilePool : ITileSource, INamedResource
    {
        private const int _initFactor = 4;

        private static int _lastId = 0;

        private int _id;
        private string _name;

        private TileRegistry _registry;
        private Texture2D _tileSource;

        private int _tileWidth;
        private int _tileHeight;

        private Dictionary<int, Tile> _tiles;
        private Dictionary<int, TileCoord> _locations;
        private List<TileCoord> _openLocations;

        protected TilePool () { }

        public TilePool (string name, TileRegistry registry, int tileWidth, int tileHeight)
            : this(++_lastId, name, registry, tileWidth, tileHeight)
        {
        }

        public TilePool (int id, string name, TileRegistry registry, int tileWidth, int tileHeight)
            : this()
        {
            _id = id;
            _name = name;

            _registry = registry;
            _tiles = new Dictionary<int, Tile>();
            _locations = new Dictionary<int, TileCoord>();
            _openLocations = new List<TileCoord>();

            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            _tileSource = new Texture2D(_registry.GraphicsDevice, _tileWidth * _initFactor, _tileHeight * _initFactor, false, SurfaceFormat.Color);
            for (int x = 0; x < _initFactor; x++) {
                for (int y = 0; y < _initFactor; y++) {
                    _openLocations.Add(new TileCoord(x, y));
                }
            }
        }

        #region Properties

        public int Capacity
        {
            get { return _openLocations.Count; }
        }

        #endregion

        #region ITileSource Properties

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

        public int TilesWide
        {
            get { return _tileSource.Width / _tileWidth; }
        }

        public int TilesHigh
        {
            get { return _tileSource.Height / _tileHeight; }
        }

        public int PixelsWide
        {
            get { return _tileSource.Width; }
        }

        public int PixelsHigh
        {
            get { return _tileSource.Height; }
        }

        public bool EnforceSetDimensions
        {
            get { return false; }
        }

        #endregion

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

        public void DrawTile (SpriteBatch spritebatch, int id, Rectangle dest)
        {
            if (!_tiles.ContainsKey(id)) {
                return;
            }

            Rectangle src = new Rectangle(_locations[id].X * _tileWidth, _locations[id].Y * _tileHeight, _tileWidth, _tileHeight);
            spritebatch.Draw(_tileSource, dest, src, Color.White);
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
    }
}