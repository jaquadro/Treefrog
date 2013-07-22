using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Treefrog.Runtime
{
    public class TileStack : IEnumerable<Tile>
    {
        private readonly Tile[] _tiles;

        internal TileStack (Tile tile)
        {
            _tiles = new Tile[1] { tile };
        }

        internal TileStack (TileStack tiles, Tile tile)
        {
            _tiles = new Tile[tiles.Count + 1];
            tiles._tiles.CopyTo(_tiles, 0);
            _tiles[_tiles.Length - 1] = tile;
        }

        internal TileStack (ICollection<Tile> tiles)
        {
            _tiles = new Tile[tiles.Count];
            tiles.CopyTo(_tiles, 0);
        }

        public int Count
        {
            get { return _tiles.Length; }
        }

        public IEnumerator<Tile> GetEnumerator ()
        {
            for (int i = 0; i < _tiles.Length; i++) {
                yield return _tiles[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        public void Draw (SpriteBatch spriteBatch, Rectangle dest, float opacity, float layerDepth)
        {
            for (int i = 0; i < _tiles.Length; i++) {
                _tiles[i].Draw(spriteBatch, dest, opacity, layerDepth);
            }
        }

        public void Draw (SpriteBatch spriteBatch, Rectangle dest, Color color, float layerDepth)
        {
            for (int i = 0; i < _tiles.Length; i++) {
                _tiles[i].Draw(spriteBatch, dest, color, layerDepth);
            }
        }
    }
}
