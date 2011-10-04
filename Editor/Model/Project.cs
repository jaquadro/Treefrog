using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Model
{
    public class Project
    {
        private ServiceContainer _services;

        private TileRegistry _registry;

        private NamedResourceCollection<TilePool> _tilePools;
        private NamedResourceCollection<TileSet2D> _tileSets;
        private NamedResourceCollection<TileMap> _tileMaps;

        private bool _initalized;

        public Project () {
            _services = new ServiceContainer();

            _tileMaps = new NamedResourceCollection<TileMap>();
            _tilePools = new NamedResourceCollection<TilePool>();
            _tileSets = new NamedResourceCollection<TileSet2D>();
        }

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

        public TileRegistry Registry
        {
            get { return _registry; }
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

            _tilePools.Add(defaultPool);
            _tileSets.Add(defaultSet);
            _tileMaps.Add(defaultMap);
        }
    }
}
