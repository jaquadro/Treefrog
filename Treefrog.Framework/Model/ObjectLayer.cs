using System;
using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using System.Xml;
using Treefrog.Framework.Compat;

namespace Treefrog.Framework.Model
{
    public class ObjectInstanceEventArgs : EventArgs
    {
        public ObjectInstance Instance { get; private set; }

        public ObjectInstanceEventArgs (ObjectInstance inst)
        {
            Instance = inst;
        }
    }

    public enum ObjectRegionTest
    {
        Origin,
        Mask,
        PartialMask,
        Image,
        PartialImage,
    }

    public enum ObjectPointTest
    {
        Mask,
        Image,
    }

    public class ObjectLayer : Layer
    {
        private List<ObjectInstance> _objects;
        private int _layerOriginX;
        private int _layerOriginY;
        private int _layerWidth;
        private int _layerHeight;

        public ObjectLayer (string name, int layerOriginX, int layerOriginY, int layerWidth, int layerHeight)
            : base(name)
        {
            _objects = new List<ObjectInstance>();
            _layerOriginX = layerOriginX;
            _layerOriginY = layerOriginY;
            _layerWidth = layerWidth;
            _layerHeight = layerHeight;
        }

        public ObjectLayer (string name, Level level)
            : this(name, level.OriginX, level.OriginY, level.Width, level.Height)
        { }

        public ObjectLayer (string name, ObjectLayer layer)
            : base(name, layer)
        {
            _layerOriginX = layer._layerOriginX;
            _layerOriginY = layer._layerOriginY;
            _layerWidth = layer._layerWidth;
            _layerHeight = layer._layerHeight;

            _objects = new List<ObjectInstance>();
            foreach (ObjectInstance inst in layer._objects)
                _objects.Add(inst.Clone() as ObjectInstance);
        }

        public ObjectLayer (ObjectLayerXmlProxy proxy, Level level)
            : this(proxy.Name, level)
        {
            Opacity = proxy.Opacity;
            IsVisible = proxy.Visible;
            Level = level;

            NamedObservableCollection<ObjectPool> pools = Level.Project.ObjectPoolManager.Pools;
            foreach (ObjectInstanceXmlProxy objProxy in proxy.Objects) {
                string[] coords = objProxy.At.Split(new char[] { ',' });

                ObjectPool pool = Level.Project.ObjectPoolManager.PoolFromItemKey(objProxy.Class);
                foreach (ObjectClass objClass in pool) {
                    if (objClass.Id == objProxy.Class) {
                        ObjectInstance inst = new ObjectInstance(objClass);
                        inst.X = Convert.ToInt32(coords[0]);
                        inst.Y = Convert.ToInt32(coords[1]);
                        inst.Rotation = MathEx.DegToRad(objProxy.Rotation);
                        AddObject(inst);
                        break;
                    }
                }
            }

            foreach (PropertyXmlProxy propertyProxy in proxy.Properties)
                CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
        }

        public static ObjectLayerXmlProxy ToXmlProxy (ObjectLayer layer)
        {
            if (layer == null)
                return null;

            List<ObjectInstanceXmlProxy> objs = new List<ObjectInstanceXmlProxy>();
            foreach (ObjectInstance inst in layer.Objects) {
                objs.Add(new ObjectInstanceXmlProxy()
                {
                    Class = inst.ObjectClass.Id,
                    At = inst.X + "," + inst.Y,
                });
            }

            List<PropertyXmlProxy> props = new List<PropertyXmlProxy>();
            foreach (Property prop in layer.CustomProperties)
                props.Add(Property.ToXmlProxy(prop));

            return new ObjectLayerXmlProxy()
            {
                Name = layer.Name,
                Opacity = layer.Opacity,
                Visible = layer.IsVisible,
                Objects = objs.Count > 0 ? objs : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public event EventHandler<ObjectInstanceEventArgs> ObjectAdded;

        public event EventHandler<ObjectInstanceEventArgs> ObjectRemoved;

        protected virtual void OnObjectAdded (ObjectInstanceEventArgs e)
        {
            if (ObjectAdded != null)
                ObjectAdded(this, e);
        }

        protected virtual void OnObjectRemoved (ObjectInstanceEventArgs e)
        {
            if (ObjectRemoved != null)
                ObjectRemoved(this, e);
        }

        public override bool IsResizable
        {
            get { return true; }
        }

        public override int LayerOriginX
        {
            get { return _layerOriginX; }
        }

        public override int LayerOriginY
        {
            get { return _layerOriginY; }
        }

        public override int LayerHeight
        {
            get { return _layerHeight; }
        }

        public override int LayerWidth
        {
            get { return _layerWidth; }
        }

        public override void RequestNewSize (int originX, int originY, int pixelsWide, int pixelsHigh)
        {
            _layerOriginX = originX;
            _layerOriginY = originY;
            _layerHeight = pixelsHigh;
            _layerWidth = pixelsWide;
        }

        public void AddObject (ObjectInstance instance)
        {
            if (!_objects.Contains(instance)) {
                _objects.Add(instance);
                OnObjectAdded(new ObjectInstanceEventArgs(instance));
            }
        }

        public void RemoveObject (ObjectInstance instance)
        {
            if (_objects.Remove(instance))
                OnObjectRemoved(new ObjectInstanceEventArgs(instance));
        }

        public IEnumerable<ObjectInstance> Objects
        {
            get { return _objects; }
        }

        public IEnumerable<ObjectInstance> ObjectsInRegion (Rectangle region, ObjectRegionTest test)
        {
            Func<Rectangle, ObjectInstance, bool> testFunc = null;
            switch (test) {
                case ObjectRegionTest.Image:
                    testFunc = TestImageBounds;
                    break;
                case ObjectRegionTest.PartialImage:
                    testFunc = TestImageBoundsPartial;
                    break;
                case ObjectRegionTest.Mask:
                    testFunc = TestMaskBounds;
                    break;
                case ObjectRegionTest.PartialMask:
                    testFunc = TestMaskBoundsPartial;
                    break;
                case ObjectRegionTest.Origin:
                    testFunc = TestOrigin;
                    break;
            }

            foreach (ObjectInstance inst in _objects) {
                if (testFunc(region, inst))
                    yield return inst;
            }
        }

        public IEnumerable<ObjectInstance> ObjectsAtPoint (Point point, ObjectPointTest test)
        {
            Func<Point, ObjectInstance, bool> testFunc = null;
            switch (test) {
                case ObjectPointTest.Image:
                    testFunc = TestImageAtPoint;
                    break;
                case ObjectPointTest.Mask:
                    testFunc = TestMaskAtPoint;
                    break;
            }

            foreach (ObjectInstance inst in _objects) {
                if (testFunc(point, inst))
                    yield return inst;
            }
        }

        private bool TestImageAtPoint (Point point, ObjectInstance instance)
        {
            Rectangle bounds = instance.ImageBounds;
            return bounds.Left <= point.X
                && bounds.Right > point.X
                && bounds.Top <= point.Y
                && bounds.Bottom > point.Y;
        }

        private bool TestMaskAtPoint (Point point, ObjectInstance instance)
        {
            Rectangle bounds = instance.MaskBounds;
            return bounds.Left <= point.X
                && bounds.Right > point.X
                && bounds.Top <= point.Y
                && bounds.Bottom > point.Y;
        }

        private bool TestImageBounds (Rectangle region, ObjectInstance instance)
        {
            Rectangle bounds = instance.ImageBounds;
            return bounds.Left >= region.Left
                && bounds.Right <= region.Right
                && bounds.Top >= region.Top
                && bounds.Bottom <= region.Bottom;
        }

        private bool TestImageBoundsPartial (Rectangle region, ObjectInstance instance)
        {
            Rectangle bounds = instance.ImageBounds;
            return bounds.Right > region.Left
                && bounds.Left < region.Right
                && bounds.Bottom > region.Top
                && bounds.Top < region.Bottom;
        }

        private bool TestMaskBounds (Rectangle region, ObjectInstance instance)
        {
            Rectangle bounds = instance.MaskBounds;
            return bounds.Left >= region.Left
                && bounds.Right <= region.Right
                && bounds.Top >= region.Top
                && bounds.Bottom <= region.Bottom;
        }

        private bool TestMaskBoundsPartial (Rectangle region, ObjectInstance instance)
        {
            Rectangle bounds = instance.MaskBounds;
            return bounds.Right > region.Left
                && bounds.Left < region.Right
                && bounds.Bottom > region.Top
                && bounds.Top < region.Bottom;
        }

        private bool TestOrigin (Rectangle region, ObjectInstance instance)
        {
            return instance.X >= region.Left
                && instance.X <= region.Right
                && instance.Y >= region.Top
                && instance.Y <= region.Bottom;
        }

        public override object Clone ()
        {
            return new ObjectLayer(Name, this);
        }

        /*
        public override void WriteXml (XmlWriter writer)
        {
            // <layer name="" type="object">
            writer.WriteStartElement("layer");
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("type", "object");

            if (Opacity < 1f) {
                writer.WriteAttributeString("opacity", Opacity.ToString("0.###"));
            }

            if (!IsVisible) {
                writer.WriteAttributeString("visible", "False");
            }

            writer.WriteStartElement("objects");
            foreach (ObjectInstance inst in _objects) {
                writer.WriteStartElement("object");
                writer.WriteAttributeString("class", inst.ObjectClass.Id.ToString());
                writer.WriteAttributeString("at", inst.X + "," + inst.Y);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            // <properties> [optional]
            if (CustomProperties.Count > 0) {
                writer.WriteStartElement("properties");

                foreach (Property property in CustomProperties) {
                    property.WriteXml(writer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        protected override bool ReadXmlElement (XmlReader reader, string name)
        {
            switch (name) {
                case "objects":
                    ReadXmlObjects(reader);
                    return true;
            }

            return base.ReadXmlElement(reader, name);
        }

        private void ReadXmlObjects (XmlReader reader)
        {
            XmlHelper.SwitchAll(reader, (xmlr, s) => {
                switch (s) {
                    case "object":
                        AddObjectFromXml(xmlr);
                        break;
                }
            });
        }

        private void AddObjectFromXml (XmlReader reader)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> {
                "class", "at",
            });

            string[] coords = attribs["at"].Split(new char[] { ',' });
            int classId = Convert.ToInt32(attribs["class"]);

            NamedObservableCollection<ObjectPool> pools = Level.Project.ObjectPoolManager.Pools;
            ObjectPool pool = Level.Project.ObjectPoolManager.PoolFromItemKey(classId);
            foreach (ObjectClass objClass in pool) {
                if (objClass.Id == classId) {
                    ObjectInstance inst = new ObjectInstance(objClass);
                    inst.X = Convert.ToInt32(coords[0]);
                    inst.Y = Convert.ToInt32(coords[1]);
                    AddObject(inst);
                    break;
                }
            }
        }
        */
    }
}
