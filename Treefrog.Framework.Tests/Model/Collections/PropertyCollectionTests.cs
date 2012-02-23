using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Model;

namespace Treefrog.Framework.Tests.Model.Collections
{
    [TestFixture]
    class PropertyCollectionTests
    {
        [Flags]
        private enum EventFlags
        {
            None = 0,
            Modified = 1,
            PropertyAdded = 2,
            PropertyRemoved = 4,
            PropertyModified = 8,
            PropertyRenamed = 16,
        }

        private EventFlags _eventsFired;

        [SetUp]
        public void TestSetup ()
        {
            _eventsFired = EventFlags.None;
        }

        private void AttachEvents (PropertyCollection collection)
        {
            collection.PropertyAdded += (s, e) => { _eventsFired |= EventFlags.PropertyAdded; };
            collection.PropertyRemoved += (s, e) => { _eventsFired |= EventFlags.PropertyRemoved; };
            collection.PropertyModified += (s, e) => { _eventsFired |= EventFlags.PropertyModified; };
            collection.PropertyRenamed += (s, e) => { _eventsFired |= EventFlags.PropertyRenamed; };
            collection.Modified += (s, e) => { _eventsFired |= EventFlags.Modified; };
        }

        [Test]
        public void AddPropertyTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            AttachEvents(collection);

            Property prop = new StringProperty("test", "orange");

            collection.PropertyAdded += (s, e) =>
            {
                Assert.AreSame(prop, e.Property);
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains("test"));
                Assert.IsTrue(collection.Contains(prop));
            };

            collection.Add(prop);

            Assert.AreEqual(EventFlags.PropertyAdded | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void RemovePropertyTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            AttachEvents(collection);

            Property prop1 = new StringProperty("test", "orange");
            Property prop2 = new StringProperty("two", "number");

            collection.Add(prop1);
            collection.Add(prop2);

            collection.PropertyRemoved += (s, e) =>
            {
                Assert.AreSame(prop1, e.Property);
                Assert.AreEqual(1, collection.Count);
                Assert.IsFalse(collection.Contains("test"));
                Assert.IsTrue(collection.Contains("two"));
                Assert.IsFalse(collection.Contains(prop1));
                Assert.IsTrue(collection.Contains(prop2));
            };

            _eventsFired = EventFlags.None;

            collection.Remove(prop1);

            Assert.AreEqual(EventFlags.PropertyRemoved | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void RemovePropertyStringTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            AttachEvents(collection);

            Property prop1 = new StringProperty("test", "orange");
            Property prop2 = new StringProperty("two", "number");

            collection.Add(prop1);
            collection.Add(prop2);

            collection.PropertyRemoved += (s, e) =>
            {
                Assert.AreSame(prop1, e.Property);
                Assert.AreEqual(1, collection.Count);
                Assert.IsFalse(collection.Contains("test"));
                Assert.IsTrue(collection.Contains("two"));
                Assert.IsFalse(collection.Contains(prop1));
                Assert.IsTrue(collection.Contains(prop2));
            };

            _eventsFired = EventFlags.None;

            collection.Remove("test");

            Assert.AreEqual(EventFlags.PropertyRemoved | EventFlags.Modified, _eventsFired);
        } 

        [Test]
        public void ModifyPropertyTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            AttachEvents(collection);

            StringProperty prop = new StringProperty("test", "orange");

            collection.PropertyModified += (s, e) =>
            {
                Assert.AreSame(prop, e.Property);
                Assert.AreEqual(1, collection.Count);
                Assert.IsTrue(collection.Contains("test"));
                Assert.IsTrue(collection.Contains(prop));
                Assert.AreEqual("blue", e.Property.ToString());
            };

            collection.Add(prop);

            _eventsFired = EventFlags.None;

            prop.Value = "blue";

            Assert.AreEqual(EventFlags.PropertyModified | EventFlags.Modified, _eventsFired);
        }

        [Test]
        public void RenamePropertyTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            AttachEvents(collection);

            StringProperty prop = new StringProperty("test", "orange");

            collection.PropertyRenamed += (s, e) =>
            {
                Assert.AreEqual("test", e.OldName);
                Assert.AreEqual("two", e.NewName);
                Assert.AreEqual(1, collection.Count);
                Assert.IsFalse(collection.Contains("test"));
                Assert.IsTrue(collection.Contains("two"));
                Assert.IsTrue(collection.Contains(prop));
            };

            collection.Add(prop);

            _eventsFired = EventFlags.None;

            prop.Name = "two";

            Assert.AreEqual(EventFlags.PropertyRenamed | EventFlags.Modified, _eventsFired);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullPropertyTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            collection.Add(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddReservedNameTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[] { "Name" });
            Property prop = new StringProperty("Name", "Treefrog");
            collection.Add(prop);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddDuplicateNameTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            Property prop1 = new StringProperty("Name", "Treefrog");
            Property prop2 = new StringProperty("Name", "Amphibian");

            collection.Add(prop1);
            collection.Add(prop2);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RenameReservedNameTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[] { "Name" });
            Property prop = new StringProperty("Author", "Treefrog");
            collection.Add(prop);

            prop.Name = "Name";
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void RenameDuplicateNameTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            Property prop1 = new StringProperty("Author", "Treefrog");
            Property prop2 = new StringProperty("Developer", "Justin");
            collection.Add(prop1);
            collection.Add(prop2);

            prop1.Name = "Developer";
        }

        [Test]
        public void ClearPropertyTest ()
        {
            PropertyCollection collection = new PropertyCollection(new string[0]);
            AttachEvents(collection);

            Property prop1 = new StringProperty("test", "orange");
            Property prop2 = new StringProperty("two", "number");

            collection.Add(prop1);
            collection.Add(prop2);

            int rmCount = 0;
            collection.PropertyRemoved += (s, e) =>
            {
                rmCount++;
            };

            _eventsFired = EventFlags.None;

            collection.Clear();

            Assert.AreEqual(EventFlags.PropertyRemoved | EventFlags.Modified, _eventsFired);
            Assert.AreEqual(2, rmCount);
            Assert.AreEqual(0, collection.Count);
        }
    }
}
