using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
    public class NamedResourceEventArgs<T> : EventArgs
        where T : INamedResource
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public T Resource { get; private set; }

        public NamedResourceEventArgs (T resource)
        {
            Id = resource.Id;
            Name = resource.Name;
            Resource = resource;
        }

        public NamedResourceEventArgs (T resource, int id, string name)
        {
            Id = id;
            Name = name;
            Resource = resource;
        }
    }

    public class NamedResourceCollection<T> : IEnumerable<T>
        where T : INamedResource
    {
        private Dictionary<int, T> _idMap;
        private Dictionary<string, T> _nameMap;

        public event EventHandler<NamedResourceEventArgs<T>> ResourceAdded;
        public event EventHandler<NamedResourceEventArgs<T>> ResourceRemoved;
        public event EventHandler<NamedResourceEventArgs<T>> ResourceRemapped;

        public NamedResourceCollection ()
        {
            _idMap = new Dictionary<int, T>();
            _nameMap = new Dictionary<string, T>();
        }

        public T this[int id]
        {
            get
            {
                if (!_idMap.ContainsKey(id)) {
                    throw new ArgumentException("The collection does not contain an item keyed by the given id", "id");
                }

                return _idMap[id];
            }
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
            if (_idMap.ContainsKey(item.Id)) {
                throw new ArgumentException("The collection already contains an item keyed by the same Id", "item");
            }

            if (_nameMap.ContainsKey(item.Name)) {
                throw new ArgumentException("The collection already contains an item keyed by the same Name", "item");
            }

            _idMap[item.Id] = item;
            _nameMap[item.Name] = item;

            item.IdChanged += IdChangedHandler;
            item.NameChanged += NameChangedHandler;

            OnResourceAdded(new NamedResourceEventArgs<T>(item));
        }

        public bool Contains (int id)
        {
            return _idMap.ContainsKey(id);
        }

        public bool Contains (string name)
        {
            return _nameMap.ContainsKey(name);
        }

        public void Remove (int id)
        {
            T item;
            if (_idMap.TryGetValue(id, out item)) {
                _nameMap.Remove(item.Name);
                _idMap.Remove(item.Id);

                item.IdChanged -= IdChangedHandler;
                item.NameChanged -= NameChangedHandler;

                OnResourceRemoved(new NamedResourceEventArgs<T>(item));
            }
        }

        public void Remove (string name)
        {
            T item;
            if (_nameMap.TryGetValue(name, out item)) {
                _nameMap.Remove(item.Name);
                _idMap.Remove(item.Id);

                item.IdChanged -= IdChangedHandler;
                item.NameChanged -= NameChangedHandler;

                OnResourceRemoved(new NamedResourceEventArgs<T>(item));
            }
        }

        private void IdChangedHandler (object sender, IdChangedEventArgs e)
        {
            if (!_idMap.ContainsKey(e.OldId)) {
                throw new ArgumentException("The collection does not contain an item keyed by the old Id");
            }

            if (_idMap.ContainsKey(e.NewId)) {
                throw new ArgumentException("There is an existing item with the new Id in the collection");
            }

            T item = _idMap[e.OldId];

            _idMap.Remove(e.OldId);
            _idMap[e.NewId] = item;

            OnResourceRemapped(new NamedResourceEventArgs<T>(item, e.OldId, item.Name));
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

            OnResourceRemapped(new NamedResourceEventArgs<T>(item, item.Id, e.OldName));
        }

        protected void OnResourceAdded (NamedResourceEventArgs<T> e)
        {
            if (ResourceAdded != null) {
                ResourceAdded(this, e);
            }
        }

        protected void OnResourceRemoved (NamedResourceEventArgs<T> e)
        {
            if (ResourceRemoved != null) {
                ResourceRemoved(this, e);
            }
        }

        protected void OnResourceRemapped (NamedResourceEventArgs<T> e)
        {
            if (ResourceRemapped != null) {
                ResourceRemapped(this, e);
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator ()
        {
            return _idMap.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return _idMap.Values.GetEnumerator();
        }

        #endregion
    }
}
