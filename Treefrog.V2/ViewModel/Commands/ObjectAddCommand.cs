using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Framework.Imaging;
using System;
using Treefrog.ViewModel.Tools;

namespace Treefrog.ViewModel.Commands
{
    public abstract class ObjectLayerCommand : Command
    {
        private WeakReference _toolRef;

        public ObjectLayerCommand ()
        { }

        public ObjectLayerCommand (ObjectSelectTool selectTool)
        {
            if (selectTool != null)
                _toolRef = new WeakReference(selectTool);
        }

        protected void SetSelectedObjects (List<ObjectInstance> objects)
        {
            if (_toolRef != null) {
                ObjectSelectTool tool = _toolRef.Target as ObjectSelectTool;
                if (tool != null && !tool.IsCancelled) {
                    tool.SelectObjects(objects);
                }
            }
        }

        protected void AddSelectedObjects (List<ObjectInstance> objects)
        {
            if (_toolRef != null) {
                ObjectSelectTool tool = _toolRef.Target as ObjectSelectTool;
                if (tool != null && !tool.IsCancelled) {
                    tool.AddObjectsToSelection(objects);
                }
            }
        }

        protected void RemoveSelectedObjects (List<ObjectInstance> objects)
        {
            if (_toolRef != null) {
                ObjectSelectTool tool = _toolRef.Target as ObjectSelectTool;
                if (tool != null && !tool.IsCancelled) {
                    tool.RemoveObjectsFromSelection(objects);
                }
            }
        }
    }

    public class ObjectAddCommand : ObjectLayerCommand
    {
        private ObjectLayer _objectSource;
        private List<ObjectInstance> _objects;

        public ObjectAddCommand (ObjectLayer source)
        {
            _objectSource = source;
            _objects = new List<ObjectInstance>();
        }

        public ObjectAddCommand (ObjectLayer source, ObjectSelectTool selectTool)
            : base(selectTool)
        {
            _objectSource = source;
            _objects = new List<ObjectInstance>();
        }

        public ObjectAddCommand (ObjectLayer source, ObjectInstance inst)
            : this(source)
        {
            _objects.Add(inst);
        }

        public ObjectAddCommand (ObjectLayer source, ObjectInstance inst, ObjectSelectTool selectTool)
            : this(source, selectTool)
        {
            _objects.Add(inst);
        }

        public ObjectAddCommand (ObjectLayer source, List<ObjectInstance> objects)
            : this(source)
        {
            foreach (ObjectInstance inst in objects)
                _objects.Add(inst);
        }

        public ObjectAddCommand (ObjectLayer source, List<ObjectInstance> objects, ObjectSelectTool selectTool)
            : this(source, selectTool)
        {
            foreach (ObjectInstance inst in objects)
                _objects.Add(inst);
        }

        public void QueueAdd (ObjectInstance inst)
        {
            _objects.Add(inst);
        }

        public override void Execute ()
        {
            foreach (ObjectInstance inst in _objects) {
                _objectSource.AddObject(inst);
            }
        }

        public override void Undo ()
        {
            foreach (ObjectInstance inst in _objects) {
                _objectSource.RemoveObject(inst);
            }

            RemoveSelectedObjects(_objects);
        }

        public override void Redo ()
        {
            foreach (ObjectInstance inst in _objects) {
                _objectSource.AddObject(inst);
            }

            AddSelectedObjects(_objects);
        }
    }

    public class ObjectRemoveCommand : ObjectLayerCommand
    {
        private ObjectLayer _objectSource;
        private List<ObjectInstance> _objects;

        public ObjectRemoveCommand (ObjectLayer source)
        {
            _objectSource = source;
            _objects = new List<ObjectInstance>();
        }

        public ObjectRemoveCommand (ObjectLayer source, ObjectSelectTool selectTool)
            : base(selectTool)
        {
            _objectSource = source;
            _objects = new List<ObjectInstance>();
        }

        public ObjectRemoveCommand (ObjectLayer source, ObjectInstance inst)
            : this(source)
        {
            _objects.Add(inst);
        }

        public ObjectRemoveCommand (ObjectLayer source, List<ObjectInstance> objects)
            : this(source)
        {
            foreach (ObjectInstance inst in objects)
                _objects.Add(inst);
        }

        public void QueueRemove (ObjectInstance inst)
        {
            _objects.Add(inst);
        }

        public override void Execute ()
        {
            foreach (ObjectInstance inst in _objects) {
                _objectSource.RemoveObject(inst);
            }
        }

        public override void Undo ()
        {
            foreach (ObjectInstance inst in _objects) {
                _objectSource.AddObject(inst);
            }

            AddSelectedObjects(_objects);
        }

        public override void Redo ()
        {
            foreach (ObjectInstance inst in _objects) {
                _objectSource.RemoveObject(inst);
            }

            RemoveSelectedObjects(_objects);
        }
    }

    public class ObjectMoveCommand : ObjectLayerCommand
    {
        private class ObjectRecord 
        {
            public ObjectInstance Instance;
            public Point OldLocation;
            public Point NewLocation;
        }

        private List<ObjectRecord> _objects;

        public ObjectMoveCommand ()
        {
            _objects = new List<ObjectRecord>();
        }

        public ObjectMoveCommand (ObjectSelectTool selectTool)
            : base(selectTool)
        {
            _objects = new List<ObjectRecord>();
        }

        public ObjectMoveCommand (ObjectInstance inst, int diffX, int diffY)
            : this()
        {
            QueueMove(inst, diffX, diffY);
        }

        public ObjectMoveCommand (List<ObjectInstance> objects, int diffX, int diffY)
            : this()
        {
            foreach (ObjectInstance inst in objects)
                QueueMove(inst, diffX, diffY);
        }

        public void QueueMove (ObjectInstance inst, int diffX, int diffY)
        {
            _objects.Add(new ObjectRecord()
            {
                Instance = inst,
                OldLocation = new Point(inst.X, inst.Y),
                NewLocation = new Point(inst.X + diffX, inst.Y + diffY),
            });
        }

        public void QueueMove (ObjectInstance inst, Point oldLocation, Point newLocation)
        {
            _objects.Add(new ObjectRecord()
            {
                Instance = inst,
                OldLocation = oldLocation,
                NewLocation = newLocation,
            });
        }

        public override void Execute ()
        {
            foreach (ObjectRecord record in _objects) {
                record.Instance.X = record.NewLocation.X;
                record.Instance.Y = record.NewLocation.Y;
            }
        }

        public override void Undo ()
        {
            List<ObjectInstance> objects = new List<ObjectInstance>();
            foreach (ObjectRecord record in _objects) {
                record.Instance.X = record.OldLocation.X;
                record.Instance.Y = record.OldLocation.Y;
                objects.Add(record.Instance);
            }

            SetSelectedObjects(objects);
        }

        public override void Redo ()
        {
            List<ObjectInstance> objects = new List<ObjectInstance>();
            foreach (ObjectRecord record in _objects) {
                record.Instance.X = record.NewLocation.X;
                record.Instance.Y = record.NewLocation.Y;
                objects.Add(record.Instance);
            }

            SetSelectedObjects(objects);
        }
    }
}
