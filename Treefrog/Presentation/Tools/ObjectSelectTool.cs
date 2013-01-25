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

        private ToolState _state;

        public ObjectSelectTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots, ILevelGeometry viewport, ObjectSelectionManager selectionManager, ILayerContext layerContext)
            : base(history, layer, gridSize)
        {
            _annots = annots;
            _viewport = viewport;
            _selectionManager = selectionManager;
            _layerContext = layerContext;

            InitializeCommandManager();

            _state = new SelectionStandbyToolState(this);
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            _state = _state.StartPointerSequence(info, viewport);

            /*switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectObjectSequence(info, viewport);
                    break;
                case PointerEventType.Secondary:
                    _selectionManager.ClearSelection();
                    if (_rotationAnnot != null) {
                        _annots.Remove(_rotationAnnot);
                        _rotationAnnot = null;
                    }
                    UpdatePropertyProvider();
                    break;
            }*/

            UpdatePointerSequence(info, viewport);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            _state = _state.UpdatePointerSequence(info, viewport);

            /*switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectObjectSequence(info, viewport);
                    break;
            }*/
        }

        protected override void EndPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            _state = _state.EndPointerSequence(info, viewport);

            /*switch (info.Type) {
                case PointerEventType.Primary:
                    EndSelectObjectSequence(info, viewport);
                    break;
            }*/
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
            Timeout,
        }

        private DateTime _timeout;
        private const int _timeoutLimit = 500;

        private Point _initialLocation;
        private Point _initialSnapLocation;

        private SnappingManager _selectSnapManager;
        private UpdateAction _action;

        private CircleAnnot _rotationAnnot;
        private ObjectInstance _rotationObj;

        private void StartSelectObjectSequence (PointerEventInfo info, ILevelGeometry viewport)
        {
            ObjectInstance hitObject = TopObject(CoarseHitTest((int)info.X, (int)info.Y));
            bool controlKey = Control.ModifierKeys.HasFlag(Keys.Control);

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
                    StartClickTimeout(info, viewport, hitObject);
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
                case UpdateAction.Timeout:
                    UpdateTimeout(info, viewport);
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
                case UpdateAction.Timeout:
                    EndTimeout(info, viewport);
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
            if (_rotationAnnot != null) {
                _annots.Remove(_rotationAnnot);
                _rotationAnnot = null;
            }

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

        private void StartClickTimeout (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
        {
            if (obj == null)
                return;

            if (_rotationAnnot != null) {
                _annots.Remove(_rotationAnnot);
                _rotationAnnot = null;
            }

            _initialLocation = new Point((int)info.X, (int)info.Y);

            _timeout = DateTime.Now;
            _action = UpdateAction.Timeout;
            _rotationObj = obj;
        }

        private void UpdateTimeout (PointerEventInfo info, ILevelGeometry viewport)
        {
            if (_initialLocation.X != info.X || _initialLocation.Y != info.Y)
                StartClickMove(info, viewport, _rotationObj);
        }

        private void EndTimeout (PointerEventInfo info, ILevelGeometry viewport)
        {
            if ((DateTime.Now - _timeout).TotalMilliseconds < _timeoutLimit) {
                if (true) {
                    _rotationAnnot = new CircleAnnot(_rotationObj.ImageBounds.Center, _rotationObj.ImageBounds.Width + 10);
                    _rotationAnnot.Outline = SelectedAnnotOutline;
                    _annots.Add(_rotationAnnot);
                }
                _action = UpdateAction.None;
            }
            else {
                StartClickMove(info, viewport, _rotationObj);
            }
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

        private bool PointInRing (Point point, Point center, float innerRadius, float outerRadius)
        {
            float diffX = point.X - center.X;
            float diffY = point.Y - center.Y;
            float distance = (float)Math.Sqrt(diffX * diffX + diffY * diffY);

            return (distance >= innerRadius && distance < outerRadius);
        }

        private float MaxBoundingDiagonal (ObjectInstance inst)
        {
            int maxDim = Math.Max(inst.ObjectClass.ImageBounds.Width, inst.ObjectClass.ImageBounds.Height);
            float diag1 = (float)Math.Sqrt(maxDim * maxDim * 2);
            float diag2 = (float)Math.Sqrt(diag1 * diag1 * 2);

            return diag2;
        }


        private abstract class ToolState
        {
            protected ObjectSelectTool Tool { get; private set; }

            protected ToolState (ObjectSelectTool tool)
            {
                Tool = tool;
            }

            public virtual ToolState StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                return this;
            }

            public virtual ToolState UpdatePointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                return this;
            }

            public virtual ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                return this;
            }
        }

        private class SelectionStandbyToolState : ToolState
        {
            public SelectionStandbyToolState (ObjectSelectTool tool) : base(tool) { }

            public override ToolState StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                switch (info.Type) {
                    case PointerEventType.Primary:
                        return StartPrimaryPointerSequence(info, viewport);
                    case PointerEventType.Secondary:
                        return new ReleaseToolState(Tool).StartPointerSequence(info, viewport);
                    default:
                        return base.StartPointerSequence(info, viewport);
                }
            }

            private ToolState StartPrimaryPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                ObjectInstance hitObject = Tool.TopObject(Tool.CoarseHitTest((int)info.X, (int)info.Y));
                bool controlKey = Control.ModifierKeys.HasFlag(Keys.Control);

                if (hitObject == null) {
                    if (controlKey)
                        return StartDragAdd(info, viewport);
                    else
                        return StartDrag(info, viewport);
                }

                if (Tool._selectionManager.IsObjectSelected(hitObject)) {
                    if (controlKey)
                        return StartClickRemove(info, viewport, hitObject);
                    else
                        return StartTimeoutSequence(info, viewport, hitObject);
                }
                else {
                    if (controlKey)
                        return StartClickAdd(info, viewport, hitObject);
                    else
                        return StartClickNew(info, viewport, hitObject);
                }
            }

            private ToolState StartTimeoutSequence (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
            {
                return new SelectionTimeoutToolState(Tool) {
                    HitObject = obj
                }.StartPointerSequence(info, viewport);
            }

            private ToolState StartClickNew (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
            {
                Tool._selectionManager.ClearSelection();
                return StartClickAdd(info, viewport, obj);
            }

            private ToolState StartClickAdd (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
            {
                Tool._selectionManager.AddObjectToSelection(obj);

                Tool.StartAutoScroll(info, viewport);
                Tool.UpdatePropertyProvider();

                return new SelectionMovingToolState(Tool) {
                    InitialLocation = new Point((int)info.X, (int)info.Y),
                    InitialSnapLocation = new Point(obj.X, obj.Y),
                    SnapManager = Tool.GetSnappingManager(obj.ObjectClass),

                }.StartPointerSequence(info, viewport);
            }

            private ToolState StartClickRemove (PointerEventInfo info, ILevelGeometry viewport, ObjectInstance obj)
            {
                Tool._selectionManager.RemoveObjectFromSelection(obj);

                Tool.StartAutoScroll(info, viewport);
                Tool.UpdatePropertyProvider();

                return new SelectionStandbyToolState(Tool).StartPointerSequence(info, viewport);
            }

            private ToolState StartDrag (PointerEventInfo info, ILevelGeometry viewport)
            {
                Tool._selectionManager.ClearSelection();
                return StartDragAdd(info, viewport);
            }

            private ToolState StartDragAdd (PointerEventInfo info, ILevelGeometry viewport)
            {
                return new SelectionAreaToolState(Tool).StartPointerSequence(info, viewport);
            }
        }

        private class RotationStandbyToolState : ToolState
        {
            private static Pen Outline = new Pen(new SolidColorBrush(new Color(128, 128, 128, 220)));

            public RotationStandbyToolState (ObjectSelectTool tool) : base(tool) { }

            public ObjectInstance HitObject { get; set; }

            private CircleAnnot Annot { get; set; }

            public override ToolState StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                switch (info.Type) {
                    case PointerEventType.Primary:
                        return StartPrimaryPointerSequence(info, viewport);
                    case PointerEventType.Secondary:
                        return StartSecondaryPointerSequence(info, viewport);
                    default:
                        return base.StartPointerSequence(info, viewport);
                }
            }

            public override ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                if (Annot == null) {
                    float radius = Tool.MaxBoundingDiagonal(HitObject) / 2 + 5;
                    Annot = new CircleAnnot(HitObject.ImageBounds.Center, radius);
                    Annot.Outline = Outline;
                    Tool._annots.Add(Annot);
                }

                return this;
            }

            private ToolState StartPrimaryPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                if (PointInRing(new Point((int)info.X, (int)info.Y), Annot.Center, Annot.Radius - 5, Annot.Radius + 5)) {
                    ClearAnnot();
                    return new SelectionRotatingToolState(Tool, HitObject).StartPointerSequence(info, viewport);
                }

                ObjectInstance hitObject = Tool.TopObject(Tool.CoarseHitTest((int)info.X, (int)info.Y));

                if (Tool._selectionManager.IsObjectSelected(hitObject)) {
                    ClearAnnot();
                    return new SelectionStandbyToolState(Tool).StartPointerSequence(info, viewport);
                }
                else {
                    ClearAnnot();
                    return new SelectionStandbyToolState(Tool).StartPointerSequence(info, viewport);
                }
            }

            private ToolState StartSecondaryPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                ClearAnnot();
                return new ReleaseToolState(Tool).StartPointerSequence(info, viewport);
            }

            private void ClearAnnot ()
            {
                if (Annot != null)
                    Tool._annots.Remove(Annot);
            }

            private bool PointInRing (Point point, Point center, float innerRadius, float outerRadius)
            {
                float diffX = point.X - center.X;
                float diffY = point.Y - center.Y;
                float distance = (float)Math.Sqrt(diffX * diffX + diffY * diffY);

                return (distance >= innerRadius && distance < outerRadius);
            }
        }

        private class ReleaseToolState : ToolState
        {
            public ReleaseToolState (ObjectSelectTool tool) : base(tool) { }

            public override ToolState StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                Tool._selectionManager.ClearSelection();
                Tool.UpdatePropertyProvider();

                return this;
            }

            public override ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                return new SelectionStandbyToolState(Tool).EndPointerSequence(info, viewport);
            }
        }

        private class SelectionAreaToolState : ToolState
        {
            public SelectionAreaToolState (ObjectSelectTool tool) : base(tool) { }

            private RubberBand Band { get; set; }
            private SelectionAnnot Annot { get; set; }

            public override ToolState StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                Band = new RubberBand(new Point((int)info.X, (int)info.Y));
                Annot = new SelectionAnnot(new Point((int)info.X, (int)info.Y)) {
                    Fill = SelectionAnnotFill,
                    Outline = SelectionAnnotOutline,
                };

                Tool._annots.Add(Annot);
                Tool.StartAutoScroll(info, viewport);

                return this;
            }

            public override ToolState UpdatePointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                Band.End = new Point((int)info.X, (int)info.Y);
                Rectangle selection = Band.Selection;

                Annot.Start = new Point(selection.Left, selection.Top);
                Annot.End = new Point(selection.Right, selection.Bottom);

                Tool.UpdateAutoScroll(info, viewport);

                return this;
            }

            public override ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                Tool._annots.Remove(Annot);

                Tool._selectionManager.AddObjectsToSelection(Tool.ObjectsInArea(Band.Selection));

                Tool.EndAutoScroll(info, viewport);
                Tool.UpdatePropertyProvider();

                return new ReleaseToolState(Tool).EndPointerSequence(info, viewport);
            }
        }

        // TODO: Register timeout callback
        private class SelectionTimeoutToolState : ToolState
        {
            public SelectionTimeoutToolState (ObjectSelectTool tool) : base(tool) { }

            private const int _timeoutThreshold = 500;

            public ObjectInstance HitObject { get; set; }

            private Point InitialLocation { get; set; }
            private DateTime Timeout { get; set; }

            public override ToolState StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                InitialLocation = new Point((int)info.X, (int)info.Y);
                Timeout = DateTime.Now;

                return this;
            }

            public override ToolState UpdatePointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                if (info.X == InitialLocation.X && info.Y == InitialLocation.Y)
                    return this;

                return new SelectionMovingToolState(Tool) {
                    InitialLocation = InitialLocation,
                    InitialSnapLocation = new Point(HitObject.X, HitObject.Y),
                    SnapManager = Tool.GetSnappingManager(HitObject.ObjectClass),
                }.UpdatePointerSequence(info, viewport);
            }

            public override ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                if ((DateTime.Now - Timeout).TotalMilliseconds < _timeoutThreshold)
                    return new RotationStandbyToolState(Tool) {
                        HitObject = HitObject
                    }.EndPointerSequence(info, viewport);
                else
                    return new SelectionStandbyToolState(Tool).EndPointerSequence(info, viewport);
            }
        }

        private class SelectionMovingToolState : ToolState
        {
            public SelectionMovingToolState(ObjectSelectTool tool) : base(tool) { }

            public Point InitialLocation { get; set; }
            public Point InitialSnapLocation { get; set; }
            public SnappingManager SnapManager { get; set; }

            public override ToolState UpdatePointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                int diffx = (int)info.X - InitialLocation.X;
                int diffy = (int)info.Y - InitialLocation.Y;

                if (diffx != 0 || diffy != 0) {
                    Point snapLoc = new Point(InitialSnapLocation.X + diffx, InitialSnapLocation.Y + diffy);
                    if (SnapManager != null)
                        snapLoc = SnapManager.Translate(snapLoc, Tool.SnappingTarget);

                    Tool._selectionManager.MoveObjectsByOffsetRelative(new Point(snapLoc.X - InitialSnapLocation.X, snapLoc.Y - InitialSnapLocation.Y));
                }

                return this;
            }

            public override ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                Tool._selectionManager.CommitMoveFromRecordedLocations();

                Tool.EndAutoScroll(info, viewport);
                Tool.UpdatePropertyProvider();

                return new SelectionStandbyToolState(Tool).EndPointerSequence(info, viewport);
            }
        }

        private class SelectionRotatingToolState : ToolState
        {
            private static Pen Outline = new Pen(new SolidColorBrush(Colors.Blue));

            public SelectionRotatingToolState(ObjectSelectTool tool, ObjectInstance hitObject) 
                : base(tool) 
            {
                HitObject = hitObject;
                InitialAngle = hitObject.Rotation;

                float radius = Tool.MaxBoundingDiagonal(HitObject) / 2 + 5;
                Annot = new CircleAnnot(HitObject.ImageBounds.Center, radius);
                Annot.Outline = Outline;
                Tool._annots.Add(Annot);
            }

            private ObjectInstance HitObject { get; set; }
            private Point InitialLocation { get; set; }
            private float InitialAngle { get; set; }
            private CircleAnnot Annot { get; set; }

            public override ToolState StartPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                InitialLocation = new Point((int)info.X, (int)info.Y);

                return this;
            }

            public override ToolState UpdatePointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                Vector vecCenter = new Vector(HitObject.X, HitObject.Y);
                Vector vec1 = new Vector(InitialLocation.X, InitialLocation.Y) - vecCenter;
                Vector vec2 = new Vector((float)info.X, (float)info.Y) - vecCenter;

                float angle = Angle(vec1, vec2);

                HitObject.Rotation = InitialAngle + angle;

                return this;
            }

            public override ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                ClearAnnot();

                Tool.UpdatePropertyProvider();

                return new RotationStandbyToolState(Tool) {
                    HitObject = HitObject,
                }.EndPointerSequence(info, viewport);
            }

            private void ClearAnnot ()
            {
                if (Annot != null) {
                    Tool._annots.Remove(Annot);
                    Annot = null;
                }
            }

            private float Angle (Vector vec1, Vector vec2)
            {
                float angle = (float)Math.Atan2(vec1.X * vec2.Y - vec2.X * vec1.Y, vec1.X * vec2.X + vec1.Y * vec2.Y);
                return angle % (2 * (float)Math.PI);
            }
        }
    }
}
