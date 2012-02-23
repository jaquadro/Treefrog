using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model.Support
{
    public struct LocatedTile
    {
        private Tile _tile;
        private TileCoord _tileCoord;

        public Tile Tile
        {
            get { return _tile; }
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

        public LocatedTile (Tile tile, int x, int y)
        {
            _tile = tile;
            _tileCoord = new TileCoord(x, y);

        }

        public LocatedTile (Tile tile, TileCoord location)
        {
            _tile = tile;
            _tileCoord = location;
        }
    }
}
