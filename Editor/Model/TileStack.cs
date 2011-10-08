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

    public class TileStack
    {
        private List<Tile> _tiles;

        public TileStack ()
        {
            _tiles = new List<Tile>();
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

        public IEnumerable<Tile> Tiles
        {
            get
            {
                foreach (Tile tile in _tiles) {
                    yield return tile;
                }
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
    }
}
