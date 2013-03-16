using System;
using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Tools;
using Treefrog.Utility;
using System.Windows.Forms;

namespace Treefrog.Presentation.Layers
{
    public class ObjectLayerPresenter : LevelLayerPresenter, IPointerResponder, ICommandSubscriber,
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

            _selectionManager.SelectionChanged += SelectionChanged;

            InitializeCommandManager();
            SetCurrentTool(NewSelectTool());
        }

        protected override void DisposeManaged ()
        {
            Bind((IObjectPoolCollectionPresenter)null);

            _selectionManager.Dispose();

            base.DisposeManaged();
        }

        public override void Activate ()
        {
            _selectionManager.ShowAnnotations();
        }

        public override void Deactivate ()
        {
            _selectionManager.HideAnnotations();
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
                    Rectangle dstRect = inst.ObjectClass.ImageBounds;
                    yield return new DrawCommand() {
                        Texture = inst.ObjectClass.ImageId,
                        SourceRect = new Rectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height),
                        DestRect = new Rectangle(
                            (int)((inst.Position.X + dstRect.X) * geometry.ZoomFactor),
                            (int)((inst.Position.Y + dstRect.Y) * geometry.ZoomFactor),
                            (int)(dstRect.Width * geometry.ZoomFactor),
                            (int)(dstRect.Height * geometry.ZoomFactor)),
                        BlendColor = Colors.White,
                        Rotation = inst.Rotation,
                        OriginX = inst.ObjectClass.Origin.X,
                        OriginY = inst.ObjectClass.Origin.Y,
                    };
                }
            }
        }

        #region Commands

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();

            _commandManager.Register(CommandKey.Delete, CommandCanDelete, CommandDelete);
            _commandManager.Register(CommandKey.SelectAll, CommandCanSelectAll, CommandSelectAll);
            _commandManager.Register(CommandKey.SelectNone, CommandCanSelectNone, CommandSelectNone);
            _commandManager.Register(CommandKey.Cut, CommandCanCut, CommandCut);
            _commandManager.Register(CommandKey.Copy, CommandCanCopy, CommandCopy);
            _commandManager.Register(CommandKey.Paste, CommandCanPaste, CommandPaste);
            _commandManager.Register(CommandKey.ObjectMoveTop, CommandCanMoveObjectsToFront, CommandMoveObjectsToFront);
            _commandManager.Register(CommandKey.ObjectMoveUp, CommandCanMoveObjectsForward, CommandMoveObjectsForward);
            _commandManager.Register(CommandKey.ObjectMoveDown, CommandCanMoveObjectsBackward, CommandMoveObjectsBackward);
            _commandManager.Register(CommandKey.ObjectMoveBottom, CommandCanMoveObjectsToBack, CommandMoveObjectsToBack);
            _commandManager.Register(CommandKey.ObjectProperties, CommandCanObjectProperties, CommandObjectProperties);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanDelete ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandDelete ()
        {
            if (CommandCanDelete()) {
                _selectionManager.DeleteSelectedObjects();
            }
        }

        private bool CommandCanSelectAll ()
        {
            return true;
        }

        private void CommandSelectAll ()
        {
            if (CommandCanSelectAll()) {
                if (Layer != null) {
                    _selectionManager.AddObjectsToSelection(Layer.Objects);
                    SetCurrentTool(NewSelectTool());
                }
            }
        }

        private bool CommandCanSelectNone ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandSelectNone ()
        {
            if (CommandCanSelectNone()) {
                _selectionManager.ClearSelection();
            }
        }

        private IEnumerable<ICommandSubscriber> CommandForwarder ()
        {
            ICommandSubscriber tool = _currentTool as ICommandSubscriber;
            if (tool != null)
                yield return tool;
        }

        private bool CommandCanCut ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandCut ()
        {
            if (CommandCanCut()) {
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
            if (CommandCanCopy()) {
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
            if (CommandCanPaste()) {
                ObjectSelectionClipboard clip = ObjectSelectionClipboard.CopyFromClipboard(Layer.Level.Project);
                if (clip == null)
                    return;

                ObjectSelectTool tool = _currentTool as ObjectSelectTool;
                if (tool == null)
                    SetCurrentTool(NewSelectTool());

                _selectionManager.ClearSelection();

                CenterObjectsInViewport(clip.Objects);

                Command command = new ObjectAddCommand(Layer, clip.Objects, _selectionManager);
                LayerContext.History.Execute(command);

                foreach (ObjectInstance inst in clip.Objects) {
                    //Layer.AddObject(inst);
                    _selectionManager.AddObjectToSelection(inst);
                }
            }
        }

        private bool CommandCanMoveObjectsToFront ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandMoveObjectsToFront ()
        {
            if (CommandCanMoveObjectsToFront()) {
                ObjectOrderCommand command = new ObjectOrderCommand(Layer);
                foreach (ObjectInstance inst in _selectionManager.SelectedObjects)
                    command.QueueMoveFront(inst);

                LayerContext.History.Execute(command);
                InvalidateObjectArrangeCommands();
            }
        }

        private bool CommandCanMoveObjectsForward ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandMoveObjectsForward ()
        {
            if (CommandCanMoveObjectsForward()) {
                ObjectOrderCommand command = new ObjectOrderCommand(Layer);
                foreach (ObjectInstance inst in _selectionManager.SelectedObjects)
                    command.QueueMoveForward(inst);

                LayerContext.History.Execute(command);
                InvalidateObjectArrangeCommands();
            }
        }

        private bool CommandCanMoveObjectsBackward ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandMoveObjectsBackward ()
        {
            if (CommandCanMoveObjectsForward()) {
                ObjectOrderCommand command = new ObjectOrderCommand(Layer);
                foreach (ObjectInstance inst in _selectionManager.SelectedObjects)
                    command.QueueMoveBackward(inst);

                LayerContext.History.Execute(command);
                InvalidateObjectArrangeCommands();
            }
        }

        private bool CommandCanMoveObjectsToBack ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandMoveObjectsToBack ()
        {
            if (CommandCanMoveObjectsToBack()) {
                ObjectOrderCommand command = new ObjectOrderCommand(Layer);
                foreach (ObjectInstance inst in _selectionManager.SelectedObjects)
                    command.QueueMoveBack(inst);

                LayerContext.History.Execute(command);
                InvalidateObjectArrangeCommands();
            }
        }

        private bool CommandCanObjectProperties ()
        {
            return _selectionManager.SelectedObjectCount == 1;
        }

        private void CommandObjectProperties ()
        {
            if (CommandCanObjectProperties()) {
                foreach (ObjectInstance inst in _selectionManager.SelectedObjects) {
                    LayerContext.ActivatePropertyProvider(inst);
                    break;
                }
            }
        }

        #endregion

        private void InvalidateObjectCommands ()
        {
            CommandManager.Invalidate(CommandKey.Cut);
            CommandManager.Invalidate(CommandKey.Copy);
            CommandManager.Invalidate(CommandKey.Delete);
            CommandManager.Invalidate(CommandKey.SelectAll);
            CommandManager.Invalidate(CommandKey.SelectNone);
            CommandManager.Invalidate(CommandKey.ObjectProperties);

            InvalidateObjectArrangeCommands();
        }

        private void InvalidateObjectArrangeCommands ()
        {
            CommandManager.Invalidate(CommandKey.ObjectMoveBottom);
            CommandManager.Invalidate(CommandKey.ObjectMoveDown);
            CommandManager.Invalidate(CommandKey.ObjectMoveTop);
            CommandManager.Invalidate(CommandKey.ObjectMoveUp);
        }

        private void SelectionChanged (object sender, EventArgs e)
        {
            InvalidateObjectCommands();
        }

        private void CenterObjectsInViewport (List<ObjectInstance> objects)
        {
            Rectangle visibleBounds = LayerContext.Geometry.VisibleBounds;
            Rectangle collectionBounds = ObjectCollectionBounds(objects);

            int centerViewX = (int)(visibleBounds.Left + (visibleBounds.Right - visibleBounds.Left) / 2);
            int centerViewY = (int)(visibleBounds.Top + (visibleBounds.Bottom - visibleBounds.Top) / 2);

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
            ObjectSelectTool tool = new ObjectSelectTool(LayerContext, Layer, gridSize, _selectionManager);
            tool.BindObjectSourceController(_objectController);

            return tool;
        }

        private ObjectDrawTool NewDrawTool ()
        {
            Treefrog.Framework.Imaging.Size gridSize = new Treefrog.Framework.Imaging.Size(16, 16);
            ObjectDrawTool tool = new ObjectDrawTool(LayerContext, Layer, gridSize);
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
}
