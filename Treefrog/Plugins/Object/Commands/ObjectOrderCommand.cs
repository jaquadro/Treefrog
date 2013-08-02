using System;
using System.Collections.Generic;
using Treefrog.Framework.Model;

namespace Treefrog.Plugins.Object.Commands
{
    public class ObjectOrderCommand : ObjectLayerCommand
    {
        private class ObjectRecord
        {
            public ObjectInstance Instance;
            public int PrevIndex;
            public int NewIndex;
        }

        private ObjectLayer _objectSource;
        
        private List<ObjectRecord> _objects;
        private List<ObjectInstance> _toFront;
        private List<ObjectInstance> _toBack;

        public ObjectOrderCommand (ObjectLayer source)
        {
            _objectSource = source;

            _objects = new List<ObjectRecord>();
            _toFront = new List<ObjectInstance>();
            _toBack = new List<ObjectInstance>();
        }

        public void QueueMoveForward (ObjectInstance inst)
        {
            ClearFromLists(inst);

            int index = _objectSource.ObjectIndex(inst);
            _objects.Add(new ObjectRecord() {
                Instance = inst,
                PrevIndex = index,
                NewIndex = Math.Min(index + 1, _objectSource.ObjectCount - 1),
            });
        }

        public void QueueMoveBackward (ObjectInstance inst)
        {
            ClearFromLists(inst);

            int index = _objectSource.ObjectIndex(inst);
            _objects.Add(new ObjectRecord() {
                Instance = inst,
                PrevIndex = index,
                NewIndex = Math.Max(index - 1, 0),
            });
        }

        public void QueueMoveFront (ObjectInstance inst)
        {
            ClearFromLists(inst);
            _toFront.Add(inst);
        }

        public void QueueMoveBack (ObjectInstance inst)
        {
            ClearFromLists(inst);
            _toBack.Add(inst);
        }

        public override void Execute ()
        {
            // Queue objects sent to back
            Dictionary<int, ObjectInstance> backMap = new Dictionary<int, ObjectInstance>();
            foreach (ObjectInstance inst in _toBack)
                backMap.Add(_objectSource.ObjectIndex(inst), inst);

            List<int> backKeys = new List<int>(backMap.Keys);
            backKeys.Sort();

            foreach (ObjectRecord record in _objects)
                record.NewIndex += backKeys.Count;

            for (int i = 0; i < backKeys.Count; i++) {
                _objects.Add(new ObjectRecord() {
                    Instance = backMap[backKeys[i]],
                    PrevIndex = backKeys[i],
                    NewIndex = i,
                });
            }

            // Sort objects moved relatively
            _objects.Sort((u, v) => {
                return u.PrevIndex.CompareTo(v.PrevIndex);
            });

            // Queue objects sent to front
            Dictionary<int, ObjectInstance> frontMap = new Dictionary<int, ObjectInstance>();
            foreach (ObjectInstance inst in _toFront)
                frontMap.Add(_objectSource.ObjectIndex(inst), inst);

            List<int> frontKeys = new List<int>(frontMap.Keys);
            frontKeys.Sort();

            for (int i = 0; i < frontKeys.Count; i++) {
                _objects.Add(new ObjectRecord() {
                    Instance = frontMap[frontKeys[i]],
                    PrevIndex = frontKeys[i],
                    NewIndex = _objectSource.ObjectCount - 1,
                });
            }

            // Execute command
            foreach (ObjectRecord record in _objects)
                _objectSource.MoveToIndex(record.Instance, record.NewIndex);
        }

        public override void Undo ()
        {
            foreach (ObjectRecord record in _objects)
                _objectSource.MoveToIndex(record.Instance, record.PrevIndex);
        }

        public override void Redo ()
        {
            foreach (ObjectRecord record in _objects)
                _objectSource.MoveToIndex(record.Instance, record.NewIndex);
        }

        private void ClearFromLists (ObjectInstance inst)
        {
            _objects.RemoveAll(v => v.Instance == inst);
            _toFront.Remove(inst);
            _toBack.Remove(inst);
        }
    }
}
