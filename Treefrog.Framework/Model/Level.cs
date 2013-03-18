using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Model.Support;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    /// <summary>
    /// Represents a complete level or map in the project.
    /// </summary>
    public class Level : INamedResource, IPropertyProvider
    {
        #region Fields

        private static string[] _reservedPropertyNames = { "Name", "OriginX", "OriginY", "Height", "Width" };

        private Project _project;
        private string _name;

        private int _x;
        private int _y;
        private int _width;
        private int _height;

        private OrderedResourceCollection<Layer> _layers;
        private PropertyCollection _properties;
        private LevelProperties _predefinedProperties;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a <see cref="Level"/> with default values and dimensions.
        /// </summary>
        /// <param name="name">A uniquely identifying name for the <see cref="Level"/>.</param>
        public Level (string name)
        {
            _name = name;

            _layers = new OrderedResourceCollection<Layer>();
            _properties = new PropertyCollection(_reservedPropertyNames); // new NamedResourceCollection<Property>();
            _predefinedProperties = new LevelProperties(this);

            _properties.Modified += CustomProperties_Modified;

            _layers.ResourceAdded += LayerAddedHandler;
            _layers.ResourceRemoved += LayerRemovedHandler;
            _layers.ResourceModified += LayersModifiedHandler;
        }

        public Level (string name, Project project)
            : this(name)
        {
            _project = project;
        }

        public Level (string name, int originX, int originY, int width, int height)
            : this(name)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Level must be created with positive area.");

            _x = originX;
            _y = originY;
            _width = width;
            _height = height;
        }

        #endregion

        #region Properties

        public Project Project
        {
            get { return _project; }
            set { _project = value; }
        }

        public int OriginX
        {
            get { return _x; }
        }

        public int OriginY
        {
            get { return _y; }
        }

        /// <summary>
        /// Gets the height of the level in pixels.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Gets the width of the level in pixels.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        /// <summary>
        /// Gets an ordered collection of <see cref="Layer"/> objects used in the level.
        /// </summary>
        public OrderedResourceCollection<Layer> Layers
        {
            get { return _layers; }
        }

        public void Resize (int originX, int originY, int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Level must be created with positive area.");

            _x = originX;
            _y = originY;
            _width = width;
            _height = height;

            foreach (Layer layer in _layers)
                layer.RequestNewSize(originX, originY, width, height);

            OnLevelSizeChanged(EventArgs.Empty);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the number of tiles in the level changes.
        /// </summary>
        public event EventHandler LevelSizeChanged;

        /// <summary>
        /// Occurs when either the tile or level dimensions change.
        /// </summary>
        public event EventHandler SizeChanged;

        /// <summary>
        /// Occurs when a new layer is added to the level.
        /// </summary>
        public event EventHandler<NamedResourceEventArgs<Layer>> LayerAdded;

        /// <summary>
        /// Occurs when a layer is removed from the level.
        /// </summary>
        public event EventHandler<NamedResourceEventArgs<Layer>> LayerRemoved;

        /// <summary>
        /// Occurs when the internal state of the Level is modified.
        /// </summary>
        public event EventHandler Modified;

        #endregion

        #region Event Dispatchers

        /// <summary>
        /// Raises the <see cref="LevelSizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnLevelSizeChanged (EventArgs e)
        {
            if (LevelSizeChanged != null) {
                LevelSizeChanged(this, e);
            }
            OnSizeChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="SizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnSizeChanged (EventArgs e)
        {
            if (SizeChanged != null) {
                SizeChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="LayerAdded"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NamedResourceEventArgs{Layer}"/> containing the name of the added layer.</param>
        protected virtual void OnLayerAdded (NamedResourceEventArgs<Layer> e)
        {
            if (LayerAdded != null) {
                LayerAdded(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="LayerRemoved"/> event.
        /// </summary>
        /// <param name="e">A <see cref="NamedResourceEventArgs{Layer}"/> containing the name of the removed layer.</param>
        protected virtual void OnLayerRemoved (NamedResourceEventArgs<Layer> e)
        {
            if (LayerRemoved != null) {
                LayerRemoved(this, e);
            }
            OnModified(EventArgs.Empty);
        }

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

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            Name = property.Value;
        }

        private void LayerAddedHandler (object sender, NamedResourceEventArgs<Layer> e)
        {
            OnLayerAdded(e);
        }

        private void LayerRemovedHandler (object sender, NamedResourceEventArgs<Layer> e)
        {
            OnLayerRemoved(e);
        }

        private void CustomProperties_Modified (object sender, EventArgs e)
        {
            OnModified(e);
        }

        private void LayersModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #endregion

        #region INamedResource Members

        /// <summary>
        /// Gets or sets the name of this <see cref="Level"/>.  This name also serves as a key in <see cref="NamedResourceCollection"/>s.
        /// </summary>
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

        /// <summary>
        /// Occurs when the <see cref="Name"/> of this level changes.
        /// </summary>
        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            if (NameChanging != null) {
                NameChanging(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="NameChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        #endregion

        #region IPropertyProvider Members

        private void PredefPropertyValueChangedHandler (object sender, EventArgs e)
        {

        }

        private class LevelProperties : PredefinedPropertyCollection
        {
            private Level _parent;

            public LevelProperties (Level parent)
                : base(_reservedPropertyNames)
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield return _parent.LookupProperty("Name");
                //yield return _parent.LookupProperty("OriginX");
                //yield return _parent.LookupProperty("OriginY");
                //yield return _parent.LookupProperty("Height");
                //yield return _parent.LookupProperty("Width");
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

        /// <summary>
        /// Gets the display name of this level in terms of a Property Provider.
        /// </summary>
        public string PropertyProviderName
        {
            get { return "Level." + _name; }
        }

        public PropertyCollection CustomProperties
        {
            get { return _properties; }
        }

        public PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties; }
        }

        /// <summary>
        /// Determines whether a given property is predefined, custom, or doesn't exist in this object.
        /// </summary>
        /// <param name="name">The name of a property to look up.</param>
        /// <returns>The category that the property falls into.</returns>
        public PropertyCategory LookupPropertyCategory (string name)
        {
            switch (name) {
                case "Name":
                //case "Left":
                //case "Right":
                //case "Top":
                //case "Bottom":
                    return PropertyCategory.Predefined;
                default:
                    return _properties.Contains(name) ? PropertyCategory.Custom : PropertyCategory.None;
            }
        }

        /// <summary>
        /// Returns a <see cref="Property"/> given its name.
        /// </summary>
        /// <param name="name">The name of a property to look up.</param>
        /// <returns>Returns either a predefined or custom <see cref="Property"/>, or <c>null</c> if the property doesn't exist.</returns>
        public Property LookupProperty (string name)
        {
            Property prop;

            switch (name) {
                case "Name":
                    prop = new StringProperty("Name", _name);
                    prop.ValueChanged += NamePropertyChangedHandler;
                    return prop;

                /*case "OriginX":
                    prop = new NumberProperty("OriginX", OriginX);
                    prop.ValueChanged += PredefPropertyValueChangedHandler;
                    return prop;

                case "OriginY":
                    prop = new NumberProperty("OriginY", Width);
                    prop.ValueChanged += PredefPropertyValueChangedHandler;
                    return prop;

                case "Height":
                    prop = new NumberProperty("Height", OriginY);
                    prop.ValueChanged += PredefPropertyValueChangedHandler;
                    return prop;

                case "Width":
                    prop = new NumberProperty("Width", Height);
                    prop.ValueChanged += PredefPropertyValueChangedHandler;
                    return prop;*/

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        #endregion

        [Obsolete]
        public static Level FromXmlProxy (LevelXmlProxy proxy, Project project)
        {
            if (proxy == null)
                return null;

            Level level = new Level(proxy.Name, proxy.OriginX, proxy.OriginY, proxy.Width, proxy.Height);
            level.Project = project;

            foreach (LayerXmlProxy layerProxy in proxy.Layers) {
                if (layerProxy is MultiTileGridLayerXmlProxy)
                    level.Layers.Add(new MultiTileGridLayer(layerProxy as MultiTileGridLayerXmlProxy, level));
                else if (layerProxy is ObjectLayerXmlProxy)
                    level.Layers.Add(new ObjectLayer(layerProxy as ObjectLayerXmlProxy, level));
            }

            foreach (PropertyXmlProxy propertyProxy in proxy.Properties)
                level.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));

            return level;
        }

        private int _lastLocalTileIndex;
        private Mapper<int, Guid> _localTileIndex = new Mapper<int,Guid>();

        public Mapper<int, Guid> TileIndex
        {
            get { return _localTileIndex; }
        }

        public static Level FromXmlProxy (LevelX proxy, Project project)
        {
            if (proxy == null)
                return null;

            Level level = new Level(proxy.Name, proxy.OriginX, proxy.OriginY, proxy.Width, proxy.Height);
            level.Project = project;

            Dictionary<int, Guid> tileIndex = new Dictionary<int, Guid>();
            foreach (var entry in proxy.TileIndex) {
                level._localTileIndex.Add(entry.Id, entry.Uid);
                level._lastLocalTileIndex = Math.Max(level._lastLocalTileIndex, entry.Id);
                tileIndex.Add(entry.Id, entry.Uid);
            }

            foreach (LevelX.LayerX layerProxy in proxy.Layers) {
                if (layerProxy is LevelX.MultiTileGridLayerX)
                    level.Layers.Add(new MultiTileGridLayer(layerProxy as LevelX.MultiTileGridLayerX, level, tileIndex));
                else if (layerProxy is LevelX.ObjectLayerX)
                    level.Layers.Add(new ObjectLayer(layerProxy as LevelX.ObjectLayerX, level));
            }

            foreach (var propertyProxy in proxy.Properties)
                level.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));

            return level;
        }

        [Obsolete]
        public static LevelXmlProxy ToXmlProxy (Level level)
        {
            if (level == null)
                return null;

            int index = 1;


            List<AbstractXmlSerializer<LayerXmlProxy>> layers = new List<AbstractXmlSerializer<LayerXmlProxy>>();
            foreach (Layer layer in level.Layers) {
                if (layer is MultiTileGridLayer)
                    layers.Add(MultiTileGridLayer.ToXmlProxy(layer as MultiTileGridLayer));
                else if (layer is ObjectLayer)
                    layers.Add(ObjectLayer.ToXmlProxy(layer as ObjectLayer));
            }

            return new LevelXmlProxy()
            {
                Name = level.Name,
                OriginX = level.OriginX,
                OriginY = level.OriginY,
                Width = level.Width,
                Height = level.Height,
                Layers = layers.Count > 0 ? layers : null,
            };
        }

        public static LevelX ToXmlProxyX (Level level)
        {
            if (level == null)
                return null;

            foreach (Layer layer in level.Layers) {
                if (layer is TileGridLayer) {
                    TileGridLayer tileLayer = layer as TileGridLayer;
                    foreach (LocatedTile tile in tileLayer.Tiles) {
                        if (!level._localTileIndex.ContainsValue(tile.Tile.Uid))
                            level._localTileIndex.Add(level._lastLocalTileIndex++, tile.Tile.Uid);
                    }
                }
            }

            List<LevelX.TileIndexEntryX> tileIndex = new List<LevelX.TileIndexEntryX>();
            foreach (var item in level._localTileIndex) {
                tileIndex.Add(new LevelX.TileIndexEntryX() {
                    Id = item.Key,
                    Uid = item.Value,
                });
            }

            List<AbstractXmlSerializer<LevelX.LayerX>> layers = new List<AbstractXmlSerializer<LevelX.LayerX>>();
            foreach (Layer layer in level.Layers) {
                if (layer is MultiTileGridLayer)
                    layers.Add(MultiTileGridLayer.ToXmlProxyX(layer as MultiTileGridLayer));
                else if (layer is ObjectLayer)
                    layers.Add(ObjectLayer.ToXmlProxyX(layer as ObjectLayer));
            }

            return new LevelX() {
                Name = level.Name,
                OriginX = level.OriginX,
                OriginY = level.OriginY,
                Width = level.Width,
                Height = level.Height,
                TileIndex = tileIndex.Count > 0 ? tileIndex : null,
                Layers = layers.Count > 0 ? layers : null,
            };
        }
    }

    [XmlRoot("Level")]
    public class LevelXmlProxy
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int OriginX { get; set; }

        [XmlAttribute]
        public int OriginY { get; set; }

        [XmlAttribute]
        public int Width { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlArray]
        [XmlArrayItem("Layer", Type = typeof(AbstractXmlSerializer<LayerXmlProxy>))]
        public List<AbstractXmlSerializer<LayerXmlProxy>> Layers { get; set; }

        [XmlArray]
        [XmlArrayItem("Property")]
        public List<PropertyXmlProxy> Properties { get; set; }
    }

    public class AbstractXmlSerializer<TBase> : IXmlSerializable
    {
        private AbstractXmlSerializer () { }

        public AbstractXmlSerializer (TBase data) 
        {
            Data = data;
        }

        public TBase Data { get; set; }

        public static implicit operator TBase (AbstractXmlSerializer<TBase> val)
        {
            return val.Data;
        }

        public static implicit operator AbstractXmlSerializer<TBase> (TBase val)
        {
            return val == null ? null : new AbstractXmlSerializer<TBase>(val);
        }

        public System.Xml.Schema.XmlSchema GetSchema ()
        {
            return null;
        }

        public void ReadXml (XmlReader reader) 
        {
            string typeAttr = reader.GetAttribute("Type");
            if (typeAttr == null)
                throw new ArgumentNullException("Unable to read Xml data for Abstract Type '" + typeof(TBase).Name +
                    " because no 'Type' attribute was specified in the Xml.");

            Type type = Type.GetType(typeAttr);
            if (type == null)
                throw new InvalidCastException("Unable to read Xml data for Abstract Type '" + typeof(TBase).Name +
                    " because the type specified in the Xml was not found ('" + typeAttr + "').");

            if (!type.IsSubclassOf(typeof(TBase)))
                throw new InvalidCastException("Unable to read Xml data for Abstract Type '" + typeof(TBase).Name +
                    " because the type in the specified Xml differs ('" + type.Name + "').");
            
            reader.ReadStartElement();
            XmlSerializer serializer = new XmlSerializer(type);
            Data = (TBase)serializer.Deserialize(reader);
            reader.ReadEndElement();
        }

        public void WriteXml (XmlWriter writer)
        {
            Type type = Data.GetType();

            writer.WriteAttributeString("Type", type.FullName);
            new XmlSerializer(type).Serialize(writer, Data);
        }
    }

    public abstract class LayerXmlProxy
    {
        protected LayerXmlProxy ()
        {
            Opacity = 1.0f;
            Visible = true;
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        [DefaultValue(1.0F)]
        public float Opacity { get; set; }

        [XmlAttribute]
        [DefaultValue(true)]
        public bool Visible { get; set; }

        [XmlAttribute]
        [DefaultValue(typeof(RasterMode), "Point")]
        public RasterMode RasterMode { get; set; }

        [XmlArray]
        [XmlArrayItem("Property")]
        public List<PropertyXmlProxy> Properties { get; set; }
    }

    [XmlRoot("LayerData")]
    public class MultiTileGridLayerXmlProxy : LayerXmlProxy
    {
        [XmlAttribute]
        public int TileWidth { get; set; }

        [XmlAttribute]
        public int TileHeight { get; set; }

        [XmlArray]
        [XmlArrayItem("Tile")]
        public List<TileStackXmlProxy> Tiles { get; set; }
    }

    [XmlRoot("LayerData")]
    public class ObjectLayerXmlProxy : LayerXmlProxy
    {
        [XmlArray]
        [XmlArrayItem("Object")]
        public List<ObjectInstanceXmlProxy> Objects { get; set; }
    }

    public class TileStackXmlProxy
    {
        [XmlAttribute]
        public string At { get; set; }

        [XmlText]
        public string Items { get; set; }
    }

    public class ObjectInstanceXmlProxy
    {
        [XmlAttribute]
        public int Class { get; set; }

        [XmlAttribute]
        public string At { get; set; }

        [XmlAttribute]
        public float Rotation { get; set; }

        [XmlArray]
        [XmlArrayItem("Property")]
        public List<PropertyXmlProxy> Properties { get; set; }
    }
}
