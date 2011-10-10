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

        #endregion

        #region Constructors

        public TileSetLayer (TileSet1D tileSet)
            : base(tileSet.TileWidth, tileSet.TileHeight)
        {
            _tileSet = tileSet;
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

        #endregion

        #region Public API

        public virtual IEnumerable<Tile> Tiles
        {
            get { return _tileSet; }
        }

        #endregion
    }
}
