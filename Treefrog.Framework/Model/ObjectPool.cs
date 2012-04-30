using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model.Collections;

namespace Treefrog.Framework.Model
{
    public class ObjectClassEventArgs : EventArgs
    {
        public ObjectClass ObjectClass { get; private set; }

        public ObjectClassEventArgs (ObjectClass objectClass)
        {
            ObjectClass = objectClass;
        }
    }

    public class ObjectPool : INamedResource, IEnumerable<ObjectClass>, IPropertyProvider
    {
        private static string[] _reservedPropertyNames = new string[] { "Name" };

        #region Fields

        private string _name;

        private NamedResourceCollection<ObjectClass> _objects;

        private PropertyCollection _properties;
        private ObjectPoolProperties _predefinedProperties;

        #endregion

        public ObjectPool (string name)
        {
            _name = name;
            _objects = new NamedResourceCollection<ObjectClass>();
            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new ObjectPoolProperties(this);

            _properties.Modified += Properties_Modified;

            _objects.ResourceAdded += Objects_ResourceAdded;
            _objects.ResourceRemoved += Objects_ResourceRemoved;
            _objects.ResourceModified += Objects_ResourceModified;
        }

        public int Count
        {
            get { return _objects.Count; }
        }

        public void AddObject (ObjectClass objClass)
        {
            if (_objects.Contains(objClass.Name))
                throw new ArgumentException("Object Pool already contains an object with the same name as objClass.");
            _objects.Add(objClass);
        }

        public void RemoveObject (string name)
        {
            _objects.Remove(name);
        }

        public NamedResourceCollection<ObjectClass> Objects
        {
            get { return _objects; }
        }

        public event EventHandler<ObjectClassEventArgs> ObjectAdded = (s, e) => { };

        public event EventHandler<ObjectClassEventArgs> ObjectRemoved = (s, e) => { };

        public event EventHandler<ObjectClassEventArgs> ObjectModified = (s, e) => { };

        protected virtual void OnObjectAdded (ObjectClassEventArgs e)
        {
            ObjectAdded(this, e);
            OnModified(e);
        }

        protected virtual void OnObjectRemoved (ObjectClassEventArgs e)
        {
            ObjectRemoved(this, e);
            OnModified(e);
        }

        protected virtual void OnObjectModified (ObjectClassEventArgs e)
        {
            ObjectModified(this, e);
            OnModified(e);
        }

        private void Properties_Modified (object sender, EventArgs e)
        {
            OnModified(e);
        }

        private void Objects_ResourceAdded (object sender, NamedResourceEventArgs<ObjectClass> e)
        {
            OnObjectAdded(new ObjectClassEventArgs(e.Resource));
        }

        private void Objects_ResourceRemoved (object sender, NamedResourceEventArgs<ObjectClass> e)
        {
            OnObjectRemoved(new ObjectClassEventArgs(e.Resource));
        }

        private void Objects_ResourceModified (object sender, NamedResourceEventArgs<ObjectClass> e)
        {
            OnObjectModified(new ObjectClassEventArgs(e.Resource));
        }

        #region INamedResource Members

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

        #endregion

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
            Name = property.Value;
        }

        #endregion
    }
}
