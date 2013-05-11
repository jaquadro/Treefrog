using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Model.Support;
using Treefrog.Framework.Model.Proxy;
using System.IO;

namespace Treefrog.Framework.Model
{
    /// <summary>
    /// Represents a complete level or map in the project.
    /// </summary>
    public class Level : INamedResource, IPropertyProvider
    {
        private static PropertyClassManager _propertyClassManager = new PropertyClassManager(typeof(Level));

        private Project _project;

        private readonly Guid _uid;
        private readonly ResourceName _name;

        private int _x;
        private int _y;
        private int _width;
        private int _height;

        private int _indexSequence;
        private Mapper<int, Guid> _localTileIndex = new Mapper<int, Guid>();

        private OrderedResourceCollection<Layer> _layers;
        private PropertyManager _propertyManager;

        private Level (Guid uid, string name)
        {
            _uid = uid;
            _name = new ResourceName(this, name);

            _layers = new OrderedResourceCollection<Layer>();
            _layers.ResourceAdded += (s, e) => OnLayerAdded(new ResourceEventArgs<Layer>(e.Resource));
            _layers.ResourceRemoved += (s, e) => OnLayerRemoved(new ResourceEventArgs<Layer>(e.Resource));
            _layers.ResourceModified += (s, e) => OnModified(EventArgs.Empty);

            _propertyManager = new PropertyManager(_propertyClassManager, this);
            _propertyManager.CustomProperties.Modified += (s, e) => OnModified(EventArgs.Empty);       
        }

        /// <summary>
        /// Creates a <see cref="Level"/> with default values and dimensions.
        /// </summary>
        /// <param name="name">A uniquely identifying name for the <see cref="Level"/>.</param>
        public Level (string name)
            : this(Guid.NewGuid(), name)
        { }

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

        public Level (Level level, Project project)
            : this(Guid.NewGuid(), Guid.NewGuid().ToString())
        {
            LevelX proxy = ToXProxy(level);
            Initialize(proxy, project);
        }

        public Level (Stream stream, Project project)
            : this(Guid.NewGuid(), "Level")
        {
            Extra = new List<XmlElement>();

            XmlReaderSettings settings = new XmlReaderSettings() {
                CloseInput = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using (XmlReader reader = XmlTextReader.Create(stream, settings)) {
                XmlSerializer serializer = new XmlSerializer(typeof(LevelX));
                LevelX proxy = serializer.Deserialize(reader) as LevelX;

                if (proxy.PropertyGroup != null)
                    _uid = proxy.PropertyGroup.LevelGuid;
                _name = new ResourceName(this, proxy.Name);

                Initialize(proxy, project);
            }
        }

        private void Initialize (LevelX proxy, Project project)
        {
            if (proxy.PropertyGroup != null) {
                Extra = new List<XmlElement>(proxy.PropertyGroup.Extra ?? new XmlElement[0]);
            }

            _project = project;
            _x = proxy.OriginX;
            _y = proxy.OriginY;
            _width = Math.Max(1, proxy.Width);
            _height = Math.Max(1, proxy.Height);
            _indexSequence = proxy.TileIndex.Sequence;

            Dictionary<int, Guid> tileIndex = new Dictionary<int, Guid>();
            foreach (var entry in proxy.TileIndex.Entries) {
                _localTileIndex.Add(entry.Id, entry.Uid);
                tileIndex.Add(entry.Id, entry.Uid);

                if (entry.Id >= _indexSequence)
                    _indexSequence = entry.Id + 1;
            }

            foreach (LevelX.LayerX layerProxy in proxy.Layers) {
                if (layerProxy is LevelX.MultiTileGridLayerX)
                    Layers.Add(new MultiTileGridLayer(layerProxy as LevelX.MultiTileGridLayerX, this, tileIndex));
                else if (layerProxy is LevelX.ObjectLayerX)
                    Layers.Add(new ObjectLayer(layerProxy as LevelX.ObjectLayerX, this));
            }

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    PropertyManager.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public List<XmlElement> Extra { get; private set; }

        public string FileName { get; set; }

        public Project Project
        {
            get { return _project; }
            set { _project = value; }
        }

        [SpecialProperty]
        public int OriginX
        {
            get { return _x; }
        }

        [SpecialProperty]
        public int OriginY
        {
            get { return _y; }
        }

        /// <summary>
        /// Gets the height of the level in pixels.
        /// </summary>
        [SpecialProperty]
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Gets the width of the level in pixels.
        /// </summary>
        [SpecialProperty]
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

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var layer in Layers)
                layer.ResetModified();
            foreach (var property in PropertyManager.CustomProperties)
                property.ResetModified();
        }

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
        public event EventHandler<ResourceEventArgs<Layer>> LayerAdded;

        /// <summary>
        /// Occurs when a layer is removed from the level.
        /// </summary>
        public event EventHandler<ResourceEventArgs<Layer>> LayerRemoved;

        /// <summary>
        /// Occurs when the internal state of the Level is modified.
        /// </summary>
        public event EventHandler Modified;

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
        protected virtual void OnLayerAdded (ResourceEventArgs<Layer> e)
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
        protected virtual void OnLayerRemoved (ResourceEventArgs<Layer> e)
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
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            TrySetName(property.Value);
        }

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

        [SpecialProperty]
        public string Name
        {
            get { return _name.Name; }
        }

        public bool TrySetName (string name)
        {
            bool result = _name.TrySetName(name);
            if (result)
                OnModified(EventArgs.Empty);

            return result;
        }

        #endregion

        #region IPropertyProvider Members

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public PropertyManager PropertyManager
        {
            get { return _propertyManager; }
        }

        /// <summary>
        /// Gets the display name of this level in terms of a Property Provider.
        /// </summary>
        public string PropertyProviderName
        {
            get { return "Level." + _name; }
        }

        #endregion

        public Mapper<int, Guid> TileIndex
        {
            get { return _localTileIndex; }
        }

        public void Save (Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings() {
                CloseOutput = true,
                Indent = true,
            };

            using (XmlWriter writer = XmlTextWriter.Create(stream, settings)) {
                LevelX proxy = ToXProxy(this);

                XmlSerializer serializer = new XmlSerializer(typeof(LevelX));
                serializer.Serialize(writer, proxy);
            }

            ResetModified();
        }

        public static Level FromXProxy (LevelX proxy, Project project)
        {
            if (proxy == null)
                return null;

            Guid uid = proxy.PropertyGroup != null ? proxy.PropertyGroup.LevelGuid : Guid.NewGuid();

            Level level = new Level(uid, proxy.Name);
            level.Initialize(proxy, project);

            return level;
        }

        public static LevelX ToXProxy (Level level)
        {
            if (level == null)
                return null;

            foreach (Layer layer in level.Layers) {
                if (layer is TileGridLayer) {
                    TileGridLayer tileLayer = layer as TileGridLayer;
                    foreach (LocatedTile tile in tileLayer.Tiles) {
                        if (!level._localTileIndex.ContainsValue(tile.Tile.Uid))
                            level._localTileIndex.Add(level._indexSequence++, tile.Tile.Uid);
                    }
                }
            }

            List<LevelX.TileIndexEntryX> tileIndexEntries = new List<LevelX.TileIndexEntryX>();
            foreach (var item in level._localTileIndex) {
                tileIndexEntries.Add(new LevelX.TileIndexEntryX() {
                    Id = item.Key,
                    Uid = item.Value,
                });
            }

            LevelX.TileIndexX tileIndex = new LevelX.TileIndexX() {
                Entries = tileIndexEntries,
                Sequence = level._indexSequence,
            };

            List<AbstractXmlSerializer<LevelX.LayerX>> layers = new List<AbstractXmlSerializer<LevelX.LayerX>>();
            foreach (Layer layer in level.Layers) {
                if (layer is MultiTileGridLayer)
                    layers.Add(MultiTileGridLayer.ToXmlProxyX(layer as MultiTileGridLayer));
                else if (layer is ObjectLayer)
                    layers.Add(ObjectLayer.ToXmlProxyX(layer as ObjectLayer));
            }

            LevelX.PropertyGroupX propGroup = new LevelX.PropertyGroupX() {
                LevelGuid = level.Uid,
                Extra = (level.Extra != null && level.Extra.Count > 0) ? level.Extra.ToArray() : null,
            };

            if (level.Project != null)
                propGroup.ProjectGuid = level.Project.Uid;

            return new LevelX() {
                PropertyGroup = propGroup,
                Name = level.Name,
                OriginX = level.OriginX,
                OriginY = level.OriginY,
                Width = level.Width,
                Height = level.Height,
                TileIndex = tileIndex,
                Layers = layers.Count > 0 ? layers : null,
            };
        }
    }
}
