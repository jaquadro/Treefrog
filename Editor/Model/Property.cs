using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Editor.Model
{
    public abstract class Property : INamedResource
    {
        private string _name;

        #region Constructors

        public Property (string name)
        {
            _name = name;
        }

        #endregion

        #region Events

        public event EventHandler ValueChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnValueChanged (EventArgs e)
        {
            if (ValueChanged != null) {
                ValueChanged(this, e);
            }
        }

        #endregion

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

        protected virtual void OnNameChanged (NameChangedEventArgs e)
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

            string type = attribs.ContainsKey("type") ? attribs["type"] : "string";
            switch (type) {
                case "string":
                    return StringProperty.FromXml(reader, attribs["name"]);
            }

            return null;
        }

        public abstract void WriteXml (XmlWriter writer);

        #endregion
    }

    public class StringProperty : Property
    {
        private string _value;

        public StringProperty (string name, string value)
            : base(name)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value) {
                    _value = value;
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }

        #region XML Import / Export

        public static StringProperty FromXml (XmlReader reader, string name)
        {
            string value = reader.ReadString();
            return new StringProperty(name, value);
        }

        public override void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", Name);
            writer.WriteString(Value);
            writer.WriteEndElement();
        }

        #endregion
    }
}
