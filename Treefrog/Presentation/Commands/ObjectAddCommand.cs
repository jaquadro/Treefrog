using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Tools;

namespace Treefrog.Presentation.Commands
{
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
}
