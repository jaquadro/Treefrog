using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
    public class Layer
    {
        private float _opacity;
        private bool _visible;
    }

    public class TileLayer : Layer
    {
        private int _tilesWide;
        private int _tilesHigh;

        private TileStack[,] _tiles;

        public TileLayer (int tilesWide, int tilesHigh)
        {
            _tilesWide = tilesWide;
            _tilesHigh = tilesHigh;

            _tiles = new TileStack[tilesHigh, tilesWide];
        }

        public IEnumerable<Tile> TilesAt (int x, int y)
        {
            if (_tiles[y, x] == null) {
                yield break;
            }

            foreach (Tile tile in _tiles[y, x].Tiles) {
                yield return tile;
            }
        }

        public void AddTile (int x, int y, Tile tile)
        {
            if (!CheckTile(x, y))
                return;

            _tiles[y, x].Remove(tile);
            _tiles[y, x].Add(tile);
        }

        public void RemoveTile (int x, int y, Tile tile)
        {
            if (!CheckTile(x, y))
                return;

            _tiles[y, x].Remove(tile);
        }

        public void ClearTile (int x, int y)
        {
            if (!CheckTile(x, y))
                return;

            _tiles[y, x].Clear();
        }

        private bool CheckTile (int x, int y)
        {
            if (!CheckBounds(x, y)) {
                return false;
            }

            if (_tiles[y, x] == null) {
                _tiles[y, x] = new TileStack();
            }

            return true;
        }

        private bool CheckBounds (int x, int y)
        {
            return x >= 0 &&
                y >= 0 &&
                x < _tilesWide &&
                y < _tilesHigh;
        }

    }
}
