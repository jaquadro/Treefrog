using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Framework.Tests.Model
{
    [TestFixture]
    class TileTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
            TextureModified = 2,
        }

        private Random _rand = new Random();

        private GraphicsDeviceService _service;
        private EventFlags _eventsFired;
        private TileRegistry _registry;
        private TilePool _pool;

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
        }

        private void AttachEvents (Tile tile)
        {
            tile.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
            tile.TextureModified += (s, e) => { _eventsFired |= EventFlags.TextureModified; };
        }

        [Test]
        [Ignore]
        public void CreatePhysicalTile ()
        {
            Tile tile = new PhysicalTile(1, _pool);

            Assert.AreEqual(1, tile.Id);
            Assert.AreEqual(_pool, tile.Pool);
            Assert.AreEqual(_pool.TileWidth, tile.Width);
            Assert.AreEqual(_pool.TileHeight, tile.Height);
        }

        [Test]
        [Ignore]
        public void UpdatePhysicalTileData ()
        {
            Tile tile = new PhysicalTile(1, _pool);
            AttachEvents(tile);

            byte[] data = new byte[16 * 16 * 4];
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(i % 255);

            tile.TextureModified += (s, e) =>
            {
                byte[] comp = _pool.GetTileTextureData(1);
                Assert.AreEqual(data.Length, comp.Length);
                for (int i = 0; i < comp.Length; i++) {
                    Assert.AreEqual(data[i], comp[i]);
                }
            };

            tile.Update(data);

            Assert.AreEqual(EventFlags.Modified | EventFlags.TextureModified, _eventsFired);
        }
    }
}
