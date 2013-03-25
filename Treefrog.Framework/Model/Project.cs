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
        private Guid _uid;
        private ServiceContainer _services;

        private NamedResourceCollection<Level> _levels;

        private Guid _defaultLibraryUid;
        //private Dictionary<Guid, Library> _libraries;
        private LibraryManager _libraryManager;

        private MetaTilePoolManager _tilePools;
        private MetaObjectPoolManager _objectPools;
        private TileBrushManager _tileBrushes;
        private TexturePool _texturePool;

        private List<XmlElement> _extra;

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

        public Project () 
        {
            _uid = Guid.NewGuid();
            _services = new ServiceContainer();
            //_texturePool = new TexturePool();

            Name = "Project";

            //_tilePools = new TilePoolManager(_texturePool);
            //ObjectPoolManager defaultObjectPool = new ObjectPoolManager(_texturePool);
            _tileBrushes = new TileBrushManager();
            _levels = new NamedResourceCollection<Level>();

            _libraryManager = new LibraryManager();

            Library defaultLibrary = new Library();
            _libraryManager.Libraries.Add(defaultLibrary);

            _extra = new List<XmlElement>();

            _texturePool = defaultLibrary.TexturePool;
            _tilePools = new MetaTilePoolManager();
            _tilePools.AddManager(defaultLibrary.Uid, defaultLibrary.TilePoolManager);
            _objectPools = new MetaObjectPoolManager();
            _objectPools.AddManager(defaultLibrary.Uid, defaultLibrary.ObjectPoolManager);

            //_tilePools.Pools.PropertyChanged += TilePoolsModifiedHandler;
            //_objectPools.Pools.PropertyChanged += HandleObjectPoolManagerPropertyChanged;
            _tileBrushes.DynamicBrushes.PropertyChanged += HandleTileBrushManagerPropertyChanged;
            _levels.ResourceModified += LevelsModifiedHandler;

            _services.AddService(typeof(TilePoolManager), _tilePools);
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public string Name { get; set; }

        public string FileName { get; set; }

        public bool Initialized
        {
            get { return true; }
        }

        public ITilePoolManager TilePoolManager
        {
            get { return _tilePools; }
        }

        public IObjectPoolManager ObjectPoolManager
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

        public NamedResourceCollection<Level> Levels
        {
            get { return _levels; }
        }

        public LibraryManager LibraryManager
        {
            get { return _libraryManager; }
        }

        public IServiceProvider Services
        {
            get { return _services; }
        }

        #region Events

        /// <summary>
        /// Occurs when the internal state of the Project is modified.
        /// </summary>
        public event EventHandler Modified = (s, e) => { };

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

            XmlSerializer serializer = new XmlSerializer(typeof(ProjectX));
            ProjectX proxy = serializer.Deserialize(reader) as ProjectX;
            Project project = Project.FromXmlProxy(proxy, resolver);

            reader.Close();

            return project;
        }

        public void Save (Stream stream, ProjectResolver resolver)
        {
            foreach (Library library in _libraryManager.Libraries) {
                if (library.Name == null)
                    library.Name = Name;

                if (library.FileName == null)
                    library.FileName = library.Name + ".tlbx";

                using (Stream libStream = resolver.OutputStream(library.FileName)) {
                    library.Save(libStream);
                }
            }

            foreach (Level level in Levels) {
                WriteLevel(resolver, level, level.Name + "_test.tlvx");
            }

            XmlWriterSettings settings = new XmlWriterSettings() {
                CloseOutput = true,
                Indent = true,
            };

            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            ProjectX proxy = new ProjectX() {
                ItemGroups = new List<ProjectX.ItemGroupX>(),
                PropertyGroup = new ProjectX.PropertyGroupX() {
                    ProjectGuid = _uid,
                    Extra = _extra.Count > 0 ? _extra : null,
                },
            };

            proxy.ItemGroups.Add(new ProjectX.ItemGroupX() {
                Libraries = new List<ProjectX.LibraryX>() {
                    new ProjectX.LibraryX() { Include = "test.tlbx" },
                }
            });

            ProjectX.ItemGroupX levelGroup = new ProjectX.ItemGroupX() {
                Levels = new List<ProjectX.LevelX>()
            };

            foreach (Level level in Levels) 
                levelGroup.Levels.Add(new ProjectX.LevelX() { Include = level.Name + "_test.tlvx" });

            proxy.ItemGroups.Add(levelGroup);

            XmlSerializer serializer = new XmlSerializer(typeof(ProjectX));
            serializer.Serialize(writer, proxy);

            writer.Close();
        }

        public static Project FromXmlProxy (ProjectX proxy, ProjectResolver resolver)
        {
            if (proxy == null)
                return null;

            Project project = new Project();

            project._tilePools = new MetaTilePoolManager();
            project._objectPools = new MetaObjectPoolManager();

            foreach (var itemGroup in proxy.ItemGroups) {
                if (itemGroup.Libraries != null) {
                    foreach (var libProxy in itemGroup.Libraries) {
                        using (Stream stream = resolver.InputStream(libProxy.Include)) {
                            project.AddLibrary(new Library(stream) {
                                FileName = libProxy.Include,
                            });
                        }
                    }
                }

                if (itemGroup.Levels != null) {
                    foreach (var level in itemGroup.Levels) {
                        LoadLevel(project, resolver, level.Include);
                    }
                }
            }

            project._uid = proxy.PropertyGroup.ProjectGuid;
            project._extra = proxy.PropertyGroup.Extra;

            //project._tilePools.Pools.PropertyChanged += project.TilePoolsModifiedHandler;
            //project._objectPools.Pools.PropertyChanged += project.HandleObjectPoolManagerPropertyChanged;

            return project;
        }

        private void AddLibrary (Library library)
        {
            _libraryManager.Libraries.Add(library);

            _texturePool = library.TexturePool;
            _tilePools.AddManager(library.Uid, library.TilePoolManager);
            _objectPools.AddManager(library.Uid, library.ObjectPoolManager);
            _tileBrushes = library.TileBrushManager;

            if (_defaultLibraryUid == Guid.Empty) {
                _defaultLibraryUid = library.Uid;
                _tilePools.Default = library.Uid;
                _objectPools.Default = library.Uid;
            }
        }

        private static void LoadLevel (Project project, ProjectResolver resolver, string levelPath)
        {
            using (Stream stream = resolver.InputStream(levelPath)) {
                XmlReaderSettings settings = new XmlReaderSettings() {
                    CloseInput = true,
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                };

                XmlReader reader = XmlTextReader.Create(stream, settings);

                XmlSerializer serializer = new XmlSerializer(typeof(LevelX));
                LevelX proxy = serializer.Deserialize(reader) as LevelX;

                project.Levels.Add(Level.FromXmlProxy(proxy, project));
            }
        }

        private static void WriteLevel (ProjectResolver resolver, Level level, string levelPath)
        {
            using (Stream stream = resolver.OutputStream(levelPath)) {
                XmlWriterSettings settings = new XmlWriterSettings() {
                    CloseOutput = true,
                    Indent = true,
                };

                XmlWriter writer = XmlTextWriter.Create(stream, settings);

                LevelX proxy = Level.ToXmlProxyX(level);
                XmlSerializer serializer = new XmlSerializer(typeof(LevelX));
                serializer.Serialize(writer, proxy);

                writer.Close();
            }
        }
    }
}
