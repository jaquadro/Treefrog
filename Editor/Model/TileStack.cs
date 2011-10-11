using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
    public struct LocatedTileStack
    {
        private TileStack _tileStack;
        private TileCoord _tileCoord;

        public TileStack Stack
        {
            get { return _tileStack; }
        }

        public TileCoord Location
        {
            get { return _tileCoord; }
        }

        public int X
        {
            get { return _tileCoord.X; }
        }

        public int Y
        {
            get { return _tileCoord.Y; }
        }

        public LocatedTileStack (TileStack tileStack, int x, int y)
        {
            _tileStack = tileStack;
            _tileCoord = new TileCoord(x, y);
        }

        public LocatedTileStack (TileStack tileStack, TileCoord location)
        {
            _tileStack = tileStack;
            _tileCoord = location;
        }
    }

    public class TileStack : IEnumerable<Tile>
    {
        private List<Tile> _tiles;

        public TileStack ()
        {
            _tiles = new List<Tile>();
        }

        public TileStack (TileStack stack)
        {
            if (stack == null) {
                stack = new TileStack();
            }

            _tiles = new List<Tile>(stack._tiles);
        }

        public Tile this[int index]
        {
            get
            {
                if (index < 0 || index >= _tiles.Count) {
                    throw new ArgumentOutOfRangeException("index");
                }
                return _tiles[index];
            }
        }

        public int Count
        {
            get { return _tiles.Count; }
        }

        public Tile Top
        {
            get
            {
                if (_tiles.Count == 0) {
                    return null;
                }

                return _tiles[_tiles.Count - 1];
            }
        }

        public void Add (Tile tile)
        {
            _tiles.Add(tile);
        }

        public void Remove (Tile tile)
        {
            _tiles.Remove(tile);
        }

        public void Clear ()
        {
            _tiles.Clear();
        }

        #region IEnumerable<Tile> Members

        public IEnumerator<Tile> GetEnumerator ()
        {
            foreach (Tile t in _tiles) {
                yield return t;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
