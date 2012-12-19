using System.Collections.Generic;
using System;

namespace Treefrog.Framework
{
    public class ObjectRegistry<T>
        where T : class
    {
        private Dictionary<string, T> _registry = new Dictionary<string, T>();

        public ObjectRegistry ()
        { }

        public IEnumerable<T> RegisteredObjects
        {
            get
            {
                foreach (KeyValuePair<string, T> kv in _registry)
                    yield return kv.Value;
            }
        }

        public T Lookup (string name)
        {
            T inst;
            if (_registry.TryGetValue(name, out inst))
                return inst;
            else
                return null;
        }

        public void Register (string name, T inst)
        {
            _registry[name] = inst;
        }
    }
}
