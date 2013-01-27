using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model.Collections
{
    [Serializable]
    public class PropertyCollection : ICollection<Property>
    {
        private Dictionary<string, Property> _properties;
        private List<string> _reservedNames;

        public PropertyCollection (string[] reservedNames)
        {
            _properties = new Dictionary<string, Property>();
            _reservedNames = new List<string>(reservedNames);
        }

        public Property this [string name] 
        {
            get { return _properties[name]; }
        }

        public event EventHandler<PropertyEventArgs> PropertyAdded = (s, e) => { };

        public event EventHandler<PropertyEventArgs> PropertyRemoved = (s, e) => { };

        public event EventHandler<PropertyEventArgs> PropertyModified = (s, e) => { };

        public event EventHandler<NameChangedEventArgs> PropertyRenamed = (s, e) => { };

        public event EventHandler Modified = (s, e) => { };

        protected virtual void OnPropertyAdded (PropertyEventArgs e)
        {
            PropertyAdded(this, e);
        }

        protected virtual void OnPropertyRemoved (PropertyEventArgs e)
        {
            PropertyRemoved(this, e);
        }

        protected virtual void OnPropertyModified (PropertyEventArgs e)
        {
            PropertyModified(this, e);
        }

        protected virtual void OnPropertyRenamed (NameChangedEventArgs e)
        {
            PropertyRenamed(this, e);
        }

        protected virtual void OnModified (EventArgs e)
        {
            Modified(this, e);
        }

        private void Property_Modified (object sender, EventArgs e)
        {
            OnPropertyModified(new PropertyEventArgs(sender as Property));
            OnModified(e);
        }

        private void Property_NameChanged (object sender, NameChangedEventArgs e)
        {
            if (!_properties.ContainsKey(e.OldName)) {
                throw new ArgumentException("The collection does not contain a property keyed by the old name: '" + e.OldName + "'");
            }
            if (_properties.ContainsKey(e.NewName)) {
                throw new ArgumentException("There is an existing property with the new name '" + e.NewName + "' in the collection");
            }
            if (_reservedNames.Contains(e.NewName)) {
                throw new ArgumentException("The requested name is reserved: '" + e.NewName + "'");
            }

            Property item = _properties[e.OldName];

            _properties.Remove(e.OldName);
            _properties[e.NewName] = item;

            OnPropertyRenamed(e);
            OnModified(EventArgs.Empty);
        }

        #region ICollection<Property> Members

        public void Add (Property item)
        {
            if (item == null) {
                throw new ArgumentNullException("item", "The property is null.");
            }
            if (_properties.ContainsKey(item.Name)) {
                throw new ArgumentException("A property with the same name already exists.", "item");
            }
            if (_reservedNames.Contains(item.Name)) {
                throw new ArgumentException("The property is using a reserved name.", "item");
            }

            _properties.Add(item.Name, item);

            item.Modified += Property_Modified;
            item.NameChanged += Property_NameChanged;

            OnPropertyAdded(new PropertyEventArgs(item));
            OnModified(EventArgs.Empty);
        }

        public void Clear ()
        {
            string[] keys = new string[_properties.Count];
            _properties.Keys.CopyTo(keys, 0);

            foreach (string key in keys) {
                Property value = _properties[key];
                _properties.Remove(key);

                value.Modified -= Property_Modified;
                value.NameChanged -= Property_NameChanged;

                OnPropertyRemoved(new PropertyEventArgs(value));
            }

            OnModified(EventArgs.Empty);
        }

        public bool Contains (Property item)
        {
            if (item == null)
                return false;

            return _properties.ContainsValue(item);
        }

        public bool Contains (string name)
        {
            if (name == null)
                return false;

            return _properties.ContainsKey(name);
        }

        public void CopyTo (Property[] array, int arrayIndex)
        {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (array.Length < _properties.Count) {
                throw new ArgumentException("array is too small to copy values to.");
            }
            if (arrayIndex < 0 || arrayIndex + _properties.Count > array.Length) {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            int index = 0;
            foreach (Property value in _properties.Values) {
                array[arrayIndex + index++] = value;
            }
        }

        public int Count
        {
            get { return _properties.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove (Property item)
        {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            if (!_properties.ContainsValue(item)) {
                return false;
            }

            _properties.Remove(item.Name);

            item.Modified -= Property_Modified;
            item.NameChanged -= Property_NameChanged;

            OnPropertyRemoved(new PropertyEventArgs(item));
            OnModified(EventArgs.Empty);
            return true;
        }

        public bool Remove (string name)
        {
            Property value;
            if (_properties.TryGetValue(name, out value)) {
                _properties.Remove(name);

                value.Modified -= Property_Modified;
                value.NameChanged -= Property_NameChanged;

                OnPropertyRemoved(new PropertyEventArgs(value));
                OnModified(EventArgs.Empty);
                return true;
            }

            return false;
        }

        #endregion

        #region IEnumerable<Property> Members

        public IEnumerator<Property> GetEnumerator ()
        {
            foreach (Property prop in _properties.Values) {
                yield return prop;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
