using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Tools;
using Treefrog.Presentation.Controllers;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Layers
{
    public class ObjectLayerPresenter : LevelLayerPresenter, IPointerResponder
    {
        private ObjectLayer _layer;
        private IObjectPoolCollectionPresenter _objectController;

        public ObjectLayerPresenter (ILayerContext layerContext, ObjectLayer layer)
            : base(layerContext, layer)
        {
            _layer = layer;

            InitializeCommandManager();
            SetCurrentTool(NewSelectTool());
        }

        protected new ObjectLayer Layer
        {
            get { return _layer; }
        }

        public void BindObjectController (IObjectPoolCollectionPresenter objectController)
        {
            if (_objectController != null) {
                _objectController.ObjectSelectionChanged -= HandleSelectedObjectChanged;
            }

            _objectController = objectController;

            if (_objectController != null) {
                _objectController.ObjectSelectionChanged += HandleSelectedObjectChanged;
            }

            SetCurrentTool(NewSelectTool());
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
            ObjectSelectTool tool = new ObjectSelectTool(LayerContext.History, Layer, gridSize, LayerContext.Annotations, null);
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

            //if (ContentInfo != null)
            //    ContentInfo.ActionUpdateCoordinates(info.X + ", " + info.Y);
        }

        public void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();
        }

        #endregion
    }
}
