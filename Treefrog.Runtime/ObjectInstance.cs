using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Runtime
{
    public class ObjectInstance
    {
        private ObjectPool _objectPool;

        public ObjectInstance (ObjectPool objectPool, int id, int x, int y)
        {
            _objectPool = objectPool;
            Id = id;
            X = x;
            Y = y;
        }

        public int Id { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }

        public ObjectPool ObjectPool
        {
            get { return _objectPool; }
        }

        public ObjectClass ObjectClass
        {
            get { return _objectPool.ObjectClasses[Id]; }
        }
    }
}
