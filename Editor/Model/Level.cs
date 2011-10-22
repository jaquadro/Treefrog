using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Editor.Model
{
    public class Level : INamedResource, IPropertyProvider
    {
        #region Fields

        private string _name;

        private int _tileWidth;
        private int _tileHeight;
        private int _tilesWide;
        private int _tilesHigh;

        //private List<Layer> _layers;
        private OrderedResourceCollection<Layer> _layers;
        private NamedResourceCollection<Property> _properties;

        #endregion

        #region Constructors

        public Level (string name)
        {
            _name = name;

            _layers = new OrderedResourceCollection<Layer>();
            _properties = new NamedResourceCollection<Property>();
        }

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

        public int PixelsHigh
        {
            get { return _tilesHigh * _tileHeight; }
        }

        public int PixelsWide
        {
            get { return _tilesWide * _tileWidth; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
            set { _tileHeight = value; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
            set { _tileWidth = value; }
        }

        public int TilesHigh
        {
            get { return _tilesHigh; }
            set { _tilesHigh = value; }
        }

        public int TilesWide
        {
            get { return _tilesWide; }
            set { _tilesWide = value; }
        }

        public OrderedResourceCollection<Layer> Layers
        {
            get { return _layers; }
        }

        public NamedResourceCollection<Property> Properties
        {
            get { return _properties; }
        }

        #endregion

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
            private set
            {
                if (_name != value) {
                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, _name));
                }
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

        #region Event Handlers

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            Name = property.Value;
        }

        #endregion

        #region XML Import / Export

        public static Level FromXml (XmlReader reader, IServiceProvider services)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "name", "width", "height", "tilewidth", "tileheight",
            });

            Level level = new Level(attribs["name"], Convert.ToInt32(attribs["tilewidth"]), Convert.ToInt32(attribs["tileheight"]),
                Convert.ToInt32(attribs["width"]), Convert.ToInt32(attribs["height"]));

            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "layers":
                        AddLayerFromXml(xmlr, services, level);
                        break;
                    case "properties":
                        AddPropertyFromXml(xmlr, level);
                        break;
                }
            });

            return level;
        }

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
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void AddLayerFromXml (XmlReader reader, IServiceProvider services, Level level)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "layer":
                        level.Layers.Add(Layer.FromXml(xmlr, services, level));
                        break;
                }
            });
        }

        private static void AddPropertyFromXml (XmlReader reader, Level level)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "property":
                        level.Properties.Add(Property.FromXml(xmlr));
                        break;
                }
            });
        }

        #endregion

        #region IPropertyProvider Members

        private void PredefPropertyValueChangedHandler (object sender, EventArgs e)
        {

        }

        public string PropertyProviderName
        {
            get { return "Level." + _name; }
        }

        public IEnumerable<Property> PredefinedProperties
        {
            get
            {
                Property prop;

                prop = new StringProperty("Name", _name);
                prop.ValueChanged += NamePropertyChangedHandler;
                yield return prop;

                prop = new StringProperty("Tile Height", _tileHeight.ToString());
                prop.ValueChanged += PredefPropertyValueChangedHandler;
                yield return prop;

                prop = new StringProperty("Tile Width", _tileWidth.ToString());
                prop.ValueChanged += PredefPropertyValueChangedHandler;
                yield return prop;
            }
        }

        public IEnumerable<Property> CustomProperties
        {
            get { return _properties; }
        }

        public void AddCustomProperty (Property property)
        {
            throw new NotImplementedException();
        }

        public void RemoveCustomProperty (string name)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
