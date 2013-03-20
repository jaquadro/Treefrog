using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class TypeRegistry
    {
        private Dictionary<string, Type> _registry = new Dictionary<string, Type>();

        public TypeRegistry ()
        { }

        public object Create (string type)
        {
            Type t;
            if (!_registry.TryGetValue(type, out t)) {
                return null;
            }

            return Activator.CreateInstance(t);
        }

        public IEnumerable<Type> RegisteredTypes
        {
            get
            {
                foreach (KeyValuePair<string, Type> kv in _registry)
                    yield return kv.Value;
            }
        }

        public Type Lookup (string type)
        {
            Type inst;
            if (_registry.TryGetValue(type, out inst))
                return inst;
            else
                return null;
        }

        public virtual void Register (string name, Type type)
        {
            _registry[name] = type;
        }
    }

    public class TypeRegistry<T> : TypeRegistry
        where T : class
    {
        public new T Create (string type)
        {
            return base.Create(type) as T;
        }

        public override void Register (string name, Type type)
        {
            if (!type.IsSubclassOf(typeof(T)) && Array.IndexOf(type.GetInterfaces(), typeof(T)) == -1)
                throw new ArgumentException("Not a valid subtype");

            base.Register(name, type);
        }
    }
}
