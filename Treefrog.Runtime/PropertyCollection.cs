using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    public class PropertyCollection : ICollection<Property>
    {
        private Dictionary<string, Property> _properties;

        public PropertyCollection ()
        {
            _properties = new Dictionary<string, Property>();
        }

        internal PropertyCollection (ContentReader reader)
            : this()
        {
            int propCount = reader.ReadInt16();

            for (int i = 0; i < propCount; i++) {
                string name = reader.ReadString();
                string value = reader.ReadString();

                _properties.Add(name, new Property(name, value));
            }
        }

        public Property this[string name]
        {
            get
            {
                Property p;
                if (TryGetValue(name, out p)) {
                    return p;
                }
                return null;
            }
        }

        public bool TryGetValue (string name, out Property property)
        {
            return _properties.TryGetValue(name, out property);
        }

        #region ICollection<Property> Members

        public void Add (Property item)
        {
            _properties.Add(item.Name, item);
        }

        public void Clear ()
        {
            _properties.Clear();
        }

        public bool Contains (Property item)
        {
            return _properties.ContainsKey(item.Name);
        }

        public void CopyTo (Property[] array, int arrayIndex)
        {
            int i = 0;
            foreach (Property prop in _properties.Values) {
                array[arrayIndex + i] = prop;
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
            return _properties.Remove(item.Name);
        }

        #endregion

        #region IEnumerable<Property> Members

        public IEnumerator<Property> GetEnumerator ()
        {
            return _properties.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return _properties.Values.GetEnumerator();
        }

        #endregion
    }
}
