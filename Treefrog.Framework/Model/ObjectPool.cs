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
                ObjectClass obj = ObjectClass.FromXProxy(objClass, TexturePool);
                obj.Pool = this;

                Objects.Add(obj);
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

            List<LibraryX.ObjectClassX> objects = new List<LibraryX.ObjectClassX>();
            foreach (ObjectClass objClass in pool.Objects) {
                LibraryX.ObjectClassX classProxy = ObjectClass.ToXProxy(objClass);
                objects.Add(classProxy);
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
