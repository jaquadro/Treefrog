using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

namespace Treefrog.Framework.Model
{
    public abstract class Layer : INamedResource, IPropertyProvider
    {
        #region Fields

        private string _name;

        private float _opacity;
        private bool _visible;

        private NamedResourceCollection<Property> _properties;

        #endregion

        #region Constructors

        public Layer (string name)
        {
            _name = name;
            _properties = new NamedResourceCollection<Property>();
            _properties.Modified += CustomPropertiesModifiedHandler;
        }

        #endregion

        #region Properties

        public bool IsVisible
        {
            get { return _visible; }
            set
            {
                if (_visible != value) {
                    _visible = value;
                    OnModified(EventArgs.Empty);
                }
            }
        }

        public float Opacity
        {
            get { return _opacity; }
            set
            {
                float opac = MathHelper.Clamp(value, 0f, 1f);
                if (_opacity != opac) {
                    _opacity = opac;
                    OnModified(EventArgs.Empty);
                }
            }
        }

        public NamedResourceCollection<Property> Properties
        {
            get { return _properties; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the internal state of the Layer is modified.
        /// </summary>
        public event EventHandler Modified;

        #endregion

        #region Event Dispatchers

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        #endregion

        #region Event Handlers

        private void CustomPropertiesModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #endregion

        #region IPropertyProvider Members

        public string PropertyProviderName
        {
            get { return "Layer." + _name; }
        }

        public IEnumerable<Property> PredefinedProperties
        {
            get 
            {
                yield return LookupProperty("Name");
                yield return LookupProperty("Opacity");
                yield return LookupProperty("Visible");
            }
        }

        public IEnumerable<Property> CustomProperties
        {
            get { return _properties; }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            switch (name) {
                case "Name":
                case "Opacity":
                case "Visible":
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
                    return prop;

                case "Opacity":
                    prop = new NumberProperty("Opacity", _opacity);
                    return prop;

                case "Visible":
                    prop = new BoolProperty("Visible", _visible);
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        public void AddCustomProperty (Property property)
        {
            if (property == null) {
                throw new ArgumentNullException("The property is null.");
            }
            if (_properties.Contains(property.Name)) {
                throw new ArgumentException("A property with the same name already exists.");
            }

            _properties.Add(property);
        }

        public void RemoveCustomProperty (string name)
        {
            if (name == null) {
                throw new ArgumentNullException("The name is null.");
            }

            _properties.Remove(name);
        }

        #endregion

        #region XML Import / Export

        public static Layer FromXml (XmlReader reader, IServiceProvider services, Level level)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "name", "type",
            });

            Layer layer = null;
            switch (attribs["type"]) {
                case "tilemulti":
                    layer = new MultiTileGridLayer(attribs["name"], level.TileWidth, level.TileHeight, level.TilesWide, level.TilesHigh);
                    break;
            }

            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) => {
                return layer.ReadXmlElement(xmlr, s, services);
            });

            return layer;
        }

        public abstract void WriteXml(XmlWriter writer);

        protected virtual bool ReadXmlElement (XmlReader reader, string name, IServiceProvider services)
        {
            return true;
        }

        #endregion

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        #endregion
    }
}
