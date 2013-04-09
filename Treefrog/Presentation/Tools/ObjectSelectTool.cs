using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;

namespace Treefrog.Presentation.Tools
{
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
        //private static Pen SelectedAnnotOutline = new Pen(new SolidColorBrush(new Color(96, 0, 255, 255)), 1);
        private static Pen SelectedAnnotOutline = new PrimitivePen(new SolidColorBrush(new Color(96, 0, 255, 255)));
        //private static Pen SelectedAnnotOutline = new Pen(new StippleBrush(StipplePattern2px, new Color(96, 0, 255, 255)));

        private ObservableCollection<Annotation> _annots;
        private ILevelGeometry _viewport;

        private ObjectSelectionManager _selectionManager;
        private ILayerContext _layerContext;

        private ToolState _state;

        public ObjectSelectTool (ILayerContext layerContext, ObjectLayer layer, Size gridSize, ObjectSelectionManager selectionManager)
            : base(layerContext, layer, gridSize)
        {
            _annots = layerContext.Annotations;
            _viewport = layerContext.Geometry;
            _selectionManager = selectionManager;
            _layerContext = layerContext;

            InitializeCommandManager();

            _state = new SelectionStandbyToolState(this);
        }

        protected override void DisposeManaged ()
        {
            _state = _state.Cancel();
            base.DisposeManaged();
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
        }

        #endregion

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

        private void ActivateObjectMenu (PointerEventInfo info)
        {
            CommandMenu menu = new CommandMenu("", new List<CommandMenuGroup>() {
                    new CommandMenuGroup() {
                        CommandKey.Cut, CommandKey.Copy, CommandKey.Paste, CommandKey.Delete,
                    },
                    new CommandMenuGroup() {
                        CommandKey.ObjectProperties,
                    },
                });

            LayerContext.ActivateContextMenu(menu, new Point((int)info.X, (int)info.Y));
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

            public virtual ToolState Cancel ()
            {
                return this;
            }

            public virtual ToolState ObjectSelectionCleared ()
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
                        return StartSecondaryPointerSequence(info, viewport);
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

            private ToolState StartSecondaryPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                ObjectInstance hitObject = Tool.TopObject(Tool.CoarseHitTest((int)info.X, (int)info.Y));
                if (hitObject == null)
                    return new ReleaseToolState(Tool).StartPointerSequence(info, viewport);

                Tool.ActivateObjectMenu(info);

                if (Tool._selectionManager.IsObjectSelected(hitObject))
                    return this;
                else
                    return StartClickNew(info, viewport, hitObject);
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
            private static Pen Outline = new PrimitivePen(new SolidColorBrush(new Color(192, 192, 192, 255)));
            private static Pen OutlineGlow = new Pen(new SolidColorBrush(new Color(0, 0, 0, 128)), 3);
            private const float RingThreshold = 8;

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
                    Annot.OutlineGlow = OutlineGlow;
                    Tool._annots.Add(Annot);
                }

                return this;
            }

            public override ToolState Cancel ()
            {
                ClearAnnot();
                return new ReleaseToolState(Tool).StartPointerSequence(null, null);
            }

            private ToolState StartPrimaryPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                float threshold = RingThreshold / viewport.ZoomFactor;
                if (PointInRing(new Point((int)info.X, (int)info.Y), Annot.Center, Annot.Radius - threshold, Annot.Radius + threshold)) {
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
                ObjectInstance hitObject = Tool.TopObject(Tool.CoarseHitTest((int)info.X, (int)info.Y));
                if (hitObject == null) {
                    ClearAnnot();
                    return new ReleaseToolState(Tool).StartPointerSequence(info, viewport);
                }

                Tool.ActivateObjectMenu(info);

                if (Tool._selectionManager.IsObjectSelected(hitObject)) {
                    return this;
                }
                else {
                    ClearAnnot();
                    return new SelectionStandbyToolState(Tool).StartPointerSequence(info, viewport);
                }
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
                ClearAnnot();

                Tool._selectionManager.AddObjectsToSelection(Tool.ObjectsInArea(Band.Selection));

                Tool.EndAutoScroll(info, viewport);
                Tool.UpdatePropertyProvider();

                return new ReleaseToolState(Tool).EndPointerSequence(info, viewport);
            }

            public override ToolState Cancel ()
            {
                ClearAnnot();
                return new ReleaseToolState(Tool).StartPointerSequence(null, null);
            }

            private void ClearAnnot ()
            {
                if (Annot != null) {
                    Tool._annots.Remove(Annot);
                    Annot = null;
                }
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
            private static Pen Outline = new PrimitivePen(new SolidColorBrush(Colors.Yellow));
            private static Pen OutlineGlow = new Pen(new SolidColorBrush(new Color(0, 0, 0, 128)), 3);

            public SelectionRotatingToolState(ObjectSelectTool tool, ObjectInstance hitObject) 
                : base(tool) 
            {
                HitObject = hitObject;
                InitialPosition = hitObject.Position;
                InitialCenter = hitObject.ImageBounds.Center;
                InitialAngle = hitObject.Rotation;

                float radius = Tool.MaxBoundingDiagonal(HitObject) / 2 + 5;
                Annot = new CircleAnnot(HitObject.ImageBounds.Center, radius);
                Annot.Outline = Outline;
                Annot.OutlineGlow = OutlineGlow;
                Tool._annots.Add(Annot);
            }

            private ObjectInstance HitObject { get; set; }
            private Point InitialPosition { get; set; }
            private Point InitialCenter { get; set; }
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
                Vector vecCenter = new Vector(InitialPosition.X, InitialPosition.Y);
                Vector vec1 = new Vector(InitialLocation.X, InitialLocation.Y) - vecCenter;
                Vector vec2 = new Vector((float)info.X, (float)info.Y) - vecCenter;

                Vector imgCenter = new Vector(InitialCenter.X, InitialCenter.Y);

                Vector originWorking = new Vector(InitialPosition.X, InitialPosition.Y) - imgCenter;

                float angle = Angle(vec1, vec2);

                float s = (float)Math.Sin(angle);
                float c = (float)Math.Cos(angle);

                Vector originPrime = new Vector(
                    originWorking.X * c - originWorking.Y * s,
                    originWorking.X * s + originWorking.Y * c);

                originPrime += imgCenter;

                HitObject.Position = new Point((int)originPrime.X, (int)originPrime.Y);
                HitObject.Rotation = InitialAngle + angle;

                return this;
            }

            public override ToolState EndPointerSequence (PointerEventInfo info, ILevelGeometry viewport)
            {
                ClearAnnot();

                Tool.UpdatePropertyProvider();

                ObjectMoveCommand command = new ObjectMoveCommand();
                command.QueueRotate(HitObject, InitialPosition, HitObject.Position, InitialAngle, HitObject.Rotation);

                Tool.History.Execute(command);

                return new RotationStandbyToolState(Tool) {
                    HitObject = HitObject,
                }.EndPointerSequence(info, viewport);
            }

            public override ToolState Cancel ()
            {
                ClearAnnot();
                return new ReleaseToolState(Tool).StartPointerSequence(null, null);
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
