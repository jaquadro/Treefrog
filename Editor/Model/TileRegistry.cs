﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace Editor.Model
{
    public class TileRegistry
    {
        private int _lastId = 0;

        private GraphicsDevice _device;

        private Dictionary<string, TilePool> _pools;
        private Dictionary<int, TilePool> _tileIndex;

        public TileRegistry (GraphicsDevice device)
        {
            _device = device;
            _pools = new Dictionary<string, TilePool>();
            _tileIndex = new Dictionary<int, TilePool>();
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

        internal GraphicsDevice GraphicsDevice
        {
            get { return _device; }
        }
    }
}