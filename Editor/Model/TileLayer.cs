using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Editor.Model
{
    public abstract class TileLayer : Layer
    {
        #region Fields

        private int _tileWidth;
        private int _tileHeight;

        #endregion

        #region Constructors

        protected TileLayer (int tileWidth, int tileHeight)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        #endregion

        #region Properties

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        #endregion
    }
}
