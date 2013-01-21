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

        public ObjectSelectionClipboard (IEnumerable<ObjectInstance> objects)
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
        private ILevelGeometry _viewport;

        private ObjectSelectionManager _selectionManager;
        private ILayerContext _layerContext;

        public ObjectSelectTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots, ILevelGeometry viewport, ObjectSelectionManager selectionManager, ILayerContext layerContext)
            : base(history, layer, gridSize)
        {
            _annots = annots;
            _viewport = viewport;
            _selectionManager = selectionManager;
            _layerContext = layerContext;

            InitializeCommandManager();
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectObjectSequence(info, viewport);
                    break;
                case PointerEventType.Secondary:
                    _selectionManager.ClearSelection();
                    UpdatePropertyProvider();
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectObjectSequence(info, viewport);
                    break;
            }
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    EndSelectObjectSequence(info, viewport);
                    break;
            }
        }

        protected override void PointerPositionCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            SelectObjectPosition(info, viewport);
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

            _commandManager.Register(CommandKey.Cut, CommandCanCut, CommandCut);
            _commandManager.Register(CommandKey.Copy, CommandCanCopy, CommandCopy);
            _commandManager.Register(CommandKey.Paste, CommandCanPaste, CommandPaste);
        }

        private bool CommandCanCut ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandCut ()
        {
            if (_selectionManager.SelectedObjectCount > 0) {
                ObjectSelectionClipboard clip = new ObjectSelectionClipboard(_selectionManager.SelectedObjects);
                clip.CopyToClipboard();

                _selectionManager.DeleteSelectedObjects();

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        private bool CommandCanCopy ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandCopy ()
        {
            if (_selectionManager.SelectedObjectCount > 0) {
                ObjectSelectionClipboard clip = new ObjectSelectionClipboard(_selectionManager.SelectedObjects);
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
            _selectionManager.ClearSelection();

            ObjectSelectionClipboard clip = ObjectSelectionClipboard.CopyFromClipboard(Layer.Level.Project);
            if (clip == null)
                return;

            CenterObjectsInViewport(clip.Objects);

            Command command = new ObjectAddCommand(Layer, clip.Objects, _selectionManager);
            History.Execute(command);

            foreach (ObjectInstance inst in clip.Objects) {
                Layer.AddObject(inst);
                _selectionManager.AddObjectToSelection(inst);
            }
        }

        private void CenterObjectsInViewport (List<ObjectInstance> objects)
        {
            Rectangle collectionBounds = ObjectCollectionBounds(objects);

            int centerViewX = (int)(_viewport.VisibleBounds.Left + (_viewport.VisibleBounds.Right - _viewport.VisibleBounds.Left) / 2);
            int centerViewY = (int)(_viewport.VisibleBounds.Top + (_viewport.VisibleBounds.Bottom - _viewport.VisibleBounds.Top) / 2);

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

        private enum UpdateAction
        {
            None,
            Move,
            Box,
        }

        private Point _initialLocation;
        private Point _initialSnapLocation;

        private SnappingManager _selectSnapManager;
        private UpdateAction _action;

        private void StartSelectObjectSequence (PointerEventInfo info, ILevelGeometry viewport)
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
            foreach (var inst in _selectionManager.SelectedObjects)
                if (inst == hitObject)
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

        private void UpdateSelectObjectSequence (PointerEventInfo info, ILevelGeometry viewport)
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

        private void EndSelectObjectSequence (PointerEventInfo info, ILevelGeometry viewport)
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

        private void SelectObjectPosition (PointerEventInfo info, ILevelGeometry viewport)
        {
            foreach (var inst in _selectionManager.SelectedObjects) {
                if (inst.ImageBounds.Contains(new Point((int)info.X, (int)info.Y))) {
                    Cursor.Current = Cursors.SizeAll;
                    return;
                }
            }

            // Pointer not over any selected object
            Cursor.Current = Cursors.Default;
        }

        #region Move Actions

        private void StartClickNew (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
        {
            _selectionManager.ClearSelection();
            StartClickAdd(info, viewport, obj);
        }

        private void StartClickAdd (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
        {
            if (obj == null)
                return;

            _initialLocation = new Point((int)info.X, (int)info.Y);
            _selectSnapManager = GetSnappingManager(obj.ObjectClass);

            _selectionManager.AddObjectToSelection(obj);

            _initialSnapLocation = new Point(obj.X, obj.Y);
            _action = UpdateAction.Move;

            StartAutoScroll(info, viewport);
            UpdatePropertyProvider();
        }

        private void StartClickRemove (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
        {
            if (obj == null)
                return;

            _selectionManager.RemoveObjectFromSelection(obj);

            _action = UpdateAction.None;

            StartAutoScroll(info, viewport);
            UpdatePropertyProvider();
        }

        private void StartClickMove (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
        {
            if (obj == null)
                return;

            _initialLocation = new Point((int)info.X, (int)info.Y);
            _selectSnapManager = GetSnappingManager(obj.ObjectClass);

            _initialSnapLocation = new Point(obj.X, obj.Y);
            _action = UpdateAction.Move;

            _selectionManager.RecordLocations();

            StartAutoScroll(info, viewport);
        }

        private void UpdateMove (PointerEventInfo info, ILevelGeometry viewport)
        {
            UpdateMoveCommon(info, viewport);
            UpdateAutoScroll(info, viewport);
        }

        private void UpdateMoveCommon (PointerEventInfo info, ILevelGeometry viewport)
        {
            int diffx = (int)info.X - _initialLocation.X;
            int diffy = (int)info.Y - _initialLocation.Y;

            if (diffx == 0 && diffy == 0)
                return;

            Point snapLoc = new Point(_initialSnapLocation.X + diffx, _initialSnapLocation.Y + diffy);
            if (_selectSnapManager != null)
                snapLoc = _selectSnapManager.Translate(snapLoc, SnappingTarget);

            _selectionManager.MoveObjectsByOffsetRelative(new Point(snapLoc.X - _initialSnapLocation.X, snapLoc.Y - _initialSnapLocation.Y));
        }

        private void EndMove (PointerEventInfo info, ILevelGeometry viewport)
        {
            _selectionManager.CommitMoveFromRecordedLocations();

            EndAutoScroll(info, viewport);
            UpdatePropertyProvider();
        }

        #endregion

        #region Select Box Action

        RubberBand _band;
        SelectionAnnot _selection;

        private void StartDrag (PointerEventInfo info, ILevelGeometry viewport)
        {
            _selectionManager.ClearSelection();
            StartDragAdd(info, viewport);
        }

        private void StartDragAdd (PointerEventInfo info, ILevelGeometry viewport)
        {
            _band = new RubberBand(new Point((int)info.X, (int)info.Y));
            _selection = new SelectionAnnot(new Point((int)info.X, (int)info.Y)) {
                Fill = SelectionAnnotFill,
                Outline = SelectionAnnotOutline,
            };

            _annots.Add(_selection);

            _action = UpdateAction.Box;

            StartAutoScroll(info, viewport);
        }

        private void UpdateDrag (PointerEventInfo info, ILevelGeometry viewport)
        {
            UpdateDragCommon(info, viewport);
            UpdateAutoScroll(info, viewport);
        }

        private void UpdateDragCommon (PointerEventInfo info, ILevelGeometry viewport)
        {
            _band.End = new Point((int)info.X, (int)info.Y);
            Rectangle selection = _band.Selection;

            _selection.Start = new Point(selection.Left, selection.Top);
            _selection.End = new Point(selection.Right, selection.Bottom);
        }

        private void EndDrag (PointerEventInfo info, ILevelGeometry viewport)
        {
            _annots.Remove(_selection);

            _selectionManager.AddObjectsToSelection(ObjectsInArea(_band.Selection));

            EndAutoScroll(info, viewport);
            UpdatePropertyProvider();
        }

        #endregion

        #endregion

        private void UpdatePropertyProvider ()
        {
            if (_selectionManager.SelectedObjectCount == 1) {
                foreach (ObjectInstance inst in _selectionManager.SelectedObjects) {
                    _layerContext.SetPropertyProvider(inst);
                }
            }
            else {
                _layerContext.SetPropertyProvider(null);
            }
        }

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
