using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Treefrog.Windows.Controls;

using TFImaging = Treefrog.Framework.Imaging;
using Treefrog.Presentation.Tools;
using Treefrog.Presentation.Commands;
using System.Windows.Forms;

namespace Treefrog.Presentation.Layers
{
    public class ObjectControlLayer : BaseControlLayer, IPointerToolResponder, ICommandSubscriber
    {
        private ObjectLayer _layer;

        public override int VirtualHeight
        {
            get 
            { 
                if (_layer == null)
                    return 0;
                return _layer.LayerHeight;
            }
        }

        public override int VirtualWidth
        {
            get 
            {
                if (_layer == null)
                    return 0; 
                return _layer.LayerWidth;
            }
        }

        public ObjectControlLayer (LayerControl control)
            : base(control)
        {
            InitializeCommandManager();
        }

        public ObjectControlLayer (LayerControl control, ObjectLayer layer)
            : this(control)
        {
            Layer = layer;
        }

        #region Command Handling

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

        public new ObjectLayer Layer
        {
            get { return _layer; }
            protected set
            {
                _layer = value;
                base.Layer = value;
            }
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch)
        {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));

            return offset;
        }

        protected void EndDraw (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        protected override void DrawContentImpl (SpriteBatch spriteBatch)
        {
            if (_layer == null || Visible == false)
                return;

            Rectangle region = Control.VisibleRegion;

            TFImaging.Rectangle castRegion = new TFImaging.Rectangle(
                    region.X, region.Y, region.Width, region.Height
                    );

            DrawObjects(spriteBatch, castRegion);
        }

        protected virtual void DrawObjects (SpriteBatch spriteBatch, TFImaging.Rectangle region)
        {
            ObjectTextureService textureService = Control.Services.GetService<ObjectTextureService>();
            if (textureService == null)
                return;

            Vector2 offset = BeginDraw(spriteBatch);

            foreach (ObjectInstance inst in Layer.ObjectsInRegion(region, ObjectRegionTest.PartialImage)) {
                TFImaging.Rectangle srcRect = inst.ObjectClass.ImageBounds;
                TFImaging.Rectangle dstRect = inst.ImageBounds;

                Texture2D texture = textureService.GetTexture(inst.ObjectClass.Id);
                if (texture == null)
                    continue;
                
                Rectangle sourceRect = new Rectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height);
                Rectangle destRect = new Rectangle(
                        (int)(dstRect.X * Control.Zoom),
                        (int)(dstRect.Y * Control.Zoom),
                        (int)(dstRect.Width * Control.Zoom),
                        (int)(dstRect.Height * Control.Zoom)
                        );

                spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
            }

            EndDraw(spriteBatch);
        }

        #region Tool Management

        private ILevelPresenter _levelController;
        private IObjectPoolCollectionPresenter _objectController;
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
            ObjectSelectTool tool = new ObjectSelectTool(_levelController.History, Layer, gridSize, _levelController.Annotations, new LayerControlViewport(Control));
            tool.BindObjectSourceController(_objectController);

            return tool;
        }

        private ObjectDrawTool NewDrawTool ()
        {
            Treefrog.Framework.Imaging.Size gridSize = new Treefrog.Framework.Imaging.Size(16, 16);
            ObjectDrawTool tool = new ObjectDrawTool(_levelController.History, Layer, gridSize, _levelController.Annotations);
            tool.BindObjectSourceController(_objectController);

            return tool;
        }

        #endregion

        public void BindLevelController (ILevelPresenter levelController)
        {
            _levelController = levelController;
            SetCurrentTool(NewSelectTool());
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
        }

        private void HandleSelectedObjectChanged (object sender, EventArgs e)
        {
            if (_objectController != null && _objectController.SelectedObject != null) {
                SetCurrentTool(NewDrawTool());
            }
        }

        #region Pointer Commands

        public void HandleStartPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.StartPointerSequence(info, new LayerControlViewport(Control));
        }

        public void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.UpdatePointerSequence(info, new LayerControlViewport(Control));
        }

        public void HandleEndPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.EndPointerSequence(info, new LayerControlViewport(Control));

            if (_currentTool is ObjectDrawTool && _currentTool.IsCancelled)
                SetCurrentTool(NewSelectTool());
        }

        public void HandlePointerPosition (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.PointerPosition(info, new LayerControlViewport(Control));
        }

        public void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();
        }

        #endregion

        public override IEditToolResponder EditToolResponder
        {
            get { return _currentTool as IEditToolResponder; }
        }

        public override IPointerToolResponder PointerToolResponder
        {
            get { return this; }
        }
    }

    public interface IPointerToolResponder
    {
        void BindLevelController (ILevelPresenter controller);

        void HandleStartPointerSequence (PointerEventInfo info);
        void HandleEndPointerSequence (PointerEventInfo info);
        void HandleUpdatePointerSequence (PointerEventInfo info);
        void HandlePointerPosition (PointerEventInfo info);
        void HandlePointerLeaveField ();
    }

    public interface IEditToolResponder
    {
        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }
        bool CanDelete { get; }

        bool CanSelectAll { get; }
        bool CanSelectNone { get; }

        void Cut ();
        void Copy ();
        void Paste ();
        void Delete ();

        void SelectAll ();
        void SelectNone ();

        event EventHandler CanCutChanged;
        event EventHandler CanCopyChanged;
        event EventHandler CanPasteChanged;
        event EventHandler CanDeleteChanged;

        event EventHandler CanSelectAllChanged;
        event EventHandler CanSelectNoneChanged;
    }

    public interface IEditCommandResponder
    {
        void BindLevelController (ILevelPresenter controller);
    }
}
