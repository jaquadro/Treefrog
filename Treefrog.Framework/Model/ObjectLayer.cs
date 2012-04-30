using System;
using System.Collections.Generic;
using Treefrog.Framework.Imaging;

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

    public class ObjectLayer : Layer
    {
        private List<ObjectInstance> _objects;
        private int _layerWidth;
        private int _layerHeight;

        public ObjectLayer (string name, int layerWidth, int layerHeight)
            : base(name)
        {
            _objects = new List<ObjectInstance>();
            _layerWidth = layerWidth;
            _layerHeight = layerHeight;
        }

        public ObjectLayer (string name, ObjectLayer layer)
            : base(name, layer)
        {
            _layerWidth = layer._layerWidth;
            _layerHeight = layer._layerHeight;

            _objects = new List<ObjectInstance>();
            foreach (ObjectInstance inst in layer._objects)
                _objects.Add(inst.Clone() as ObjectInstance);
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

        public override int LayerHeight
        {
            get { return _layerHeight; }
        }

        public override int LayerWidth
        {
            get { return _layerWidth; }
        }

        public override void RequestNewSize (int pixelsWide, int pixelsHigh)
        {
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

        public IEnumerable<ObjectInstance> ObjectsInRegion (Rectangle region)
        {
            foreach (ObjectInstance inst in _objects) {
                ObjectClass oc = inst.ObjectClass;
                if (inst.X - oc.Origin.X - oc.ImageBounds.Left >= region.Left &&
                    inst.X - oc.Origin.X + oc.ImageBounds.Right <= region.Right &&
                    inst.Y - oc.Origin.Y - oc.ImageBounds.Top >= region.Top &&
                    inst.Y - oc.Origin.Y + oc.ImageBounds.Bottom <= region.Bottom)
                    yield return inst;
            }
        }

        public override object Clone ()
        {
            return new ObjectLayer(Name, this);
        }

        public override void WriteXml (System.Xml.XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
