using System;
using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Tools;
using Treefrog.Utility;

namespace Treefrog.Presentation.Layers
{
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

            _commandManager.Register(CommandKey.Delete, CommandCanDelete, CommandDelete);
            _commandManager.Register(CommandKey.SelectAll, CommandCanSelectAll, CommandSelectAll);
            _commandManager.Register(CommandKey.SelectNone, CommandCanSelectNone, CommandSelectNone);
            _commandManager.Register(CommandKey.Paste, CommandCanPaste, CommandPaste);
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
            _selectionManager.DeleteSelectedObjects();
        }

        private bool CommandCanSelectAll ()
        {
            return true;
        }

        private void CommandSelectAll ()
        {
            if (Layer != null) {
                _selectionManager.AddObjectsToSelection(Layer.Objects);
                SetCurrentTool(NewSelectTool());
            }
        }

        private bool CommandCanSelectNone ()
        {
            return _selectionManager.SelectedObjectCount > 0;
        }

        private void CommandSelectNone ()
        {
            _selectionManager.ClearSelection();
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
            ObjectSelectTool tool = new ObjectSelectTool(LayerContext.History, Layer, gridSize, LayerContext.Annotations, null, _selectionManager, LayerContext);
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
