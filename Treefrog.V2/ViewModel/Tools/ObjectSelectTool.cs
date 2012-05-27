using System.Collections.Generic;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Annotations;
using Treefrog.ViewModel.Commands;
using System.Windows.Input;
using System;

namespace Treefrog.ViewModel.Tools
{
    public class ObjectSelectTool : ObjectPointerTool
    {
        private ObservableCollection<Annotation> _annots;

        public ObjectSelectTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots)
            : base(history, layer, gridSize)
        {
            _annots = annots;
        }

        protected override void DisposeManaged ()
        {
            ClearSelected();
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectObjectSequence(info);
                    break;
                case PointerEventType.Secondary:
                    ClearSelected();
                    break;
            }

            UpdatePointerSequence(info);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectObjectSequence(info);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndSelectObjectSequence(info);
                    break;
            }
        }

        #region Select Object Sequence

        private class SelectedObjectRecord
        {
            public ObjectInstance Instance { get; set; }
            public SelectionAnnot Annot { get; set; }
            public Point InitialLocation { get; set; }
        }

        private enum UpdateAction
        {
            None,
            Move,
            Box,
        }

        private Point _initialLocation;
        private Point _initialSnapLocation;
        private List<SelectedObjectRecord> _selectedObjects = new List<SelectedObjectRecord>();

        private SnappingManager _selectSnapManager;
        private UpdateAction _action;

        private void StartSelectObjectSequence (PointerEventInfo info)
        {
            ObjectInstance hitObject = TopObject(CoarseHitTest((int)info.X, (int)info.Y));
            bool controlKey = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (hitObject == null) {
                if (controlKey)
                    StartDragAdd(info);
                else
                    StartDrag(info);
                return;
            }

            bool alreadySelected = false;
            foreach (SelectedObjectRecord record in _selectedObjects)
                if (record.Instance == hitObject)
                    alreadySelected = true;

            if (alreadySelected) {
                if (controlKey)
                    StartClickRemove(info, hitObject);
                else
                    StartClickMove(info, hitObject);
            }
            else {
                if (controlKey)
                    StartClickAdd(info, hitObject);
                else
                    StartClickNew(info, hitObject);
            }
        }

        private void UpdateSelectObjectSequence (PointerEventInfo info)
        {
            switch (_action) {
                case UpdateAction.Move:
                    UpdateMove(info);
                    break;
                case UpdateAction.Box:
                    UpdateDrag(info);
                    break;
            }
        }

        private void EndSelectObjectSequence (PointerEventInfo info)
        {
            switch (_action) {
                case UpdateAction.Move:
                    EndMove(info);
                    break;
                case UpdateAction.Box:
                    EndDrag(info);
                    break;
            }
        }

        #region Move Actions

        private void StartClickNew (PointerEventInfo info, ObjectInstance obj)
        {
            ClearSelected();
            StartClickAdd(info, obj);
        }

        private void StartClickAdd (PointerEventInfo info, ObjectInstance obj)
        {
            if (obj == null)
                return;

            _initialLocation = new Point((int)info.X, (int)info.Y);
            _selectSnapManager = GetSnappingManager(obj.ObjectClass);

            AddSelected(obj);

            _initialSnapLocation = new Point(obj.X, obj.Y);
            _action = UpdateAction.Move;
        }

        private void StartClickRemove (PointerEventInfo info, ObjectInstance obj)
        {
            if (obj == null)
                return;

            RemoveSelected(obj);

            _action = UpdateAction.None;
        }

        private void StartClickMove (PointerEventInfo info, ObjectInstance obj)
        {
            if (obj == null)
                return;

            _initialLocation = new Point((int)info.X, (int)info.Y);
            _selectSnapManager = GetSnappingManager(obj.ObjectClass);

            _initialSnapLocation = new Point(obj.X, obj.Y);
            _action = UpdateAction.Move;
        }

        private void UpdateMove (PointerEventInfo info)
        {
            int diffx = (int)info.X - _initialLocation.X;
            int diffy = (int)info.Y - _initialLocation.Y;

            if (diffx == 0 && diffy == 0)
                return;

            Point snapLoc = new Point(_initialSnapLocation.X + diffx, _initialSnapLocation.Y + diffy);
            if (_selectSnapManager != null)
                snapLoc = _selectSnapManager.Translate(snapLoc, SnappingTarget);

            foreach (SelectedObjectRecord record in _selectedObjects) {
                int snapDiffX = snapLoc.X - _initialSnapLocation.X;
                int snapDiffY = snapLoc.Y - _initialSnapLocation.Y;
                Point newLoc = new Point(record.InitialLocation.X + snapDiffX, record.InitialLocation.Y + snapDiffY);

                record.Instance.X = newLoc.X;
                record.Instance.Y = newLoc.Y;
                record.Annot.MoveTo(record.Instance.ImageBounds.Location);
            }
        }

        private void EndMove (PointerEventInfo info)
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                record.InitialLocation = new Point(record.Instance.X, record.Instance.Y);
            }
        }

        #endregion

        #region Select Box Action

        RubberBand _band;
        SelectionAnnot _selection;

        private void StartDrag (PointerEventInfo info)
        {
            ClearSelected();
            StartDragAdd(info);
        }

        private void StartDragAdd (PointerEventInfo info)
        {
            _band = new RubberBand(new Point((int)info.X, (int)info.Y));
            _selection = new SelectionAnnot(new Point((int)info.X, (int)info.Y))
            {
                Fill = new SolidColorBrush(new Color(64, 96, 216, 96)),
                Outline = new Pen(new SolidColorBrush(new Color(64, 96, 216, 200))),
            };

            _annots.Add(_selection);

            _action = UpdateAction.Box;
        }

        private void UpdateDrag (PointerEventInfo info)
        {
            _band.End = new Point((int)info.X, (int)info.Y);
            Rectangle selection = _band.Selection;

            _selection.Start = new Point(selection.Left, selection.Top);
            _selection.End = new Point(selection.Right, selection.Bottom);
        }

        private void EndDrag (PointerEventInfo info)
        {
            _annots.Remove(_selection);

            foreach (ObjectInstance inst in ObjectsInArea(_band.Selection)) {
                AddSelected(inst);
            }
        }

        #endregion

        private void AddSelected (ObjectInstance inst)
        {
            foreach (SelectedObjectRecord instRecord in _selectedObjects)
                if (instRecord.Instance == inst)
                    return;

            SelectedObjectRecord record = new SelectedObjectRecord()
            {
                Instance = inst,
                Annot = new SelectionAnnot(inst.ImageBounds.Location)
                {
                    End = new Point(inst.ImageBounds.Right, inst.ImageBounds.Bottom),
                    Outline = new Pen(new SolidColorBrush(new Color(64, 255, 255)), 2),
                },
                InitialLocation = new Point(inst.X, inst.Y),
            };

            _selectedObjects.Add(record);
            _annots.Add(record.Annot);
        }

        private void RemoveSelected (ObjectInstance inst)
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                if (record.Instance == inst) {
                    _annots.Remove(record.Annot);
                    _selectedObjects.Remove(record);
                    break;
                }
            }
        }

        private void ClearSelected ()
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                _annots.Remove(record.Annot);
            }

            _selectedObjects.Clear();
        }

        #endregion

        private List<ObjectInstance> CoarseHitTest (int x, int y)
        {
            List<ObjectInstance> list = new List<ObjectInstance>();
            foreach (ObjectInstance inst in Layer.ObjectsAtPoint(new Point(x, y), ObjectPointTest.Image)) {
                list.Add(inst);
            }

            return list;
        }

        private List<ObjectInstance> ObjectsInArea (Rectangle area)
        {
            List<ObjectInstance> list = new List<ObjectInstance>();
            foreach (ObjectInstance inst in Layer.ObjectsInRegion(area, ObjectRegionTest.Image)) {
                list.Add(inst);
            }

            return list;
        }

        private ObjectInstance TopObject (List<ObjectInstance> objects)
        {
            if (objects.Count == 0)
                return null;

            return objects[objects.Count - 1];
        }
    }
}
