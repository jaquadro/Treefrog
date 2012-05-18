using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
