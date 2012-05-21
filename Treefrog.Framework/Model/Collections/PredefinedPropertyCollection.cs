using System;
using System.Collections.Generic;

namespace Treefrog.Framework.Model.Collections
{
    public abstract class PredefinedPropertyCollection : ICollection<Property>
    {
        private List<string> _names;

        protected PredefinedPropertyCollection (IEnumerable<string> names)
        {
            _names = new List<string>(names);
        }

        public Property this[string name]
        {
            get { return LookupProperty(name); }
        }

        protected abstract IEnumerable<Property> PredefinedProperties ();

        protected abstract Property LookupProperty (string name);

        #region ICollection<Property> Members

        public bool Contains (Property item)
        {
            if (item == null) {
                return false;
            }

            return _names.Contains(item.Name);
        }

        public bool Contains (string name)
        {
            return _names.Contains(name);
        }

        public void CopyTo (Property[] array, int arrayIndex)
        {
            if (array == null) {
                throw new ArgumentNullException("array");
            }
            if (array.Length < _names.Count) {
                throw new ArgumentException("array is too small to copy values to.");
            }
            if (arrayIndex < 0 || arrayIndex + _names.Count > array.Length) {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }

            int index = 0;
            foreach (Property prop in this) {
                array[arrayIndex + index++] = prop;
            }
        }

        public int Count
        {
            get { return _names.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        void ICollection<Property>.Add (Property item)
        {
            throw new NotImplementedException();
        }

        void ICollection<Property>.Clear ()
        {
            throw new NotImplementedException();
        }

        bool ICollection<Property>.Remove (Property item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<Property> Members

        public IEnumerator<Property> GetEnumerator ()
        {
            return PredefinedProperties().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return PredefinedProperties().GetEnumerator();
        }

        #endregion
    }
}
