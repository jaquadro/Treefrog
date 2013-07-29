using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Treefrog.Runtime
{
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
                int id = reader.ReadInt32();
                int dx = reader.ReadInt32();
                int dy = reader.ReadInt32();
                float rotation = reader.ReadSingle();
                float scaleX = reader.ReadSingle();
                float scaleY = reader.ReadSingle();

                PropertyCollection properties = new PropertyCollection(reader);

                _objects.Add(new ObjectInstance(_registry.GetObjectPool(id), id, dx, dy) {
                    Rotation = rotation,
                    ScaleX = scaleX,
                    ScaleY = scaleY,
                    Properties = properties,
                });
            }
        }

        public List<ObjectInstance> Objects
        {
            get { return _objects; }
        }

        

        public override void Draw (SpriteBatch spriteBatch, Rectangle region)
        {
            if (!Visible)
                return;

            Color mixColor = Color.White * Opacity;

            foreach (ObjectInstance obj in _objects) {
                Rectangle objBounds = obj.Bounds;
                Rectangle dest = new Rectangle(
                    (int)(objBounds.X * ScaleX),
                    (int)(objBounds.Y * ScaleY),
                    (int)(objBounds.Width * ScaleX),
                    (int)(objBounds.Height * ScaleY));

                if (!region.Intersects(dest))
                    continue;

                obj.ObjectClass.Draw(spriteBatch, dest, mixColor, LayerDepth, obj.Rotation);
            }
        }
    }
}
