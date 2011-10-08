using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
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
            _layers.Add(new MultiTileLayer(TileWidth, TileHeight, TilesWide, TilesHigh));
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
