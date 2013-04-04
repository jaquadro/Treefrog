using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Model.Proxy;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model
{
    public class LibraryManager
    {
        private ResourceCollection<Library> _libraries;

        public LibraryManager ()
        {
            _libraries = new ResourceCollection<Library>();
        }

        public IResourceCollection<Library> Libraries
        {
            get { return _libraries; }
        }
    }

    public class Library : INamedResource
    {
        private readonly Guid _uid;
        private readonly ResourceName _name;

        private Library (Guid uid, string name)
        {
            _uid = uid;
            _name = new ResourceName(this, name);
        }

        public Library ()
            : this(Guid.NewGuid(), "Default")
        {
            Extra = new List<XmlElement>();

            TexturePool = new TexturePool();

            ObjectPoolManager = new ObjectPoolManager(TexturePool);
            ObjectPoolManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);

            TilePoolManager = new TilePoolManager(TexturePool);
            TilePoolManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);

            TileBrushManager = new TileBrushManager();
            TileBrushManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);
        }

        public Library (Stream stream)
            : this(Guid.NewGuid(), "Default")
        {
            Extra = new List<XmlElement>();

            XmlReaderSettings settings = new XmlReaderSettings() {
                CloseInput = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            using (XmlReader reader = XmlTextReader.Create(stream, settings)) {
                XmlSerializer serializer = new XmlSerializer(typeof(LibraryX));
                LibraryX proxy = serializer.Deserialize(reader) as LibraryX;

                if (proxy.PropertyGroup != null) {
                    _uid = proxy.PropertyGroup.LibraryGuid;
                    _name = new ResourceName(this, proxy.PropertyGroup.LibraryName);
                }

                Initialize(proxy);
            }
        }

        private void Initialize (LibraryX proxy)
        {
            if (proxy.PropertyGroup != null) {
                Extra = new List<XmlElement>(proxy.PropertyGroup.Extra ?? new XmlElement[0]);
            }

            TexturePool = TexturePool.FromXmlProxy(proxy.TextureGroup) ?? new TexturePool();

            ObjectPoolManager = ObjectPoolManager.FromXmlProxy(proxy.ObjectGroup, TexturePool) ?? new ObjectPoolManager(TexturePool);
            ObjectPoolManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);

            TilePoolManager = TilePoolManager.FromXmlProxy(proxy.TileGroup, TexturePool) ?? new TilePoolManager(TexturePool);
            TilePoolManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);

            TileBrushManager = TileBrushManager.FromXProxy(proxy.TileBrushGroup, TilePoolManager, Project.DynamicBrushClassRegistry) ?? new TileBrushManager();
            TileBrushManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public List<XmlElement> Extra { get; private set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public TexturePool TexturePool { get; private set; }

        public ObjectPoolManager ObjectPoolManager { get; private set; }

        public TilePoolManager TilePoolManager { get; private set; }

        public TileBrushManager TileBrushManager { get; private set; }

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var pool in ObjectPoolManager.Pools)
                pool.ResetModified();
            foreach (var pool in TilePoolManager.Pools)
                pool.ResetModified();
            foreach (var pool in TileBrushManager.Pools)
                pool.ResetModified();
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        private bool SetField<T> (ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) 
                return false;

            field = value;
            
            OnModified(EventArgs.Empty);
            return true;
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

        public void Save (Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings() {
                CloseOutput = true,
                Indent = true,
            };

            using (XmlWriter writer = XmlTextWriter.Create(stream, settings)) {
                LibraryX proxy = ToXProxy(this);

                XmlSerializer serializer = new XmlSerializer(typeof(LibraryX));
                serializer.Serialize(writer, proxy);
            }

            ResetModified();
        }

        public static Library FromXProxy (LibraryX proxy)
        {
            if (proxy == null)
                return null;

            Guid uid = proxy.PropertyGroup != null ? proxy.PropertyGroup.LibraryGuid : Guid.NewGuid();
            string name = proxy.PropertyGroup != null ? proxy.PropertyGroup.LibraryName : "Default";

            Library library = new Library(uid, name);
            library.Initialize(proxy);

            return library;
        }

        public static LibraryX ToXProxy (Library library)
        {
            if (library == null)
                return null;

            return new LibraryX() {
                TextureGroup = TexturePool.ToXmlProxyX(library.TexturePool),
                ObjectGroup = ObjectPoolManager.ToXmlProxyX(library.ObjectPoolManager),
                TileGroup = TilePoolManager.ToXmlProxyX(library.TilePoolManager),
                TileBrushGroup = TileBrushManager.ToXProxy(library.TileBrushManager),
                PropertyGroup = new LibraryX.PropertyGroupX() {
                    LibraryGuid = library.Uid,
                    LibraryName = library.Name,
                    Extra = (library.Extra.Count > 0) ? library.Extra.ToArray() : null,
                },
            };
        }
    }
}
