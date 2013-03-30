using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public delegate void EventHandler2<in T> (object sender, T e);

    /*public interface IResourceCollectionEvents<out T>
        where T : IResource
    {
        event EventHandler2<IResourceEventArgs<T>> ResourceAdded;
        event EventHandler2<IResourceEventArgs<T>> ResourceRemoved;
        event EventHandler2<IResourceEventArgs<T>> ResourceModified;

        event EventHandler Modified;


        event EventHandler2<IResourceEventArgs<T>> ResourceExists;
    }*/

    public interface IResourceCollection<T> : IEnumerable<T>
        where T : IResource
    {
        event EventHandler<ResourceEventArgs<T>> ResourceAdded;
        event EventHandler<ResourceEventArgs<T>> ResourceRemoved;
        event EventHandler<ResourceEventArgs<T>> ResourceModified;

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
}
