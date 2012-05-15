using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Runtime
{
    public class ObjectClass
    {
        private ObjectPool _objectPool;

        public ObjectClass (ObjectPool pool, int id, string name)
        {
            _objectPool = pool;
            Id = id;
            Name = name;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public PropertyCollection Properties { get; internal set; }
    }

    public class ObjectPool
    {
        private ContentManager _manager;
        private Dictionary<int, ObjectClass> _objects;

        internal ObjectPool ()
        {
            _objects = new Dictionary<int, ObjectClass>();
        }

        internal ObjectPool (ContentReader reader)
            : this()
        {
            _manager = reader.ContentManager;

            int version = reader.ReadInt16();
            int id = reader.ReadInt16();

            Properties = new PropertyCollection(reader);

            int objCount = reader.ReadInt16();
            for (int i = 0; i < objCount; i++) {
                int objId = reader.ReadInt16();
                string name = reader.ReadString();

                ObjectClass objClass = new ObjectClass(this, objId, name);
                _objects.Add(objId, objClass);
            }
        }

        public PropertyCollection Properties { get; private set; }

        public Dictionary<int, ObjectClass> ObjectClasses
        {
            get { return _objects; }
        }
    }

    public class Tileset
    {
        private ContentManager _manager;

        private Dictionary<int, Tile> _tiles;
        private Texture2D _texture;

        internal Tileset ()
        {
            _tiles = new Dictionary<int, Tile>();
        }

        internal Tileset (ContentReader reader)
            : this()
        {
            _manager = reader.ContentManager;

            int version = reader.ReadInt16();
            int id = reader.ReadInt16();

            TileWidth = reader.ReadInt16();
            TileHeight = reader.ReadInt16();
            string texAsset = reader.ReadString();

            Properties = new PropertyCollection(reader);

            int tileCount = reader.ReadInt16();
            for (int i = 0; i < tileCount; i++) {
                int tileId = reader.ReadInt16();
                int tileX = reader.ReadInt16();
                int tileY = reader.ReadInt16();

                Tile tile = new Tile(tileId, this, tileX, tileY)
                {
                    Properties = new PropertyCollection(reader),
                };
                _tiles.Add(tileId, tile);
            }

            _texture = _manager.Load<Texture2D>(texAsset);
        }

        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        public PropertyCollection Properties { get; private set; }

        public Dictionary<int, Tile> Tiles
        {
            get { return _tiles; }
        }

        public Texture2D Texture
        {
            get { return _texture; }
        }
    }
}
