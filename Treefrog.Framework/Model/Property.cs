using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class PropertyXmlProxy
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    /// <summary>
    /// A generic named property base class.
    /// </summary>
    public abstract class Property : INamedResource, ICloneable
    {
        #region Fields

        private string _name;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="Property"/> instance with a given name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        protected Property (string name)
        {
            if (name == null) {
                throw new ArgumentNullException("Property names cannot be null.");
            }
            _name = name;
        }

        protected Property (string name, Property property)
            : this(name)
        {

        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the property's underlying value is changed.
        /// </summary>
        public event EventHandler ValueChanged;

        #endregion

        #region Event Dispatchers

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnValueChanged (EventArgs e)
        {
            if (ValueChanged != null) {
                ValueChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        #endregion

        /// <summary>
        /// Parses a given string value into the underlying data type and assign it as the property's value.
        /// </summary>
        /// <param name="value">A string representation of the value to assign.</param>
        /// <exception cref="ArgumentException">Thrown when the underlying conversion fails.  Check InnerException for specific failure information.</exception>
        public abstract void Parse (string value);

        #region INamedResource Members

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string  Name
        {
            get { return _name; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("Property names cannot be null.");
                }
                if (_name != value) {
                    NameChangingEventArgs ea = new NameChangingEventArgs(_name, value);
                    OnNameChanging(ea);
                    if (ea.Cancel)
                        return;

                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, value));
                }
            }
        }

        public event EventHandler<NameChangingEventArgs> NameChanging;

        /// <summary>
        /// Occurs when the property's name is changed.
        /// </summary>
        public event EventHandler<NameChangedEventArgs>  NameChanged;

        /// <summary>
        /// Occurs when the property is modified.
        /// </summary>
        public event EventHandler Modified;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            if (NameChanging != null) {
                NameChanging(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="NameChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        #endregion

        #region XML Import / Export

        /// <summary>
        /// Creates a new <see cref="Property"/> object from an XML data stream.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> object currently set to a "Property" element.</param>
        /// <returns>A concrete <see cref="Property"/> object, dependent on the type attribute of the property element that was parsed.</returns>
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

        /// <summary>
        /// Writes an XML representation of the concrete <see cref="Property"/> instance to the given XML data stream.
        /// </summary>
        /// <param name="writer">An <see cref="XmlWriter"/> to write the property data into.</param>
        public abstract void WriteXml (XmlWriter writer);

        #endregion

        #region ICloneable Members

        public abstract object Clone ();

        #endregion

        public static Property FromXmlProxy (PropertyXmlProxy proxy)
        {
            if (proxy == null)
                return null;

            return new StringProperty(proxy.Name, proxy.Value);
        }

        public static PropertyXmlProxy ToXmlProxy (Property property)
        {
            if (property == null)
                return null;

            return new PropertyXmlProxy()
            {
                Name = property.Name,
                Value = property.ToString(),
            };
        }
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a <see cref="String"/> value.
    /// </summary>
    public class StringProperty : Property
    {
        private string _value;

        /// <summary>
        /// Creates a new <see cref="StringProperty"/> instance from a given name and value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public StringProperty (string name, string value)
            : base(name)
        {
            _value = value;
        }

        public StringProperty (string name, StringProperty property)
            : base(name, property)
        {
            _value = property._value;
        }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
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

        /// <inherit/>
        public override void Parse (string value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns the property's value as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of the property's value.</returns>
        public override string ToString ()
        {
            return _value;
        }

        #region XML Import / Export

        /// <summary>
        /// Creates a new <see cref="StringProperty"/> object from an XML data stream.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> object currently set to a "Property" element.</param>
        /// <param name="name">The name to give the new <see cref="StringProperty"/>.</param>
        /// <returns>A <see cref="StringProperty"/> object with the given name and XML-derived value.</returns>
        public static StringProperty FromXml (XmlReader reader, string name)
        {
            string value = reader.ReadElementContentAsString();
            return new StringProperty(name, value);
        }

        /// <summary>
        /// Writes an XML representation of the <see cref="StringProperty"/> instance to the given XML data stream.
        /// </summary>
        /// <param name="writer">An <see cref="XmlWriter"/> to write the property data into.</param>
        public override void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", Name);
            writer.WriteString(Value);
            writer.WriteEndElement();
        }

        #endregion

        #region ICloneable Members

        public override object Clone ()
        {
            return new StringProperty(Name, this);
        }

        #endregion
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a numeric <see cref="Single"/> value.
    /// </summary>
    public class NumberProperty : Property
    {
        private float _value;

        /// <summary>
        /// Creates a new <see cref="NumberProperty"/> instance from a given name and value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public NumberProperty (string name, float value)
            : base(name)
        {
            _value = value;
        }

        public NumberProperty (string name, NumberProperty property)
            : base(name, property)
        {
            _value = property._value;
        }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
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

        /// <inherit/>
        public override void Parse (string value)
        {
            try {
                Value = Convert.ToSingle(value);
            }
            catch (Exception e) {
                throw new ArgumentException("Failed to convert value to this property's value type.", e);
            }
        }

        /// <summary>
        /// Returns the property's value as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of the property's value.</returns>
        public override string ToString ()
        {
            return _value.ToString("0.###");
        }

        #region XML Import / Export

        /// <summary>
        /// Creates a new <see cref="NumberProperty"/> object from an XML data stream.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> object currently set to a "Property" element.</param>
        /// <param name="name">The name to give the new <see cref="NumberProperty"/>.</param>
        /// <returns>A <see cref="NumberProperty"/> object with the given name and XML-derived value.</returns>
        public static NumberProperty FromXml (XmlReader reader, string name)
        {
            float value = reader.ReadElementContentAsFloat();
            return new NumberProperty(name, value);
        }

        /// <summary>
        /// Writes an XML representation of the <see cref="NumberProperty"/> instance to the given XML data stream.
        /// </summary>
        /// <param name="writer">An <see cref="XmlWriter"/> to write the property data into.</param>
        public override void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "number");
            writer.WriteValue(Value);
            writer.WriteEndElement();
        }

        #endregion

        #region ICloneable Members

        public override object Clone ()
        {
            return new NumberProperty(Name, this);
        }

        #endregion
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a <see cref="Boolean"/> value.
    /// </summary>
    public class BoolProperty : Property
    {
        private bool _value;

        /// <summary>
        /// Creates a new <see cref="BoolProperty"/> instance from a given name and value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public BoolProperty (string name, bool value)
            : base(name)
        {
            _value = value;
        }

        public BoolProperty (string name, BoolProperty property)
            : base(name, property)
        {
            _value = property._value;
        }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
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

        /// <inherit/>
        public override void Parse (string value)
        {
            try {
                Value = Convert.ToBoolean(value);
            }
            catch (Exception e) {
                throw new ArgumentException("Failed to convert value to this property's value type.", e);
            }
        }

        /// <summary>
        /// Returns the property's value as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of the property's value.</returns>
        public override string ToString ()
        {
            return _value ? "true" : "false";
        }

        #region XML Import / Export

        /// <summary>
        /// Creates a new <see cref="CoolProperty"/> object from an XML data stream.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> object currently set to a "Property" element.</param>
        /// <param name="name">The name to give the new <see cref="BoolProperty"/>.</param>
        /// <returns>A <see cref="BoolProperty"/> object with the given name and XML-derived value.</returns>
        public static BoolProperty FromXml (XmlReader reader, string name)
        {
            bool value = reader.ReadElementContentAsBoolean();
            return new BoolProperty(name, value);
        }

        /// <summary>
        /// Writes an XML representation of the <see cref="BoolProperty"/> instance to the given XML data stream.
        /// </summary>
        /// <param name="writer">An <see cref="XmlWriter"/> to write the property data into.</param>
        public override void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "flag");
            writer.WriteValue(Value);
            writer.WriteEndElement();
        }

        #endregion

        #region ICloneable Members

        public override object Clone ()
        {
            return new BoolProperty(Name, this);
        }

        #endregion
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a <see cref="Color"/> value.
    /// </summary>
    public class ColorProperty : Property
    {
        private Color _value;

        /// <summary>
        /// Creates a new <see cref="NumberProperty"/> instance from a given name and value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        public ColorProperty (string name, Color value)
            : base(name)
        {
            _value = value;
        }

        public ColorProperty (string name, ColorProperty property)
            : base(name, property)
        {
            _value = property._value;
        }

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        public Color Value
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

        /// <inherit/>
        public override void Parse (string value)
        {
            try {
                Value = Color.ParseArgbHex(value);
            }
            catch (Exception e) {
                throw new ArgumentException("Failed to convert value to this property's value type.", e);
            }
        }

        /// <summary>
        /// Returns the property's value as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> representation of the property's value.</returns>
        public override string ToString ()
        {
            return _value.ToArgbHex();
        }

        #region XML Import / Export

        /// <summary>
        /// Creates a new <see cref="NumberProperty"/> object from an XML data stream.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> object currently set to a "Property" element.</param>
        /// <param name="name">The name to give the new <see cref="NumberProperty"/>.</param>
        /// <returns>A <see cref="NumberProperty"/> object with the given name and XML-derived value.</returns>
        public static ColorProperty FromXml (XmlReader reader, string name)
        {
            string value = reader.ReadElementContentAsString();
            ColorProperty prop = new ColorProperty(name, Colors.Black);
            prop.Parse(value);

            return prop;
        }

        /// <summary>
        /// Writes an XML representation of the <see cref="NumberProperty"/> instance to the given XML data stream.
        /// </summary>
        /// <param name="writer">An <see cref="XmlWriter"/> to write the property data into.</param>
        public override void WriteXml (XmlWriter writer)
        {
            writer.WriteStartElement("property");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "color");
            writer.WriteValue(Value.ToArgbHex());
            writer.WriteEndElement();
        }

        #endregion

        #region ICloneable Members

        public override object Clone ()
        {
            return new ColorProperty(Name, this);
        }

        #endregion
    }
}
