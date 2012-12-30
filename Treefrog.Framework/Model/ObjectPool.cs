using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Compat;

namespace Treefrog.Framework.Model
{
    [XmlRoot("ObjectPool")]
    public class ObjectPoolXmlProxy
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlArray]
        [XmlArrayItem("ObjectClass")]
        public List<ObjectClassXmlProxy> ObjectClasses { get; set; }
    }

    public class ObjectPool : IKeyProvider<string>, IEnumerable<ObjectClass>, IPropertyProvider, INotifyPropertyChanged
    {
        private static string[] _reservedPropertyNames = new string[] { "Name" };

        #region Fields

        private string _name;

        private ObjectPoolManager _manager;

        private NamedObservableCollection<ObjectClass> _objects;

        private PropertyCollection _properties;
        private ObjectPoolProperties _predefinedProperties;

        #endregion

        protected ObjectPool ()
        {
            _objects = new NamedObservableCollection<ObjectClass>();
            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new ObjectPoolProperties(this);

            _properties.Modified += Properties_Modified;
            _objects.CollectionChanged += HandleCollectionChanged;
        }

        public ObjectPool (string name, ObjectPoolManager manager)
            : this()
        {
            _name = name;
            _manager = manager;
        }

        public TexturePool TexturePool
        {
            get { return _manager.TexturePool; }
        }

        public int Count
        {
            get { return _objects.Count; }
        }

        public ObjectClass GetObject (int id)
        {
            foreach (ObjectClass objClass in _objects) {
                if (objClass.Id == id)
                    return objClass;
            }

            return null;
        }

        public void AddObject (ObjectClass objClass)
        {
            int id = _manager.TakeKey();
            AddObject(objClass, id);
        }

        public void AddObject (ObjectClass objClass, int id)
        {
            if (_objects.Contains(objClass.Name))
                throw new ArgumentException("Object Pool already contains an object with the same name as objClass.");

            objClass.Id = id;
            objClass.Pool = this;

            _objects.Add(objClass);

            _manager.LinkItemKey(id, this);
            if (_manager.LastKey < id)
                _manager.LastKey = id;
        }

        public void RemoveObject (string name)
        {
            if (_objects.Contains(name)) {
                ObjectClass objClass = _objects[name];
                objClass.Pool = null;

                _manager.UnlinkItemKey(objClass.Id);

                _objects.Remove(name);
            }
        }

        public NamedObservableCollection<ObjectClass> Objects
        {
            get { return _objects; }
        }

        private void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            //OnModified(EventArgs.Empty);
            RaisePropertyChanged("Objects");
        }

        private void Properties_Modified (object sender, EventArgs e)
        {
            //OnModified(e);
            RaisePropertyChanged("Objects");
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

        /*#region INamedResource Members

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value) {
                    NameChangingEventArgs ea = new NameChangingEventArgs(_name, value);
                    OnNameChanging(ea);
                    if (ea.Cancel)
                        return;

                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, _name));
                    OnPropertyProviderNameChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler<NameChangingEventArgs> NameChanging = (s, e) => { };

        public event EventHandler<NameChangedEventArgs> NameChanged = (s, e) => { };

        public event EventHandler Modified = (s, e) => { };

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            NameChanging(this, e);
        }

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            NameChanged(this, e);
        }

        protected virtual void OnModified (EventArgs e)
        {
            Modified(this, e);
        }

        #endregion*/

        #region IEnumerable<ObjectClass> Members

        public IEnumerator<ObjectClass> GetEnumerator ()
        {
            return _objects.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return _objects.GetEnumerator();
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
        }

        #endregion

        public static ObjectPoolXmlProxy ToXmlProxy (ObjectPool pool)
        {
            if (pool == null)
                return null;

            List<ObjectClassXmlProxy> objects = new List<ObjectClassXmlProxy>();
            foreach (ObjectClass objClass in pool._objects) {
                ObjectClassXmlProxy classProxy = ObjectClass.ToXmlProxy(objClass);
                objects.Add(classProxy);
            }

            return new ObjectPoolXmlProxy()
            {
                Name = pool._name,
                ObjectClasses = objects,
            };
        }

        public static ObjectPool FromXmlProxy (ObjectPoolXmlProxy proxy, ObjectPoolManager manager)
        {
            if (proxy == null)
                return null;

            ObjectPool pool = manager.CreatePool(proxy.Name);
            foreach (ObjectClassXmlProxy objClass in proxy.ObjectClasses) {
                ObjectClass inst = ObjectClass.FromXmlProxy(objClass, manager.TexturePool);
                pool.AddObject(inst, objClass.Id);
            }

            return pool;
        }
    }
}
