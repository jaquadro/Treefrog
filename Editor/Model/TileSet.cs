using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Editor.Model
{
    public abstract class TileSet : ITileSource, INamedResource
    {
        private static int _lastId = 0;

        private int _id;
        private string _name;

        protected int tileCount;
        protected TilePool _pool;

        protected TileSet ()
        {
            tileCount = 0;
        }

        protected TileSet (string name, TilePool pool)
            : this(++_lastId, name, pool)
        {
        }

        protected TileSet (int id, string name, TilePool pool)
            : this()
        {
            _name = name;
            _pool = pool;
        }

        public int Count
        {
            get { return tileCount; }
        }

        public int TileHeight
        {
            get { return _pool.TileHeight; }
        }

        public int TileWidth
        {
            get { return _pool.TileWidth; }
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<Tile> GetEnumerator ();

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

    public class TileSet1D : TileSet, ITileSource1D
    {
        private Tile[] _tiles;

        public TileSet1D (string name, TilePool pool, int capacity)
            : base(name, pool)
        {
            _tiles = new Tile[capacity];
        }

        public static TileSet1D CreatePoolSet (string name, TilePool pool)
        {
            TileSet1D set = new TileSet1D(name, pool, pool.Count);

            int index = 0;
            foreach (Tile tile in pool) {
                set._tiles[index] = tile;
                index++;
            }

            set.tileCount = pool.Count;

            return set;
        }

        public int Capacity
        {
            get { return _tiles.Length; }
        }

        public Tile this[int index]
        {
            get
            {
                if (index < 0 || index >= tileCount) {
                    throw new ArgumentOutOfRangeException("index", "The specified index is out of range.");
                }

                return _tiles[index];
            }

            set
            {
                if (index < 0 || index >= tileCount) {
                    throw new ArgumentOutOfRangeException("index", "The specified index is out of range.");
                }

                if (_tiles[index] == null && value != null) {
                    tileCount++;
                }
                else if (_tiles[index] != null && value == null) {
                    tileCount--;
                }

                _tiles[index] = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return Tiles.GetEnumerator();
        }

        public override IEnumerator<Tile> GetEnumerator ()
        {
            return Tiles.GetEnumerator();
        }

        private IEnumerable<Tile> Tiles
        {
            get
            {
                for (int i = 0; i < _tiles.Length; i++) {
                    if (_tiles[i] != null) {
                        yield return _tiles[i];
                    }
                }
            }
        }
    }

    public class TileSet2D : TileSet, ITileSource2D
    {
        private int _height;        // in tiles
        private int _width;         // in tiles

        private Tile[,] _tiles;

        public TileSet2D (string name, TilePool pool, int width, int height)
            : base(name, pool)
        {
            _width = width;
            _height = height;

            _tiles = new Tile[_height, _width];
        }

        public int TilesHigh
        {
            get { return _height; }
        }

        public int TilesWide
        {
            get { return _width; }
        }

        public int PixelsHigh
        {
            get { return _height * TileHeight; }
        }

        public int PixelsWide
        {
            get { return _width * TileWidth; }
        }

        public Tile this[TileCoord coord]
        {
            get { return this[coord.X, coord.Y]; }
            set { this[coord.X, coord.Y] = value; }
        }

        public Tile this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= _width) {
                    throw new ArgumentOutOfRangeException("x", "The index is out of range.");
                }
                else if (y < 0 || y >= _height) {
                    throw new ArgumentOutOfRangeException("y", "The index is out of range.");
                }

                return _tiles[y, x];
            }

            set
            {
                if (x < 0 || x >= _width) {
                    throw new ArgumentOutOfRangeException("x", "The index is out of range.");
                }
                else if (y < 0 || y >= _height) {
                    throw new ArgumentOutOfRangeException("y", "The index is out of range.");
                }

                if (_tiles[y, x] == null && value != null) {
                    tileCount++;
                }
                else if (_tiles[y, x] != null && value == null) {
                    tileCount--;
                }

                _tiles[y, x] = value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return Tiles.GetEnumerator();
        }

        public override IEnumerator<Tile> GetEnumerator ()
        {
            return Tiles.GetEnumerator();
        }

        private IEnumerable<Tile> Tiles
        {
            get
            {
                for (int y = 0; y < _height; y++) {
                    for (int x = 0; x < _width; x++) {
                        if (_tiles[y, x] == null) {
                            continue;
                        }
                        yield return _tiles[y, x];
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<TileCoord, Tile>> Region (Rectangle rect)
        {
            int xmin = Math.Max(rect.X, 0);
            int ymin = Math.Max(rect.Y, 0);
            int xmax = Math.Min(rect.X + rect.Width, _width);
            int ymax = Math.Min(rect.Y + rect.Height, _height);

            for (int y = ymin; y < ymax; y++) {
                for (int x = xmin; x < xmax; x++) {
                    if (_tiles[y, x] != null) {
                        yield return new KeyValuePair<TileCoord, Tile>(new TileCoord(x, y), _tiles[y, x]);
                    }
                }
            }
        }

        /*public static Tileset Import (Texture2D source, int tileWidth, int tileHeight, int spaceX, int spaceY, int marginX, int marginY)
        {
            return Import(source, tileWidth, tileHeight, spaceX, spaceY, marginX, marginY, false);
        }

        public static Tileset Import (Texture2D source, int tileWidth, int tileHeight, int spaceX, int spaceY, int marginX, int marginY, bool removeDuplicates)
        {
            Tileset set = new Tileset();

            set._tileSource = source;
            set._tileWidth = tileWidth;
            set._tileHeight = tileHeight;
            set._spaceX = spaceX;
            set._spaceY = spaceY;
            set._marginX = marginX;
            set._marginY = marginY;
            set._width = (source.Width - marginX) / (tileWidth + spaceX);
            set._height = (source.Height - marginY) / (tileHeight + spaceY);

            HashSet<string> hashes = new HashSet<string>();
            int last_id = 0;

            for (int y = 0; y < set._height; y++) {
                for (int x = 0; x < set._width; x++) {
                    Rectangle srcLoc = new Rectangle(marginX + x * (tileWidth + spaceX), marginY + y * (tileHeight + spaceY), tileWidth, tileHeight);
                    //Tile tile = new Tile(++last_id, source, srcLoc);
                    //set._tiles.Add(last_id, tile);
                }
            }

            return set;
        }*/
    }
}
