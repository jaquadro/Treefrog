using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Editor.Model
{
    class Property : INamedResource
    {
        private string _name;

        public Property (string name)
        {
            _name = name;
        }

        #region INamedResource Members

        public string  Name
        {
            get { return _name; }
            set {
                if (_name != value) {
                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, value));
                }
            }
        }

        public event EventHandler<NameChangedEventArgs>  NameChanged;

        protected void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        #endregion

        #region XML Import / Export

        public static Property FromXml (XmlReader reader)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "name",
            });

            string value = reader.ReadString();

            Property property = new Property(attribs["name"]);

            return property;
        }

        public void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", _name);
            writer.WriteString("");
            writer.WriteEndElement();
        }

        #endregion
    }

    //class PropertySet : IEnumerable<Property>
    class PropertySet : NamedResourceCollection<Property>
    {
        /*private Dictionary<string, Property> _properties;

        public PropertySet ()
        {
            _properties = new Dictionary<string, Property>();
        }

        #region Event Handlers

        private void PropertyNameChangedHandler (object sender, NameChangedEventArgs e)
        {
            if (e.OldName == e.NewName) {
                return;
            }

            if (!_properties.ContainsKey(e.OldName)) {
                throw new InvalidOperationException("No property with name '" + e.OldName + "' found in the PropertySet.");
            }
            if (_properties.ContainsKey(e.NewName)) {
                throw new InvalidOperationException("Tried to rename property '" + e.OldName + "' to '" + e.NewName + "', but a PropertySet it's in already has a property with that name.");
            }

            Property p = _properties[e.OldName];
            _properties.Remove(e.OldName);
            _properties.Add(e.NewName, p);
        }

        #endregion

        public void Add (Property property)
        {
            if (property == null) {
                throw new ArgumentNullException("property");
            }
            if (_properties.ContainsKey(property.Name)) {
                throw new ArgumentException("Property collection already contains a property with the name '" + property.Name + "'");
            }

            _properties.Add(property.Name, property);

            property.NameChanged += PropertyNameChangedHandler;
        }

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

        #endregion*/
    }
}
