using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Tools;

namespace Treefrog.ViewModel.Commands
{
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
