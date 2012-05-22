using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using System.Windows;
using Treefrog.Framework;

namespace Treefrog.ViewModel.Layers
{
    public class LevelLayerVM : RenderLayerVM
    {
        private LevelDocumentVM _level;
        private Layer _layer;
        private ViewportVM _viewport;

        public LevelLayerVM (LevelDocumentVM level, Layer layer, ViewportVM viewport)
        {
            _level = level;
            _layer = layer;
            _viewport = viewport ?? new ViewportVM()
            {
                Viewport = new Size(layer.LayerWidth, layer.LayerHeight),
            };

            _layer.NameChanged += HandleNameChanged;
            _layer.VisibilityChanged += HandleVisibilityChanged;
            _layer.OpacityChanged += HandleOpacityChanged;
        }

        public LevelLayerVM (LevelDocumentVM level, Layer layer)
            : this(level, layer, null)
        {
        }

        protected LevelDocumentVM Level
        {
            get { return _level; }
        }

        protected Layer Layer
        {
            get { return _layer; }
        }

        protected ViewportVM Viewport
        {
            get { return _viewport; }
        }

        public override string LayerName
        {
            get { return _layer.Name; }
            set { _layer.Name = value; }
        }

        public override bool IsVisible
        {
            get { return _layer.IsVisible; }
            set
            {
                if (_layer.IsVisible != value) {
                    _layer.IsVisible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        public virtual Vector GetCoordinates (double x, double y)
        {
            return new Vector(x, y);
        }

        public virtual Rect CoordinateBounds
        {
            get { return new Rect(0, 0, LayerWidth, LayerHeight); }
        }

        public override double LayerWidth
        {
            get { return _layer.LayerWidth; }
        }

        public override double LayerHeight
        {
            get { return _layer.LayerHeight; }
        }

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get { yield break; }
        }

        public float Opacity
        {
            get { return _layer.Opacity; }
            set { _layer.Opacity = value; }
        }

        private void HandleNameChanged (object sender, NameChangedEventArgs e)
        {
            RaisePropertyChanged("LayerName");
        }

        private void HandleVisibilityChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("IsVisible");
        }

        private void HandleOpacityChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("Opacity");
        }

        #region Pointer Handlers

        public virtual void HandleStartPointerSequence (PointerEventInfo info)
        {
        }

        public virtual void HandleEndPointerSequence (PointerEventInfo info)
        {
        }

        public virtual void HandleUpdatePointerSequence (PointerEventInfo info)
        {
        }

        public virtual void HandlePointerPosition (PointerEventInfo info)
        {
        }

        public virtual void HandlePointerLeaveField ()
        {
        }

        #endregion
    }
}
