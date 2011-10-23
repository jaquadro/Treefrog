using System;
using System.Collections.Generic;
using System.Xml;

namespace Treefrog.Framework.Model
{
    public abstract class Property : INamedResource
    {
        #region Fields

        private string _name;

        #endregion

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

        public abstract void Parse (string value);

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
                case "number":
                    return NumberProperty.FromXml(reader, attribs["name"]);
                case "flag":
                    return BoolProperty.FromXml(reader, attribs["name"]);
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

        public override void Parse (string value)
        {
            Value = value;
        }

        public override string ToString ()
        {
            return _value;
        }

        #region XML Import / Export

        public static StringProperty FromXml (XmlReader reader, string name)
        {
            string value = reader.ReadElementContentAsString();
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

    public class NumberProperty : Property
    {
        private float _value;

        public NumberProperty (string name, float value)
            : base(name)
        {
            _value = value;
        }

        public float Value
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

        public override void Parse (string value)
        {
            Value = Convert.ToSingle(value);
        }

        public override string ToString ()
        {
            return _value.ToString("#.###");
        }

        #region XML Import / Export

        public static NumberProperty FromXml (XmlReader reader, string name)
        {
            float value = reader.ReadElementContentAsFloat();
            return new NumberProperty(name, value);
        }

        public override void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "number");
            writer.WriteValue(Value);
            writer.WriteEndElement();
        }

        #endregion
    }

    public class BoolProperty : Property
    {
        private bool _value;

        public BoolProperty (string name, bool value)
            : base(name)
        {
            _value = value;
        }

        public bool Value
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

        public override void Parse (string value)
        {
            Value = Convert.ToBoolean(value);
        }

        public override string ToString ()
        {
            return _value.ToString();
        }

        #region XML Import / Export

        public static BoolProperty FromXml (XmlReader reader, string name)
        {
            bool value = reader.ReadElementContentAsBoolean();
            return new BoolProperty(name, value);
        }

        public override void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "flag");
            writer.WriteValue(Value);
            writer.WriteEndElement();
        }

        #endregion
    }
}
