using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

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

            //int version = reader.ReadInt16();
            int id = reader.ReadInt32();
            string texAsset = reader.ReadString();

            Properties = new PropertyCollection(reader);

            int objCount = reader.ReadInt32();
            for (int i = 0; i < objCount; i++) {
                int objId = reader.ReadInt32();
                string name = reader.ReadString();

                ObjectClass objClass = new ObjectClass(this, objId, name) {
                    Origin = new Point(reader.ReadInt32(), reader.ReadInt32()),
                    MaskBounds = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
                    TexRotated = reader.ReadBoolean(),
                    TexX = reader.ReadInt32(),
                    TexY = reader.ReadInt32(),
                    TexWidth = reader.ReadInt32(),
                    TexHeight = reader.ReadInt32(),
                    TexOriginalWidth = reader.ReadInt32(),
                    TexOriginalHeight = reader.ReadInt32(),
                    TexOffsetX = reader.ReadInt32(),
                    TexOffsetY = reader.ReadInt32(),
                    Properties = new PropertyCollection(reader),
                };

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
