using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model.Support
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
}
