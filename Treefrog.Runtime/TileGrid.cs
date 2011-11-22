using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Runtime
{
    public class TileGrid
    {
        private readonly TileStack[,] _tiles;

        internal TileGrid (int width, int height)
        {
            _tiles = new TileStack[height, width];
            Width = width;
            Height = height;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public TileStack this[int x, int y]
        {
            get { return _tiles[y, x]; }
            set { _tiles[y, x] = value; }
        }
    }
}
