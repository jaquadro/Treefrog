using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Point Origin { get; set; }
        public Rectangle MaskBounds { get; set; }

        public bool TexRotated { get; set; }
        public int TexX { get; set; }
        public int TexY { get; set; }
        public int TexWidth { get; set; }
        public int TexHeight { get; set; }
        public int TexOriginalWidth { get; set; }
        public int TexOriginalHeight { get; set; }
        public int TexOffsetX { get; set; }
        public int TexOffsetY { get; set; }

        public Rectangle Source
        {
            get { return new Rectangle(TexX, TexY, TexWidth, TexHeight); }
        }

        public void Draw (SpriteBatch spriteBatch, Rectangle dest, Color color, float layerDepth, float rotation)
        {
            spriteBatch.Draw(_objectPool.Texture, 
                dest, Source, 
                color, 
                rotation, new Vector2(Origin.X, Origin.Y), 
                SpriteEffects.None, layerDepth);
        }
    }
}
