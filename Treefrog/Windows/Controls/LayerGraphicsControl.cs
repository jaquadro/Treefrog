using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Windows.Layers;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Treefrog.Presentation;
using TFImaging = Treefrog.Framework.Imaging;
using Treefrog.Presentation.Controllers;

namespace Treefrog.Windows.Controls
{
    public class LayerGraphicsControl : GraphicsDeviceControl, IScrollableControl, IPointerTarget
    {
        private bool _initialized;
        private bool _disposed;

        private bool _widthSynced;
        private bool _heightSynced;
        private int _refWidth;
        private int _refHeight;

        private CanvasLayer _rootLayer;
        private TextureCache _textureCache;
        private LayerGraphicsControlGeometry _geometry;

        public LayerGraphicsControl ()
        {
            ControlInitialized += GraphicsDeviceControlInitialized;

            _textureCache = new TextureCache();
            _geometry = new LayerGraphicsControlGeometry(this);

            ClearColor = Color.LightGray;
        }

        private void GraphicsDeviceControlInitialized (object sender, EventArgs e)
        {
            ControlInitialized -= GraphicsDeviceControlInitialized;
            _textureCache.GraphicsDevice = GraphicsDevice;
            _initialized = true;
        }

        protected override void Dispose (bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    _textureCache.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        public bool Initialized
        {
            get { return _initialized; }
        }

        public CanvasLayer RootLayer
        {
            get { return _rootLayer; }
            set
            {
                if (_rootLayer != null) {
                    _rootLayer.LevelGeometry = null;
                    _rootLayer.TextureCache = null;
                }

                _rootLayer = value;
                if (_rootLayer != null) {
                    _rootLayer.LevelGeometry = _geometry;
                    _rootLayer.TextureCache = _textureCache;
                }
            }
        }

        public ILevelGeometry LevelGeometry
        {
            get { return _geometry; }
        }

        public int ReferenceOriginX { get; set; }

        public int ReferenceOriginY { get; set; }

        public int ReferenceWidth
        {
            get { return _refWidth; }
            set
            {
                if (_refWidth != value) {
                    _refWidth = value;
                    if (!_widthSynced)
                        OnVirtualSizeChanged(EventArgs.Empty);
                }
            }
        }

        public int ReferenceHeight
        {
            get { return _refHeight; }
            set
            {
                if (_refHeight != value) {
                    _refHeight = value;
                    if (!_heightSynced)
                        OnVirtualSizeChanged(EventArgs.Empty);
                }
            }
        }

        public int VirtualOriginX
        {
            get { return _widthSynced ? 0 : ReferenceOriginX; }
        }

        public int VirtualOriginY
        {
            get { return _heightSynced ? 0 : ReferenceOriginY; }
        }

        public int VirtualWidth
        {
            get
            {
                if (_widthSynced)
                    return Width;
                if (_heightSynced && RootLayer != null)
                    return RootLayer.DependentWidth;
                return ReferenceWidth;
            }
        }

        public int VirtualHeight
        {
            get
            {
                if (_heightSynced)
                    return Height;
                if (_widthSynced && RootLayer != null)
                    return RootLayer.DependentHeight;
                return ReferenceHeight;
            }
        }

        public bool WidthSynced
        {
            get { return _widthSynced; }
            set
            {
                if (_widthSynced != value) {
                    _widthSynced = value;
                    if (value && _refWidth != Width)
                        OnVirtualSizeChanged(EventArgs.Empty);

                    _canScrollH = !value;
                    OnScrollPropertyChanged(EventArgs.Empty);
                }
            }
        }

        public bool HeightSynced
        {
            get { return _heightSynced; }
            set
            {
                if (_heightSynced != value) {
                    _heightSynced = value;
                    if (value && _refHeight != Height)
                        OnVirtualSizeChanged(EventArgs.Empty);

                    _canScrollV = !value;
                    OnScrollPropertyChanged(EventArgs.Empty);
                }
            }
        }

        public CanvasAlignment CanvasAlignment { get; set; }

        public Color ClearColor { get; set; }

        public TextureCache TextureCache
        {
            get { return _textureCache; }
            set { _textureCache = value; }
        }

        #region GraphicsDeviceControl Members

        /// <summary>
        /// Initializes control to a usable state, called at time of control creation.
        /// </summary>
        protected override void Initialize ()
        {
            //_services = new System.ComponentModel.Design.ServiceContainer();
            //_services.AddService(typeof(IGraphicsDeviceService), GraphicsDeviceService);
            //_spriteBatch = new SpriteBatch(GraphicsDevice);

            //_content = new ContentManager(_services);
            //_content.RootDirectory = "EditorContent";

            Application.Idle += delegate { Invalidate(); };
        }

        protected override void Draw ()
        {
            GraphicsDevice.Clear(ClearColor);

            if (RootLayer != null)
                RootLayer.Render(GraphicsDevice);
        }

        #endregion

        protected override void OnSizeChanged (EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        #region IScrollableControl Members

        private float _zoom = 1f;

        private bool _canScrollH = true;
        private bool _canScrollV = true;

        private int _scrollH;
        private int _scrollV;

        public event ScrollEventHandler Scroll;
        public event EventHandler VirtualSizeChanged;
        public event EventHandler ScrollPropertyChanged;
        public event EventHandler ZoomChanged;

        protected virtual void OnVirtualSizeChanged (EventArgs e)
        {
            if (VirtualSizeChanged != null) {
                VirtualSizeChanged(this, e);
            }
        }

        protected virtual void OnZoomChanged (EventArgs e)
        {
            if (ZoomChanged != null) {
                ZoomChanged(this, e);
            }
        }

        protected virtual void OnScroll (ScrollEventArgs e)
        {
            if (Scroll != null) {
                Scroll(this, e);
            }
        }

        protected virtual void OnScrollPropertyChanged (EventArgs e)
        {
            if (ScrollPropertyChanged != null) {
                ScrollPropertyChanged(this, e);
            }
        }

        public Control Control
        {
            get { return this; }
        }

        public System.Drawing.Rectangle VirtualSize
        {
            get { return new System.Drawing.Rectangle(0, 0, VirtualWidth, VirtualHeight); }
        }

        public int GetScrollValue (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _scrollH;
                case ScrollOrientation.VerticalScroll:
                    return _scrollV;
            }
            return 0;
        }

        public bool GetScrollEnabled (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _canScrollH;
                case ScrollOrientation.VerticalScroll:
                    return _canScrollV;
            }
            return false;
        }

        public void SetScrollValue (ScrollOrientation orientation, int value)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    _scrollH = value;
                    break;
                case ScrollOrientation.VerticalScroll:
                    _scrollV = value;
                    break;
            }

            Invalidate();
        }

        /// <summary>
        /// Gets the current zoom factor applied to the virtual surface.
        /// </summary>
        [DefaultValue(1.0f)]
        [Description("The zoom factor applied to the contents of the control")]
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoom != value) {
                    _zoom = value;
                    if (Initialized) {
                        OnVirtualSizeChanged(EventArgs.Empty);
                        CheckScrollValue();
                    }

                    OnZoomChanged(EventArgs.Empty);
                }
            }
        }

        private void CheckScrollValue ()
        {
            int maxH = Math.Max(0, (int)(((VirtualWidth * _zoom) - Width) / _zoom));
            int maxV = Math.Max(0, (int)(((VirtualHeight * _zoom) - Height) / _zoom));

            _scrollH = Math.Min(_scrollH, maxH);
            _scrollV = Math.Min(_scrollV, maxV);
        }

        #endregion

        #region IPointerTarget

        public System.Drawing.Point OriginOffset
        {
            get { return new System.Drawing.Point(VirtualOriginX, VirtualOriginY); }
        }

        public System.Drawing.Point InteriorOffset
        {
            get
            {
                TFImaging.Point offset = _geometry.CanvasBounds.Location;
                return new System.Drawing.Point(offset.X, offset.Y);
            }
        }

        public System.Drawing.Point ScrollOffset
        {
            get
            {
                return new System.Drawing.Point(
                    GetScrollValue(ScrollOrientation.HorizontalScroll),
                    GetScrollValue(ScrollOrientation.VerticalScroll));
            }
        }

        float IPointerTarget.Zoom
        {
            get { return Zoom; }
        }

        #endregion
    }

    public class LayerGraphicsControlGeometry : ILevelGeometry
    {
        private LayerGraphicsControl _control;

        public LayerGraphicsControlGeometry (LayerGraphicsControl control)
        {
            _control = control;
        }

        public TFImaging.Point ScrollPosition
        {
            get
            {
                int scrollH = _control.GetScrollValue(ScrollOrientation.HorizontalScroll);
                int scrollV = _control.GetScrollValue(ScrollOrientation.VerticalScroll);
                return new TFImaging.Point(scrollH, scrollV);
            }
        }

        public TFImaging.Rectangle LevelBounds
        {
            get
            {
                return new TFImaging.Rectangle(_control.VirtualOriginX, _control.VirtualOriginY,
                    _control.VirtualWidth, _control.VirtualHeight);
            }
            set
            {
                _control.ReferenceOriginX = value.X;
                _control.ReferenceOriginY = value.Y;
                _control.ReferenceWidth = value.Width;
                _control.ReferenceHeight = value.Height;
            }
        }

        public TFImaging.Rectangle ViewportBounds
        {
            get { return new TFImaging.Rectangle(0, 0, _control.Width, _control.Height); }
        }

        public TFImaging.Rectangle VisibleBounds
        {
            get
            {
                int scrollH = _control.GetScrollValue(ScrollOrientation.HorizontalScroll);
                int scrollV = _control.GetScrollValue(ScrollOrientation.VerticalScroll);

                return new TFImaging.Rectangle(scrollH + _control.VirtualOriginX, scrollV + _control.VirtualOriginY,
                  (int)Math.Ceiling(Math.Min(_control.Width / _control.Zoom, _control.VirtualWidth)),
                  (int)Math.Ceiling(Math.Min(_control.Height / _control.Zoom, _control.VirtualHeight))
                  );
            }
        }

        public TFImaging.Rectangle CanvasBounds
        {
            get
            {
                return new TFImaging.Rectangle(VirtualOffsetX(), VirtualOffsetY(),
                    (int)Math.Ceiling(Math.Min(_control.Width, _control.VirtualWidth * _control.Zoom)),
                    (int)Math.Ceiling(Math.Min(_control.Height, _control.VirtualHeight * _control.Zoom))
                    );
            }
        }

        public CanvasAlignment CanvasAlignment
        {
            get { return _control.CanvasAlignment; }
        }

        public float ZoomFactor
        {
            get { return _control.Zoom; }
            set { _control.Zoom = value; }
        }

        private int VirtualOffsetX ()
        {
            if (_control.Width > _control.VirtualWidth * _control.Zoom) {
                switch (CanvasAlignment) {
                    case CanvasAlignment.Center:
                    case CanvasAlignment.Upper:
                    case CanvasAlignment.Lower:
                        return (int)(_control.Width - _control.VirtualWidth * _control.Zoom) / 2;
                    case CanvasAlignment.Right:
                    case CanvasAlignment.UpperRight:
                    case CanvasAlignment.LowerRight:
                        return (int)(_control.Width - _control.VirtualWidth * _control.Zoom);
                }
            }

            return 0;
        }

        private int VirtualOffsetY ()
        {
            if (_control.Height > _control.VirtualHeight * _control.Zoom) {
                switch (CanvasAlignment) {
                    case CanvasAlignment.Center:
                    case CanvasAlignment.Left:
                    case CanvasAlignment.Right:
                        return (int)(_control.Height - _control.VirtualHeight * _control.Zoom) / 2;
                    case CanvasAlignment.Lower:
                    case CanvasAlignment.LowerLeft:
                    case CanvasAlignment.LowerRight:
                        return (int)(_control.Height - _control.VirtualHeight * _control.Zoom);
                }
            }

            return 0;
        }
    }
}
