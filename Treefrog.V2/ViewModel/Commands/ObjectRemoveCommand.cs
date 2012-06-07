using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Tools;

namespace Treefrog.ViewModel.Commands
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
}
