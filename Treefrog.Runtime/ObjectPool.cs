using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
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
}
