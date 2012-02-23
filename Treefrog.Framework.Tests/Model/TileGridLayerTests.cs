using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Treefrog.Framework.Tests.Model
{
    [TestFixture]
    class TileGridLayerTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
            LayerSizeChanged = 2,
            TileAdded = 4,
            TileRemoved = 8,
            TileCleared = 16,
        }

        private EventFlags _eventsFired;
        private GraphicsDeviceService _service;
        private TileRegistry _registry;
        private TilePool _pool;

        private Tile _tile1;
        private Tile _tile2;

        [TestFixtureSetUp]
        public void FixtureSetup ()
        {
            _service = MockGraphicsDeviceService.AddRef(800, 600);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown ()
        {
            _service.Release();
        }

        [SetUp]
        public void TestSetup ()
        {
            _eventsFired = EventFlags.None;
            _registry = new TileRegistry(_service.GraphicsDevice);
            _pool = new TilePool("pool", _registry, 16, 16);

            Texture2D tex1 = new Texture2D(_service.GraphicsDevice, 16, 16);
            Texture2D tex2 = new Texture2D(_service.GraphicsDevice, 16, 16);

            _tile1 = _pool.GetTile(_pool.AddTile(tex1));
            _tile2 = _pool.GetTile(_pool.AddTile(tex2));
        }

        private void AttachEvents (TileGridLayer layer)
        {
            layer.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
            layer.LayerSizeChanged += (s, e) => { _eventsFired |= EventFlags.LayerSizeChanged; };
            layer.TileAdded += (s, e) => { _eventsFired |= EventFlags.TileAdded; };
            layer.TileRemoved += (s, e) => { _eventsFired |= EventFlags.TileRemoved; };
            layer.TileCleared += (s, e) => { _eventsFired |= EventFlags.TileCleared; };
        }

        [Test]
        public void CreateLayer ()
        {
            TileGridLayer layer = new MultiTileGridLayer("tiles", 16, 24, 10, 20);

            Assert.AreEqual(16, layer.TileWidth);
            Assert.AreEqual(24, layer.TileHeight);
            Assert.AreEqual(10, layer.TilesWide);
            Assert.AreEqual(20, layer.TilesHigh);
            Assert.AreEqual(16 * 10, layer.LayerWidth);
            Assert.AreEqual(24 * 20, layer.LayerHeight);
            Assert.IsTrue(layer.IsResizable);
        }

        [Test]
        public void AddTile ()
        {
            TileGridLayer layer = new MultiTileGridLayer("tiles", 16, 16, 10, 10);
            AttachEvents(layer);

            layer.TileAdded += (s, e) =>
            {
                Assert.AreEqual(2, e.X);
                Assert.AreEqual(4, e.Y);
                Assert.AreSame(_tile1, e.Tile);
                Assert.AreEqual(1, layer.Tiles.Count());
                Assert.AreEqual(1, layer.TilesAt(new TileCoord(2, 4)).Count());
            };

            layer.AddTile(2, 4, _tile1);

            Assert.AreEqual(EventFlags.TileAdded | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void AddTileOutOfBounds ()
        {
            TileGridLayer layer = new MultiTileGridLayer("tiles", 16, 16, 10, 10);
            try {
                layer.AddTile(-1, 2, _tile1);
                Assert.Fail();
            }
            catch { }

            try {
                layer.AddTile(2, -1, _tile1);
                Assert.Fail();
            }
            catch { }

            try {
                layer.AddTile(10, 8, _tile1);
                Assert.Fail();
            }
            catch { }

            try {
                layer.AddTile(8, 10, _tile1);
                Assert.Fail();
            }
            catch { }
        }

        [Test]
        public void GetTilesInRegion ()
        {
            TileGridLayer layer = new MultiTileGridLayer("tiles", 16, 16, 10, 10);
            layer.AddTile(2, 3, _tile1);
            layer.AddTile(6, 7, _tile2);

            Assert.AreEqual(2, layer.TilesAt(new Rectangle(2, 2, 5, 6)).Count());
            Assert.AreEqual(1, layer.TilesAt(new Rectangle(3, 2, 5, 6)).Count());
            Assert.AreEqual(1, layer.TilesAt(new Rectangle(2, 4, 5, 6)).Count());
            Assert.AreEqual(1, layer.TilesAt(new Rectangle(2, 2, 4, 6)).Count());
            Assert.AreEqual(1, layer.TilesAt(new Rectangle(2, 2, 5, 5)).Count());
            Assert.AreEqual(0, layer.TilesAt(new Rectangle(3, 2, 5, 5)).Count());
        }

        [Test]
        public void GetTilesAtLocation ()
        {
            TileGridLayer layer = new MultiTileGridLayer("tiles", 16, 16, 10, 10);
            layer.AddTile(2, 3, _tile1);
            layer.AddTile(6, 7, _tile2);

            Assert.AreEqual(1, layer.TilesAt(new TileCoord(2, 3)).Count());
            Assert.AreEqual(0, layer.TilesAt(new TileCoord(1, 3)).Count());
            Assert.AreEqual(0, layer.TilesAt(new TileCoord(3, 3)).Count());
            Assert.AreEqual(0, layer.TilesAt(new TileCoord(1, 2)).Count());
            Assert.AreEqual(0, layer.TilesAt(new TileCoord(1, 4)).Count());
        }

        [Test]
        public void RemoveTile ()
        {
            TileGridLayer layer = new MultiTileGridLayer("tiles", 16, 16, 10, 10);
            layer.AddTile(2, 4, _tile1);
            layer.AddTile(6, 7, _tile2);

            AttachEvents(layer);

            layer.TileRemoved += (s, e) =>
            {
                Assert.AreEqual(2, e.X);
                Assert.AreEqual(4, e.Y);
                Assert.AreSame(_tile1, e.Tile);
                Assert.AreEqual(1, layer.Tiles.Count());
                Assert.AreEqual(0, layer.TilesAt(new TileCoord(2, 4)).Count());
            };

            layer.RemoveTile(2, 4, _tile1);

            Assert.AreEqual(EventFlags.TileRemoved | EventFlags.Modified, _eventsFired);
        }


    }
}
