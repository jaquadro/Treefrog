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
        public string Name { get; private set; }

        public bool Visible { get; set; }

        public float Opacity { get; set; }

        public float LayerDepth { get; set; }

        public PropertyCollection Properties { get; private set; }

        internal Layer (ContentReader reader)
        {
            int id = reader.ReadInt16();
            Name = reader.ReadString();
            Visible = reader.ReadBoolean();
            Opacity = reader.ReadSingle();
            Properties = new PropertyCollection(reader);
        }

        public virtual void Draw (SpriteBatch spriteBatch, Rectangle region) { }
    }
}
