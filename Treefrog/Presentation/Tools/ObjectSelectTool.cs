using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Clipboard = System.Windows.Forms.Clipboard;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Framework.Imaging;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Annotations;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Presentation.Layers;

namespace Treefrog.Presentation.Tools
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

    public class ObjectSelectTool : ObjectPointerTool, ICommandSubscriber
    {
        private static bool[,] StipplePattern2px = new bool[,] {
            { true, true, false, false },
            { true, true, false, false },
            { false, false, true, true },
            { false, false, true, true },
        };

        private static Brush SelectionAnnotFill = new SolidColorBrush(new Color(77, 180, 255, 128));
        private static Pen SelectionAnnotOutline = null; //new Pen(new SolidColorBrush(new Color(64, 96, 216, 200)));

        private static Brush SelectedAnnotFill = new SolidColorBrush(new Color(128, 77, 255, 96));
        private static Pen SelectedAnnotOutline = new Pen(new SolidColorBrush(new Color(96, 0, 255, 255)), 1);
        //private static Pen SelectedAnnotOutline = new Pen(new StippleBrush(StipplePattern2px, new Color(96, 0, 255, 255)));

        private ObservableCollection<Annotation> _annots;
        //private ViewportVM _viewport;
        private IViewport _viewport;

        public ObjectSelectTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots, IViewport viewport)
            : base(history, layer, gridSize)
        {
            _annots = annots;
            _viewport = viewport;

            InitializeCommandManager();
        }

        protected override void DisposeManaged ()
        {
            ClearSelected();
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectObjectSequence(info, viewport);
                    break;
                case PointerEventType.Secondary:
                    ClearSelected();
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectObjectSequence(info, viewport);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, IViewport viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndSelectObjectSequence(info, viewport);
                    break;
            }
        }

        #region Command Handling

        private CommandManager _commandManager;

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();

            _commandManager.Register(CommandKey.Delete, CommandCanDelete, CommandDelete);
            _commandManager.Register(CommandKey.SelectAll, CommandCanSelectAll, CommandSelectAll);
            _commandManager.Register(CommandKey.SelectNone, CommandCanSelectNone, CommandSelectNone);
            _commandManager.Register(CommandKey.Cut, CommandCanCut, CommandCut);
            _commandManager.Register(CommandKey.Copy, CommandCanCopy, CommandCopy);
            _commandManager.Register(CommandKey.Paste, CommandCanPaste, CommandPaste);
        }

        private bool CommandCanDelete ()
        {
            return _selectedObjects != null && _selectedObjects.Count > 0;
        }

        private void CommandDelete ()
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

        private bool CommandCanSelectAll ()
        {
            return true;
        }

        private void CommandSelectAll ()
        {
            if (Layer != null) {
                foreach (ObjectInstance inst in Layer.Objects) {
                    AddSelected(inst);
                }
            }
        }

        private bool CommandCanSelectNone ()
        {
            return _selectedObjects != null && _selectedObjects.Count > 0;
        }

        private void CommandSelectNone ()
        {
            if (_selectedObjects != null) {
                ClearSelected();
            }
        }

        private bool CommandCanCut ()
        {
            return _selectedObjects != null && _selectedObjects.Count > 0;
        }

        private void CommandCut ()
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

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        private bool CommandCanCopy ()
        {
            return _selectedObjects != null && _selectedObjects.Count > 0;
        }

        private void CommandCopy ()
        {
            if (_selectedObjects != null) {
                List<ObjectInstance> objects = new List<ObjectInstance>();
                foreach (SelectedObjectRecord record in _selectedObjects)
                    objects.Add(record.Instance);

                ObjectSelectionClipboard clip = new ObjectSelectionClipboard(objects);
                clip.CopyToClipboard();

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        private bool CommandCanPaste ()
        {
            return ObjectSelectionClipboard.ContainsData;
        }

        private void CommandPaste ()
        {
            ClearSelected();

            ObjectSelectionClipboard clip = ObjectSelectionClipboard.CopyFromClipboard(Layer.Level.Project);
            if (clip == null)
                return;

            CenterObjectsInViewport(clip.Objects);

            Command command = new ObjectAddCommand(Layer, clip.Objects, this);
            History.Execute(command);

            foreach (ObjectInstance inst in clip.Objects) {
                Layer.AddObject(inst);
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

        private void StartSelectObjectSequence (PointerEventInfo info, IViewport viewport)
        {
            ObjectInstance hitObject = TopObject(CoarseHitTest((int)info.X, (int)info.Y));
            bool controlKey = Control.ModifierKeys.HasFlag(Keys.Control); // Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (hitObject == null) {
                if (controlKey)
                    StartDragAdd(info, viewport);
                else
                    StartDrag(info, viewport);
                return;
            }

            bool alreadySelected = false;
            foreach (SelectedObjectRecord record in _selectedObjects)
                if (record.Instance == hitObject)
                    alreadySelected = true;

            if (alreadySelected) {
                if (controlKey)
                    StartClickRemove(info, viewport, hitObject);
                else
                    StartClickMove(info, viewport, hitObject);
            }
            else {
                if (controlKey)
                    StartClickAdd(info, viewport, hitObject);
                else
                    StartClickNew(info, viewport, hitObject);
            }
        }

        private void UpdateSelectObjectSequence (PointerEventInfo info, IViewport viewport)
        {
            switch (_action) {
                case UpdateAction.Move:
                    UpdateMove(info, viewport);
                    break;
                case UpdateAction.Box:
                    UpdateDrag(info, viewport);
                    break;
            }
        }

        private void EndSelectObjectSequence (PointerEventInfo info, IViewport viewport)
        {
            switch (_action) {
                case UpdateAction.Move:
                    EndMove(info, viewport);
                    break;
                case UpdateAction.Box:
                    EndDrag(info, viewport);
                    break;
            }
        }

        #region Move Actions

        private void StartClickNew (PointerEventInfo info, IViewport viewport, ObjectInstance obj)
        {
            ClearSelected();
            StartClickAdd(info, viewport, obj);
        }

        private void StartClickAdd (PointerEventInfo info, IViewport viewport, ObjectInstance obj)
        {
            if (obj == null)
                return;

            _initialLocation = new Point((int)info.X, (int)info.Y);
            _selectSnapManager = GetSnappingManager(obj.ObjectClass);

            AddSelected(obj);

            _initialSnapLocation = new Point(obj.X, obj.Y);
            _action = UpdateAction.Move;

            //StartAutoScroll(info, viewport);
        }

        private void StartClickRemove (PointerEventInfo info, IViewport viewport, ObjectInstance obj)
        {
            if (obj == null)
                return;

            RemoveSelected(obj);

            _action = UpdateAction.None;

            //StartAutoScroll(info, viewport);
        }

        private void StartClickMove (PointerEventInfo info, IViewport viewport, ObjectInstance obj)
        {
            if (obj == null)
                return;

            _initialLocation = new Point((int)info.X, (int)info.Y);
            _selectSnapManager = GetSnappingManager(obj.ObjectClass);

            _initialSnapLocation = new Point(obj.X, obj.Y);
            _action = UpdateAction.Move;

            //StartAutoScroll(info, viewport);
        }

        private void UpdateMove (PointerEventInfo info, IViewport viewport)
        {
            UpdateMoveCommon(info, viewport);
            //UpdateAutoScroll(info, viewport);
        }

        private void UpdateMoveCommon (PointerEventInfo info, IViewport viewport)
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

        private void EndMove (PointerEventInfo info, IViewport viewport)
        {
            ObjectMoveCommand command = new ObjectMoveCommand(this);

            foreach (SelectedObjectRecord record in _selectedObjects) {
                Point newLocation = new Point(record.Instance.X, record.Instance.Y);
                command.QueueMove(record.Instance, record.InitialLocation, newLocation);
                record.InitialLocation = newLocation;
            }

            History.Execute(command);

            //EndAutoScroll(info, viewport);
        }

        #endregion

        #region Select Box Action

        RubberBand2 _band;
        SelectionAnnot _selection;

        private void StartDrag (PointerEventInfo info, IViewport viewport)
        {
            ClearSelected();
            StartDragAdd(info, viewport);
        }

        private void StartDragAdd (PointerEventInfo info, IViewport viewport)
        {
            _band = new RubberBand2(new Point((int)info.X, (int)info.Y));
            _selection = new SelectionAnnot(new Point((int)info.X, (int)info.Y)) {
                Fill = SelectionAnnotFill,
                Outline = SelectionAnnotOutline,
            };

            _annots.Add(_selection);

            _action = UpdateAction.Box;

            //StartAutoScroll(info, viewport);
        }

        private void UpdateDrag (PointerEventInfo info, IViewport viewport)
        {
            UpdateDragCommon(info, viewport);
            //UpdateAutoScroll(info, viewport);
        }

        private void UpdateDragCommon (PointerEventInfo info, IViewport viewport)
        {
            _band.End = new Point((int)info.X, (int)info.Y);
            Rectangle selection = _band.Selection;

            _selection.Start = new Point(selection.Left, selection.Top);
            _selection.End = new Point(selection.Right, selection.Bottom);
        }

        private void EndDrag (PointerEventInfo info, IViewport viewport)
        {
            _annots.Remove(_selection);

            foreach (ObjectInstance inst in ObjectsInArea(_band.Selection)) {
                AddSelected(inst);
            }

            //EndAutoScroll(info, viewport);
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

            SelectedObjectRecord record = new SelectedObjectRecord() {
                Instance = inst,
                Annot = new SelectionAnnot(inst.ImageBounds.Location) {
                    End = new Point(inst.ImageBounds.Right, inst.ImageBounds.Bottom),
                    Fill = SelectedAnnotFill,
                    Outline = SelectedAnnotOutline,
                },
                InitialLocation = new Point(inst.X, inst.Y),
            };

            _selectedObjects.Add(record);
            _annots.Add(record.Annot);

            if (_selectedObjects.Count == 1) {
                CommandManager.Invalidate(CommandKey.Delete);
                CommandManager.Invalidate(CommandKey.SelectNone);
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
                CommandManager.Invalidate(CommandKey.Delete);
                CommandManager.Invalidate(CommandKey.SelectNone);
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

            CommandManager.Invalidate(CommandKey.Delete);
            CommandManager.Invalidate(CommandKey.SelectNone);
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
