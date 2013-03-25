using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public class ObjectPool : IResource, IResourceManager<ObjectClass>, IPropertyProvider, INotifyPropertyChanged
    {
        private static string[] _reservedPropertyNames = new string[] { "Name" };

        #region Fields

        private string _name;

        private ObjectPoolManager _manager;

        private ResourceCollection<ObjectClass> _objects;

        private PropertyCollection _properties;
        private ObjectPoolProperties _predefinedProperties;

        #endregion

        protected ObjectPool ()
        {
            Uid = Guid.NewGuid();

            _objects = new ResourceCollection<ObjectClass>();
            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new ObjectPoolProperties(this);

            _properties.Modified += Properties_Modified;
        }

        public ObjectPool (string name, ObjectPoolManager manager)
            : this()
        {
            _name = name;
            _manager = manager;
        }

        IResourceCollection<ObjectClass> IResourceManager<ObjectClass>.Items
        {
            get { return _objects; }
        }

        public Guid Uid { get; private set; }

        public TexturePool TexturePool
        {
            get { return _manager.TexturePool; }
        }

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

            //_manager.LinkItemKey(uid, this);
        }

        public void RemoveObject (Guid uid)
        {
            if (_objects.Contains(uid)) {
                ObjectClass objClass = _objects[uid];
                objClass.Pool = null;

                //_manager.UnlinkItemKey(objClass.Uid);

                _objects.Remove(uid);
            }
        }

        public ResourceCollection<ObjectClass> Objects
        {
            get { return _objects; }
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

        public event EventHandler<KeyProviderEventArgs<string>> KeyChanging;
        public event EventHandler<KeyProviderEventArgs<string>> KeyChanged;

        protected virtual void OnKeyChanging (KeyProviderEventArgs<string> e)
        {
            if (KeyChanging != null)
                KeyChanging(this, e);
        }

        protected virtual void OnKeyChanged (KeyProviderEventArgs<string> e)
        {
            if (KeyChanged != null)
                KeyChanged(this, e);
        }

        public string GetKey ()
        {
            return Name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool SetName (string name)
        {
            if (_name != name) {
                KeyProviderEventArgs<string> e = new KeyProviderEventArgs<string>(_name, name);
                try {
                    OnKeyChanging(e);
                }
                catch (KeyProviderException) {
                    return false;
                }

                _name = name;
                OnKeyChanged(e);
                OnPropertyProviderNameChanged(EventArgs.Empty);
                RaisePropertyChanged("Name");
            }

            return true;
        }

        /*#region IEnumerable<ObjectClass> Members

        public IEnumerator<ObjectClass> GetEnumerator ()
        {
            return _objects.GetEnumerator();
        }

        #endregion*/

        /*#region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return _objects.GetEnumerator();
        }

        #endregion*/

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
            get { return _name; }
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
                    prop = new StringProperty("Name", _name);
                    prop.ValueChanged += NameProperty_ValueChanged;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        private void NameProperty_ValueChanged (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            SetName(property.Value);
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

        public static LibraryX.ObjectPoolX ToXmlProxyX (ObjectPool pool)
        {
            if (pool == null)
                return null;

            List<LibraryX.ObjectClassX> objects = new List<LibraryX.ObjectClassX>();
            foreach (ObjectClass objClass in pool._objects) {
                LibraryX.ObjectClassX classProxy = ObjectClass.ToXmlProxyX(objClass);
                objects.Add(classProxy);
            }

            return new LibraryX.ObjectPoolX() {
                Uid = pool.Uid,
                Name = pool._name,
                ObjectClasses = objects,
            };
        }

        public static ObjectPool FromXmlProxy (LibraryX.ObjectPoolX proxy, ObjectPoolManager manager)
        {
            if (proxy == null)
                return null;

            ObjectPool pool = new ObjectPool(proxy.Name, manager) { 
                Uid = proxy.Uid
            };
            manager.Pools.Add(pool);

            foreach (var objClass in proxy.ObjectClasses) {
                ObjectClass inst = ObjectClass.FromXmlProxy(objClass, manager.TexturePool);
                pool.AddObject(inst);
            }

            return pool;
        }
    }
}
