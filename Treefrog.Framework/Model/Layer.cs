using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using Treefrog.Framework.Model.Collections;

namespace Treefrog.Framework.Model
{
    public abstract class Layer : INamedResource, IPropertyProvider, ICloneable
    {
        #region Fields

        private static string[] _reservedPropertyNames = { "Name", "Opacity", "Visible" };

        private string _name;

        private float _opacity;
        private bool _visible;

        private PropertyCollection _properties;
        private LayerProperties _predefinedProperties;

        #endregion

        #region Constructors

        protected Layer (string name)
        {
            _opacity = 1f;
            _visible = true;

            _name = name;
            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new LayerProperties(this);

            _properties.Modified += CustomPropertiesModifiedHandler;
        }

        protected Layer (string name, Layer layer)
            : this(name)
        {
            foreach (Property prop in layer._properties) {
                _properties.Add(prop.Clone() as Property);
            }

            _opacity = layer._opacity;
            _visible = layer._visible;
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

                    OnVisibilityChanged(EventArgs.Empty);
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

                    OnOpacityChanged(EventArgs.Empty);
                    OnModified(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the internal state of the Layer is modified.
        /// </summary>
        public event EventHandler Modified = (s, e) => { };

        public event EventHandler OpacityChanged = (s, e) => { };

        public event EventHandler VisibilityChanged = (s, e) => { };

        #endregion

        #region Event Dispatchers

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            Modified(this, e);
        }

        protected virtual void OnOpacityChanged (EventArgs e)
        {
            OpacityChanged(this, e);
        }

        protected virtual void OnVisibilityChanged (EventArgs e)
        {
            VisibilityChanged(this, e);
        }

        #endregion

        #region Event Handlers

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            Name = property.Value;
        }

        private void OpacityPropertyChangedHandler (object sender, EventArgs e)
        {
            NumberProperty property = sender as NumberProperty;
            _opacity = MathHelper.Clamp(property.Value, 0f, 1f);

            OnOpacityChanged(e);
            OnModified(e);
        }

        private void VisiblePropertyChangedHandler (object sender, EventArgs e)
        {
            BoolProperty property = sender as BoolProperty;
            _visible = property.Value;

            OnVisibilityChanged(e);
            OnModified(e);
        }

        private void CustomPropertiesModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #endregion

        public event EventHandler LayerSizeChanged = (s, e) => { };

        protected virtual void OnLayerSizeChanged (EventArgs e)
        {
            LayerSizeChanged(this, e);
            OnModified(e);
        }

        /// <summary>
        /// Gets the layer's width in pixels.  Returns -1 if the layer does not provide size information.
        /// </summary>
        public virtual int LayerWidth
        {
            get { return -1; }
        }

        /// <summary>
        /// Gets the layer's height in pixels.  Returns -1 if the layer does not provide size information.
        /// </summary>
        public virtual int LayerHeight
        {
            get { return -1; }
        }

        /// <summary>
        /// Makes a request to the layer to resize itself to overall pixel dimensions equal to or greater than the requested size.
        /// </summary>
        /// <param name="pixelsWide">The requested minimum width of the layer in pixels.</param>
        /// <param name="pixelsHigh">The request minimum height of the layer in pixels.</param>
        /// <remarks>If an implementing layer's <see cref="IsResizable"/> property returns to, then it is required to honor
        /// the new size request.  Due to differences in layer resolution, an implementing layer may not be able to exactly
        /// match the requested pixel size.  However, an implementing layer is required to never pick a size smaller than the
        /// request.</remarks>
        public virtual void RequestNewSize (int pixelsWide, int pixelsHigh) { }

        /// <summary>
        /// If set to <c>true</c>, the layer will honor requests to resize itself.
        /// </summary>
        public virtual bool IsResizable
        {
            get { return false; }
        }

        #region IPropertyProvider Members

        private class LayerProperties : PredefinedPropertyCollection
        {
            private Layer _parent;

            public LayerProperties (Layer parent)
                : base(_reservedPropertyNames)
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield return _parent.LookupProperty("Name");
                yield return _parent.LookupProperty("Opacity");
                yield return _parent.LookupProperty("Visible");
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public string PropertyProviderName
        {
            get { return "Layer." + _name; }
        }

        public PropertyCollection CustomProperties
        {
            get { return _properties; }
        }

        public PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties; }
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
                    prop.ValueChanged += NamePropertyChangedHandler;
                    return prop;

                case "Opacity":
                    prop = new NumberProperty("Opacity", _opacity);
                    prop.ValueChanged += OpacityPropertyChangedHandler;
                    return prop;

                case "Visible":
                    prop = new BoolProperty("Visible", _visible);
                    prop.ValueChanged += VisiblePropertyChangedHandler;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
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

            if (attribs.ContainsKey("opacity")) {
                layer._opacity = MathHelper.Clamp(Convert.ToSingle(attribs["opacity"]), 0f, 1f);
            }

            if (attribs.ContainsKey("visible")) {
                layer._visible = Convert.ToBoolean(attribs["visible"]);
            }

            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) => {
                switch (s) {
                    case "properties":
                        AddPropertyFromXml(xmlr, layer);
                        return true;
                    default:
                        return layer.ReadXmlElement(xmlr, s, services);
                }
            });

            return layer;
        }

        public abstract void WriteXml(XmlWriter writer);

        protected virtual bool ReadXmlElement (XmlReader reader, string name, IServiceProvider services)
        {
            return true;
        }

        private static void AddPropertyFromXml (XmlReader reader, Layer layer)
        {
            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "property":
                        layer.CustomProperties.Add(Property.FromXml(xmlr));
                        return false;
                    default:
                        return true;
                }
            });
        }

        #endregion

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

        public event EventHandler<NameChangingEventArgs> NameChanging;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            if (NameChanging != null) {
                NameChanging(this, e);
            }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        #endregion

        #region ICloneable Members

        public abstract object Clone ();

        #endregion
    }
}
