using System;
using System.Collections.Generic;
using System.Xml;
using Treefrog.Framework.Model.Collections;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Treefrog.Framework.Model
{
    /// <summary>
    /// Represents a complete level or map in the project.
    /// </summary>
    public class Level : INamedResource, IPropertyProvider
    {
        #region Fields

        private static string[] _reservedPropertyNames = { "Name", "TileWidth", "TileHeight" };

        private Project _project;
        private string _name;

        private int _tileWidth = 16;
        private int _tileHeight = 16;
        private int _tilesWide = 30;
        private int _tilesHigh = 20;

        private OrderedResourceCollection<Layer> _layers;
        //private NamedResourceCollection<Property> _properties;
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

        /// <summary>
        /// Creates a <see cref="Level"/> with given dimensions.
        /// </summary>
        /// <param name="name">A uniquely identifying name for the <see cref="Level"/>.</param>
        /// <param name="tileWidth">The width of tiles in the level.</param>
        /// <param name="tileHeight">The height of tiles in the level.</param>
        /// <param name="width">The width of the level, in tiles.</param>
        /// <param name="height">The height of the level, in tiles.</param>
        public Level (string name, int tileWidth, int tileHeight, int width, int height)
            : this(name)
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _tilesWide = width;
            _tilesHigh = height;
        }

        #endregion

        #region Properties

        public Project Project
        {
            get { return _project; }
            set { _project = value; }
        }

        /// <summary>
        /// Gets the height of the level in pixels.
        /// </summary>
        public int PixelsHigh
        {
            get { return _tilesHigh * _tileHeight; }
        }

        /// <summary>
        /// Gets the width of the level in pixels.
        /// </summary>
        public int PixelsWide
        {
            get { return _tilesWide * _tileWidth; }
        }

        /// <summary>
        /// Gets or sets the height of tiles used in the level.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the set value is not positive.</exception>
        public int TileHeight
        {
            get { return _tileHeight; }
            set
            {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("TileHeight", "Tile dimensions must be positive.");
                }

                if (_tileHeight != value) {
                    _tileHeight = value;
                    OnTileSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of tiles used in the level.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the set value is not positive.</exception>
        public int TileWidth
        {
            get { return _tileWidth; }
            set
            {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("TileWidth", "Tile dimensions must be positive.");
                }

                if (_tileWidth != value) {
                    _tileWidth = value;
                    OnTileSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of the level in tiles.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the set value is not positive.</exception>
        public int TilesHigh
        {
            get { return _tilesHigh; }
            set
            {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("TilesHigh", "Level dimensions must be positive.");
                }

                if (_tilesHigh != value) {
                    _tilesHigh = value;
                    OnLevelSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the level in tiles.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the set value is not positive.</exception>
        public int TilesWide
        {
            get { return _tilesWide; }
            set
            {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException("TilesWide", "Level dimensions must be positive.");
                }

                if (_tilesWide != value) {
                    _tilesWide = value;
                    OnLevelSizeChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets an ordered collection of <see cref="Layer"/> objects used in the level.
        /// </summary>
        public OrderedResourceCollection<Layer> Layers
        {
            get { return _layers; }
        }

        /// <summary>
        /// Gets a collection of <see cref="Property"/> objects used in the level.
        /// </summary>
        //public NamedResourceCollection<Property> Properties
        //{
        //    get { return _properties; }
        //}

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the number of tiles in the level changes.
        /// </summary>
        public event EventHandler LevelSizeChanged;

        /// <summary>
        /// Occurs when the dimension of the tiles in the level changes.
        /// </summary>
        public event EventHandler TileSizeChanged;
        
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
        /// Raises the <see cref="TileSizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnTileSizeChanged (EventArgs e)
        {
            if (TileSizeChanged != null) {
                TileSizeChanged(this, e);
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
            private set
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
                yield return _parent.LookupProperty("TileWidth");
                yield return _parent.LookupProperty("TileHeight");
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

        /// <summary>
        /// Gets an enumerator that returns all the pre-defined, "special" properties of the <see cref="Level"/>.
        /// </summary>
        //public IEnumerable<Property> PredefinedProperties
        //{
        //    get
        //    {
        //        yield return LookupProperty("name");
        //        yield return LookupProperty("tile_height");
        //        yield return LookupProperty("tile_width");
        //    }
        //}

        /// <summary>
        /// Gets an enumerator that returns all of the custom, user-defined properties on this particular <see cref="Level"/> object.
        /// </summary>
        //public IEnumerable<Property> CustomProperties
        //{
        //    get { return _properties; }
        //}

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
                case "TileHeight":
                case "TileWidth":
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

                case "TileHeight":
                    prop = new NumberProperty("TileHeight", _tileHeight);
                    prop.ValueChanged += PredefPropertyValueChangedHandler;
                    return prop;

                case "TileWidth":
                    prop = new NumberProperty("TileWidth", _tileWidth);
                    prop.ValueChanged += PredefPropertyValueChangedHandler;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        /// <summary>
        /// Adds a new custom property to the level.
        /// </summary>
        /// <param name="property">The named property to add to the level.</param>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
        /// <exception cref="ArgumentException">A custom property with the same name already exists in the <see cref="Level"/>.</exception>
        //public void AddCustomProperty (Property property)
        //{
        //    if (property == null) {
        //        throw new ArgumentNullException("The property is null.");
        //    }
        //    if (_properties.Contains(property.Name)) {
        //        throw new ArgumentException("A property with the same name already exists.");
        //    }

        //    _properties.Add(property);
        //}

        /// <summary>
        /// Removes a custom property from the level.
        /// </summary>
        /// <param name="name">The name of the property to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        //public void RemoveCustomProperty (string name)
        //{
        //    if (name == null) {
        //        throw new ArgumentNullException("The name is null.");
        //    }

        //    _properties.Remove(name);
        //}

        #endregion

        #region XML Import / Export

        /// <summary>
        /// Creates a new <see cref="Level"/> object from an XML data stream.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> currently set to a "Level" element.</param>
        /// <param name="services">A <see cref="Project"/>-level service provider.</param>
        /// <returns>A new <see cref="Level"/> object.</returns>
        public static Level FromXml (XmlReader reader, Project parent)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "name", "width", "height", "tilewidth", "tileheight",
            });

            Level level = new Level(attribs["name"], Convert.ToInt32(attribs["tilewidth"]), Convert.ToInt32(attribs["tileheight"]),
                Convert.ToInt32(attribs["width"]), Convert.ToInt32(attribs["height"]))
                {
                    Project = parent,
                };

            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "layers":
                        AddLayerFromXml(xmlr, level);
                        break;
                    case "properties":
                        AddPropertyFromXml(xmlr, level);
                        break;
                }
            });

            return level;
        }

        /// <summary>
        /// Writes an XML representation of this <see cref="Level"/> object to the given XML data stream.
        /// </summary>
        /// <param name="writer">An <see cref="XmlWriter"/> to write the level data into.</param>
        public void WriteXml (XmlWriter writer)
        {
            // <level name="" height="" width="">
            writer.WriteStartElement("level");
            writer.WriteAttributeString("name", _name);
            writer.WriteAttributeString("width", _tilesWide.ToString());
            writer.WriteAttributeString("height", _tilesHigh.ToString());
            writer.WriteAttributeString("tilewidth", _tileWidth.ToString());
            writer.WriteAttributeString("tileheight", _tileHeight.ToString());

            //   <layers>
            writer.WriteStartElement("layers");

            foreach (Layer layer in _layers) {
                layer.WriteXml(writer);
            }

            //   <properties> [optional]
            if (_properties.Count > 0) {
                writer.WriteStartElement("properties");

                foreach (Property property in _properties) {
                    property.WriteXml(writer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void AddLayerFromXml (XmlReader reader, Level level)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "layer":
                        level.Layers.Add(Layer.FromXml(xmlr, level));
                        break;
                }
            });
        }

        private static void AddPropertyFromXml (XmlReader reader, Level level)
        {
            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "property":
                        level.CustomProperties.Add(Property.FromXml(xmlr));
                        return false;
                    default:
                        return true;
                }
            });
        }

        #endregion 

        public static Level FromXmlProxy (LevelXmlProxy proxy, Project project)
        {
            if (proxy == null)
                return null;

            Level level = new Level(proxy.Name, proxy.TileWidth, proxy.TileHeight, proxy.Width, proxy.Height);
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

        public static LevelXmlProxy ToXmlProxy (Level level)
        {
            if (level == null)
                return null;

            List<LayerXmlProxy> layers = new List<LayerXmlProxy>();
            foreach (Layer layer in level.Layers) {
                if (layer is MultiTileGridLayer)
                    layers.Add(MultiTileGridLayer.ToXmlProxy(layer as MultiTileGridLayer));
                else if (layer is ObjectLayer)
                    layers.Add(ObjectLayer.ToXmlProxy(layer as ObjectLayer));
            }

            return new LevelXmlProxy()
            {
                Name = level.Name,
                Width = level.TilesWide,
                Height = level.TilesHigh,
                TileWidth = level.TileWidth,
                TileHeight = level.TileHeight,
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
        public int Width { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlAttribute]
        public int TileWidth { get; set; }

        [XmlAttribute]
        public int TileHeight { get; set; }

        [XmlArray]
        [XmlArrayItem("Layer", Type = typeof(AbstractXmlSerializer<LayerXmlProxy>))]
        public List<LayerXmlProxy> Layers { get; set; }

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

        [XmlArray]
        [XmlArrayItem("Property")]
        public List<PropertyXmlProxy> Properties { get; set; }
    }

    [XmlRoot("LayerData")]
    public class MultiTileGridLayerXmlProxy : LayerXmlProxy
    {
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
    }
}
