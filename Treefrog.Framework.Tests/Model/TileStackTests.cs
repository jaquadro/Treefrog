using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Framework.Tests.Model
{
    [TestFixture]
    class TileStackTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
        }

        private Random _rand = new Random();

        private GraphicsDeviceService _service;
        private EventFlags _eventsFired;
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

        private void AttachEvents (TileStack stack)
        {
            stack.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
        }

        [Test]
        public void AddTileTest ()
        {
            TileStack stack = new TileStack();
            AttachEvents(stack);

            stack.Add(_tile1);

            Assert.AreEqual(EventFlags.Modified, _eventsFired);
            Assert.AreEqual(1, stack.Count);
            Assert.AreSame(_tile1, stack.Top);
        }

        [Test]
        public void AddTileTest2 ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile2);

            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual(_tile1, stack[0]);
            Assert.AreEqual(_tile2, stack[1]);
            Assert.AreEqual(_tile2, stack.Top);
        }

        [Test]
        public void AddTileDuplicateTest ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile1);

            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual(_tile1, stack[0]);
            Assert.AreEqual(_tile1, stack[1]);
        }

        [Test]
        public void RemoveTileTest ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile2);

            AttachEvents(stack);

            stack.Remove(_tile1);

            Assert.AreEqual(EventFlags.Modified, _eventsFired);
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(_tile2, stack.Top);
        }

        [Test]
        public void RemoveTileNotPresentTest ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile2);

            AttachEvents(stack);

            Texture2D texture = new Texture2D(_service.GraphicsDevice, 16, 16);
            Tile tile3 = _pool.GetTile(_pool.AddTile(texture));

            stack.Remove(tile3);

            Assert.AreEqual(EventFlags.None, _eventsFired);
            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual(_tile1, stack[0]);
            Assert.AreEqual(_tile2, stack[1]);
        }

        [Test]
        public void RemoveTileNullTest ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile2);

            AttachEvents(stack);

            stack.Remove(null);

            Assert.AreEqual(EventFlags.None, _eventsFired);
            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual(_tile1, stack[0]);
            Assert.AreEqual(_tile2, stack[1]);
        }

        [Test]
        public void ClearTest ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile2);

            AttachEvents(stack);

            stack.Clear();

            Assert.AreEqual(EventFlags.Modified, _eventsFired);
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void ClearEmptyTest ()
        {
            TileStack stack = new TileStack();
            AttachEvents(stack);

            stack.Clear();

            Assert.AreEqual(EventFlags.None, _eventsFired);
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void CreateStackCopyConstructorTest ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile2);

            TileStack stack2 = new TileStack(stack);

            Assert.AreEqual(2, stack2.Count);
            Assert.AreSame(_tile1, stack2[0]);
            Assert.AreSame(_tile2, stack2[1]);
        }

        [Test]
        public void CloneStackTest ()
        {
            TileStack stack = new TileStack();

            stack.Add(_tile1);
            stack.Add(_tile2);

            TileStack stack2 = stack.Clone() as TileStack;

            Assert.AreEqual(2, stack2.Count);
            Assert.AreSame(_tile1, stack2[0]);
            Assert.AreSame(_tile2, stack2[1]);
        }
    }
}
