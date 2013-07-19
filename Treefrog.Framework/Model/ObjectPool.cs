using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;
using System.Collections;

namespace Treefrog.Framework.Model
{
    public interface IObjectCollection : IResourceManager<ObjectClass>
    {
        ObjectClass GetObject (Guid uid);
        //void AddObject (ObjectClass objClass);
        //void RemoveObject (Guid uid);
    }

    public abstract class ObjectCollection : INamedResource, IObjectCollection
    {
        private readonly Guid _uid;
        private readonly ResourceName _name;

        protected ObjectCollection (Guid uid, string name)
        {
            _uid = uid;
            _name = new ResourceName(this, name);
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public ObjectClass GetObject (Guid uid)
        {
            return GetObjectCore(uid);
        }

        protected abstract ObjectClass GetObjectCore (Guid uid);

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var obj in this)
                obj.ResetModified();
            //foreach (var property in PropertyManager.CustomProperties)
            //    property.ResetModified();
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        #region Name Interface

        public event EventHandler<NameChangingEventArgs> NameChanging
        {
            add { _name.NameChanging += value; }
            remove { _name.NameChanging -= value; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged
        {
            add { _name.NameChanged += value; }
            remove { _name.NameChanged -= value; }
        }

        [SpecialProperty]
        public string Name
        {
            get { return _name.Name; }
        }

        public bool TrySetName (string name)
        {
            bool result = _name.TrySetName(name);
            if (result)
                OnModified(EventArgs.Empty);

            return result;
        }

        #endregion

        #region Resource Manager Explicit Interface

        private EventHandler<ResourceEventArgs<ObjectClass>> _objectResourceAdded;
        private EventHandler<ResourceEventArgs<ObjectClass>> _objectResourceRemoved;
        private EventHandler<ResourceEventArgs<ObjectClass>> _objectResourceModified;

        event EventHandler<ResourceEventArgs<ObjectClass>> IResourceManager<ObjectClass>.ResourceAdded
        {
            add { _objectResourceAdded += value; }
            remove { _objectResourceAdded -= value; }
        }

        event EventHandler<ResourceEventArgs<ObjectClass>> IResourceManager<ObjectClass>.ResourceRemoved
        {
            add { _objectResourceRemoved += value; }
            remove { _objectResourceRemoved -= value; }
        }

        event EventHandler<ResourceEventArgs<ObjectClass>> IResourceManager<ObjectClass>.ResourceModified
        {
            add { _objectResourceModified += value; }
            remove { _objectResourceModified -= value; }
        }

        IResourceCollection<ObjectClass> IResourceManager<ObjectClass>.Collection
        {
            get { return Collection; }
        }

        IEnumerator<ObjectClass> System.Collections.Generic.IEnumerable<ObjectClass>.GetEnumerator ()
        {
            return GetObjectEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetObjectEnumerator();
        }

        protected virtual void OnResourceAdded (ObjectClass resource)
        {
            var ev = _objectResourceAdded;
            if (ev != null)
                ev(this, new ResourceEventArgs<ObjectClass>(resource));
        }

        protected virtual void OnResourceRemoved (ObjectClass resource)
        {
            var ev = _objectResourceRemoved;
            if (ev != null)
                ev(this, new ResourceEventArgs<ObjectClass>(resource));
        }

        protected virtual void OnResourceModified (ObjectClass resource)
        {
            var ev = _objectResourceModified;
            if (ev != null)
                ev(this, new ResourceEventArgs<ObjectClass>(resource));
        }

        protected virtual IEnumerator<ObjectClass> GetObjectEnumerator ()
        {
            yield break;
        }

        protected virtual IResourceCollection<ObjectClass> Collection
        {
            get { return null; }
        }

        #endregion
    }

    public interface IObjectCollection<T> : IObjectCollection
        where T : ObjectClass
    {
        NamedResourceCollection<T> Objects { get; }
        int Count { get; }

        new T GetObject (Guid uid);
    }

    public class ObjectCollection<T> : ObjectCollection, IObjectCollection<T>
        where T : ObjectClass
    {
        private ResourceCollectionAdapter<ObjectClass, T> _objectCollectionAdapter;

        protected ObjectCollection (Guid uid, string name)
            : base(uid, name)
        {
            Objects = new NamedResourceCollection<T>();

            _objectCollectionAdapter = new ResourceCollectionAdapter<ObjectClass, T>(Objects);

            Objects.Modified += (s, e) => OnModified(EventArgs.Empty);
            Objects.ResourceAdded += (s, e) => OnResourceAdded(e.Resource);
            Objects.ResourceRemoved += (s, e) => OnResourceRemoved(e.Resource);
            Objects.ResourceModified += (s, e) => OnResourceModified(e.Resource);
        }

        public ObjectCollection (string name)
            : this(Guid.NewGuid(), name)
        { }

        public NamedResourceCollection<T> Objects { get; private set; }

        public int Count
        {
            get { return Objects.Count; }
        }

        protected override ObjectClass GetObjectCore (Guid uid)
        {
            return GetObject(uid);
        }

        public new T GetObject (Guid uid)
        {
            foreach (T obj in Objects) {
                if (obj.Uid == uid)
                    return obj;
            }

            return null;
        }

        protected override IEnumerator<ObjectClass> GetObjectEnumerator ()
        {
            foreach (ObjectClass obj in Objects)
                yield return obj;
        }

        public static LibraryX.ObjectCollectionX<TProxy> ToXProxy<TProxy> (ObjectCollection<T> objectCollection, Func<T, TProxy> itemXmlFunc)
            where TProxy : LibraryX.ObjectClassX
        {
            if (objectCollection == null)
                return null;

            List<TProxy> objects = new List<TProxy>();
            foreach (T obj in objectCollection.Objects) {
                TProxy brushProxy = itemXmlFunc(obj);
                objects.Add(brushProxy);
            }

            return new LibraryX.ObjectCollectionX<TProxy>() {
                Uid = objectCollection.Uid,
                Name = objectCollection.Name,
                ObjectClasses = objects,
            };
        }
    }

    public class ObjectPool : INamedResource, IResourceManager<ObjectClass>, IPropertyProvider, INotifyPropertyChanged
    {
        private static PropertyClassManager _propertyClassManager = new PropertyClassManager(typeof(ObjectPool));

        private readonly Guid _uid;
        private readonly ResourceName _name;

        private NamedResourceCollection<ObjectClass> _objects;

        private PropertyManager _propertyManager;

        protected ObjectPool ()
        {
            _uid = Guid.NewGuid();
            _name = new ResourceName(this);

            Objects = new NamedResourceCollection<ObjectClass>();
            Objects.Modified += (s, e) => OnModified(EventArgs.Empty);

            _propertyManager = new PropertyManager(_propertyClassManager, this);
            _propertyManager.CustomProperties.Modified += (s, e) => OnModified(EventArgs.Empty);
        }

        public ObjectPool (string name)
            : this()
        {
            _name = new ResourceName(this, name);

            TexturePool = new TexturePool();
        }

        private ObjectPool (LibraryX.ObjectPoolX proxy, ObjectPoolManager manager)
            : this()
        {
            _uid = proxy.Uid;
            _name = new ResourceName(this, proxy.Name);

            TexturePool = manager.TexturePool;

            foreach (var objClass in proxy.ObjectClasses) {
                if (objClass is LibraryX.RasterObjectClassX) {
                    RasterObjectClass obj = RasterObjectClass.FromXProxy(objClass as LibraryX.RasterObjectClassX, TexturePool);
                    obj.Pool = this;

                    Objects.Add(obj);
                }
                else {
                    ObjectClass obj = ObjectClass.FromXProxy(objClass, TexturePool);
                    obj.Pool = this;

                    Objects.Add(obj);
                }
            }

            manager.Pools.Add(this);
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public ITexturePool TexturePool { get; internal set; }

        public int Count
        {
            get { return _objects.Count; }
        }

        public ObjectClass GetObject (Guid uid)
        {
            foreach (ObjectClass objClass in _objects) {
                if (objClass.Uid == uid)
                    return objClass;
            }

            return null;
        }

        public void AddObject (ObjectClass objClass)
        {
            if (_objects.Contains(objClass.Uid))
                throw new ArgumentException("Object Pool already contains an object with the same uid as objClass.");

            objClass.Pool = this;

            _objects.Add(objClass);
        }

        public void RemoveObject (Guid uid)
        {
            if (_objects.Contains(uid)) {
                ObjectClass objClass = _objects[uid];
                objClass.Pool = null;

                _objects.Remove(uid);
            }
        }

        public NamedResourceCollection<ObjectClass> Objects
        {
            get { return _objects; }
            private set { _objects = value; }
        }

        IResourceCollection<ObjectClass> IResourceManager<ObjectClass>.Collection
        {
            get { return _objects; }
        }

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var obj in Objects)
                obj.ResetModified();
            foreach (var property in PropertyManager.CustomProperties)
                property.ResetModified();
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        #region Name Interface

        public event EventHandler<NameChangingEventArgs> NameChanging
        {
            add { _name.NameChanging += value; }
            remove { _name.NameChanging -= value; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged
        {
            add { _name.NameChanged += value; }
            remove { _name.NameChanged -= value; }
        }

        [SpecialProperty]
        public string Name
        {
            get { return _name.Name; }
        }

        public bool TrySetName (string name)
        {
            bool result = _name.TrySetName(name);
            if (result)
                OnModified(EventArgs.Empty);

            return result;
        }

        #endregion

        #region Resource Manager Explicit Interface

        event EventHandler<ResourceEventArgs<ObjectClass>> IResourceManager<ObjectClass>.ResourceAdded
        {
            add { Objects.ResourceAdded += value; }
            remove { Objects.ResourceAdded -= value; }
        }

        event EventHandler<ResourceEventArgs<ObjectClass>> IResourceManager<ObjectClass>.ResourceRemoved
        {
            add { Objects.ResourceRemoved += value; }
            remove { Objects.ResourceRemoved -= value; }
        }

        event EventHandler<ResourceEventArgs<ObjectClass>> IResourceManager<ObjectClass>.ResourceModified
        {
            add { Objects.ResourceModified += value; }
            remove { Objects.ResourceModified -= value; }
        }

        IEnumerator<ObjectClass> System.Collections.Generic.IEnumerable<ObjectClass>.GetEnumerator ()
        {
            return Objects.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return Objects.GetEnumerator();
        }

        #endregion

        #region IPropertyProvider Members

        public string PropertyProviderName
        {
            get { return Name; }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public PropertyManager PropertyManager
        {
            get { return _propertyManager; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void RaisePropertyChanged (string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
            OnModified(EventArgs.Empty);
        }

        #endregion

        public static LibraryX.ObjectPoolX ToXProxy (ObjectPool pool)
        {
            if (pool == null)
                return null;

            List<LibraryX.RasterObjectClassX> objects = new List<LibraryX.RasterObjectClassX>();
            foreach (ObjectClass objClass in pool.Objects) {
                if (objClass is RasterObjectClass) {
                    LibraryX.RasterObjectClassX classProxy = RasterObjectClass.ToXProxy(objClass as RasterObjectClass);
                    objects.Add(classProxy);
                }
                else {
                    //LibraryX.ObjectClassX classProxy = ObjectClass.ToXProxy(objClass);
                    //objects.Add(classProxy);
                }
            }

            return new LibraryX.ObjectPoolX() {
                Uid = pool.Uid,
                Name = pool.Name,
                ObjectClasses = objects,
            };
        }

        public static ObjectPool FromXProxy (LibraryX.ObjectPoolX proxy, ObjectPoolManager manager)
        {
            if (proxy == null)
                return null;

            return new ObjectPool(proxy, manager);
        }
    }
}
