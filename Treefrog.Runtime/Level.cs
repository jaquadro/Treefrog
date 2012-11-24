using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Treefrog.Runtime
{
    public class Level
    {
        private TileRegistry _tileRegistry;
        private ObjectRegistry _objectRegistry;
        private List<Layer> _layers;

        /// <summary>
        /// The height of the level (in pixels)
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The width of the level (in pixels)
        /// </summary>
        public int Width { get; private set; }

        public PropertyCollection Properties { get; private set; }

        //public IEnumerable<Tile> Tiles { get; }

        private float _scaleX;
        public float ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
                foreach (Layer layer in _layers) {
                    layer.ScaleX = value;
                }
            }
        }

        private float _scaleY;
        public float ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
                foreach (Layer layer in _layers) {
                    layer.ScaleY = value;
                }
            }
        }

        public List<Layer> Layers
        {
            get { return _layers; }
        }

        internal Level ()
        {
            _tileRegistry = new TileRegistry();
            _objectRegistry = new ObjectRegistry();
            _layers = new List<Layer>();
        }

        internal Level (ContentReader reader)
            : this()
        {
            int version = reader.ReadInt16();

            int tileWidth = reader.ReadInt16();
            int tileHeight = reader.ReadInt16();
            int width = reader.ReadInt16();
            int height = reader.ReadInt16();

            Width = tileWidth * width;
            Height = tileHeight * height;

            Properties = new PropertyCollection(reader);

            int tilesetCount = reader.ReadInt32();
            for (int i = 0; i < tilesetCount; i++) {
                string asset = reader.ReadString();
                Tileset tileset = reader.ContentManager.Load<Tileset>(asset);

                _tileRegistry.Add(tileset);
            }

            int objectPoolCount = reader.ReadInt32();
            for (int i = 0; i < objectPoolCount; i++) {
                string asset = reader.ReadString();
                ObjectPool pool = reader.ContentManager.Load<ObjectPool>(asset);

                _objectRegistry.Add(pool);
            }

            int layerCount = reader.ReadInt16();
            for (int i = 0; i < layerCount; i++) {
                string type = reader.ReadString();

                switch (type) {
                    case "TILE":
                        _layers.Add(new TileLayer(reader, _tileRegistry));
                        break;
                    case "OBJE":
                        _layers.Add(new ObjectLayer(reader, _objectRegistry));
                        break;
                }
            }
        }

        public void Draw (SpriteBatch spriteBatch, Rectangle region)
        {
            foreach (Layer layer in _layers) {
                layer.Draw(spriteBatch, region);
            }
        }

        public void DrawLayer (SpriteBatch spriteBatch, Rectangle region, int index)
        {
            if (index >= 0 && index < _layers.Count) {
                _layers[index].Draw(spriteBatch, region);
            }
        }
    }
}
