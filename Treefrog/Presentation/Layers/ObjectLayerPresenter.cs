using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Tools;
using Treefrog.Presentation.Controllers;
using Treefrog.Framework.Imaging;
using Treefrog.Utility;
using Treefrog.Presentation.Annotations;
using Treefrog.Framework.Imaging.Drawing;
using System.Collections.ObjectModel;

namespace Treefrog.Presentation.Layers
{
    public class ObjectSelectionManager
    {
        private class SelectedObjectRecord
        {
            public ObjectInstance Instance { get; set; }
            public SelectionAnnot Annot { get; set; }
            public Point InitialLocation { get; set; }
        }

        private List<SelectedObjectRecord> _selectedObjects;

        private static Brush SelectedAnnotFill = new SolidColorBrush(new Color(128, 77, 255, 96));
        private static Pen SelectedAnnotOutline = new Pen(new SolidColorBrush(new Color(96, 0, 255, 255)), 1);

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

            _selectedObjects.Add(record);
            if (Annotations != null)
                Annotations.Add(record.Annot);

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
            }

            _selectedObjects.Clear();

            //CommandManager.Invalidate(CommandKey.Delete);
            //CommandManager.Invalidate(CommandKey.SelectNone);
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
                record.Annot.MoveTo(record.Instance.ImageBounds.Location);
            }
        }

        public void MoveObjectsByOffsetRelative (Point offset)
        {
            foreach (var record in _selectedObjects) {
                Point newLocation = new Point(record.InitialLocation.X + offset.X, record.InitialLocation.Y + offset.Y);

                record.Instance.X = newLocation.X;
                record.Instance.Y = newLocation.Y;
                record.Annot.MoveTo(record.Instance.ImageBounds.Location);
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

    public class ObjectLayerPresenter : LevelLayerPresenter, IPointerResponder, 
        IBindable<IObjectPoolCollectionPresenter>
    {
        private ObjectLayer _layer;
        private IObjectPoolCollectionPresenter _objectController;

        private ObjectSelectionManager _selectionManager;

        public ObjectLayerPresenter (ILayerContext layerContext, ObjectLayer layer)
            : base(layerContext, layer)
        {
            _layer = layer;

            _selectionManager = new ObjectSelectionManager() {
                Layer = layer,
                History = layerContext.History,
                Annotations = layerContext.Annotations,
            };

            InitializeCommandManager();
            SetCurrentTool(NewSelectTool());
        }

        protected override void DisposeManaged ()
        {
            Bind((IObjectPoolCollectionPresenter)null);

            base.DisposeManaged();
        }

        public void Bind (IObjectPoolCollectionPresenter controller)
        {
            if (_objectController != null) {
                _objectController.ObjectSelectionChanged -= HandleSelectedObjectChanged;
            }

            _objectController = controller;

            if (_objectController != null) {
                _objectController.ObjectSelectionChanged += HandleSelectedObjectChanged;
            }

            SetCurrentTool(NewSelectTool());
        }

        protected new ObjectLayer Layer
        {
            get { return _layer; }
        }

        private void HandleSelectedObjectChanged (object sender, EventArgs e)
        {
            if (_objectController != null && _objectController.SelectedObject != null) {
                SetCurrentTool(NewDrawTool());
            }
        }

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get
            {
                if (Layer == null || LayerContext.Geometry == null)
                    yield break;

                ILevelGeometry geometry = LayerContext.Geometry;

                Rectangle region = geometry.VisibleBounds;
                foreach (ObjectInstance inst in Layer.ObjectsInRegion(region, ObjectRegionTest.PartialImage)) {
                    Rectangle srcRect = inst.ObjectClass.ImageBounds;
                    Rectangle dstRect = inst.ImageBounds;
                    yield return new DrawCommand() {
                        Texture = inst.ObjectClass.ImageId,
                        SourceRect = new Rectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height),
                        DestRect = new Rectangle(
                            (int)(dstRect.X * geometry.ZoomFactor),
                            (int)(dstRect.Y * geometry.ZoomFactor),
                            (int)(dstRect.Width * geometry.ZoomFactor),
                            (int)(dstRect.Height * geometry.ZoomFactor)),
                        BlendColor = Colors.White,
                    };
                }
            }
        }

        #region Commands

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();

            _commandManager.Register(CommandKey.Paste, CommandCanPaste, CommandPaste);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private IEnumerable<ICommandSubscriber> CommandForwarder ()
        {
            ICommandSubscriber tool = _currentTool as ICommandSubscriber;
            if (tool != null)
                yield return tool;
        }

        private bool CommandCanPaste ()
        {
            return ObjectSelectionClipboard.ContainsData;
        }

        private void CommandPaste ()
        {
            ObjectSelectTool tool = _currentTool as ObjectSelectTool;
            if (tool == null)
                SetCurrentTool(NewSelectTool());

            tool.CommandManager.Perform(CommandKey.Paste);
        }

        #endregion

        #region Tool Management

        private PointerTool _currentTool;

        private void SetCurrentTool (PointerTool tool)
        {
            ObjectSelectTool objTool = _currentTool as ObjectSelectTool;
            if (objTool != null) {
                objTool.Cancel();

                _commandManager.RemoveCommandSubscriber(objTool);
            }

            _currentTool = tool;

            objTool = _currentTool as ObjectSelectTool;
            if (objTool != null && objTool.CommandManager != null) {
                _commandManager.AddCommandSubscriber(objTool);
            }
        }

        private ObjectSelectTool NewSelectTool ()
        {
            Treefrog.Framework.Imaging.Size gridSize = new Treefrog.Framework.Imaging.Size(16, 16);
            ObjectSelectTool tool = new ObjectSelectTool(LayerContext.History, Layer, gridSize, LayerContext.Annotations, null, _selectionManager);
            tool.BindObjectSourceController(_objectController);

            return tool;
        }

        private ObjectDrawTool NewDrawTool ()
        {
            Treefrog.Framework.Imaging.Size gridSize = new Treefrog.Framework.Imaging.Size(16, 16);
            ObjectDrawTool tool = new ObjectDrawTool(LayerContext.History, Layer, gridSize, LayerContext.Annotations);
            tool.BindObjectSourceController(_objectController);

            return tool;
        }

        public override IPointerResponder PointerEventResponder
        {
            get { return this; }
        }

        public void HandleStartPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.StartPointerSequence(info, LayerContext.Geometry);
        }

        public void HandleEndPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.EndPointerSequence(info, LayerContext.Geometry);

            if (_currentTool is ObjectDrawTool && _currentTool.IsCancelled)
                SetCurrentTool(NewSelectTool());
        }

        public void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.UpdatePointerSequence(info, LayerContext.Geometry);
        }

        public void HandlePointerPosition (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.PointerPosition(info, LayerContext.Geometry);

            if (Info != null)
                Info.ActionUpdateCoordinates(info.X + ", " + info.Y);
        }

        public void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();

            if (Info != null)
                Info.ActionUpdateCoordinates("");
        }

        #endregion
    }
}
