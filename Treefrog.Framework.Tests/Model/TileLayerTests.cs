using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Framework.Tests.Model
{
    [TestFixture]
    class TileLayerTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
        }

        private EventFlags _eventsFired;

        [SetUp]
        public void TestSetup ()
        {
            _eventsFired = EventFlags.None;
        }

        private void AttachEvents (TileLayer layer)
        {
            layer.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
        }

        [Test]
        public void CreateTileLayer ()
        {
            TileLayer layer = new MultiTileGridLayer("layer", 16, 24, 30, 50);

            Assert.AreEqual(16, layer.TileWidth);
            Assert.AreEqual(24, layer.TileHeight);
        }

        [Test]
        public void CloneTileLayer ()
        {
            TileLayer layer = new MultiTileGridLayer("layer", 16, 24, 30, 50);
            TileLayer layer2 = layer.Clone() as TileLayer;

            Assert.AreEqual(16, layer2.TileWidth);
            Assert.AreEqual(24, layer2.TileHeight);
        }
    }
}
