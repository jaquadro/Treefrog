using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class NamedResourceCollection<T> : IEnumerable<T>
        where T : INamedResource
    {
        private Dictionary<string, T> _nameMap;

        public event EventHandler<NamedResourceEventArgs<T>> ResourceAdded = (s, e) => { };
        public event EventHandler<NamedResourceEventArgs<T>> ResourceRemoved = (s, e) => { };
        public event EventHandler<NamedResourceEventArgs<T>> ResourceModified = (s, e) => { };
        public event EventHandler<NamedResourceRemappedEventArgs<T>> ResourceRemapped = (s, e) => { };

        public event EventHandler Modified = (s, e) => { };

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
                    throw new ArgumentException("The collection does not contain an item keyed by the given name: '" + name + "'", "name");
                }

                return _nameMap[name];
            }
        }

        public void Add (T item)
        {
            if (!CheckAdd(item)) {
                return;
            }

            if (_nameMap.ContainsKey(item.Name)) {
                throw new ArgumentException("The collection already contains an item keyed by the same name: '" + item.Name + "'", "item");
            }

            _nameMap[item.Name] = item;

            item.NameChanging += NameChangingHandler;
            item.NameChanged += NameChangedHandler;
            item.Modified += ResourceModifiedHandler;

            OnResourceAdded(new NamedResourceEventArgs<T>(item));
        }

        public bool Contains (string name)
        {
            return name != null && _nameMap.ContainsKey(name);
        }

        public void Remove (string name)
        {
            if (!CheckRemove(name)) {
                return;
            }

            T item;
            if (_nameMap.TryGetValue(name, out item)) {
                _nameMap.Remove(item.Name);

                item.NameChanging -= NameChangingHandler;
                item.NameChanged -= NameChangedHandler;
                item.Modified -= ResourceModifiedHandler;

                OnResourceRemoved(new NamedResourceEventArgs<T>(item));
            }
        }

        public void Clear ()
        {
            string[] keys = new string[_nameMap.Count];
            _nameMap.Keys.CopyTo(keys, 0);

            foreach (string key in keys) {
                T resource = _nameMap[key];
                _nameMap.Remove(key);
                OnResourceRemoved(new NamedResourceEventArgs<T>(resource));
            }

            OnModified(EventArgs.Empty);
        }

        protected virtual bool CheckAdd (T item)
        {
            return true;
        }

        protected virtual bool CheckRemove (string name)
        {
            return true;
        }

        private void NameChangingHandler (object sender, NameChangingEventArgs e)
        {
            if (!_nameMap.ContainsKey(e.OldName)) {
                e.Cancel = true;
            }

            if (_nameMap.ContainsKey(e.NewName)) {
                e.Cancel = true;
            }
        }

        private void NameChangedHandler (object sender, NameChangedEventArgs e)
        {
            if (!_nameMap.ContainsKey(e.OldName)) {
                throw new ArgumentException("The collection does not contain an item keyed by the old name: '" + e.OldName + "'");
            }

            if (_nameMap.ContainsKey(e.NewName)) {
                throw new ArgumentException("There is an existing item with the new name '" + e.NewName + "' in the collection");
            }

            T item = _nameMap[e.OldName];

            _nameMap.Remove(e.OldName);
            _nameMap[e.NewName] = item;

            OnResourceRemapped(new NamedResourceRemappedEventArgs<T>(item, e.OldName, e.NewName));
        }

        protected virtual void OnResourceAdded (NamedResourceEventArgs<T> e)
        {
            ResourceAdded(this, e);
            //OnModified(EventArgs.Empty);
        }

        protected virtual void OnResourceRemoved (NamedResourceEventArgs<T> e)
        {
            ResourceRemoved(this, e);
            //OnModified(EventArgs.Empty);
        }

        protected virtual void OnResourceModified (NamedResourceEventArgs<T> e)
        {
            ResourceModified(this, e);
            //OnModified(EventArgs.Empty);
        }

        protected virtual void OnResourceRemapped (NamedResourceRemappedEventArgs<T> e)
        {
            ResourceRemapped(this, e);
            //OnModified(EventArgs.Empty);
        }

        protected virtual void OnModified (EventArgs e)
        {
            Modified(this, e);
        }

        private void ResourceModifiedHandler (object sender, EventArgs e)
        {
            NamedResourceEventArgs<T> args = new NamedResourceEventArgs<T>((T)sender);
            OnResourceModified(args);
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
