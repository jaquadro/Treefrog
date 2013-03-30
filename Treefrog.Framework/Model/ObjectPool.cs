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
        private static string[] _reservedPropertyNames = new string[] { "Name" };

        private readonly Guid _uid;
        private readonly ResourceName _name;

        private ResourceCollection<ObjectClass> _objects;

        private PropertyCollection _properties;
        private ObjectPoolProperties _predefinedProperties;

        protected ObjectPool ()
        {
            _uid = Guid.NewGuid();
            _name = new ResourceName(this);

            Objects = new ResourceCollection<ObjectClass>();

            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new ObjectPoolProperties(this);

            _properties.Modified += Properties_Modified;
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

        public TexturePool TexturePool { get; internal set; }

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

        public ResourceCollection<ObjectClass> Objects
        {
            get { return _objects; }
            private set { _objects = value; }
        }

        private void Properties_Modified (object sender, EventArgs e)
        {
            RaisePropertyChanged("CustomProperties");
        }

        private EventHandler _eventModified;
        public event EventHandler Modified
        {
            add
            {
                _eventModified += value;
                //_objects.Modified += value;
            }

            remove
            {
                _eventModified -= value;
                //_objects.Modified -= value;
            }
        }

        protected virtual void OnModified (EventArgs e)
        {
            var ev = _eventModified;
            if (ev != null)
                ev(this, e);
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

        private class ObjectPoolProperties : PredefinedPropertyCollection
        {
            private ObjectPool _parent;

            public ObjectPoolProperties (ObjectPool parent)
                : base(_reservedPropertyNames)
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield return _parent.LookupProperty("Name");
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public string PropertyProviderName
        {
            get { return Name; }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public Collections.PropertyCollection CustomProperties
        {
            get { return _properties; }
        }

        public Collections.PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties; }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            switch (name) {
                case "Name":
                    return PropertyCategory.Predefined;
                default:
                    return _properties.Contains(name) ? PropertyCategory.Custom : PropertyCategory.None;
            }
        }

        public Property LookupProperty (string name)
        {
            Property prop;

            switch (name) {
                case "Name":
                    prop = new StringProperty("Name", Name);
                    prop.ValueChanged += NameProperty_ValueChanged;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        private void NameProperty_ValueChanged (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            TrySetName(property.Value);
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
