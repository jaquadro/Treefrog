using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Runtime
{
    public class ObjectRegistry
    {
        private Dictionary<int, ObjectPool> _registry;
        private Dictionary<int, ObjectClass> _objectRegistry;

        internal ObjectRegistry ()
        {
            _registry = new Dictionary<int, ObjectPool>();
            _objectRegistry = new Dictionary<int, ObjectClass>();
        }

        public ObjectClass this[int id]
        {
            get { return _objectRegistry[id]; }
        }

        public ObjectPool GetObjectPool (int id)
        {
            return _registry[id];
        }

        public void Add (ObjectPool objectPool)
        {
            foreach (ObjectClass objClass in objectPool.ObjectClasses.Values) {
                _registry[objClass.Id] = objectPool;
                _objectRegistry[objClass.Id] = objClass;
            }
        }
    }
}
