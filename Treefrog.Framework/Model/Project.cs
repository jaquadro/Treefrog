using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Treefrog.Framework.Model.Proxy;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class TilePoolEventArgs : EventArgs 
    {
        public TilePool TilePool { get; private set; }

        public TilePoolEventArgs (TilePool pool) 
        {
            TilePool = pool;
        }
    }

    public class Project
    {
        #region Fields

        private ServiceContainer _services;

        //private TileRegistry _registry;

        //private NamedResourceCollection<TilePool> _tilePools;
        //private NamedResourceCollection<ObjectPool> _objectPools;
        //private NamedResourceCollection<TileSet2D> _tileSets;
        private NamedResourceCollection<Level> _levels;

        private TilePoolManager _tilePools;
        private ObjectPoolManager _objectPools;
        private TileBrushManager _tileBrushes;
        private TexturePool _texturePool;

        //private bool _initalized;

        private static TileBrushRegistry _tileBrushRegistry = new TileBrushRegistry();
        public static TileBrushRegistry TileBrushRegistry
        {
            get { return _tileBrushRegistry; }
        }

        private static DynamicTileBrushClassRegistry _dynamicBrushRegistry = new DynamicTileBrushClassRegistry();
        public static DynamicTileBrushClassRegistry DynamicBrushClassRegistry
        {
            get { return _dynamicBrushRegistry; }
        }

        #endregion

        #region Constructors

        public Project () 
        {
            _services = new ServiceContainer();
            _texturePool = new TexturePool();

            //_tilePools = new NamedResourceCollection<TilePool>();
            _tilePools = new TilePoolManager(_texturePool);
            _objectPools = new ObjectPoolManager(_texturePool);
            _tileBrushes = new TileBrushManager();
            //_objectPools = new NamedResourceCollection<ObjectPool>();
            //_tileSets = new NamedResourceCollection<TileSet2D>();
            _levels = new NamedResourceCollection<Level>();
            

            _tilePools.Pools.Modified += TilePoolsModifiedHandler;
            _objectPools.Pools.PropertyChanged += HandleObjectPoolManagerPropertyChanged;
            _tileBrushes.DynamicBrushes.PropertyChanged += HandleTileBrushManagerPropertyChanged;
            _levels.ResourceModified += LevelsModifiedHandler;

            _services.AddService(typeof(TilePoolManager), _tilePools);
        }

        #endregion

        #region Properties

        public bool Initialized
        {
            get { return true; }
        }

        public NamedResourceCollection<TilePool> TilePools
        {
            get { return _tilePools.Pools; }
        }

        public TilePoolManager TilePoolManager
        {
            get { return _tilePools; }
        }

        //public NamedResourceCollection<ObjectPool> ObjectPools
        //{
        //    get { return _objectPools.Pools; }
        //}

        public ObjectPoolManager ObjectPoolManager
        {
            get { return _objectPools; }
        }

        public TileBrushManager TileBrushManager
        {
            get { return _tileBrushes; }
        }

        public TexturePool TexturePool
        {
            get { return _texturePool; }
        }

        /*public NamedResourceCollection<TileSet2D> TileSets
        {
            get { return _tileSets; }
        }*/

        public NamedResourceCollection<Level> Levels
        {
            get { return _levels; }
        }

        /*public TileRegistry Registry
        {
            get { return _registry; }
        }*/

        public IServiceProvider Services
        {
            get { return _services; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the internal state of the Project is modified.
        /// </summary>
        public event EventHandler Modified = (s, e) => { };

        //public event EventHandler<TilePoolEventArgs> TilePoolAdded = (s, e) => { };

        //public event EventHandler<TilePoolEventArgs> TilePoolRemoved = (s, e) => { };

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

        #endregion

        #region Event Handlers

        private void HandleObjectPoolManagerPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            OnModified(e);
        }

        private void HandleTileBrushManagerPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            OnModified(e);
        }

        private void TilePoolsModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        /*private void ObjectPoolsModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }*/

        private void LevelsModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #endregion

        public static Project Open (Stream stream, ProjectResolver resolver)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                CloseInput = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            XmlReader reader = XmlTextReader.Create(stream, settings);

            //XmlSerializer serializer = new XmlSerializer(typeof(ProjectXmlProxy));
            //ProjectXmlProxy proxy = serializer.Deserialize(reader) as ProjectXmlProxy;
            //Project project = Project.FromXmlProxy(proxy);

            XmlSerializer serializer = new XmlSerializer(typeof(ProjectXmlProxyX));
            ProjectXmlProxyX proxy = serializer.Deserialize(reader) as ProjectXmlProxyX;
            Project project = Project.FromXmlProxy(proxy, resolver);

            reader.Close();

            return project;
        }

        public void Save (Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                CloseOutput = true,
                Indent = true,
            };

            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            ProjectXmlProxy proxy = Project.ToXmlProxy(this);
            XmlSerializer serializer = new XmlSerializer(typeof(ProjectXmlProxy));
            serializer.Serialize(writer, proxy);

            writer.Close();
        }

        public static Project FromXmlProxy (ProjectXmlProxyX proxy, ProjectResolver resolver)
        {
            if (proxy == null)
                return null;

            Project project = new Project();

            foreach (var itemGroup in proxy.ItemGroups) {
                if (itemGroup.Libraries != null) {
                    foreach (var library in itemGroup.Libraries) {
                        LoadLibrary(project, resolver, library.Include);
                    }
                }
            }

            project._tilePools.Pools.Modified += project.TilePoolsModifiedHandler;
            project._objectPools.Pools.PropertyChanged += project.HandleObjectPoolManagerPropertyChanged;

            return project;
        }

        private static void LoadLibrary (Project project, ProjectResolver resolver, string libraryPath)
        {
            using (Stream stream = resolver.InputStream(libraryPath)) {
                XmlReaderSettings settings = new XmlReaderSettings() {
                    CloseInput = true,
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                };

                XmlReader reader = XmlTextReader.Create(stream, settings);

                XmlSerializer serializer = new XmlSerializer(typeof(LibraryX));
                LibraryX proxy = serializer.Deserialize(reader) as LibraryX;

                project._texturePool = TexturePool.FromXmlProxy(proxy.TextureGroup);
                if (project._texturePool == null)
                    project._texturePool = new TexturePool();

                project._objectPools = ObjectPoolManager.FromXmlProxy(proxy.ObjectGroup, project._texturePool);
                if (project._objectPools == null)
                    project._objectPools = new ObjectPoolManager(project._texturePool);

                project._tilePools = TilePoolManager.FromXmlProxy(proxy.TileGroup, project._texturePool);
                if (project._tilePools == null)
                    project._tilePools = new TilePoolManager(project._texturePool);

                project._tileBrushes = TileBrushManager.FromXmlProxy(proxy.TileBrushGroup, project._tilePools, Project.DynamicBrushClassRegistry);
                if (project._tileBrushes == null)
                    project._tileBrushes = new TileBrushManager();
            }
        }

        public static Project FromXmlProxy (ProjectXmlProxy proxy)
        {
            if (proxy == null)
                return null;

            Project project = new Project();
            project._texturePool = TexturePool.FromXmlProxy(proxy.TexturePool);
            if (project._texturePool == null)
                project._texturePool = new TexturePool();

            project._objectPools = ObjectPoolManager.FromXmlProxy(proxy.ObjectPools, project._texturePool);
            if (project._objectPools == null)
                project._objectPools = new ObjectPoolManager(project._texturePool);

            project._tilePools = TilePoolManager.FromXmlProxy(proxy.TilePools, project._texturePool);
            if (project._tilePools == null)
                project._tilePools = new TilePoolManager(project._texturePool);

            project._tileBrushes = TileBrushManager.FromXmlProxy(proxy.TileBrushes, project._tilePools, Project.DynamicBrushClassRegistry);
            if (project._tileBrushes == null)
                project._tileBrushes = new TileBrushManager();
            
            foreach (LevelXmlProxy levelProxy in proxy.Levels)
                project.Levels.Add(Level.FromXmlProxy(levelProxy, project));

            project._tilePools.Pools.Modified += project.TilePoolsModifiedHandler;
            project._objectPools.Pools.PropertyChanged += project.HandleObjectPoolManagerPropertyChanged;

            return project;
        }

        public static ProjectXmlProxy ToXmlProxy (Project project)
        {
            if (project == null)
                return null;

            List<LevelXmlProxy> levels = new List<LevelXmlProxy>();
            foreach (Level level in project.Levels)
                levels.Add(Level.ToXmlProxy(level));

            return new ProjectXmlProxy()
            {
                TexturePool = TexturePool.ToXmlProxy(project.TexturePool),
                ObjectPools = ObjectPoolManager.ToXmlProxy(project.ObjectPoolManager),
                TilePools = TilePoolManager.ToXmlProxy(project.TilePoolManager),
                TileBrushes = TileBrushManager.ToXmlProxy(project.TileBrushManager),
                Levels = levels,
            };
        }
    }

    public abstract class ProjectResolver
    {
        public abstract Stream InputStream (string relativePath);
        public abstract Stream OutputStream (string relativePath);
    }

    public class FileProjectResolver : ProjectResolver
    {
        private string _basePath;

        public FileProjectResolver (string projectFilePath)
        {
            _basePath = Path.GetDirectoryName(projectFilePath);
        }

        public override Stream InputStream (string relativePath)
        {
            return File.OpenRead(Path.Combine(_basePath, relativePath));
        }

        public override Stream OutputStream (string relativePath)
        {
            return File.OpenWrite(Path.Combine(_basePath, relativePath));
        }
    }

    [XmlRoot("Project")]
    public class ProjectXmlProxy
    {
        [XmlElement]
        public TexturePoolXmlProxy TexturePool { get; set; }

        [XmlElement]
        public ObjectPoolManagerXmlProxy ObjectPools { get; set; }

        [XmlElement]
        public TilePoolManagerXmlProxy TilePools { get; set; }

        [XmlElement]
        public TileBrushManagerXmlProxy TileBrushes { get; set; }

        [XmlArray]
        [XmlArrayItem("Level")]
        public List<LevelXmlProxy> Levels { get; set; }
    }

    [XmlRoot("Project")]
    public class ProjectXmlProxyX
    {
        public class PropertyGroupX
        {
            [XmlAnyElement]
            public List<XmlElement> Properties { get; set; }
        }

        public class ItemGroupX
        {
            [XmlElement("Library")]
            public LibraryX[] Libraries { get; set; }

            [XmlElement("Level")]
            public LevelX[] Levels { get; set; }
        }

        public class LibraryX
        {
            [XmlAttribute]
            public string Include { get; set; }
        }

        public class LevelX
        {
            [XmlAttribute]
            public string Include { get; set; }
        }

        [XmlElement]
        public PropertyGroupX PropertyGroup { get; set; }

        [XmlElement("ItemGroup")]
        public ItemGroupX[] ItemGroups { get; set; }
    }

    [XmlRoot("Library")]
    public class LibraryX
    {
        public class PropertyGroupX
        {
            [XmlAnyElement]
            public List<XmlElement> Properties { get; set; }
        }

        public class TextureGroupX
        {
            [XmlElement("Texture")]
            public TextureX[] Textures { get; set; }
        }

        public class ObjectGroupX
        {
            [XmlElement("ObjectPool")]
            public ObjectPoolX[] ObjectPools { get; set; }
        }

        public class TileGroupX
        {
            [XmlElement("TilePool")]
            public TilePoolX[] TilePools { get; set; }
        }

        public class TileBrushGroupX
        {
            [XmlElement]
            public TileBrushCollectionX<StaticTileBrushX> StaticBrushes { get; set; }

            [XmlElement]
            public TileBrushCollectionX<DynamicTileBrushX> DynamicBrushes { get; set; }
        }

        public class TextureX
        {
            [XmlAttribute]
            public int Id { get; set; }

            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Include { get; set; }

            [XmlElement]
            public TextureResource.XmlProxy TextureData { get; set; }
        }

        /*public class TextureDataX
        {
            [XmlAttribute]
            public int Width { get; set; }

            [XmlAttribute]
            public int Height { get; set; }

            [XmlElement(IsNullable = true)]
            public byte[] Data { get; set; }
        }*/

        public class ObjectPoolX
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlArray]
            [XmlArrayItem("ObjectClass")]
            public ObjectClassX[] ObjectClasses { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public PropertyX[] Properties { get; set; }
        }

        public class ObjectClassX
        {
            [XmlAttribute]
            public int Id { get; set; }

            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public string Texture { get; set; }

            [XmlElement]
            public Rectangle ImageBounds { get; set; }

            [XmlElement]
            public Rectangle MaskBounds { get; set; }

            [XmlElement]
            public Point Origin { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public PropertyX[] Properties { get; set; }
        }

        public class TilePoolX
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public int TileWidth { get; set; }

            [XmlAttribute]
            public int TileHeight { get; set; }

            [XmlAttribute]
            public string Texture { get; set; }

            [XmlArray]
            [XmlArrayItem("TileDef")]
            public TileDefX[] TileDefinitions { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public PropertyX[] Properties { get; set; }
        }

        public class TileDefX
        {
            [XmlAttribute]
            public int Id { get; set; }

            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute("Loc")]
            public string Location { get; set; }

            [XmlArray]
            [XmlArrayItem("Property")]
            public PropertyX[] Properties { get; set; }
        }

        public class PropertyX
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlText]
            public string Value { get; set; }
        }

        public class TileBrushCollectionX<TProxy>
            where TProxy : TileBrushX
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlElement("Brush")]
            public TProxy[] Brushes { get; set; }
        }

        public class TileBrushX
        {
            [XmlAttribute]
            public int Id { get; set; }

            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public int TileWidth { get; set; }

            [XmlAttribute]
            public int TileHeight { get; set; }
        }

        public class StaticTileBrushX : TileBrushX
        {
            [XmlAttribute]
            public int Id { get; set; }

            [XmlAttribute]
            public Guid Uid { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public int TileWidth { get; set; }

            [XmlAttribute]
            public int TileHeight { get; set; }

            [XmlArray]
            [XmlArrayItem("Tile")]
            public List<TileStackX> Tiles { get; set; }
        }

        public class DynamicTileBrushX : TileBrushX
        {
            [XmlAttribute]
            public string Type { get; set; }

            [XmlElement("Entry")]
            public BrushEntryX[] Entries { get; set; }
        }

        public class BrushEntryX
        {
            [XmlAttribute]
            public int Slot { get; set; }

            [XmlAttribute]
            public int TileId { get; set; }
        }

        public class TileStackX
        {
            [XmlAttribute]
            public string At { get; set; }

            [XmlText]
            public string Items { get; set; }
        }

        [XmlElement]
        public PropertyGroupX PropertyGroup { get; set; }

        [XmlElement]
        public TextureGroupX TextureGroup { get; set; }

        [XmlElement]
        public ObjectGroupX ObjectGroup { get; set; }

        [XmlElement]
        public TileGroupX TileGroup { get; set; }

        [XmlElement]
        public TileBrushGroupX TileBrushGroup { get; set; }
    }
}
