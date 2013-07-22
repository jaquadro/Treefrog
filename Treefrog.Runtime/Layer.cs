using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Treefrog.Runtime
{
    public class Layer
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public bool Visible { get; set; }

        public float Opacity { get; set; }

        public float LayerDepth { get; set; }

        public PropertyCollection Properties { get; private set; }

        public float ScaleX { get; set; }

        public float ScaleY { get; set; }

        internal Layer (ContentReader reader)
        {
            ScaleX = 1f;
            ScaleY = 1f;

            Id = reader.ReadInt32();
            Name = reader.ReadString();
            Visible = reader.ReadBoolean();
            Opacity = reader.ReadSingle();
            int rasterMode = reader.ReadInt16();

            Properties = new PropertyCollection(reader);
        }

        public virtual void Draw (SpriteBatch spriteBatch, Rectangle region) { }
    }
}
