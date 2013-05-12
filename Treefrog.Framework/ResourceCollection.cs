using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public interface IResourceCollection<T> : IResourceManager<T>
        where T : IResource
    {
        event EventHandler Modified;

        int Count { get; }
        T this[Guid uid] { get; }

        bool Add (T resource);
        bool Contains (Guid uid);
        bool Remove (Guid uid);
        void Clear ();
    }

    public interface IResourceManager<T> : IEnumerable<T>
        where T : IResource
    {
        event EventHandler<ResourceEventArgs<T>> ResourceAdded;
        event EventHandler<ResourceEventArgs<T>> ResourceRemoved;
        event EventHandler<ResourceEventArgs<T>> ResourceModified;

        IResourceCollection<T> Collection { get; }
    }

    public class ResourceCollection<T> : IResourceCollection<T>
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

        IResourceCollection<T> IResourceManager<T>.Collection
        {
            get { return this; }
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
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnResourceRemoved (ResourceEventArgs<T> e)
        {
            var ev = ResourceRemoved;
            if (ev != null)
                ev(this, e);
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnResourceModified (ResourceEventArgs<T> e)
        {
            var ev = ResourceModified;
            if (ev != null)
                ev(this, e);
            OnModified(EventArgs.Empty);
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

    public class MetaResourceCollection<T, TSubType> : IResourceCollection<T>
        where T : IResource
        where TSubType : IResourceManager<T>
    {
        private Guid _default;
        private Dictionary<Guid, TSubType> _collections;

        public MetaResourceCollection ()
        {
            _collections = new Dictionary<Guid, TSubType>();
        }

        public Guid Default
        {
            get { return _default; }
            set
            {
                if (!_collections.ContainsKey(value))
                    throw new ArgumentException("Can only set default library UID to a value that has been previously added.");
                _default = value;
            }
        }

        IResourceCollection<T> IResourceManager<T>.Collection
        {
            get { return this; }
        }

        public TSubType GetCollection (Guid libraryUid)
        {
            return _collections[MapAndCheckUid(libraryUid)];
        }

        public void AddCollection (Guid libraryUid, TSubType collection)
        {
            if (libraryUid == Guid.Empty)
                throw new ArgumentException("Library UID must be non-empty.");
            if (_collections.ContainsKey(libraryUid))
                throw new ArgumentException("A manager with the given UID has already been added.");

            _collections.Add(libraryUid, collection);
            if (_collections.Count == 1)
                _default = libraryUid;

            collection.ResourceAdded += HandleCollectionAdded;
            collection.ResourceRemoved += HandleCollectionRemoved;
            collection.ResourceModified += HandleCollectionModified;
            collection.Collection.Modified += HandleModified;
        }

        public bool RemoveCollection (Guid libraryUid)
        {
            if (_default == libraryUid)
                _default = Guid.Empty;

            TSubType collection;
            if (_collections.TryGetValue(libraryUid, out collection)) {
                collection.ResourceAdded -= HandleCollectionAdded;
                collection.ResourceRemoved -= HandleCollectionRemoved;
                collection.ResourceModified -= HandleCollectionModified;
                collection.Collection.Modified -= HandleModified;
            }

            return _collections.Remove(libraryUid);
        }

        public event EventHandler<ResourceEventArgs<T>> ResourceAdded;

        public event EventHandler<ResourceEventArgs<T>> ResourceRemoved;

        public event EventHandler<ResourceEventArgs<T>> ResourceModified;

        public event EventHandler Modified;

        private void HandleCollectionAdded (object sender, ResourceEventArgs<T> e)
        {
            var ev = ResourceAdded;
            if (ev != null)
                ev(this, e);
        }

        private void HandleCollectionRemoved (object sender, ResourceEventArgs<T> e)
        {
            var ev = ResourceRemoved;
            if (ev != null)
                ev(this, e);
        }

        private void HandleCollectionModified (object sender, ResourceEventArgs<T> e)
        {
            var ev = ResourceModified;
            if (ev != null)
                ev(this, e);
        }

        private void HandleModified (object sender, EventArgs e)
        {
            var ev = Modified;
            if (ev != null)
                ev(this, e);
        }

        public int Count
        {
            get 
            {
                int accum = 0;
                foreach (var col in _collections.Values)
                    accum += col.Collection.Count;
                return accum;
            }
        }

        public T this[Guid uid]
        {
            get
            {
                foreach (var col in _collections.Values) {
                    if (col.Collection.Contains(uid))
                        return col.Collection[uid];
                }
                return default(T);
            }
        }

        public bool Add (T resource)
        {
            return GetCollection(Default).Collection.Add(resource);
        }

        public bool Contains (Guid uid)
        {
            foreach (var col in _collections.Values) {
                if (col.Collection.Contains(uid))
                    return true;
            }
            return false;
        }

        public bool Remove (Guid uid)
        {
            foreach (var col in _collections.Values) {
                if (col.Collection.Remove(uid))
                    return true;
            }
            return false;
        }

        public void Clear ()
        {
            foreach (var col in _collections.Values)
                col.Collection.Clear();
        }

        public IEnumerator<T> GetEnumerator ()
        {
            foreach (var col in _collections.Values) {
                foreach (T item in col)
                    yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        private Guid MapAndCheckUid (Guid libraryUid)
        {
            if (libraryUid == Guid.Empty)
                libraryUid = _default;
            if (libraryUid == Guid.Empty)
                throw new InvalidOperationException("No default library has been set for this manager.");
            if (!_collections.ContainsKey(libraryUid))
                throw new ArgumentException("The specified library has not been registered with this manager.");

            return libraryUid;
        }
    }
}
