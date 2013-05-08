using System;
using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using System.Xml;
using Treefrog.Framework.Compat;
using Treefrog.Framework;
using Treefrog.Framework.Model.Proxy;

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

        public ObjectLayer (LevelX.ObjectLayerX proxy, Level level)
            : this(proxy.Name, level)
        {
            Opacity = proxy.Opacity;
            IsVisible = proxy.Visible;
            RasterMode = proxy.RasterMode;
            GridColor = Color.ParseArgbHex(proxy.GridColor);
            GridWidth = proxy.GridWidth;
            GridHeight = proxy.GridHeight;
            Level = level;

            ResourceCollection<ObjectPool> pools = Level.Project.ObjectPoolManager.Pools;
            foreach (var objProxy in proxy.Objects) {
                ObjectInstance inst = ObjectInstance.FromXProxy(objProxy, Level.Project.ObjectPoolManager);
                if (inst != null)
                    AddObject(inst);
            }

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }
        }

        public static LevelX.ObjectLayerX ToXmlProxyX (ObjectLayer layer)
        {
            if (layer == null)
                return null;

            List<LevelX.ObjectInstanceX> objs = new List<LevelX.ObjectInstanceX>();
            foreach (ObjectInstance inst in layer.Objects)
                objs.Add(ObjectInstance.ToXProxy(inst));

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in layer.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LevelX.ObjectLayerX() {
                Name = layer.Name,
                Opacity = layer.Opacity,
                Visible = layer.IsVisible,
                RasterMode = layer.RasterMode,
                GridColor = layer.GridColor.ToArgbHex(),
                GridWidth = layer.GridWidth,
                GridHeight = layer.GridHeight,
                Objects = objs.Count > 0 ? objs : null,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public override void ResetModified ()
        {
            base.ResetModified();
            foreach (var inst in Objects)
                inst.ResetModified();
        }

        public event EventHandler<ObjectInstanceEventArgs> ObjectAdded;

        public event EventHandler<ObjectInstanceEventArgs> ObjectRemoved;

        public event EventHandler<ObjectInstanceEventArgs> ObjectReordered;

        protected virtual void OnObjectAdded (ObjectInstanceEventArgs e)
        {
            if (ObjectAdded != null)
                ObjectAdded(this, e);
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnObjectRemoved (ObjectInstanceEventArgs e)
        {
            if (ObjectRemoved != null)
                ObjectRemoved(this, e);
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnObjectReordered (ObjectInstanceEventArgs e)
        {
            if (ObjectReordered != null)
                ObjectReordered(this, e);
            OnModified(EventArgs.Empty);
        }

        private void ObjectInst_Modified (object sender, EventArgs e)
        {
            OnModified(EventArgs.Empty);
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
            if (_layerOriginX != originX || _layerOriginY != originY || _layerHeight != LayerHeight || _layerWidth != LayerWidth) {
                _layerOriginX = originX;
                _layerOriginY = originY;
                _layerHeight = pixelsHigh;
                _layerWidth = pixelsWide;

                OnLayerSizeChanged(EventArgs.Empty);
            }
        }

        public void AddObject (ObjectInstance instance)
        {
            if (!_objects.Contains(instance)) {
                _objects.Add(instance);
                instance.Modified += ObjectInst_Modified;
                OnObjectAdded(new ObjectInstanceEventArgs(instance));
            }
        }

        public void RemoveObject (ObjectInstance instance)
        {
            if (_objects.Remove(instance)) {
                instance.Modified -= ObjectInst_Modified;
                OnObjectRemoved(new ObjectInstanceEventArgs(instance));
            }
        }

        public void MoveObjectBackward (ObjectInstance instance)
        {
            if (_objects.IndexOf(instance) > 1) {
                _objects.MoveItemBy(instance, -1);
                OnObjectReordered(new ObjectInstanceEventArgs(instance));
            }
        }

        public void MoveObjectForward (ObjectInstance instance)
        {
            if (_objects.IndexOf(instance) < _objects.Count - 1) {
                _objects.MoveItemBy(instance, 1);
                OnObjectReordered(new ObjectInstanceEventArgs(instance));
            }
        }

        public void MoveObjectToFront (ObjectInstance instance)
        {
            if (_objects.MoveItem(instance, _objects.Count - 1))
                OnObjectReordered(new ObjectInstanceEventArgs(instance));
        }

        public void MoveObjectToBack (ObjectInstance instance)
        {
            if (_objects.MoveItem(instance, 0))
                OnObjectReordered(new ObjectInstanceEventArgs(instance));
        }

        public void MoveObjectsToFront (IEnumerable<ObjectInstance> instances)
        {
            foreach (ObjectInstance inst in QueueOrderedObjects(instances))
                MoveObjectToFront(inst);
        }

        public void MoveObjectsForward (IEnumerable<ObjectInstance> instances)
        {
            foreach (ObjectInstance inst in QueueOrderedObjects(instances))
                MoveObjectForward(inst);
        }

        public void MoveObjectsBackward (IEnumerable<ObjectInstance> instances)
        {
            foreach (ObjectInstance inst in QueueOrderedObjects(instances))
                MoveObjectBackward(inst);
        }

        public void MoveObjectsToBack (IEnumerable<ObjectInstance> instances)
        {
            foreach (ObjectInstance inst in QueueOrderedObjects(instances))
                MoveObjectToBack(inst);
        }

        public void MoveToIndex (ObjectInstance inst, int index)
        {
            _objects.MoveItem(inst, index);
        }

        public int ObjectIndex (ObjectInstance inst)
        {
            return _objects.IndexOf(inst);
        }

        public int ObjectCount
        {
            get { return _objects.Count; }
        }

        private IEnumerable<ObjectInstance> QueueOrderedObjects (IEnumerable<ObjectInstance> instances)
        {
            HashSet<ObjectInstance> objs = new HashSet<ObjectInstance>(instances);
            List<ObjectInstance> queue = new List<ObjectInstance>();

            foreach (ObjectInstance inst in _objects) {
                if (objs.Contains(inst))
                    queue.Add(inst);
            }

            return queue;
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
    }
}
