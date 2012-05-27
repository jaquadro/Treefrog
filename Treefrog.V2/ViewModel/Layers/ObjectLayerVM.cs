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
            SetupPoolService();

            _textures = new ObservableDictionary<string, TextureResource>();

            if (Layer != null && Layer.Level != null && Layer.Level.Project != null) {
                foreach (ObjectInstance inst in Layer.Objects) {
                    if (!_textures.ContainsKey(inst.ObjectClass.Name))
                        _textures[inst.ObjectClass.Name] = inst.ObjectClass.Image;
                }

                Layer.ObjectAdded += HandleObjectAdded;
            }

            SetCurrentTool(new ObjectSelectTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations));
        }

        private void SetupPoolService ()
        {
            if (_poolService == null) {
                _poolService = ServiceContainer.Default.GetService<ObjectPoolManagerService>();
                if (_poolService != null)
                    _poolService.PropertyChanged += HandlePoolServicePropertyChanged;
            }
        }

        private void HandlePoolServicePropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveObjectClass" && _poolService.ActiveObjectClass != null) {
                if (_currentTool != null)
                    _currentTool.Cancel();
                SetCurrentTool(new ObjectDrawTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations));
            }
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

        private PointerTool _currentTool;

        private void SetCurrentTool (PointerTool tool)
        {
            ObjectSelectTool objTool = _currentTool as ObjectSelectTool;
            if (objTool != null) {
                objTool.CanDeleteChanged -= HandleCanDeleteChanged;
                objTool.CanSelectAllChanged -= HandleCanSelectAllChanged;
                objTool.CanSelectNoneChanged -= HandleCanSelectNoneChanged;
            }

            _currentTool = tool;

            objTool = _currentTool as ObjectSelectTool;
            if (objTool != null) {
                objTool.CanDeleteChanged += HandleCanDeleteChanged;
                objTool.CanSelectAllChanged += HandleCanSelectAllChanged;
                objTool.CanSelectNoneChanged += HandleCanSelectNoneChanged;
            }
        }

        public override void HandleStartPointerSequence (PointerEventInfo info)
        {
            SetupPoolService();

            if (_currentTool != null)
                _currentTool.StartPointerSequence(info);
        }

        public override void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.UpdatePointerSequence(info);
        }

        public override void HandleEndPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.EndPointerSequence(info);

            if (_currentTool is ObjectDrawTool && _currentTool.IsCancelled)
                SetCurrentTool(new ObjectSelectTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations));
        }

        public override void HandlePointerPosition (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.PointerPosition(info);
        }

        public override void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();
        }

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
    }
}
