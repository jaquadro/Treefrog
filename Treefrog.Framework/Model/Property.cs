using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Proxy;
using Treefrog.Framework.Model.Collections;
using System.Reflection;

namespace Treefrog.Framework.Model
{
    /// <summary>
    /// A generic named property base class.
    /// </summary>
    [Serializable]
    public abstract class Property : INamedResource, ICloneable
    {
        private readonly Guid _uid;
        private readonly ResourceName _name;

        /// <summary>
        /// Creates a new <see cref="Property"/> instance with a given name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        protected Property (string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("Property names cannot be empty.");

            _uid = Guid.NewGuid();
            _name = new ResourceName(this, name);
        }

        protected Property (string name, Property property)
            : this(name)
        {

        }

        public Guid Uid
        {
            get { return _uid; }
        }

        /// <summary>
        /// Occurs when the property's underlying value is changed.
        /// </summary>
        public event EventHandler ValueChanged;

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

        /// <summary>
        /// Parses a given string value into the underlying data type and assign it as the property's value.
        /// </summary>
        /// <param name="value">A string representation of the value to assign.</param>
        /// <exception cref="ArgumentException">Thrown when the underlying conversion fails.  Check InnerException for specific failure information.</exception>
        public abstract void Parse (string value);

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
        }

        /// <summary>
        /// Occurs when the property is modified.
        /// </summary>
        public event EventHandler Modified;

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                if (Modified != null) {
                    Modified(this, e);
                }
            }
        }

        public abstract object Clone ();

        #region Name Interface

        public event EventHandler<NameChangingEventArgs> NameChanging
        {
            add { _name.NameChanging += value; }
            remove { _name.NameChanging -= value; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged
        {
            add { _name.NameChanged += value; }
            remove { _name.NameChanged -= value; }
        }

        public string Name
        {
            get { return _name.Name; }
        }

        public bool TrySetName (string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new Exception("Property names cannot be empty.");

            bool result = _name.TrySetName(name);
            if (result)
                OnModified(EventArgs.Empty);

            return result;
        }

        #endregion

        public static Property FromXmlProxy (CommonX.PropertyX proxy)
        {
            if (proxy == null)
                return null;

            return StringProperty.FromXProxy(proxy);
        }

        public static CommonX.PropertyX ToXmlProxyX (Property property)
        {
            if (property == null)
                return null;

            return new CommonX.PropertyX() {
                Name = property.Name,
                Value = property.ToString(),
            };
        }
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a <see cref="String"/> value.
    /// </summary>
    [Serializable]
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

        private StringProperty (CommonX.PropertyX proxy)
            : base(proxy.Name)
        {
            _value = proxy.Value;
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

        public override object Clone ()
        {
            return new StringProperty(Name, this);
        }

        public static StringProperty FromXProxy (CommonX.PropertyX proxy)
        {
            if (proxy == null)
                return null;

            return new StringProperty(proxy);
        }
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a numeric <see cref="Single"/> value.
    /// </summary>
    [Serializable]
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

        public override object Clone ()
        {
            return new NumberProperty(Name, this);
        }
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a <see cref="Boolean"/> value.
    /// </summary>
    [Serializable]
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

        public override object Clone ()
        {
            return new BoolProperty(Name, this);
        }
    }

    /// <summary>
    /// A concrete <see cref="Property"/> type that represents a <see cref="Color"/> value.
    /// </summary>
    [Serializable]
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

        public override object Clone ()
        {
            return new ColorProperty(Name, this);
        }
    }
}
