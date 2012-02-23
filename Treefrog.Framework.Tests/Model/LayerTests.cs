using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Framework.Tests.Model
{
    [TestFixture]
    class LayerTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
            NameChanged = 2,
            OpacityChanged = 4,
            VisibilityChanged = 8,
        }

        private EventFlags _eventsFired;

        [SetUp]
        public void TestSetup ()
        {
            _eventsFired = EventFlags.None;
        }

        private void AttachEvents (Layer layer)
        {
            layer.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
            layer.NameChanged += (s, e) => { _eventsFired |= EventFlags.NameChanged; };
            layer.OpacityChanged += (s, e) => { _eventsFired |= EventFlags.OpacityChanged; };
            layer.VisibilityChanged += (s, e) => { _eventsFired |= EventFlags.VisibilityChanged; };
        }

        [Test]
        public void RenamePoolByProperty1 ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            Property nameProperty = layer.LookupProperty("Name");

            RenameLayer(layer, nameProperty);
        }

        [Test]
        public void RenamePoolByProperty2 ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            Property nameProperty = layer.PredefinedProperties["Name"];

            RenameLayer(layer, nameProperty);
        }

        private void RenameLayer (Layer layer, Property property)
        {
            AttachEvents(layer);

            layer.NameChanged += (s, e) =>
            {
                Assert.AreEqual("layer", e.OldName);
                Assert.AreEqual("tiles", e.NewName);
                Assert.AreEqual(e.NewName, layer.Name);
            };

            Assert.NotNull(property);
            Assert.AreEqual("layer", layer.Name);

            property.Parse("tiles");

            property = layer.LookupProperty("Name");
            Assert.NotNull(property);
            Assert.AreEqual("tiles", property.ToString());

            Assert.AreEqual(EventFlags.NameChanged, _eventsFired);
        }

        [Test]
        public void AddProperty ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            AttachEvents(layer);

            Property prop = new StringProperty("author", "Justin");
            layer.CustomProperties.Add(prop);

            Assert.AreEqual(EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void ChangeLayerOpacity ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            AttachEvents(layer);

            Assert.AreEqual(1f, layer.Opacity);

            layer.Opacity = 0.5f;

            Assert.AreEqual(EventFlags.OpacityChanged | EventFlags.Modified, _eventsFired);
            Assert.AreEqual(0.5f, layer.Opacity);
        }

        [Test]
        public void ChangeLayerOpacityProperty ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            AttachEvents(layer);

            Assert.AreEqual(1f, layer.Opacity);

            layer.PredefinedProperties["Opacity"].Parse("0.5");

            Assert.AreEqual(EventFlags.OpacityChanged | EventFlags.Modified, _eventsFired);
            Assert.AreEqual(0.5f, layer.Opacity);
        }

        [Test]
        public void LayerOpacityHighRange ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            layer.Opacity = 1.5f;

            Assert.AreEqual(1.0f, layer.Opacity);
        }

        [Test]
        public void LayerOpacityLowRange ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            layer.Opacity = -0.5f;

            Assert.AreEqual(0f, layer.Opacity);
        }

        [Test]
        public void ChangeLayerVisibility ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            AttachEvents(layer);

            Assert.AreEqual(true, layer.IsVisible);

            layer.IsVisible = false;

            Assert.AreEqual(EventFlags.VisibilityChanged | EventFlags.Modified, _eventsFired);
            Assert.AreEqual(false, layer.IsVisible);
        }

        [Test]
        public void ChangeLayerVisibiliityProperty ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);
            AttachEvents(layer);

            Assert.AreEqual(true, layer.IsVisible);

            layer.PredefinedProperties["Visible"].Parse("false");

            Assert.AreEqual(EventFlags.VisibilityChanged | EventFlags.Modified, _eventsFired);
            Assert.AreEqual(false, layer.IsVisible);
        }

        [Test]
        public void CloneLayerTest ()
        {
            Layer layer = new MultiTileGridLayer("layer", 16, 16, 16, 16);

            layer.Opacity = 0.5f;
            layer.IsVisible = false;

            Property prop = new StringProperty("author", "Justin");
            layer.CustomProperties.Add(prop);

            Layer layer2 = layer.Clone() as Layer;

            Assert.AreEqual(layer.Opacity, layer2.Opacity);
            Assert.AreEqual(layer.IsVisible, layer2.IsVisible);
            Assert.AreEqual(1, layer2.CustomProperties.Count);
            Assert.AreNotSame(prop, layer2.CustomProperties["author"]);
            Assert.AreEqual("Justin", layer2.CustomProperties["author"].ToString());
        }
    }
}
