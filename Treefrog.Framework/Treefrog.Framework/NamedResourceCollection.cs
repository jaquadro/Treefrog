using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class NamedResourceEventArgs<T> : EventArgs
        where T : INamedResource
    {
        public string Name { get; private set; }
        public T Resource { get; private set; }

        public NamedResourceEventArgs (T resource)
        {
            Name = resource.Name;
            Resource = resource;
        }

        public NamedResourceEventArgs (T resource, string name)
        {
            Name = name;
            Resource = resource;
        }
    }

    public class OrderedResourceEventArgs<T> : NamedResourceEventArgs<T>
        where T : INamedResource
    {
        public int Order { get; private set; }

        public OrderedResourceEventArgs (T resource, int order)
            : base(resource)
        {
            Order = order;
        }
    }

    public class NamedResourceCollection<T> : IEnumerable<T>
        where T : INamedResource
    {
        private Dictionary<string, T> _nameMap;

        public event EventHandler<NamedResourceEventArgs<T>> ResourceAdded;
        public event EventHandler<NamedResourceEventArgs<T>> ResourceRemoved;
        public event EventHandler<NamedResourceEventArgs<T>> ResourceRemapped;

        public NamedResourceCollection ()
        {
            _nameMap = new Dictionary<string, T>();
        }

        public int Count
        {
            get { return _nameMap.Count; }
        }

        public T this[string name]
        {
            get
            {
                if (!_nameMap.ContainsKey(name)) {
                    throw new ArgumentException("The collection does not contain an item keyed by the given name", "name");
                }

                return _nameMap[name];
            }
        }

        public void Add (T item)
        {
            if (_nameMap.ContainsKey(item.Name)) {
                throw new ArgumentException("The collection already contains an item keyed by the same Name", "item");
            }

            _nameMap[item.Name] = item;

            item.NameChanged += NameChangedHandler;

            OnResourceAdded(new NamedResourceEventArgs<T>(item));
        }

        public bool Contains (string name)
        {
            return name != null && _nameMap.ContainsKey(name);
        }

        public void Remove (string name)
        {
            T item;
            if (_nameMap.TryGetValue(name, out item)) {
                _nameMap.Remove(item.Name);

                item.NameChanged -= NameChangedHandler;

                OnResourceRemoved(new NamedResourceEventArgs<T>(item));
            }
        }

        private void NameChangedHandler (object sender, NameChangedEventArgs e)
        {
            if (!_nameMap.ContainsKey(e.OldName)) {
                throw new ArgumentException("The collection does not contain an item keyed by the old Name");
            }

            if (_nameMap.ContainsKey(e.NewName)) {
                throw new ArgumentException("There is an existing item with the new Name in the collection");
            }

            T item = _nameMap[e.OldName];

            _nameMap.Remove(e.OldName);
            _nameMap[e.NewName] = item;

            OnResourceRemapped(new NamedResourceEventArgs<T>(item, e.OldName));
        }

        protected virtual void OnResourceAdded (NamedResourceEventArgs<T> e)
        {
            if (ResourceAdded != null) {
                ResourceAdded(this, e);
            }
        }

        protected virtual void OnResourceRemoved (NamedResourceEventArgs<T> e)
        {
            if (ResourceRemoved != null) {
                ResourceRemoved(this, e);
            }
        }

        protected virtual void OnResourceRemapped (NamedResourceEventArgs<T> e)
        {
            if (ResourceRemapped != null) {
                ResourceRemapped(this, e);
            }
        }

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator ()
        {
            return _nameMap.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
