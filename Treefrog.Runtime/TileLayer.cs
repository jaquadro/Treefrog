using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Runtime
{
    public class ObjectInstance
    {
        private ObjectPool _objectPool;

        public ObjectInstance (ObjectPool objectPool, int id, int x, int y)
        {
            _objectPool = objectPool;
            Id = id;
            X = x;
            Y = y;
        }

        public int Id { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }

        public ObjectPool ObjectPool 
        {
            get { return _objectPool; }
        }

        public ObjectClass ObjectClass
        {
            get { return _objectPool.ObjectClasses[Id]; }
        }
    }

    public class ObjectLayer : Layer
    {
        private ObjectRegistry _registry;
        private List<ObjectInstance> _objects;

        internal ObjectLayer (ContentReader reader, ObjectRegistry registry)
            : base(reader)
        {
            _registry = registry;
            _objects = new List<ObjectInstance>();

            int objCount = reader.ReadInt32();
            for (int i = 0; i < objCount; i++) {
                int dx = reader.ReadInt32();
                int dy = reader.ReadInt32();
                int id = reader.ReadInt16();

                _objects.Add(new ObjectInstance(_registry.GetObjectPool(id), id, dx, dy));
            }
        }

        public List<ObjectInstance> Objects
        {
            get { return _objects; }
        }
    }

    public class TileLayer : Layer
    {
        private TileRegistry _registry;
        private TileGrid _tiles;

        public int TileHeight { get; private set; }

        public int TileWidth { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public TileGrid Tiles
        {
            get { return _tiles; }
        }

        internal TileLayer (ContentReader reader, TileRegistry registry)
            : base(reader)
        {
            _registry = registry;

            TileWidth = reader.ReadInt16();
            TileHeight = reader.ReadInt16();
            Width = reader.ReadInt16();
            Height = reader.ReadInt16();

            _tiles = new TileGrid(Width, Height);

            int stackCount = reader.ReadInt32();
            for (int i = 0; i < stackCount; i++) {
                int dx = reader.ReadInt16();
                int dy = reader.ReadInt16();

                int tcount = reader.ReadInt16();
                Tile[] st = new Tile[tcount];

                for (int j = 0; j < tcount; j++) {
                    st[j] = _registry[reader.ReadInt16()];
                }

                _tiles[dx, dy] = new TileStack(st);
            }
        }

        public override void Draw (SpriteBatch spriteBatch, Rectangle region)
        {
            if (!Visible) {
                return;
            }

            float scaleTileW = TileWidth * ScaleX;
            float scaleTileH = TileHeight * ScaleY;
            int scaleTileWi = (int)scaleTileW;
            int scaleTileHi = (int)scaleTileH;

            int startX = region.X / scaleTileWi;
            int startY = region.Y / scaleTileHi;
            int endX = startX + (region.Width + scaleTileWi * 2 - 1) / scaleTileWi;
            int endY = startY + (region.Height + scaleTileHi * 2 - 1) / scaleTileHi;

            startX = Math.Max(startX, 0);
            startY = Math.Max(startY, 0);
            endX = Math.Min(endX, Width);
            endY = Math.Min(endY, Height);

            Color mixColor = Color.White * Opacity;

            for (int y = startY; y < endY; y++) {
                for (int x = startX; x < endX; x++) {
                    if (_tiles[x, y] == null)
                        continue;

                    
                    Rectangle dest = new Rectangle(
                        (int)(x * scaleTileW),
                        (int)(y * scaleTileH),
                        scaleTileWi,
                        scaleTileHi
                        );

                    _tiles[x, y].Draw(spriteBatch, dest, mixColor, LayerDepth);
                }
            }
        }
    }
}
