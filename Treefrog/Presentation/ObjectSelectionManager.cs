using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation
{
    public class ObjectSelectionManager
    {
        private class SelectedObjectRecord
        {
            public ObjectInstance Instance { get; set; }
            public SelectionAnnot Annot { get; set; }
            public Point InitialLocation { get; set; }
        }

        private static bool[,] StipplePattern2px = new bool[,] {
            { true, true, false, false },
            { true, true, false, false },
            { false, false, true, true },
            { false, false, true, true },
        };

        private List<SelectedObjectRecord> _selectedObjects;

        private static Brush SelectedAnnotFill = null; //new SolidColorBrush(new Color(128, 77, 255, 96));
        private static Pen SelectedAnnotOutline = new Pen(new SolidColorBrush(new Color(96, 0, 255, 255)), 1);
        //private static Pen SelectedAnnotOutline = new Pen(new StippleBrush(StipplePattern2px, new Color(96, 0, 255, 255)));

        public ObjectSelectionManager ()
        {
            _selectedObjects = new List<SelectedObjectRecord>();
        }

        public ObjectLayer Layer { get; set; }

        public CommandHistory History { get; set; }

        public ObservableCollection<Annotation> Annotations { get; set; }

        public int SelectedObjectCount
        {
            get { return _selectedObjects.Count; }
        }

        public IEnumerable<ObjectInstance> SelectedObjects
        {
            get
            {
                foreach (SelectedObjectRecord record in _selectedObjects)
                    yield return record.Instance;
            }
        }

        public void AddObjectToSelection (ObjectInstance obj)
        {
            foreach (SelectedObjectRecord instRecord in _selectedObjects)
                if (instRecord.Instance == obj)
                    return;

            SelectedObjectRecord record = new SelectedObjectRecord() {
                Instance = obj,
                Annot = new SelectionAnnot(obj.ImageBounds.Location) {
                    End = new Point(obj.ImageBounds.Right, obj.ImageBounds.Bottom),
                    Fill = SelectedAnnotFill,
                    Outline = SelectedAnnotOutline,
                },
                InitialLocation = new Point(obj.X, obj.Y),
            };

            /*CircleAnnot circle = new CircleAnnot() {
                Outline = SelectedAnnotOutline,
            };
            circle.SizeToBound(obj.ImageBounds);
            circle.Radius += 5;
            */

            obj.PositionChanged += InstancePositionChanged;
            obj.RotationChanged += InstanceRotationChanged;

            _selectedObjects.Add(record);
            if (Annotations != null)
                Annotations.Add(record.Annot);

            //Annotations.Add(circle);

            //if (_selectedObjects.Count == 1) {
            //    CommandManager.Invalidate(CommandKey.Delete);
            //    CommandManager.Invalidate(CommandKey.SelectNone);
            //}
        }

        public void AddObjectsToSelection (IEnumerable<ObjectInstance> objs)
        {
            foreach (ObjectInstance inst in objs)
                AddObjectToSelection(inst);
        }

        public void RemoveObjectFromSelection (ObjectInstance obj)
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                if (record.Instance == obj) {
                    if (Annotations != null)
                        Annotations.Remove(record.Annot);
                    _selectedObjects.Remove(record);

                    obj.PositionChanged -= InstancePositionChanged;
                    obj.RotationChanged -= InstanceRotationChanged;
                    break;
                }
            }

            //if (_selectedObjects.Count == 0) {
            //    CommandManager.Invalidate(CommandKey.Delete);
            //    CommandManager.Invalidate(CommandKey.SelectNone);
            //}
        }

        public void RemoveObjectsFromSelection (IEnumerable<ObjectInstance> objs)
        {
            foreach (ObjectInstance inst in objs)
                RemoveObjectFromSelection(inst);
        }

        public void ClearSelection ()
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                if (Annotations != null)
                    Annotations.Remove(record.Annot);

                record.Instance.PositionChanged -= InstancePositionChanged;
                record.Instance.RotationChanged -= InstanceRotationChanged;
            }

            _selectedObjects.Clear();

            //CommandManager.Invalidate(CommandKey.Delete);
            //CommandManager.Invalidate(CommandKey.SelectNone);
        }

        public bool IsObjectSelected (ObjectInstance obj)
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                if (record.Instance == obj)
                    return true;
            }

            return false;
        }

        private void InstancePositionChanged (object sender, EventArgs e)
        {
            ObjectInstance inst = sender as ObjectInstance;
            if (inst != null) {
                foreach (var record in _selectedObjects) {
                    if (record.Instance == inst) {
                        record.Annot.MoveTo(inst.ImageBounds.Location);
                        break;
                    }
                }
            }
        }

        private void InstanceRotationChanged (object sender, EventArgs e)
        {
            ObjectInstance inst = sender as ObjectInstance;
            if (inst != null) {
                foreach (var record in _selectedObjects) {
                    if (record.Instance == inst) {
                        record.Annot.Start = inst.ImageBounds.Location;
                        record.Annot.End = new Point(inst.ImageBounds.Right, inst.ImageBounds.Bottom);
                        break;
                    }
                }
            }
        }

        public void RecordLocations ()
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                record.InitialLocation = new Point(record.Instance.X, record.Instance.Y);
            }
        }

        public void MoveObjectsByOffset (Point offset)
        {
            foreach (var record in _selectedObjects) {
                Point newLocation = new Point(record.Instance.X + offset.X, record.Instance.Y + offset.Y);

                record.Instance.X = newLocation.X;
                record.Instance.Y = newLocation.Y;
                //record.Annot.MoveTo(record.Instance.ImageBounds.Location);
            }
        }

        public void MoveObjectsByOffsetRelative (Point offset)
        {
            foreach (var record in _selectedObjects) {
                Point newLocation = new Point(record.InitialLocation.X + offset.X, record.InitialLocation.Y + offset.Y);

                record.Instance.X = newLocation.X;
                record.Instance.Y = newLocation.Y;
                //record.Annot.MoveTo(record.Instance.ImageBounds.Location);
            }
        }

        public void CommitMoveFromRecordedLocations ()
        {
            ObjectMoveCommand command = new ObjectMoveCommand(this);

            foreach (SelectedObjectRecord record in _selectedObjects) {
                Point newLocation = new Point(record.Instance.X, record.Instance.Y);
                command.QueueMove(record.Instance, record.InitialLocation, newLocation);
                record.InitialLocation = newLocation;
            }

            ExecuteCommand(command);
        }

        public void DeleteSelectedObjects ()
        {
            if (Layer != null) {
                ObjectRemoveCommand command = new ObjectRemoveCommand(Layer, this);
                foreach (SelectedObjectRecord inst in _selectedObjects)
                    command.QueueRemove(inst.Instance);

                ExecuteCommand(command);
            }

            ClearSelection();
        }

        public Rectangle SelectionBounds ()
        {
            return SelectionBounds(ObjectRegionTest.Image);
        }

        public Rectangle SelectionBounds (ObjectRegionTest test)
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (SelectedObjectRecord record in _selectedObjects) {
                Rectangle reference = Rectangle.Empty;

                switch (test) {
                    case ObjectRegionTest.Image:
                        reference = record.Instance.ImageBounds;
                        break;
                    case ObjectRegionTest.Mask:
                        reference = record.Instance.MaskBounds;
                        break;
                    case ObjectRegionTest.Origin:
                        reference = new Rectangle(record.Instance.X, record.Instance.Y, 0, 0);
                        break;
                    case ObjectRegionTest.PartialImage:
                        reference = record.Instance.ImageBounds;
                        break;
                    case ObjectRegionTest.PartialMask:
                        reference = record.Instance.MaskBounds;
                        break;
                }

                minX = Math.Min(minX, reference.Left);
                minY = Math.Min(minY, reference.Top);
                maxX = Math.Max(maxX, reference.Right);
                maxY = Math.Max(maxY, reference.Bottom);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private void ExecuteCommand (Command command)
        {
            CommandHistory history = History ?? new CommandHistory();
            history.Execute(command);
        }
    }
}
