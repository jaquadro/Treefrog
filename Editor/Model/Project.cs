using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.IO;

namespace Editor.Model
{
    public class Project
    {
        #region Fields

        private ServiceContainer _services;

        private TileRegistry _registry;

        private NamedResourceCollection<TilePool> _tilePools;
        private NamedResourceCollection<TileSet2D> _tileSets;
        private NamedResourceCollection<TileMap> _tileMaps; // XX
        private NamedResourceCollection<Level> _levels;

        private bool _initalized;

        #endregion

        #region Constructors

        public Project () {
            _services = new ServiceContainer();

            _tileMaps = new NamedResourceCollection<TileMap>();
            _tilePools = new NamedResourceCollection<TilePool>();
            _tileSets = new NamedResourceCollection<TileSet2D>();
            _levels = new NamedResourceCollection<Level>();
        }

        #endregion

        #region Properties

        public bool Initialized
        {
            get { return _initalized; }
        }

        public NamedResourceCollection<TileMap> TileMaps
        {
            get { return _tileMaps; }
        }

        public NamedResourceCollection<TilePool> TilePools
        {
            get { return _tilePools; }
        }

        public NamedResourceCollection<TileSet2D> TileSets
        {
            get { return _tileSets; }
        }

        public NamedResourceCollection<Level> Levels
        {
            get { return _levels; }
        }

        public TileRegistry Registry
        {
            get { return _registry; }
        }

        #endregion

        #region XML Import / Export

        public void ReadXml (XmlReader reader)
        {

        }

        public void WriteXml (XmlWriter writer)
        {
            // <project>
            writer.WriteStartElement("project");

            //   <tilesets>
            writer.WriteStartElement("tilesets");
            writer.WriteEndElement();

            //   <templates>
            writer.WriteStartElement("templates");
            writer.WriteEndElement();

            //   <levels>
            writer.WriteStartElement("levels");
            foreach (Level level in _levels) {
                level.WriteXml(writer);
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        #endregion

        public void Save (Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                CloseOutput = true,
                Indent = true,
            };

            XmlWriter writer = XmlTextWriter.Create(stream, settings);

            WriteXml(writer);

            writer.Close();
        }

        public void Initialize (IntPtr windowHandle)
        {
            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(windowHandle, 128, 128);
            _services.AddService(typeof(IGraphicsDeviceService), gds);

            Initialize(gds.GraphicsDevice);
        }

        public void Initialize (GraphicsDevice device)
        {
            _registry = new TileRegistry(device);

            _initalized = true;
        }

        public void SetupDefaults ()
        {
            if (!_initalized) {
                return;
            }

            TilePool defaultPool = new TilePool("Default", _registry, 16, 16);
            TileSet2D defaultSet = new TileSet2D("Default", defaultPool, 12, 24);
            TileMap defaultMap = new TileMap("Default");
            Level defaultLevle = new Level("Level 1");

            _tilePools.Add(defaultPool);
            _tileSets.Add(defaultSet);
            _tileMaps.Add(defaultMap);
            _levels.Add(defaultLevle);
        }
    }
}
