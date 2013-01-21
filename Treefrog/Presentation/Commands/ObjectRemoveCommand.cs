using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Tools;
using Treefrog.Presentation.Layers;

namespace Treefrog.Presentation.Commands
{
    public class ObjectRemoveCommand : ObjectLayerCommand
    {
        private ObjectLayer _objectSource;
        private List<ObjectInstance> _objects;

        public ObjectRemoveCommand (ObjectLayer source)
        {
            _objectSource = source;
            _objects = new List<ObjectInstance>();
        }

        public ObjectRemoveCommand (ObjectLayer source, ObjectSelectionManager selectionManager)
            : base(selectionManager)
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
}
