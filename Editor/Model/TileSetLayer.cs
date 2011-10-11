using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
    public class TileSetLayer : TileLayer
    {
        #region Fields

        TileSet1D _tileSet;

        private List<Tile> _index;

        #endregion

        #region Constructors

        public TileSetLayer (TileSet1D tileSet)
            : base(tileSet.TileWidth, tileSet.TileHeight)
        {
            _tileSet = tileSet;
            SyncIndex();
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return _tileSet.Count; }
        }

        public int Capacity
        {
            get { return _tileSet.Capacity; }
        }

        public Tile this[int index]
        {
            get
            {
                if (index < 0 || index >= _index.Count) {
                    throw new ArgumentOutOfRangeException("index");
                }

                return _index[index];
            }
        }

        #endregion

        #region Public API

        public virtual IEnumerable<Tile> Tiles
        {
            get { return _tileSet; }
        }

        #endregion

        private void SyncIndex ()
        {
            _index = new List<Tile>();

            foreach (Tile t in _tileSet) {
                _index.Add(t);
            }
        }
    }
}
