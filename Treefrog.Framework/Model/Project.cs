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

    public class Project : IResource
    {
        private ServiceContainer _services;

        private NamedResourceCollection<Level> _levels;

        private Guid _defaultLibraryUid;
        private LibraryManager _libraryManager;

        private MetaTilePoolManager _tilePools;
        private MetaObjectPoolManager _objectPools;
        private MetaTileBrushManager _tileBrushes;
        private TexturePool _texturePool;

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
            Uid = Guid.NewGuid();
            _services = new ServiceContainer();

            Name = "Project";

            _levels = new NamedResourceCollection<Level>();
            _levels.Modified += (s, e) => OnModified(EventArgs.Empty);

            _libraryManager = new LibraryManager();
            _libraryManager.Libraries.Modified += (s, e) => OnModified(EventArgs.Empty);

            Library defaultLibrary = new Library() {
                Name = "Default"
            };

            _libraryManager.Libraries.Add(defaultLibrary);

            Extra = new List<XmlElement>();

            _texturePool = defaultLibrary.TexturePool;
            _tilePools = new MetaTilePoolManager();
            _tilePools.AddManager(defaultLibrary.Uid, defaultLibrary.TilePoolManager);
            _objectPools = new MetaObjectPoolManager();
            _objectPools.AddManager(defaultLibrary.Uid, defaultLibrary.ObjectPoolManager);
            _tileBrushes = new MetaTileBrushManager();
            _tileBrushes.AddManager(defaultLibrary.Uid, defaultLibrary.TileBrushManager);

            SetDefaultLibrary(defaultLibrary);

            _services.AddService(typeof(TilePoolManager), _tilePools);

            ResetModified();
        }

        public Project (Stream stream, ProjectResolver resolver)
        {
            _services = new ServiceContainer();
            _levels = new NamedResourceCollection<Level>();
            _levels.Modified += (s, e) => OnModified(EventArgs.Empty);

            _libraryManager = new LibraryManager();
            _libraryManager.Libraries.Modified += (s, e) => OnModified(EventArgs.Empty);

            Extra = new List<XmlElement>();

            XmlReaderSettings settings = new XmlReaderSettings() {
                CloseInput = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using (XmlReader reader = XmlTextReader.Create(stream, settings)) {
                XmlSerializer serializer = new XmlSerializer(typeof(ProjectX));
                ProjectX proxy = serializer.Deserialize(reader) as ProjectX;

                FromXProxy(proxy, resolver, this);
            }

            ResetModified();
        }

        public Guid Uid { get; private set; }

        public List<XmlElement> Extra { get; private set; }

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

        public ITileBrushManager TileBrushManager
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

        public Library DefaultLibrary
        {
            get { return _libraryManager.Libraries.Contains(_defaultLibraryUid) ? _libraryManager.Libraries[_defaultLibraryUid] : null; }
            set
            {
                if (value != null) {
                    if (!_libraryManager.Libraries.Contains(value.Uid))
                        AddLibrary(value);
                    SetDefaultLibrary(value);
                }
            }
        }

        public IServiceProvider Services
        {
            get { return _services; }
        }

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var level in Levels)
                level.ResetModified();
            foreach (var library in LibraryManager.Libraries)
                library.ResetModified();
        }

        /// <summary>
        /// Occurs when the internal state of the Project is modified.
        /// </summary>
        public event EventHandler Modified;

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

        public void Save (Stream stream, ProjectResolver resolver)
        {
            List<ProjectX.LibraryX> libraries = new List<ProjectX.LibraryX>();
            List<ProjectX.LevelX> levels = new List<ProjectX.LevelX>();

            foreach (Library library in _libraryManager.Libraries) {
                if (library.Name == null)
                    library.Name = Name;

                if (library.FileName == null)
                    library.FileName = FormatSafeFileName(library.Name + ".tlbx");

                using (Stream libStream = resolver.OutputStream(library.FileName)) {
                    library.Save(libStream);
                }

                libraries.Add(new ProjectX.LibraryX() {
                    Include = library.FileName,
                });
            }

            foreach (Level level in Levels) {
                if (level.FileName == null)
                    level.FileName = FormatSafeFileName(level.Name + ".tlvx");

                using (Stream levStream = resolver.OutputStream(level.FileName)) {
                    level.Save(levStream);
                }

                levels.Add(new ProjectX.LevelX() {
                    Include = level.FileName,
                });
            }

            XmlWriterSettings settings = new XmlWriterSettings() {
                CloseOutput = true,
                Indent = true,
            };

            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            ProjectX proxy = new ProjectX() {
                ItemGroups = new List<ProjectX.ItemGroupX>(),
                PropertyGroup = new ProjectX.PropertyGroupX() {
                    ProjectGuid = Uid,
                    ProjectName = Name,
                    DefaultLibrary = _defaultLibraryUid,
                    Extra = Extra.Count > 0 ? Extra : null,
                },
            };

            proxy.ItemGroups.Add(new ProjectX.ItemGroupX() {
                Libraries = libraries,
            });
            proxy.ItemGroups.Add(new ProjectX.ItemGroupX() {
                Levels = levels,
            });

            XmlSerializer serializer = new XmlSerializer(typeof(ProjectX));
            serializer.Serialize(writer, proxy);

            writer.Close();

            ResetModified();
        }

        private string FormatSafeFileName (string name)
        {
            return String.Join("_", name.Split(Path.GetInvalidFileNameChars()));
        }

        public static Project FromXProxy (ProjectX proxy, ProjectResolver resolver)
        {
            if (proxy == null)
                return null;

            return FromXProxy(proxy, resolver, new Project());
        }

        public static Project FromXProxy (ProjectX proxy, ProjectResolver resolver, Project project)
        {
            if (proxy == null)
                return null;

            if (proxy.PropertyGroup != null) {
                project.Uid = proxy.PropertyGroup.ProjectGuid;
                project.Name = proxy.PropertyGroup.ProjectName;
                project._defaultLibraryUid = proxy.PropertyGroup.DefaultLibrary;
                project.Extra = proxy.PropertyGroup.Extra;
            }

            project._tilePools = new MetaTilePoolManager();
            project._objectPools = new MetaObjectPoolManager();
            project._tileBrushes = new MetaTileBrushManager();

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
                        using (Stream stream = resolver.InputStream(level.Include)) {
                            project.Levels.Add(new Level(stream, project) {
                                FileName = level.Include,
                            });
                        }
                    }
                }
            }

            //project._tilePools.Pools.PropertyChanged += project.TilePoolsModifiedHandler;
            //project._objectPools.Pools.PropertyChanged += project.HandleObjectPoolManagerPropertyChanged;

            return project;
        }

        private void AddLibrary (Library library)
        {
            _libraryManager.Libraries.Add(library);

            //_texturePool = library.TexturePool;
            _tilePools.AddManager(library.Uid, library.TilePoolManager);
            _objectPools.AddManager(library.Uid, library.ObjectPoolManager);
            _tileBrushes.AddManager(library.Uid, library.TileBrushManager);

            if (_defaultLibraryUid == Guid.Empty)
                _defaultLibraryUid = library.Uid;

            if (_defaultLibraryUid == library.Uid) {
                SetDefaultLibrary(library);
            }
        }

        private void AddLevel (Level level)
        {
            _levels.Add(level);
        }

        private void SetDefaultLibrary (Library library)
        {
            _defaultLibraryUid = library.Uid;

            _texturePool = library.TexturePool;

            _tilePools.Default = library.Uid;
            _objectPools.Default = library.Uid;
            _tileBrushes.Default = library.Uid;
        }

        private static void WriteLevel (ProjectResolver resolver, Level level, string levelPath)
        {
            using (Stream stream = resolver.OutputStream(levelPath)) {
                XmlWriterSettings settings = new XmlWriterSettings() {
                    CloseOutput = true,
                    Indent = true,
                };

                XmlWriter writer = XmlTextWriter.Create(stream, settings);

                LevelX proxy = Level.ToXProxy(level);
                XmlSerializer serializer = new XmlSerializer(typeof(LevelX));
                serializer.Serialize(writer, proxy);

                writer.Close();
            }
        }
    }
}
