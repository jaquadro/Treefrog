using System;
using System.Collections.Generic;
using System.Xml;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public enum RasterMode
    {
        Point,
        Linear,
    }

    public abstract class PropertyConverter
    {
        public abstract Property Pack (string name, object value);
        public abstract object Unpack (Property property);
    }

    public class RasterModePropertyConverter : PropertyConverter
    {
        public override Property Pack (string name, object value)
        {
            if (!(value is RasterMode))
                return new StringProperty(name, "");
            return new StringProperty(name, ((RasterMode)value).ToString());
        }

        public override object Unpack (Property property)
        {
            StringProperty sp = property as StringProperty;
            switch (sp.Value) {
                case "Point":
                    return Model.RasterMode.Point;
                default:
                    return Model.RasterMode.Linear;
            }
        }
    }

    public abstract class Layer : INamedResource, IPropertyProvider, ICloneable
    {
        private static PropertyClassManager _propertyClassManager = new PropertyClassManager(typeof(Layer));

        private static string[] _reservedPropertyNames = { "Name", "Opacity", "Visible", "RasterMode" };

        private Level _level;
        private string _name;

        private float _opacity;
        private bool _visible;
        private RasterMode _rasterMode;

        private int _gridWidth;
        private int _gridHeight;
        private Color _gridColor;

        private PropertyManager _propertyManager;
        private PropertyCollection _properties;
        private LayerProperties _predefinedProperties;

        protected Layer (string name)
        {
            Uid = Guid.NewGuid();

            _opacity = 1f;
            _visible = true;
            _rasterMode = RasterMode.Point;

            _gridWidth = 16;
            _gridHeight = 16;
            _gridColor = new Color(0, 0, 0, 128);

            _name = name;

            _propertyManager = new PropertyManager(_propertyClassManager, this);
            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new LayerProperties(this);

            _properties.Modified += (s, e) => OnModified(EventArgs.Empty);
        }

        protected Layer (string name, Layer layer)
            : this(name)
        {
            foreach (Property prop in layer._properties) {
                _properties.Add(prop.Clone() as Property);
            }

            _opacity = layer._opacity;
            _visible = layer._visible;
            _rasterMode = layer._rasterMode;
            _gridWidth = layer._gridWidth;
            _gridHeight = layer._gridHeight;
            _gridColor = layer._gridColor;
        }

        public Guid Uid { get; private set; }

        public Level Level
        {
            get { return _level; }
            set { _level = value; }
        }

        [SpecialProperty]
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

        [SpecialProperty]
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                float opac = Math.Min(1f, Math.Max(0f, value));
                if (_opacity != opac) {
                    _opacity = opac;

                    OnOpacityChanged(EventArgs.Empty);
                    OnModified(EventArgs.Empty);
                }
            }
        }

        [SpecialProperty(Converter=typeof(RasterModePropertyConverter))]
        public RasterMode RasterMode
        {
            get { return _rasterMode; }
            set
            {
                if (_rasterMode != value) {
                    _rasterMode = value;

                    OnRasterModeChanged(EventArgs.Empty);
                    OnModified(EventArgs.Empty);
                }
            }
        }

        public virtual int GridWidth
        {
            get { return _gridWidth; }
            set
            {
                if (_gridWidth != value) {
                    _gridWidth = value;

                    OnGridChanged(EventArgs.Empty);
                    OnModified(EventArgs.Empty);
                }
            }
        }

        public virtual int GridHeight
        {
            get { return _gridHeight; }
            set
            {
                if (_gridHeight != value) {
                    _gridHeight = value;

                    OnGridChanged(EventArgs.Empty);
                    OnModified(EventArgs.Empty);
                }
            }
        }

        public virtual bool GridIsIndependent
        {
            get { return true; }
        }

        public virtual Color GridColor
        {
            get { return _gridColor; }
            set
            {
                if (_gridColor != value) {
                    _gridColor = value;

                    OnGridChanged(EventArgs.Empty);
                    OnModified(EventArgs.Empty);
                }
            }
        }

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var property in CustomProperties)
                property.ResetModified();
        }

        /// <summary>
        /// Occurs when the internal state of the Layer is modified.
        /// </summary>
        public event EventHandler Modified;

        public event EventHandler OpacityChanged;

        public event EventHandler VisibilityChanged;

        public event EventHandler RasterModeChanged;

        public event EventHandler GridChanged;

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        protected virtual void OnOpacityChanged (EventArgs e)
        {
            var ev = OpacityChanged;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnVisibilityChanged (EventArgs e)
        {
            var ev = VisibilityChanged;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnRasterModeChanged (EventArgs e)
        {
            var ev = RasterModeChanged;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnGridChanged (EventArgs e)
        {
            var ev = GridChanged;
            if (ev != null)
                ev(this, e);
        }

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            Name = property.Value;
        }

        private void OpacityPropertyChangedHandler (object sender, EventArgs e)
        {
            NumberProperty property = sender as NumberProperty;
            _opacity = Math.Min(1f, Math.Max(0f, property.Value));

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

        private void RasterModePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            if (property.Value == "Point")
                RasterMode = Model.RasterMode.Point;
            else
                RasterMode = Model.RasterMode.Linear;
        }

        public event EventHandler LayerSizeChanged = (s, e) => { };

        protected virtual void OnLayerSizeChanged (EventArgs e)
        {
            LayerSizeChanged(this, e);
            OnModified(e);
        }

        public virtual int LayerOriginX
        {
            get { return -1; }
        }

        public virtual int LayerOriginY
        {
            get { return -1; }
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
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="pixelsWide">The requested minimum width of the layer in pixels.</param>
        /// <param name="pixelsHigh">The request minimum height of the layer in pixels.</param>
        /// <remarks>If an implementing layer's <see cref="IsResizable"/> property returns true, then it is required to honor
        /// the new size request.  Due to differences in layer resolution, an implementing layer may not be able to exactly
        /// match the requested pixel size.  However, an implementing layer is required to never pick a size smaller than the
        /// request.</remarks>
        public virtual void RequestNewSize (int originX, int originY, int pixelsWide, int pixelsHigh) { }

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
                yield return _parent.LookupProperty("RasterMode");
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged;

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            var ev = PropertyProviderNameChanged;
            if (ev != null)
                ev(this, e);
        }

        public PropertyManager PropertyManager
        {
            get { return _propertyManager; }
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
                case "RasterMode":
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

                case "RasterMode":
                    prop = new StringProperty("RasterMode", _rasterMode.ToString());
                    prop.ValueChanged += RasterModePropertyChangedHandler;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        #endregion

        #region INamedResource Members

        [SpecialProperty]
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
