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

    public class Library : IResource
    {
        private string _name;

        public Library ()
        {
            Uid = Guid.NewGuid();
            Extra = new List<XmlElement>();

            TexturePool = new TexturePool();
            ObjectPoolManager = new ObjectPoolManager(TexturePool);
            TilePoolManager = new TilePoolManager(TexturePool);
            TileBrushManager = new TileBrushManager();

            ObjectPoolManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);
            TilePoolManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);
            TileBrushManager.Pools.Modified += (s, e) => OnModified(EventArgs.Empty);
        }

        public Library (Stream stream)
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

                FromXProxy(proxy, this);
            }
        }

        public Guid Uid { get; private set; }

        public List<XmlElement> Extra { get; private set; }

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        public string FileName { get; set; }

        public string Path { get; set; }

        public TexturePool TexturePool { get; private set; }

        public ObjectPoolManager ObjectPoolManager { get; private set; }

        public TilePoolManager TilePoolManager { get; private set; }

        public TileBrushManager TileBrushManager { get; private set; }

        public bool IsModified { get; set; }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            var ev = Modified;
            if (ev != null)
                ev(this, e);

            IsModified = true;
        }

        private bool SetField<T> (ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) 
                return false;

            field = value;
            
            OnModified(EventArgs.Empty);
            return true;
        }

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

            IsModified = false;
        }

        public static Library FromXProxy (LibraryX proxy)
        {
            if (proxy == null)
                return null;

            return FromXProxy(proxy, new Library());
        }

        private static Library FromXProxy (LibraryX proxy, Library library)
        {
            if (proxy == null)
                return null;

            if (proxy.PropertyGroup != null) {
                library.Uid = proxy.PropertyGroup.LibraryGuid;
                library.Name = proxy.PropertyGroup.LibraryName;
                library.Extra = proxy.PropertyGroup.Extra ?? new List<XmlElement>();
            }

            library.TexturePool = TexturePool.FromXmlProxy(proxy.TextureGroup);
            if (library.TexturePool == null)
                library.TexturePool = new TexturePool();

            library.ObjectPoolManager = ObjectPoolManager.FromXmlProxy(proxy.ObjectGroup, library.TexturePool);
            if (library.ObjectPoolManager == null)
                library.ObjectPoolManager = new ObjectPoolManager(library.TexturePool);

            library.TilePoolManager = TilePoolManager.FromXmlProxy(proxy.TileGroup, library.TexturePool);
            if (library.TilePoolManager == null)
                library.TilePoolManager = new TilePoolManager(library.TexturePool);

            library.TileBrushManager = TileBrushManager.FromXProxy(proxy.TileBrushGroup, library.TilePoolManager, Project.DynamicBrushClassRegistry);
            if (library.TileBrushManager == null)
                library.TileBrushManager = new TileBrushManager();

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
                    Extra = (library.Extra.Count > 0) ? library.Extra : null,
                },
            };
        }
    }
}
