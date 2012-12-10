﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

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

        //private bool _initalized;

        #endregion

        #region Constructors

        public Project () 
        {
            _services = new ServiceContainer();

            //_tilePools = new NamedResourceCollection<TilePool>();
            _tilePools = new TilePoolManager();
            _objectPools = new ObjectPoolManager();
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

        public static Project Open (Stream stream)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                CloseInput = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            };

            XmlReader reader = XmlTextReader.Create(stream, settings);

            //Project project = Project.FromXml(reader, device);

            XmlSerializer serializer = new XmlSerializer(typeof(ProjectXmlProxy));
            ProjectXmlProxy proxy = serializer.Deserialize(reader) as ProjectXmlProxy;
            Project project = Project.FromXmlProxy(proxy);

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

            //WriteXml(writer);

            ProjectXmlProxy proxy = Project.ToXmlProxy(this);
            XmlSerializer serializer = new XmlSerializer(typeof(ProjectXmlProxy));
            serializer.Serialize(writer, proxy);

            writer.Close();
        }

        /*
        #region XML Import / Export

        public static Project FromXml (XmlReader reader)
        {
            Project project = new Project();

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

            // <ObjectPools>
            XmlSerializer serializer = new XmlSerializer(typeof(ObjectPoolManagerXmlProxy));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(writer, ObjectPoolManager.ToXmlProxy(_objectPools), ns);

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
                    //case "tilesets":
                    //    ReadXmlTilesets(xmlr);
                    //    break;
                    case "TilePools":
                        ReadXmlTilePools(xmlr);
                        break;
                    case "levels":
                        ReadXmlLevels(xmlr);
                        break;
                    case "ObjectPools":
                        ReadXmlObjectPools(xmlr);
                        break;
                }
            });
        }

        private void ReadXmlObjectPools (XmlReader reader)
        {
            reader.MoveToContent();

            XmlSerializer serializer = new XmlSerializer(typeof(ObjectPoolManagerXmlProxy));
            ObjectPoolManagerXmlProxy proxy = serializer.Deserialize(reader) as ObjectPoolManagerXmlProxy;
            _objectPools = ObjectPoolManager.FromXmlProxy(proxy);
        }

        private void ReadXmlTilePools (XmlReader reader)
        {
            reader.MoveToContent();

            XmlSerializer serializer = new XmlSerializer(typeof(TilePoolManagerXmlProxy));
            TilePoolManagerXmlProxy proxy = serializer.Deserialize(reader) as TilePoolManagerXmlProxy;
            _tilePools = TilePoolManager.FromXmlProxy(proxy);
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
            reader.MoveToContent();

            XmlSerializer serializer = new XmlSerializer(typeof(LevelXmlProxy));
            TilePoolManagerXmlProxy proxy = serializer.Deserialize(reader) as TilePoolManagerXmlProxy;
            _tilePools = TilePoolManager.FromXmlProxy(proxy);

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
        */

        public static Project FromXmlProxy (ProjectXmlProxy proxy)
        {
            if (proxy == null)
                return null;

            Project project = new Project();
            project._objectPools = ObjectPoolManager.FromXmlProxy(proxy.ObjectPools);
            if (project._objectPools == null)
                project._objectPools = new ObjectPoolManager();

            project._tilePools = TilePoolManager.FromXmlProxy(proxy.TilePools);
            if (project._tilePools == null)
                project._tilePools = new TilePoolManager();

            project._tileBrushes = TileBrushManager.FromXmlProxy(proxy.TileBrushes, project._tilePools);
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
                ObjectPools = ObjectPoolManager.ToXmlProxy(project.ObjectPoolManager),
                TilePools = TilePoolManager.ToXmlProxy(project.TilePoolManager),
                TileBrushes = TileBrushManager.ToXmlProxy(project.TileBrushManager),
                Levels = levels,
            };
        }
    }

    [XmlRoot("Project")]
    public class ProjectXmlProxy
    {
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
}
