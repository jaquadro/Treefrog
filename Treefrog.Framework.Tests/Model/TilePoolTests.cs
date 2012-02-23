using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace Treefrog.Framework.Tests.Model
{
    [TestFixture]
    public class TilePoolTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
            TileAdded = 2,
            TileRemoved = 4,
            TileModified = 8,
            NameChanged = 16,
            PropertyAdded = 32,
            PropertyRemoved = 64,
            PropertyModified = 128,
            PropertyRenamed = 256,
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

            _pool.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
            _pool.NameChanged += (s, e) => { _eventsFired |= EventFlags.NameChanged; };
            _pool.TileAdded += (s, e) => { _eventsFired |= EventFlags.TileAdded; };
            _pool.TileRemoved += (s, e) => { _eventsFired |= EventFlags.TileRemoved; };
            _pool.TileModified += (s, e) => { _eventsFired |= EventFlags.TileModified; };

            _pool.CustomProperties.PropertyAdded += (s, e) => { _eventsFired |= EventFlags.PropertyAdded; };
            _pool.CustomProperties.PropertyRemoved += (s, e) => { _eventsFired |= EventFlags.PropertyRemoved; };
            _pool.CustomProperties.PropertyModified += (s, e) => { _eventsFired |= EventFlags.PropertyModified; };
            _pool.CustomProperties.PropertyRenamed += (s, e) => { _eventsFired |= EventFlags.PropertyRenamed; };
        }

        [Test]
        public void AddTile ()
        {
            Assert.AreEqual(0, _registry.TileCount);
            Assert.AreEqual(0, _pool.Count);

            Texture2D tex = new Texture2D(_service.GraphicsDevice, 16, 16);

            _pool.TileAdded += (s, e) =>
            {
                Assert.AreEqual(_pool, e.Tile.Pool);
                Assert.AreEqual(tex.Width, e.Tile.Width);
                Assert.AreEqual(tex.Height, e.Tile.Height);
            };
            
            int id = _pool.AddTile(tex);

            Assert.AreEqual(EventFlags.Modified | EventFlags.TileAdded, _eventsFired);
            Assert.AreEqual(1, _registry.TileCount);
            Assert.AreEqual(1, _pool.Count);
        }

        [Test]
        public void RemoveTile ()
        {
            Texture2D tex = new Texture2D(_service.GraphicsDevice, 16, 16);
            int id = _pool.AddTile(tex);

            _eventsFired = EventFlags.None;

            _pool.TileRemoved += (s, e) =>
            {
                Assert.AreEqual(_pool, e.Tile.Pool);
                Assert.AreEqual(id, e.Tile.Id);
                Assert.AreEqual(tex.Width, e.Tile.Width);
                Assert.AreEqual(tex.Height, e.Tile.Height);
            };

            _pool.RemoveTile(id);

            Assert.AreEqual(EventFlags.Modified | EventFlags.TileRemoved, _eventsFired);
            Assert.AreEqual(0, _registry.TileCount);
            Assert.AreEqual(0, _pool.Count);
        }

        [Test]
        public void DoNotRepeatTileId ()
        {
            Texture2D tex = new Texture2D(_service.GraphicsDevice, 16, 16);
            int id1 = _pool.AddTile(tex);
            _pool.RemoveTile(id1);
            int id2 = _pool.AddTile(tex);

            Assert.AreEqual(id1 + 1, id2);
            Assert.AreEqual(1, _registry.TileCount);
            Assert.AreEqual(1, _pool.Count);
        }

        [Test]
        public void ModifyTileTexture ()
        {
            Texture2D tex1 = RandomTexture(16, 16);
            Texture2D tex2 = RandomTexture(16, 16);
            Assert.IsFalse(TexturesEqual(tex1, tex2));

            int id = _pool.AddTile(tex1);

            _eventsFired = EventFlags.None;

            _pool.TileModified += (s, e) =>
            {
                Assert.AreEqual(_pool, e.Tile.Pool);
                Assert.AreEqual(id, e.Tile.Id);
                Assert.IsTrue(TexturesEqual(tex2, e.Tile.Pool.GetTileTexture(id)));
            };

            _pool.SetTileTexture(id, tex2);

            Assert.AreEqual(EventFlags.Modified | EventFlags.TileModified, _eventsFired);
            Assert.AreEqual(1, _registry.TileCount);
            Assert.AreEqual(1, _pool.Count);
        }

        [Test]
        public void ModifyTileTextureData ()
        {
            Texture2D tex1 = RandomTexture(16, 16);
            Texture2D tex2 = RandomTexture(16, 16);
            Assert.IsFalse(TexturesEqual(tex1, tex2));

            byte[] data = new byte[16 * 16 * 4];
            tex2.GetData(data);

            int id = _pool.AddTile(tex1);

            _eventsFired = EventFlags.None;

            _pool.TileModified += (s, e) =>
            {
                Assert.AreEqual(_pool, e.Tile.Pool);
                Assert.AreEqual(id, e.Tile.Id);
                Assert.IsTrue(TexturesEqual(tex2, e.Tile.Pool.GetTileTexture(id)));
            };

            _pool.SetTileTextureData(id, data);

            Assert.AreEqual(EventFlags.Modified | EventFlags.TileModified, _eventsFired);
            Assert.AreEqual(1, _registry.TileCount);
            Assert.AreEqual(1, _pool.Count);
        }

        private Texture2D RandomTexture (int w, int h)
        {
            Color[] data = new Color[w * h];
            for (int i = 0; i < data.Length; i++) {
                data[i] = new Color((float)_rand.NextDouble(), (float)_rand.NextDouble(), (float)_rand.NextDouble());
            }

            Texture2D tex = new Texture2D(_service.GraphicsDevice, w, h);
            tex.SetData(data);

            return tex;
        }

        private bool TexturesEqual (Texture2D tex1, Texture2D tex2)
        {
            if (tex1.Width != tex2.Width || tex1.Height != tex2.Height)
                return false;

            Color[] data1 = new Color[tex1.Width * tex1.Height];
            Color[] data2 = new Color[tex2.Width * tex2.Height];
            tex1.GetData(data1);
            tex2.GetData(data2);

            for (int i = 0; i < data1.Length; i++) {
                if (data1[i] != data2[i])
                    return false;
            }

            return true;
        }

        [Test]
        public void ModifyTileTextureDirect ()
        {
            Texture2D tex1 = RandomTexture(16, 16);
            Texture2D tex2 = RandomTexture(16, 16);
            Assert.IsFalse(TexturesEqual(tex1, tex2));

            int id = _pool.AddTile(tex1);

            _eventsFired = EventFlags.None;

            _pool.TileModified += (s, e) =>
            {
                Assert.AreEqual(_pool, e.Tile.Pool);
                Assert.AreEqual(id, e.Tile.Id);
                Assert.IsTrue(TexturesEqual(tex2, e.Tile.Pool.GetTileTexture(id)));
            };

            byte[] data = new byte[16 * 16 * 4];
            tex2.GetData(data);

            Tile tile = _pool.GetTile(id);
            tile.Update(data);

            Assert.AreEqual(EventFlags.Modified | EventFlags.TileModified, _eventsFired);
            Assert.AreEqual(1, _registry.TileCount);
            Assert.AreEqual(1, _pool.Count);
        }

        [Test]
        public void RenamePool ()
        {
            _pool.NameChanged += (s, e) =>
            {
                Assert.AreEqual("pool", e.OldName);
                Assert.AreEqual("ocean", e.NewName);
                Assert.AreEqual(e.NewName, _pool.Name);
            };

            _pool.Name = "ocean";

            Assert.AreEqual(EventFlags.NameChanged, _eventsFired);
        }

        [Test]
        public void RenamePoolByProperty ()
        {
            _pool.NameChanged += (s, e) =>
            {
                Assert.AreEqual("pool", e.OldName);
                Assert.AreEqual("ocean", e.NewName);
                Assert.AreEqual(e.NewName, _pool.Name);
            };

            Property nameProperty = _pool.LookupProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.AreEqual("pool", nameProperty.ToString());

            nameProperty.Parse("ocean");

            nameProperty = _pool.LookupProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.AreEqual("ocean", nameProperty.ToString());

            Assert.AreEqual(EventFlags.NameChanged, _eventsFired);
        }

        [Test]
        public void RenamePoolByProperty2 ()
        {
            _pool.NameChanged += (s, e) =>
            {
                Assert.AreEqual("pool", e.OldName);
                Assert.AreEqual("ocean", e.NewName);
                Assert.AreEqual(e.NewName, _pool.Name);
            };

            Property nameProperty = _pool.PredefinedProperties["Name"];
            Assert.NotNull(nameProperty);
            Assert.AreEqual("pool", nameProperty.ToString());

            nameProperty.Parse("ocean");

            nameProperty = _pool.LookupProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.AreEqual("ocean", nameProperty.ToString());

            Assert.AreEqual(EventFlags.NameChanged, _eventsFired);
        }

        [Test]
        public void AddProperty ()
        {
            Assert.AreEqual(0, _pool.CustomProperties.Count());
            
            Property prop = new StringProperty("author", "Justin");

            _pool.CustomProperties.PropertyAdded += (s, e) =>
            {
                Assert.AreEqual(prop, e.Property);
            };
            _pool.CustomProperties.Add(prop);

            Assert.AreEqual(EventFlags.Modified | EventFlags.PropertyAdded, _eventsFired);
            Assert.AreEqual(1, _pool.CustomProperties.Count());
            Assert.AreEqual(PropertyCategory.Custom, _pool.LookupPropertyCategory("author"));
            Assert.AreEqual(prop, _pool.LookupProperty("author"));
        }

        [Test]
        public void RemoveProperty ()
        {
            Assert.AreEqual(0, _pool.CustomProperties.Count());

            Property prop1 = new StringProperty("author", "Justin");
            Property prop2 = new StringProperty("date", "May");
            _pool.CustomProperties.Add(prop1);
            _pool.CustomProperties.Add(prop2);

            Assert.AreEqual(2, _pool.CustomProperties.Count());

            _eventsFired = EventFlags.None;
            _pool.CustomProperties.PropertyRemoved += (s, e) => 
            {
                Assert.AreEqual(prop1, e.Property);
            };
            _pool.CustomProperties.Remove("author");

            Assert.AreEqual(EventFlags.Modified | EventFlags.PropertyRemoved, _eventsFired);
            Assert.AreEqual(1, _pool.CustomProperties.Count());

            Assert.Null(_pool.LookupProperty("author"));
            Assert.AreEqual(PropertyCategory.Custom, _pool.LookupPropertyCategory("date"));
            Assert.AreEqual(prop2, _pool.LookupProperty("date"));
        }

        [Test]
        public void ModifyPropertyValue ()
        {
            Assert.AreEqual(0, _pool.CustomProperties.Count());

            StringProperty prop = new StringProperty("author", "Justin");
            _pool.CustomProperties.Add(prop);

            _eventsFired = EventFlags.None;

            _pool.CustomProperties.PropertyModified += (s, e) =>
            {
                Assert.AreEqual(prop, e.Property);
                Assert.AreEqual("Andy", e.Property.ToString());
            };
            prop.Value = "Andy";

            Assert.AreEqual(EventFlags.Modified | EventFlags.PropertyModified, _eventsFired);
            Assert.AreEqual(1, _pool.CustomProperties.Count());
            Assert.AreEqual(PropertyCategory.Custom, _pool.LookupPropertyCategory("author"));
            Assert.AreEqual(prop, _pool.LookupProperty("author"));
        }

        [Test]
        public void RenameProperty ()
        {
            Assert.AreEqual(0, _pool.CustomProperties.Count());

            StringProperty prop = new StringProperty("author", "Justin");
            _pool.CustomProperties.Add(prop);

            _eventsFired = EventFlags.None;

            _pool.CustomProperties.PropertyRenamed += (s, e) =>
            {
                Assert.AreEqual("author", e.OldName);
                Assert.AreEqual("developer", e.NewName);
            };
            prop.Name = "developer";

            Assert.AreEqual(EventFlags.Modified | EventFlags.PropertyRenamed, _eventsFired);
            Assert.AreEqual(1, _pool.CustomProperties.Count());

            Assert.Null(_pool.LookupProperty("author"));
            Assert.AreEqual(PropertyCategory.Custom, _pool.LookupPropertyCategory("developer"));
            Assert.AreEqual(prop, _pool.LookupProperty("developer"));
        }

        [Test]
        public void TestImport ()
        {
            using (FileStream fs = File.OpenRead("TestContent/purple_caves.png")) {
                TilePool pool = TilePool.Import("Test", _registry, fs, 16, 16);

                Assert.AreEqual(64, pool.Count);
                Assert.AreEqual(64, pool.Capacity);

                Assert.AreEqual("Test", pool.Name);
                Assert.AreEqual(16, pool.TileWidth);
                Assert.AreEqual(16, pool.TileHeight);
            }
        }

        [Test]
        public void TestExport ()
        {
            using (FileStream fs = File.OpenRead("TestContent/purple_caves.png")) {
                TilePool pool = TilePool.Import("Test", _registry, fs, 16, 16);
                pool.Export("TestOutput/purple_caves1.png");
            }

            using (FileStream fs = File.OpenRead("TestOutput/purple_caves1.png")) {
                TilePool pool = TilePool.Import("Test", _registry, fs, 16, 16);

                Assert.AreEqual(64, pool.Count);
                Assert.AreEqual(64, pool.Capacity);
            }
        }

        [Test]
        public void AddTileOverCapacity ()
        {
            using (FileStream fs = File.OpenRead("TestContent/purple_caves.png")) {
                TilePool pool = TilePool.Import("Test", _registry, fs, 16, 16);

                Texture2D tex = new Texture2D(_service.GraphicsDevice, 16, 16);

                pool.TileAdded += (s, e) =>
                {
                    Assert.AreEqual(pool, e.Tile.Pool);
                    Assert.AreEqual(tex.Width, e.Tile.Width);
                    Assert.AreEqual(tex.Height, e.Tile.Height);
                };

                int id = pool.AddTile(tex);

                Assert.AreEqual(65, _registry.TileCount);
                Assert.AreEqual(65, pool.Count);
                Assert.AreEqual(128, pool.Capacity);
            }
        }
    }
}
