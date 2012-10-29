using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using Treefrog.ViewModel.Tools;
using System.ComponentModel;
using Treefrog.Framework.Imaging;

using Rect = System.Windows.Rect;
using Clipboard = System.Windows.Clipboard;

namespace Treefrog.ViewModel.Layers
{
    public class ObjectLayerVM : LevelLayerVM
    {
        private ObjectPoolManagerService _poolService;
        private ObservableDictionary<string, TextureResource> _textures;

        private Size _gridSize = new Size(16, 16);

        public ObjectLayerVM (LevelDocumentVM level, ObjectLayer layer, ViewportVM viewport)
            : base(level, layer, viewport)
        {
            Initialize();
        }

        public ObjectLayerVM (LevelDocumentVM level, ObjectLayer layer)
            : base(level, layer)
        {
            Initialize();
        }

        private void Initialize ()
        {
            ServiceContainer.Default.ServiceSet += HandleServiceContainerSet;
            SetPoolService();

            _textures = new ObservableDictionary<string, TextureResource>();

            if (Layer != null && Layer.Level != null && Layer.Level.Project != null) {
                foreach (ObjectInstance inst in Layer.Objects) {
                    if (!_textures.ContainsKey(inst.ObjectClass.Name))
                        _textures[inst.ObjectClass.Name] = inst.ObjectClass.Image;
                }

                Layer.ObjectAdded += HandleObjectAdded;
            }

            SetCurrentTool(NewSelectTool());
        }

        public override void Cleanup ()
        {
            SetCurrentTool(null);
            SetPoolService(null);
            ServiceContainer.Default.ServiceSet -= HandleServiceContainerSet;
            
            base.Cleanup();
        }

        private void SetPoolService ()
        {
            SetPoolService(ServiceContainer.Default.GetService<ObjectPoolManagerService>());
        }

        private void SetPoolService (ObjectPoolManagerService service)
        {
            if (_poolService != null) {
                _poolService.PropertyChanged -= HandlePoolServicePropertyChanged;
            }

            _poolService = service;

            if (_poolService != null) {
                _poolService.PropertyChanged += HandlePoolServicePropertyChanged;
            }
        }

        private void HandleServiceContainerSet (object sender, ServiceEventArgs e)
        {
            if (e.ServiceType == typeof(ObjectPoolManagerService)) {
                SetPoolService();
            }
        }

        private void HandlePoolServicePropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveObjectClass" && _poolService.ActiveObjectClass != null)
                SetCurrentTool(NewDrawTool());
        }

        private void HandleObjectAdded (object sender, ObjectInstanceEventArgs e)
        {
            if (!_textures.ContainsKey(e.Instance.ObjectClass.Name))
                _textures[e.Instance.ObjectClass.Name] = e.Instance.ObjectClass.Image;
        }

        protected new ObjectLayer Layer
        {
            get { return base.Layer as ObjectLayer; }
        }

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get
            {
                if (Layer == null)
                    yield break;

                Rect region = Viewport.VisibleRegion;
                Rectangle castRegion = new Rectangle(
                    (int)Math.Floor(region.X),
                    (int)Math.Floor(region.Y),
                    (int)Math.Ceiling(region.Width),
                    (int)Math.Ceiling(region.Height));

                foreach (ObjectInstance inst in Layer.ObjectsInRegion(castRegion, ObjectRegionTest.PartialImage)) {
                    Rectangle srcRect = inst.ObjectClass.ImageBounds;
                    Rectangle dstRect = inst.ImageBounds;
                    yield return new DrawCommand()
                    {
                        Texture = inst.ObjectClass.Name,
                        SourceRect = new Rectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height),
                        DestRect = new Rectangle(
                            (int)(dstRect.X * Viewport.ZoomFactor), 
                            (int)(dstRect.Y * Viewport.ZoomFactor), 
                            (int)(dstRect.Width * Viewport.ZoomFactor), 
                            (int)(dstRect.Height * Viewport.ZoomFactor)),
                        BlendColor = Colors.White,
                    };
                }
            }
        }

        public override ObservableDictionary<string, TextureResource> TextureSource
        {
            get
            {
                return _textures;
            }
        }

        #region Tool Management

        private PointerTool _currentTool;

        private void SetCurrentTool (PointerTool tool)
        {
            ObjectSelectTool objTool = _currentTool as ObjectSelectTool;
            if (objTool != null) {
                objTool.Cancel();

                objTool.CanDeleteChanged -= HandleCanDeleteChanged;
                objTool.CanSelectAllChanged -= HandleCanSelectAllChanged;
                objTool.CanSelectNoneChanged -= HandleCanSelectNoneChanged;
                objTool.CanCutChanged -= HandleCanCutChanged;
                objTool.CanCopyChanged -= HandleCanCopyChanged;
            }

            _currentTool = tool;

            objTool = _currentTool as ObjectSelectTool;
            if (objTool != null) {
                objTool.CanDeleteChanged += HandleCanDeleteChanged;
                objTool.CanSelectAllChanged += HandleCanSelectAllChanged;
                objTool.CanSelectNoneChanged += HandleCanSelectNoneChanged;
                objTool.CanCutChanged += HandleCanCutChanged;
                objTool.CanCopyChanged += HandleCanCopyChanged;
            }
        }

        private ObjectSelectTool NewSelectTool ()
        {
            return new ObjectSelectTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations, Viewport);
        }

        private ObjectDrawTool NewDrawTool ()
        {
            return new ObjectDrawTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations);
        }

        #endregion

        #region Pointer Commands

        public override void HandleStartPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.StartPointerSequence(info, Viewport);
        }

        public override void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.UpdatePointerSequence(info, Viewport);
        }

        public override void HandleEndPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.EndPointerSequence(info, Viewport);

            if (_currentTool is ObjectDrawTool && _currentTool.IsCancelled)
                SetCurrentTool(NewSelectTool());
        }

        public override void HandlePointerPosition (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.PointerPosition(info, Viewport);
        }

        public override void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();
        }

        #endregion

        #region Edit Commands

        #region Cut

        public override bool CanCut
        {
            get
            {
                ObjectSelectTool tool = _currentTool as ObjectSelectTool;
                return (tool != null)
                    ? tool.CanCut
                    : false;
            }
        }

        public override void Cut ()
        {
            ObjectSelectTool tool = _currentTool as ObjectSelectTool;
            if (tool != null) {
                tool.Cut();
                OnCanPasteChanged(EventArgs.Empty);
            }
        }

        private void HandleCanCutChanged (object sender, EventArgs e)
        {
            OnCanCutChanged(e);
        }

        #endregion

        #region Copy

        public override bool CanCopy
        {
            get
            {
                ObjectSelectTool tool = _currentTool as ObjectSelectTool;
                return (tool != null)
                    ? tool.CanCopy
                    : false;
            }
        }

        public override void Copy ()
        {
            ObjectSelectTool tool = _currentTool as ObjectSelectTool;
            if (tool != null) {
                tool.Copy();
                OnCanPasteChanged(EventArgs.Empty);
            }
        }

        private void HandleCanCopyChanged (object sender, EventArgs e)
        {
            OnCanCopyChanged(e);
        }

        #endregion

        #region Paste

        public override bool CanPaste
        {
            get
            {
                return Clipboard.ContainsData(typeof(ObjectSelectionClipboard).FullName);
            }
        }

        public override void Paste ()
        {
            ObjectSelectTool tool = _currentTool as ObjectSelectTool;
            if (tool == null)
                SetCurrentTool(NewSelectTool());

            if (tool != null)
                tool.Paste();
        }

        #endregion

        #region Delete

        public override bool CanDelete
        {
            get
            {
                ObjectSelectTool tool = _currentTool as ObjectSelectTool;
                return (tool != null)
                    ? tool.CanDelete
                    : false;
            }
        }

        public override void Delete ()
        {
            ObjectSelectTool tool = _currentTool as ObjectSelectTool;
            if (tool != null)
                tool.Delete();
        }

        private void HandleCanDeleteChanged (object sender, EventArgs e)
        {
            OnCanDeleteChanged(e);
        }

        #endregion

        #region Select All

        public override bool CanSelectAll
        {
            get
            {
                ObjectSelectTool tool = _currentTool as ObjectSelectTool;
                return (tool != null)
                    ? tool.CanSelectAll
                    : false;
            }
        }

        public override void SelectAll ()
        {
            ObjectSelectTool tool = _currentTool as ObjectSelectTool;
            if (tool != null)
                tool.SelectAll();
        }

        private void HandleCanSelectAllChanged (object sender, EventArgs e)
        {
            OnCanSelectAllChanged(e);
        }

        #endregion

        #region Select None

        public override bool CanSelectNone
        {
            get
            {
                ObjectSelectTool tool = _currentTool as ObjectSelectTool;
                return (tool != null)
                    ? tool.CanSelectNone
                    : false;
            }
        }

        public override void SelectNone ()
        {
            ObjectSelectTool tool = _currentTool as ObjectSelectTool;
            if (tool != null)
                tool.SelectNone();
        }

        private void HandleCanSelectNoneChanged (object sender, EventArgs e)
        {
            OnCanSelectNoneChanged(e);
        }

        #endregion

        #endregion
    }
}
