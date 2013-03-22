using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class NamedResourceCollection<T> : ResourceCollection<T>
        where T : INamedResource
    {
        private Dictionary<string, T> _indexMap;

        public event EventHandler<NamedResourceRemappedEventArgs<T>> ResourceRenamed;

        public NamedResourceCollection ()
        {
            _indexMap = new Dictionary<string, T>();
        }

        public T this[string name]
        {
            get
            {
                if (!_indexMap.ContainsKey(name))
                    throw new ArgumentException("The collection does not contain an item named '" + name + "'", "name");

                return _indexMap[name];
            }
        }

        public bool Contains (string name)
        {
            return _indexMap.ContainsKey(name);
        }

        protected override bool CheckAdd (T item)
        {
            if (_indexMap.ContainsKey(item.Name))
                return false;

            return base.CheckAdd(item);
        }

        protected override void AddCore (T item)
        {
            _indexMap.Add(item.Name, item);

            base.AddCore(item);
        }

        protected override void RemoveCore (T item)
        {
            base.RemoveCore(item);

            _indexMap.Remove(item.Name);
        }

        protected virtual void OnResourceRenamed (NamedResourceRemappedEventArgs<T> e)
        {
            var ev = ResourceRenamed;
            if (ev != null)
                ev(this, e);
        }

        private void NameChangingHandler (object sender, NameChangingEventArgs e)
        {
            if (!_indexMap.ContainsKey(e.OldName)) {
                e.Cancel = true;
            }

            if (_indexMap.ContainsKey(e.NewName)) {
                e.Cancel = true;
            }
        }

        private void NameChangedHandler (object sender, NameChangedEventArgs e)
        {
            if (!_indexMap.ContainsKey(e.OldName)) {
                throw new ArgumentException("The collection does not contain an item keyed by the old name: '" + e.OldName + "'");
            }

            if (_indexMap.ContainsKey(e.NewName)) {
                throw new ArgumentException("There is an existing item with the new name '" + e.NewName + "' in the collection");
            }

            T item = _indexMap[e.OldName];

            _indexMap.Remove(e.OldName);
            _indexMap[e.NewName] = item;

            OnResourceRenamed(new NamedResourceRemappedEventArgs<T>(item, e.OldName, e.NewName));
        }
    }

    public class ResourceCollection<T> : IEnumerable<T>
        where T : IResource
    {
        private Dictionary<Guid, T> _resourceMap;

        public event EventHandler<ResourceEventArgs<T>> ResourceAdded;
        public event EventHandler<ResourceEventArgs<T>> ResourceRemoved;
        public event EventHandler<ResourceEventArgs<T>> ResourceModified;

        public event EventHandler Modified;

        public ResourceCollection ()
        {
            _resourceMap = new Dictionary<Guid, T>();
        }

        public int Count
        {
            get { return _resourceMap.Count; }
        }

        public T this[Guid uid]
        {
            get 
            {
                if (!_resourceMap.ContainsKey(uid))
                    throw new ArgumentException("The collection does not contain an item keyed by uid: '" + uid + "'", "uid");

                return _resourceMap[uid];
            }
        }

        public bool Add (T item)
        {
            if (!CheckAdd(item))
                return false;

            if (_resourceMap.ContainsKey(item.Uid))
                throw new ArgumentException("The collection already contains an item keyed by uid: '" + item.Uid + "'", "uid");

            _resourceMap[item.Uid] = item;

            item.Modified += ResourceModifiedHandler;

            AddCore(item);

            OnResourceAdded(new ResourceEventArgs<T>(item));
            OnModified(EventArgs.Empty);

            return true;
        }

        protected virtual void AddCore (T item)
        { }

        public bool Contains (Guid uid)
        {
            return _resourceMap.ContainsKey(uid);
        }

        public bool Remove (Guid uid)
        {
            T item;
            if (_resourceMap.TryGetValue(uid, out item)) {
                if (!CheckRemove(item))
                    return false;

                RemoveCore(item);

                _resourceMap.Remove(item.Uid);

                item.Modified -= ResourceModifiedHandler;

                OnResourceRemoved(new ResourceEventArgs<T>(item));
                OnModified(EventArgs.Empty);

                return true;
            }

            return false;
        }

        protected virtual void RemoveCore (T item)
        { }

        public void Clear ()
        {
            if (_resourceMap.Count == 0)
                return;

            Guid[] keys = new Guid[_resourceMap.Count];
            _resourceMap.Keys.CopyTo(keys, 0);

            foreach (Guid key in keys) {
                T resource = _resourceMap[key];
                _resourceMap.Remove(key);
                OnResourceRemoved(new ResourceEventArgs<T>(resource));
            }

            OnModified(EventArgs.Empty);
        }

        protected virtual void OnResourceAdded (ResourceEventArgs<T> e)
        {
            var ev = ResourceAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnResourceRemoved (ResourceEventArgs<T> e)
        {
            var ev = ResourceRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnResourceModified (ResourceEventArgs<T> e)
        {
            var ev = ResourceModified;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnModified (EventArgs e)
        {
            var ev = Modified;
            if (ev != null)
                ev(this, e);
        }

        private void ResourceModifiedHandler (object sender, EventArgs e)
        {
            ResourceEventArgs<T> args = new ResourceEventArgs<T>((T)sender);
            OnResourceModified(args);
        }

        protected virtual bool CheckAdd (T item)
        {
            return true;
        }

        protected virtual bool CheckRemove (T item)
        {
            return true;
        }

        public virtual IEnumerator<T> GetEnumerator ()
        {
            return _resourceMap.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

    }

    /*public class NamedResourceCollection<T> : IEnumerable<T>
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
    }*/
}
