using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using XRectangle = Microsoft.Xna.Framework.Rectangle;
using XColor = Microsoft.Xna.Framework.Color;
using Treefrog.V2.ViewModel.Tools;
using System.ComponentModel;
using Treefrog.Framework.Imaging;

using Rect = System.Windows.Rect;

namespace Treefrog.V2.ViewModel.Layers
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

            _currentTool = new ObjectSelectTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations);
        }

        private void SetupPoolService ()
        {
            if (_poolService == null) {
                _poolService = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<ObjectPoolManagerService>();
                if (_poolService != null)
                    _poolService.PropertyChanged += HandlePoolServicePropertyChanged;
            }
        }

        private void HandlePoolServicePropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveObjectClass" && _poolService.ActiveObjectClass != null) {
                if (_currentTool != null)
                    _currentTool.Cancel();
                _currentTool = new ObjectDrawTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations);
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
                        SourceRect = new XRectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height),
                        DestRect = new XRectangle(
                            (int)(dstRect.X * Viewport.ZoomFactor), 
                            (int)(dstRect.Y * Viewport.ZoomFactor), 
                            (int)(dstRect.Width * Viewport.ZoomFactor), 
                            (int)(dstRect.Height * Viewport.ZoomFactor)),
                        BlendColor = XColor.White,
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
                _currentTool = new ObjectSelectTool(Level.CommandHistory, Layer as ObjectLayer, _gridSize, Level.Annotations);
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
    }
}
