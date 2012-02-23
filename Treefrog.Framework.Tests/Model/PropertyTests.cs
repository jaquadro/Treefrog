using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Framework.Tests.Model
{
    [TestFixture]
    class PropertyTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
            NameChanged = 2,
            ValueChanged = 4,
        }

        private EventFlags _eventsFired;

        [SetUp]
        public void TestSetup ()
        {
            _eventsFired = EventFlags.None;
        }

        private void AttachEvents (Property property)
        {
            property.NameChanged += (s, e) => { _eventsFired |= EventFlags.NameChanged; };
            property.ValueChanged += (s, e) => { _eventsFired |= EventFlags.ValueChanged; };
            property.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
        }

        [Test]
        public void CreateStringProperty ()
        {
            StringProperty prop = new StringProperty("test", "orange");
            Assert.AreEqual("orange", prop.Value);
        }

        [Test]
        public void StringToString ()
        {
            Property prop = new StringProperty("test", "orange");
            Assert.AreEqual("orange", prop.ToString());
        }

        [Test]
        public void ModifyString ()
        {
            StringProperty prop = new StringProperty("test", "orange");
            AttachEvents(prop);

            prop.ValueChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual("green", prop.Value);
            };
            
            prop.Value = "green";

            Assert.AreEqual("green", prop.Value);
            Assert.AreEqual(EventFlags.ValueChanged | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void ModifyStringSame ()
        {
            StringProperty prop = new StringProperty("test", "orange");
            AttachEvents(prop);

            prop.Value = "orange";

            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void ModifyName ()
        {
            Property prop = new StringProperty("test", "orange");
            AttachEvents(prop);

            prop.NameChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual("test", e.OldName);
                Assert.AreEqual("cake", e.NewName);
                Assert.AreEqual("cake", prop.Name);
            };

            prop.Name = "cake";

            Assert.AreEqual("cake", prop.Name);
            Assert.AreEqual(EventFlags.NameChanged, _eventsFired);
        }

        [Test]
        public void ModifyNameSame ()
        {
            Property prop = new StringProperty("test", "orange");
            AttachEvents(prop);

            prop.Name = "test";

            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ModifyNameNull ()
        {
            Property prop = new StringProperty("test", "orange");
            prop.Name = null;
        }

        [Test]
        public void CloneString ()
        {
            Property prop = new StringProperty("test", "orange");
            AttachEvents(prop);

            Property prop2 = prop.Clone() as Property;

            StringProperty sp1 = prop as StringProperty;
            StringProperty sp2 = prop2 as StringProperty;
            Assert.AreEqual(sp1.Name, sp2.Name);
            Assert.AreEqual(sp1.Value, sp2.Value);

            // Make sure events were not cloned.
            sp2.Name = "test2";
            sp2.Value = "apple";
            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void CopyConstructString ()
        {
            StringProperty prop = new StringProperty("test", "orange");
            AttachEvents(prop);

            StringProperty prop2 = new StringProperty("new", prop);

            Assert.AreEqual("new", prop2.Name);
            Assert.AreEqual(prop.Value, prop2.Value);

            // Make sure events were not cloned.
            prop2.Name = "test2";
            prop2.Value = "apple";
            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void ParseString ()
        {
            Property prop = new StringProperty("test", "orange");
            AttachEvents(prop);

            prop.ValueChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual("green", ((StringProperty)prop).Value);
            };

            prop.Parse("green");

            Assert.AreEqual("green", ((StringProperty)prop).Value);
            Assert.AreEqual(EventFlags.ValueChanged | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void CreateNumberProperty ()
        {
            NumberProperty prop = new NumberProperty("test", 12.5f);
            Assert.AreEqual(12.5f, prop.Value);
        }

        [Test]
        public void NumberToString ()
        {
            Property prop = new NumberProperty("test", 12.5f);
            Assert.AreEqual("12.5", prop.ToString());
        }

        [Test]
        public void ModifyNumber ()
        {
            NumberProperty prop = new NumberProperty("test", 12.5f);
            AttachEvents(prop);

            prop.ValueChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual(55, prop.Value);
            };

            prop.Value = 55;

            Assert.AreEqual(55, prop.Value);
            Assert.AreEqual(EventFlags.ValueChanged | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void ModifyNumberSame ()
        {
            NumberProperty prop = new NumberProperty("test", 12.5f);
            AttachEvents(prop);

            prop.Value = 12.5f;

            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void CloneNumber ()
        {
            Property prop = new NumberProperty("test", 12.5f);
            AttachEvents(prop);

            Property prop2 = prop.Clone() as Property;

            NumberProperty sp1 = prop as NumberProperty;
            NumberProperty sp2 = prop2 as NumberProperty;
            Assert.AreEqual(sp1.Name, sp2.Name);
            Assert.AreEqual(sp1.Value, sp2.Value);

            // Make sure events were not cloned.
            sp2.Name = "test2";
            sp2.Value = 55;
            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void CopyConstructNumber ()
        {
            NumberProperty prop = new NumberProperty("test", 12.5f);
            AttachEvents(prop);

            NumberProperty prop2 = new NumberProperty("new", prop);

            Assert.AreEqual("new", prop2.Name);
            Assert.AreEqual(prop.Value, prop2.Value);

            // Make sure events were not cloned.
            prop2.Name = "test2";
            prop2.Value = 55f;
            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void ParseNumber ()
        {
            Property prop = new NumberProperty("test", 12.5f);
            AttachEvents(prop);

            prop.ValueChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual(55.5f, ((NumberProperty)prop).Value);
            };

            prop.Parse("55.5");

            Assert.AreEqual(55.5f, ((NumberProperty)prop).Value);
            Assert.AreEqual(EventFlags.ValueChanged | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void CreateBoolProperty ()
        {
            BoolProperty prop = new BoolProperty("test", true);
            Assert.AreEqual(true, prop.Value);
        }

        [Test]
        public void BoolToString ()
        {
            Property prop = new BoolProperty("test", true);
            Assert.AreEqual("true", prop.ToString());
        }

        [Test]
        public void ModifyBool ()
        {
            BoolProperty prop = new BoolProperty("test", true);
            AttachEvents(prop);

            prop.ValueChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual(false, prop.Value);
            };

            prop.Value = false;

            Assert.AreEqual(false, prop.Value);
            Assert.AreEqual(EventFlags.ValueChanged | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void ModifyBoolSame ()
        {
            BoolProperty prop = new BoolProperty("test", true);
            AttachEvents(prop);

            prop.Value = true;

            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void CloneBool ()
        {
            Property prop = new BoolProperty("test", true);
            AttachEvents(prop);

            Property prop2 = prop.Clone() as Property;

            BoolProperty sp1 = prop as BoolProperty;
            BoolProperty sp2 = prop2 as BoolProperty;
            Assert.AreEqual(sp1.Name, sp2.Name);
            Assert.AreEqual(sp1.Value, sp2.Value);

            // Make sure events were not cloned.
            sp2.Name = "test2";
            sp2.Value = false;
            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void CopyConstructBool ()
        {
            BoolProperty prop = new BoolProperty("test", true);
            AttachEvents(prop);

            BoolProperty prop2 = new BoolProperty("new", prop);

            Assert.AreEqual("new", prop2.Name);
            Assert.AreEqual(prop.Value, prop2.Value);

            // Make sure events were not cloned.
            prop2.Name = "test2";
            prop2.Value = false;
            Assert.AreEqual(EventFlags.None, _eventsFired);
        }

        [Test]
        public void ParseBoolFalse ()
        {
            Property prop = new BoolProperty("test", true);
            AttachEvents(prop);

            prop.ValueChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual(false, ((BoolProperty)prop).Value);
            };

            prop.Parse("false");

            Assert.AreEqual(false, ((BoolProperty)prop).Value);
            Assert.AreEqual(EventFlags.ValueChanged | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void ParseBoolTrue ()
        {
            Property prop = new BoolProperty("test", false);
            AttachEvents(prop);

            prop.ValueChanged += (s, e) =>
            {
                Assert.AreSame(prop, s);
                Assert.AreEqual(true, ((BoolProperty)prop).Value);
            };

            prop.Parse("true");

            Assert.AreEqual(true, ((BoolProperty)prop).Value);
            Assert.AreEqual(EventFlags.ValueChanged | EventFlags.Modified, _eventsFired);
        }
    }
}
