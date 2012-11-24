using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace Treefrog.Framework.Model
{
    public class TileRegistry
    {
        private int _lastId = 0;

        private GraphicsDevice _device;

        private Dictionary<string, TilePool> _pools;
        private Dictionary<int, TilePool> _tileIndex;


        #region Constructors

        public TileRegistry ()
        {
            _pools = new Dictionary<string, TilePool>();
            _tileIndex = new Dictionary<int, TilePool>();
        }

        public TileRegistry (GraphicsDevice device)
        {
            _device = device;
            _pools = new Dictionary<string, TilePool>();
            _tileIndex = new Dictionary<int, TilePool>();
        }

        #endregion

        #region Properties

        public int TileCount
        {
            get { return _tileIndex.Count; }
        }

        #endregion

        public void Initialize (GraphicsDevice device)
        {
            if (_device != null)
                throw new InvalidOperationException("GraphicsDevice already set");

            _device = device;
            OnGrpahicsDeviceInitialized(EventArgs.Empty);
        }

        public event EventHandler GraphicsDeviceInitialized = (s, e) => { };

        protected virtual void OnGrpahicsDeviceInitialized (EventArgs e)
        {
            GraphicsDeviceInitialized(this, e);
        }

        public TilePool PoolFromTileId (int id)
        {
            return _tileIndex[id];
        }

        public void Reset ()
        {
            _lastId = 0;
            _pools = new Dictionary<string, TilePool>();
            _tileIndex = new Dictionary<int, TilePool>();
        }

        internal int LastId
        {
            get { return _lastId; }
            set { _lastId = value; }
        }

        internal int TakeId ()
        {
            _lastId++;
            return _lastId;
        }

        internal void LinkTile (int id, TilePool pool) {
            _tileIndex[id] = pool;
        }

        internal void UnlinkTile (int id)
        {
            _tileIndex.Remove(id);
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return _device; }
        }
    }
}
