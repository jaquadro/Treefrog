using System.Collections.Generic;
using System;
using System.Collections;

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

    public class InstanceRegistryEventArgs<T> : EventArgs
    {
        public Type Type { get; private set; }
        public T Instance { get; private set; }

        public InstanceRegistryEventArgs (Type type, T instance)
        {
            Type = type;
            Instance = instance;
        }
    }

    public class InstanceRegistry<T> : IEnumerable<KeyValuePair<Type, T>>
        where T : class
    {
        private Dictionary<Type, T> _registry = new Dictionary<Type, T>();

        public InstanceRegistry ()
        { }

        public event EventHandler<InstanceRegistryEventArgs<T>> InstanceRegistered;
        public event EventHandler<InstanceRegistryEventArgs<T>> InstanceUnregistered;

        protected virtual void OnInstanceRegistered (InstanceRegistryEventArgs<T> e)
        {
            var ev = InstanceRegistered;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnInstanceUnregistered (InstanceRegistryEventArgs<T> e)
        {
            var ev = InstanceUnregistered;
            if (ev != null)
                ev(this, e);
        }

        public IEnumerable<T> RegisteredInstances
        {
            get { return _registry.Values; }
        }

        public IEnumerable<Type> RegisteredTypes
        {
            get { return _registry.Keys; }
        }

        public T Lookup (Type type)
        {
            T inst;
            if (_registry.TryGetValue(type, out inst))
                return inst;
            else
                return null;
        }

        public TKey Lookup<TKey> ()
            where TKey : class, T
        {
            return Lookup(typeof(TKey)) as TKey;
        }

        public void Register (Type type, T inst)
        {
            if (!type.IsSubclassOf(typeof(T)))
                throw new ArgumentException("Type " + type + " is not a subclass of " + typeof(T));

            Unregister(type);

            _registry[type] = inst;
            OnInstanceRegistered(new InstanceRegistryEventArgs<T>(type, inst));
        }

        public void Register<TKey> (TKey inst)
            where TKey : class, T
        {
            Register(typeof(TKey), inst);
        }

        public void Register (T inst)
        {
            Register(inst.GetType(), inst);
        }

        public void Unregister (Type type)
        {
            if (_registry.ContainsKey(type)) {
                T oldInst = _registry[type];
                _registry.Remove(type);

                OnInstanceUnregistered(new InstanceRegistryEventArgs<T>(type, oldInst));
            }
        }

        public void Unregister<TKey> ()
            where TKey : class, T
        {
            Unregister(typeof(TKey));
        }

        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator ()
        {
            foreach (var kv in _registry)
                yield return kv;
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }
    }
}
