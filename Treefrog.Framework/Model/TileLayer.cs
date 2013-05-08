
using System;
namespace Treefrog.Framework.Model
{
    public abstract class TileLayer : Layer
    {
        private int _tileWidth;
        private int _tileHeight;

        protected TileLayer (string name, int tileWidth, int tileHeight)
            : base(name)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        protected TileLayer (string name, TileLayer layer)
            : base(name, layer)
        {
            _tileHeight = layer._tileHeight;
            _tileWidth = layer._tileWidth;
        }

        public override bool GridIsIndependent
        {
            get { return false; }
        }

        public override int GridWidth
        {
            get { return _tileWidth; }
            set { throw new NotSupportedException(); }
        }

        public override int GridHeight
        {
            get { return _tileHeight; }
            set { throw new NotSupportedException(); }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }
    }
}
