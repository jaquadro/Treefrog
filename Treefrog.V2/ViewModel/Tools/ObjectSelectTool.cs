using System.Collections.Generic;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Annotations;
using Treefrog.ViewModel.Commands;
using System.Windows.Input;
using System;

using Clipboard = System.Windows.Clipboard;

namespace Treefrog.ViewModel.Tools
{
    [Serializable]
    public class ObjectSelectionClipboard
    {
        public List<ObjectInstance> Objects;

        public ObjectSelectionClipboard (List<ObjectInstance> objects)
        {
            Objects = new List<ObjectInstance>();
            foreach (ObjectInstance inst in objects)
                Objects.Add(inst);
        }

        public static bool ContainsData
        {
            get { return Clipboard.ContainsData(typeof(ObjectSelectionClipboard).FullName); }
        }

        public void CopyToClipboard ()
        {
            foreach (ObjectInstance inst in Objects)
                inst.PreSerialize();

            Clipboard.SetData(typeof(ObjectSelectionClipboard).FullName, this);
        }

        public static ObjectSelectionClipboard CopyFromClipboard (Project project)
        {
            ObjectSelectionClipboard clip = Clipboard.GetData(typeof(ObjectSelectionClipboard).FullName) as ObjectSelectionClipboard;
            if (clip == null)
                return null;

            foreach (ObjectInstance inst in clip.Objects)
                inst.PostDeserialize(project);

            return clip;
        }
    }

    public class ObjectSelectTool : ObjectPointerTool
    {
        private ObservableCollection<Annotation> _annots;
        private ViewportVM _viewport;

        public ObjectSelectTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots, ViewportVM viewport)
            : base(history, layer, gridSize)
        {
            _annots = annots;
            _viewport = viewport;
        }

        protected override void DisposeManaged ()
        {
            ClearSelected();
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectObjectSequence(info);
                    break;
                case PointerEventType.Secondary:
                    ClearSelected();
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectObjectSequence(info);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndSelectObjectSequence(info);
                    break;
            }
        }

        #region Edit Interface

        public bool CanDelete
        {
            get { return _selectedObjects != null && _selectedObjects.Count > 0; }
        }

        public void Delete ()
        {
            if (_selectedObjects != null) {
                if (Layer != null) {
                    ObjectRemoveCommand command = new ObjectRemoveCommand(Layer, this);
                    foreach (SelectedObjectRecord inst in _selectedObjects)
                        command.QueueRemove(inst.Instance);

                    History.Execute(command);
                }

                ClearSelected();
            }
        }

        public event EventHandler CanDeleteChanged;

        protected virtual void OnCanDeleteChanged (EventArgs e)
        {
            if (CanDeleteChanged != null)
                CanDeleteChanged(this, e);
        }

        public bool CanSelectAll
        {
            get { return true; }
        }

        public void SelectAll ()
        {
            if (Layer != null) {
                foreach (ObjectInstance inst in Layer.Objects) {
                    AddSelected(inst);
                }
            }
        }

        public event EventHandler CanSelectAllChanged;

        protected virtual void OnCanSelectAllChanged (EventArgs e)
        {
            if (CanSelectAllChanged != null)
                CanSelectAllChanged(this, e);
        }

        public bool CanSelectNone
        {
            get { return _selectedObjects != null && _selectedObjects.Count > 0; }
        }

        public void SelectNone ()
        {
            if (_selectedObjects != null) {
                ClearSelected();
            }
        }

        public event EventHandler CanSelectNoneChanged;

        protected virtual void OnCanSelectNoneChanged (EventArgs e)
        {
            if (CanSelectNoneChanged != null)
                CanSelectNoneChanged(this, e);
        }

        public bool CanCut
        {
            get { return _selectedObjects != null && _selectedObjects.Count > 0; }
        }

        public void Cut ()
        {
            if (_selectedObjects != null) {
                List<ObjectInstance> objects = new List<ObjectInstance>();
                foreach (SelectedObjectRecord record in _selectedObjects)
                    objects.Add(record.Instance);

                ObjectSelectionClipboard clip = new ObjectSelectionClipboard(objects);
                clip.CopyToClipboard();

                ObjectRemoveCommand command = new ObjectRemoveCommand(Layer, this);
                foreach (SelectedObjectRecord inst in _selectedObjects)
                    command.QueueRemove(inst.Instance);
                History.Execute(command);

                ClearSelected();

                OnCanPasteChanged(EventArgs.Empty);
            }
        }

        public event EventHandler CanCutChanged;

        protected virtual void OnCanCutChanged (EventArgs e)
        {
            if (CanCutChanged != null)
                CanCutChanged(this, e);
        }

        public bool CanCopy
        {
            get { return _selectedObjects != null && _selectedObjects.Count > 0; }
        }

        public void Copy ()
        {
            if (_selectedObjects != null) {
                List<ObjectInstance> objects = new List<ObjectInstance>();
                foreach (SelectedObjectRecord record in _selectedObjects)
                    objects.Add(record.Instance);

                ObjectSelectionClipboard clip = new ObjectSelectionClipboard(objects);
                clip.CopyToClipboard();

                OnCanPasteChanged(EventArgs.Empty);
            }
        }

        public event EventHandler CanCopyChanged;

        protected virtual void OnCanCopyChanged (EventArgs e)
        {
            if (CanCopyChanged != null)
                CanCopyChanged(this, e);
        }

        public bool CanPaste
        {
            get { return ObjectSelectionClipboard.ContainsData; }
        }

        public void Paste ()
        {
            ClearSelected();

            ObjectSelectionClipboard clip = ObjectSelectionClipboard.CopyFromClipboard(Layer.Level.Project);
            if (clip == null)
                return;

            CenterObjectsInViewport(clip.Objects);

            Command command = new ObjectAddCommand(Layer, clip.Objects, this);
            History.Execute(command);

            foreach (ObjectInstance inst in clip.Objects) {
                //Layer.AddObject(inst);
                AddSelected(inst);
            }
        }

        private void CenterObjectsInViewport (List<ObjectInstance> objects)
        {
            Rectangle collectionBounds = ObjectCollectionBounds(objects);

            int centerViewX = (int)(_viewport.VisibleRegion.Left + (_viewport.VisibleRegion.Right - _viewport.VisibleRegion.Left) / 2);
            int centerViewY = (int)(_viewport.VisibleRegion.Top + (_viewport.VisibleRegion.Bottom - _viewport.VisibleRegion.Top) / 2);

            int diffX = centerViewX - collectionBounds.Center.X;
            int diffY = centerViewY - collectionBounds.Center.Y;

            foreach (ObjectInstance inst in objects) {
                inst.X += diffX;
                inst.Y += diffY;
            }
        }

        private Rectangle ObjectCollectionBounds (List<ObjectInstance> objects)
        {
            int minX = Int32.MaxValue;
            int minY = Int32.MaxValue;
            int maxX = Int32.MinValue;
            int maxY = Int32.MinValue;

            foreach (ObjectInstance inst in objects) {
                minX = Math.Min(minX, inst.ImageBounds.Left);
                minY = Math.Min(minY, inst.ImageBounds.Top);
                maxX = Math.Max(maxX, inst.ImageBounds.Right);
                maxY = Math.Max(maxY, inst.ImageBounds.Bottom);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public event EventHandler CanPasteChanged;

        protected virtual void OnCanPasteChanged (EventArgs e)
        {
            if (CanPasteChanged != null)
                CanPasteChanged(this, e);
        }

        #endregion

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
            ObjectMoveCommand command = new ObjectMoveCommand(this);

            foreach (SelectedObjectRecord record in _selectedObjects) {
                Point newLocation = new Point(record.Instance.X, record.Instance.Y);
                command.QueueMove(record.Instance, record.InitialLocation, newLocation);
                record.InitialLocation = newLocation;
            }

            History.Execute(command);
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

        public void SelectObjects (List<ObjectInstance> objects)
        {
            ClearSelected();
            AddObjectsToSelection(objects);
        }

        public void AddObjectsToSelection (List<ObjectInstance> objects)
        {
            if (objects == null)
                return;

            foreach (ObjectInstance inst in objects)
                AddSelected(inst);
        }

        public void RemoveObjectsFromSelection (List<ObjectInstance> objects)
        {
            if (objects == null)
                return;

            foreach (ObjectInstance inst in objects)
                RemoveSelected(inst);
        }

        private void AddSelected (ObjectInstance inst)
        {
            if (_selectedObjects == null)
                return;

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

            if (_selectedObjects.Count == 1) {
                OnCanDeleteChanged(EventArgs.Empty);
                OnCanSelectNoneChanged(EventArgs.Empty);
            }
        }

        private void RemoveSelected (ObjectInstance inst)
        {
            if (_selectedObjects == null || _selectedObjects.Count == 0)
                return;

            foreach (SelectedObjectRecord record in _selectedObjects) {
                if (record.Instance == inst) {
                    _annots.Remove(record.Annot);
                    _selectedObjects.Remove(record);
                    break;
                }
            }

            if (_selectedObjects.Count == 0) {
                OnCanDeleteChanged(EventArgs.Empty);
                OnCanSelectNoneChanged(EventArgs.Empty);
            }
        }

        private void ClearSelected ()
        {
            if (_selectedObjects == null || _selectedObjects.Count == 0)
                return;

            foreach (SelectedObjectRecord record in _selectedObjects) {
                _annots.Remove(record.Annot);
            }

            _selectedObjects.Clear();

            OnCanDeleteChanged(EventArgs.Empty);
            OnCanSelectNoneChanged(EventArgs.Empty);
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
