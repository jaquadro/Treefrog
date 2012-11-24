using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

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
}
