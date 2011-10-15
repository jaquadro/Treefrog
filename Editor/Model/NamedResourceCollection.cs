using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
    public class NamedResourceEventArgs<T> : EventArgs
        where T : INamedResource
    {
        //public int Id { get; private set; }
        public string Name { get; private set; }
        public T Resource { get; private set; }

        public NamedResourceEventArgs (T resource)
        {
            //Id = resource.Id;
            Name = resource.Name;
            Resource = resource;
        }

        public NamedResourceEventArgs (T resource, string name)
        {
            //Id = id;
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
        //private Dictionary<int, T> _idMap;
        private Dictionary<string, T> _nameMap;

        public event EventHandler<NamedResourceEventArgs<T>> ResourceAdded;
        public event EventHandler<NamedResourceEventArgs<T>> ResourceRemoved;
        public event EventHandler<NamedResourceEventArgs<T>> ResourceRemapped;

        public NamedResourceCollection ()
        {
            //_idMap = new Dictionary<int, T>();
            _nameMap = new Dictionary<string, T>();
        }

        /*public T this[int id]
        {
            get
            {
                if (!_idMap.ContainsKey(id)) {
                    throw new ArgumentException("The collection does not contain an item keyed by the given id", "id");
                }

                return _idMap[id];
            }
        }*/

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
            /*if (_idMap.ContainsKey(item.Id)) {
                throw new ArgumentException("The collection already contains an item keyed by the same Id", "item");
            }*/

            if (_nameMap.ContainsKey(item.Name)) {
                throw new ArgumentException("The collection already contains an item keyed by the same Name", "item");
            }

            //_idMap[item.Id] = item;
            _nameMap[item.Name] = item;

            //item.IdChanged += IdChangedHandler;
            item.NameChanged += NameChangedHandler;

            OnResourceAdded(new NamedResourceEventArgs<T>(item));
        }

        /*public bool Contains (int id)
        {
            return _idMap.ContainsKey(id);
        }*/

        public bool Contains (string name)
        {
            return _nameMap.ContainsKey(name);
        }

        /*public void Remove (int id)
        {
            T item;
            if (_idMap.TryGetValue(id, out item)) {
                _nameMap.Remove(item.Name);
                _idMap.Remove(item.Id);

                item.IdChanged -= IdChangedHandler;
                item.NameChanged -= NameChangedHandler;

                OnResourceRemoved(new NamedResourceEventArgs<T>(item));
            }
        }*/

        public void Remove (string name)
        {
            T item;
            if (_nameMap.TryGetValue(name, out item)) {
                _nameMap.Remove(item.Name);
                //_idMap.Remove(item.Id);

                //item.IdChanged -= IdChangedHandler;
                item.NameChanged -= NameChangedHandler;

                OnResourceRemoved(new NamedResourceEventArgs<T>(item));
            }
        }

        /*private void IdChangedHandler (object sender, IdChangedEventArgs e)
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
        }*/

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

    public class OrderedResourceCollection<T> : NamedResourceCollection<T>
        where T : INamedResource
    {
        private List<string> _order;

        public event EventHandler<OrderedResourceEventArgs<T>> ResourceOrderChanged;

        public OrderedResourceCollection ()
            : base()
        {
            _order = new List<string>();
        }

        protected override void OnResourceAdded (NamedResourceEventArgs<T> e)
        {
            _order.Add(e.Name);

            base.OnResourceAdded(e);
        }

        protected override void OnResourceRemoved (NamedResourceEventArgs<T> e)
        {
            _order.Remove(e.Name);

            base.OnResourceRemoved(e);
        }

        protected override void OnResourceRemapped (NamedResourceEventArgs<T> e)
        {
            int index = _order.IndexOf(e.Name);
            _order.Insert(index, e.Resource.Name);
            _order.Remove(e.Name);

            base.OnResourceRemapped(e);
        }

        protected virtual void OnResourceOrderChanged (OrderedResourceEventArgs<T> e)
        {
            if (ResourceOrderChanged != null) {
                ResourceOrderChanged(this, e);
            }
        }

        public override IEnumerator<T> GetEnumerator ()
        {
            foreach (string name in _order) {
                yield return this[name];
            }
        }

        public void ChangeIndexRelative (string name, int offset)
        {
            if (offset == 0) {
                return;
            }

            if (!_order.Contains(name)) {
                throw new ArgumentException("No resource with the given name", "name");
            }

            int index = _order.IndexOf(name);
            if (index + offset < 0 || index + offset >= _order.Count) {
                throw new ArgumentOutOfRangeException("offset", "The relative offset results in an invalid index for this item");
            }

            _order.Remove(name);
            _order.Insert(index + offset, name);

            OnResourceOrderChanged(new OrderedResourceEventArgs<T>(this[name], index));
        }
    }
}
