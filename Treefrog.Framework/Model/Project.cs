using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.IO;

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
        private NamedResourceCollection<ObjectPool> _objectPools;
        //private NamedResourceCollection<TileSet2D> _tileSets;
        private NamedResourceCollection<Level> _levels;

        private TilePoolManager _tilePools;

        //private bool _initalized;

        #endregion

        #region Constructors

        public Project () 
        {
            _services = new ServiceContainer();

            //_tilePools = new NamedResourceCollection<TilePool>();
            _tilePools = new TilePoolManager();
            _objectPools = new NamedResourceCollection<ObjectPool>();
            //_tileSets = new NamedResourceCollection<TileSet2D>();
            _levels = new NamedResourceCollection<Level>();

            _tilePools.Pools.Modified += TilePoolsModifiedHandler;
            _objectPools.Modified += ObjectPoolsModifiedHandler;
            _levels.Modified += LevelsModifiedHandler;

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

        public NamedResourceCollection<ObjectPool> ObjectPools
        {
            get { return _objectPools; }
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

        private void TilePoolsModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        private void ObjectPoolsModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
        }

        private void LevelsModifiedHandler (object sender, EventArgs e)
        {
            OnModified(e);
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

        /*public void Initialize (GraphicsDevice device)
        {
            _registry = new TileRegistry(device);

            _services.AddService(typeof(IGraphicsDeviceService), device);
            _services.AddService(typeof(TileRegistry), _registry);

            _initalized = true;
        }*/

        #region XML Import / Export

        public static Project FromXml (XmlReader reader, GraphicsDevice device)
        {
            Project project = new Project();
            //project.Initialize(device);

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
            writer.WriteAttributeString("lastid", _tilePools.LastId.ToString());

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

        public void WriteXmlTilesets (XmlWriter writer)
        {
            writer.WriteStartElement("tilesets");
            writer.WriteAttributeString("lastid", _tilePools.LastId.ToString());

            foreach (TilePool pool in TilePools) {
                pool.WriteXml(writer);
            }
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

        public void ReadXmlTilesets (XmlReader reader)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "lastid",
            });

            _tilePools.LastId = Convert.ToInt32(attribs["lastid"]);

            XmlHelper.SwitchAll(reader, (xmlr, s) =>
            {
                switch (s) {
                    case "tileset":
                        TilePool.FromXml(xmlr, _tilePools);
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
                        _levels.Add(Level.FromXml(xmlr, this));
                        break;
                }
            });
        }

        #endregion
    }
}
