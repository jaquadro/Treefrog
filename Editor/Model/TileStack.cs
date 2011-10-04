using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
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
