using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.IO;

namespace Treefrog.Framework.Model
{
    public class Project
    {
        #region Fields

        private ServiceContainer _services;

        private TileRegistry _registry;

        private NamedResourceCollection<TilePool> _tilePools;
        //private NamedResourceCollection<TileSet2D> _tileSets;
        private NamedResourceCollection<Level> _levels;

        private bool _initalized;

        #endregion

        #region Constructors

        public Project () 
        {
            _services = new ServiceContainer();

            _tilePools = new NamedResourceCollection<TilePool>();
            //_tileSets = new NamedResourceCollection<TileSet2D>();
            _levels = new NamedResourceCollection<Level>();
        }

        #endregion

        #region Properties

        public bool Initialized
        {
            get { return _initalized; }
        }

        public NamedResourceCollection<TilePool> TilePools
        {
            get { return _tilePools; }
        }

        /*public NamedResourceCollection<TileSet2D> TileSets
        {
            get { return _tileSets; }
        }*/

        public NamedResourceCollection<Level> Levels
        {
            get { return _levels; }
        }

        public TileRegistry Registry
        {
            get { return _registry; }
        }

        public IServiceProvider Services
        {
            get { return _services; }
        }

        #endregion

        public static Project Open (Stream stream, GraphicsDevice device)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                CloseInput = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            XmlReader reader = XmlTextReader.Create(stream, settings);

            Project project = Project.FromXml(reader, device);

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

            WriteXml(writer);

            writer.Close();
        }

        /*public void Initialize (IntPtr windowHandle)
        {
            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(windowHandle, 128, 128);
            _services.AddService(typeof(IGraphicsDeviceService), gds);

            Initialize(gds.GraphicsDevice);
        }*/

        public void Initialize (GraphicsDevice device)
        {
            _registry = new TileRegistry(device);

            _services.AddService(typeof(IGraphicsDeviceService), device);
            _services.AddService(typeof(TileRegistry), _registry);

            _initalized = true;
        }

        /*public void SetupDefaults ()
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
        }*/

        #region XML Import / Export

        public static Project FromXml (XmlReader reader, GraphicsDevice device)
        {
            Project project = new Project();
            project.Initialize(device);

            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "project":
                        project.ReadXmlProject(xmlr);
                        break;
                }
            });

            return project;
        }

        public void WriteXml (XmlWriter writer)
        {
            // <project>
            writer.WriteStartElement("project");

            //   <tilesets>
            writer.WriteStartElement("tilesets");
            writer.WriteAttributeString("lastid", _registry.LastId.ToString());

            foreach (TilePool pool in TilePools) {
                pool.WriteXml(writer);
            }
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

        private void ReadXmlProject (XmlReader reader)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "tilesets":
                        ReadXmlTilesets(xmlr);
                        break;
                    case "levels":
                        ReadXmlLevels(xmlr);
                        break;
                }
            });
        }

        private void ReadXmlTilesets (XmlReader reader)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "tileset":
                        _tilePools.Add(TilePool.FromXml(xmlr, _services));
                        break;
                }
            });
        }

        private void ReadXmlLevels (XmlReader reader)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "level":
                        _levels.Add(Level.FromXml(xmlr, _services));
                        break;
                }
            });
        }

        #endregion
    }
}
