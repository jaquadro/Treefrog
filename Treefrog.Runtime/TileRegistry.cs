using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Runtime
{
    public class TileRegistry
    {
        private Dictionary<int, Tileset> _registry;
        private Dictionary<int, Tile> _tileRegistry;

        internal TileRegistry ()
        {
            _registry = new Dictionary<int, Tileset>();
            _tileRegistry = new Dictionary<int, Tile>();
        }

        public Tile this[int id]
        {
            get { return _tileRegistry[id]; }
        }

        public Tileset GetTileset (int id)
        {
            return _registry[id];
        }

        public void Add (Tileset tileset)
        {
            foreach (Tile tile in tileset.Tiles.Values) {
                _registry[tile.Id] = tileset;
                _tileRegistry[tile.Id] = tile;
            }
        }
    }

    public class ObjectRegistry
    {

        //private Dictionary<int, ObjectClass> _objectRegistry;
    }
}
