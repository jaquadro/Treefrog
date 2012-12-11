using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Amphibian.Drawing;
using Treefrog.Framework;
using Treefrog.Presentation.Layers;
using Treefrog.Framework.Model;
using Treefrog.Aux;
using System.Collections.Specialized;
using Treefrog.Presentation;

using TFCompat = Treefrog.Framework.Compat;
using Treefrog.Windows.Forms;
using Treefrog.Presentation.Controllers;

namespace Treefrog.Windows.Controls
{
    public enum LayerCondition
    {
        Never,
        Selected,
        Always
    }

    public enum LayerControlAlignment
    {
        Center,
        Left,
        Right,
        Upper,
        Lower,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }

    public class LayerControl : GraphicsDeviceControl, IScrollableControl, IPointerTarget
    {
        #region Fields

        private OrderedResourceCollection<BaseControlLayer> _layers;
        private AnnotationLayer _annotLayer;
        private CanvasLayer _canvasLayer;

        // Graphics and Service

        private SpriteBatch _spriteBatch;
        private IServiceContainer _services;
        private ContentManager _content;

        //private Effect _effect;
        //private EffectParameter _effectTransColor;

        private Dictionary<TilePool, Texture2D> _poolTextureMap = new Dictionary<TilePool, Texture2D>();

        private bool _initialized;

        // Dimensions

        private int _vWidth;
        private int _vHeight;
        private float _zoom = 1f;

        private bool _widthSynced;
        private bool _heightSynced;

        // Drawing

        private Color _backColor = Color.LightGray;

        private LayerControlAlignment _alignment = LayerControlAlignment.Center;

        // Scrolling

        private const int _autoScrollThreshold = 24;
        private const int _autoScrollBase = 2;
        private const int _autoScrollMult = 8;

        private bool _canAutoScroll = false;
        private bool _canScrollH = true;
        private bool _canScrollV = true;

        private Timer _scrollTimer = new Timer();
        private Vector2 _scrollGradient;

        private int _scrollH;
        private int _scrollV;
        private int _scrollSmChangeH;
        private int _scrollSmChangeV;
        private int _scrollLgChangeH;
        private int _scrollLgChangeV;

        // Ordered Events

        #endregion
        
        #region Events

        public event EventHandler<DrawLayerEventArgs> DrawLayerContent;
        public event EventHandler<DrawLayerEventArgs> DrawLayerGrid;
        public event EventHandler<DrawLayerEventArgs> DrawExtra;
        
        public event EventHandler ZoomChanged;

        #endregion
        
        #region Constructors

        public LayerControl ()
            : base()
        {
            ControlInitialized += ControlInitializedHandler;

            _layers = new OrderedResourceCollection<BaseControlLayer>();
        }

        public LayerControl (int vWidth, int vHeight)
            : this()
        {
            _vWidth = vWidth;
            _vHeight = vHeight;
        }

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the control has been initialized yet.
        /// </summary>
        public bool Initialized 
        {
            get { return _initialized; }
        }

        // Graphics and Service

        /// <summary>
        /// Gets the Content Manager for this control.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return _content; }
        }

        // Area

        public LayerControlAlignment Alignment
        {
            get { return _alignment; }
            set { _alignment = value; }
        }

        public bool HeightSynced
        {
            get { return _heightSynced; }
            set
            {
                if (_heightSynced != value) {
                    _heightSynced = value;
                    if (value && _vHeight != Height) {
                        _vHeight = Height;
                        OnVirtualSizeChanged(EventArgs.Empty);
                    }

                    _canScrollV = !value;
                    OnScrollPropertyChanged();
                }
            }
        }

        public bool WidthSynced
        {
            get { return _widthSynced; }
            set
            {
                if (_widthSynced != value) {
                    _widthSynced = value;
                    if (value && _vWidth != Width) {
                        _vWidth = Width;
                        OnVirtualSizeChanged(EventArgs.Empty);
                    }

                    _canScrollH = !value;
                    OnScrollPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the offset of the (scaled) virtual surface within this control.
        /// </summary>
        /// <remarks>If the scaled virtual surface is larger than the control area, the offset is 0.</remarks>
        public Vector2 VirtualSurfaceOffset
        {
            get
            {
                float offsetX = 0;
                float offsetY = 0;

                if (Width > VirtualWidth * _zoom) {
                    switch (_alignment) {
                        case LayerControlAlignment.Center:
                        case LayerControlAlignment.Upper:
                        case LayerControlAlignment.Lower:
                            offsetX = (Width - VirtualWidth * _zoom) / 2;
                            break;
                        case LayerControlAlignment.Right:
                        case LayerControlAlignment.UpperRight:
                        case LayerControlAlignment.LowerRight:
                            offsetX = (Width - VirtualWidth * _zoom);
                            break;
                    }
                }

                if (Height > VirtualHeight * _zoom) {
                    switch (_alignment) {
                        case LayerControlAlignment.Center:
                        case LayerControlAlignment.Left:
                        case LayerControlAlignment.Right:
                            offsetY = (Height - VirtualHeight * _zoom) / 2;
                            break;
                        case LayerControlAlignment.Lower:
                        case LayerControlAlignment.LowerLeft:
                        case LayerControlAlignment.LowerRight:
                            offsetY = (Height - VirtualHeight * _zoom);
                            break;
                    }
                }

                return new Vector2(offsetX, offsetY);
            }
        }

        /// <summary>
        /// Gets or sets the height of the virtual surface at standard (1:1) zoom.
        /// </summary>
        public int VirtualHeight
        {
            get { return _vHeight; }
        }

        /// <summary>
        /// Gets or sets the width of the virtual surface at standard (1:1) zoom.
        /// </summary>
        public int VirtualWidth
        {
            get { return _vWidth; }
        }

        /// <summary>
        /// Gets the bounds of the visible part of the virtual surface at the current zoom setting.
        /// </summary>
        public Rectangle VisibleRegion
        {
            get
            {
                return new Rectangle(_scrollH, _scrollV,
                    (int)Math.Ceiling(Math.Min(Width / _zoom, VirtualWidth)),
                    (int)Math.Ceiling(Math.Min(Height / _zoom, VirtualHeight))
                    );
            }
        }

        

        public IEnumerable<BaseControlLayer> ControlLayers
        {
            get
            {
                return _layers;
            }
        }

        public CanvasLayer CanvasLayer
        {
            get { return _canvasLayer; }
            set { _canvasLayer = value; }
        }

        public AnnotationLayer AnnotationLayer
        {
            get { return _annotLayer; }
            set { _annotLayer = value; }
        }

        // Scrolling

        /// <summary>
        /// Gets or sets a value indicating the control should auto-scroll based on mouse position relative to the control's edge.
        /// </summary>
        [DefaultValue(true)]
        [Description("Indicates whether the control will auto-scroll during qualifying actions")]
        public bool CanAutoScroll
        {
            get { return _canAutoScroll; }
            set
            {
                _canAutoScroll = value;
                if (!_canAutoScroll) {
                    StopAutoScroll();
                }
            }
        }

        #endregion

        #region Event Dispatchers

        protected virtual void OnDrawLayerContent (DrawLayerEventArgs e)
        {
            if (DrawLayerContent != null) {
                DrawLayerContent(this, e);
            }
        }

        protected virtual void OnDrawLayerGrid (DrawLayerEventArgs e)
        {
            if (DrawLayerGrid != null) {
                DrawLayerGrid(this, e);
            }
        }

        protected virtual void OnDrawExtra (DrawLayerEventArgs e)
        {
            if (DrawExtra != null) {
                DrawExtra(this, e);
            }
        }

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

        protected override void OnSizeChanged (EventArgs e)
        {
            base.OnSizeChanged(e);

            if ((_widthSynced && _vWidth != Width) || (_heightSynced && _vHeight != Height)) {
                if (_widthSynced) {
                    _vWidth = Width;
                }
                if (_heightSynced) {
                    _vHeight = Height;
                }

                OnVirtualSizeChanged(EventArgs.Empty);
            }
        }

        private MouseButtons _mouseDown;

        protected override void OnMouseDown (MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _mouseDown |= e.Button;
            if (_mouseDown != MouseButtons.None) {
                CheckStartAutoScroll(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseMove (MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDown != MouseButtons.None) {
                CheckStopAutoScroll(new Point(e.X, e.Y));
                CheckStartAutoScroll(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseUp (MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseDown &= ~e.Button;
            if (_mouseDown == MouseButtons.None) {
                CheckStopAutoScroll(new Point(e.X, e.Y));
            }
        }

        #endregion
        
        #region Event Handlers

        private void ControlInitializedHandler (object sender, EventArgs e)
        {
            _initialized = true;
        }

        private void LayerVirtualSizeChangedHandler (object sender, EventArgs e)
        {
            CalculateVirtualSize();
        }

        #endregion
        
        internal void AddLayer (BaseControlLayer layer)
        {
            _layers.Add(layer);

            layer.VirtualSizeChanged += LayerVirtualSizeChangedHandler;

            CalculateVirtualSize();
            CheckScrollValue();
        }

        internal void RemoveLayer (BaseControlLayer layer)
        {
            _layers.Remove(layer.Name);

            layer.VirtualSizeChanged -= LayerVirtualSizeChangedHandler;

            CalculateVirtualSize();
            CheckScrollValue();
        }

        internal void ChangeLayerOrderRelative (BaseControlLayer layer, int offset)
        {
            _layers.ChangeIndexRelative(layer.Name, offset);
        }

        private void CalculateVirtualSize ()
        {
            int vHeight = 0;
            int vWidth = 0;
            foreach (BaseControlLayer layer in _layers) {
                vHeight = Math.Max(vHeight, layer.VirtualHeight);
                vWidth = Math.Max(vWidth, layer.VirtualWidth);
            }

            if (HeightSynced) {
                vHeight = Height;
            }
            if (WidthSynced) {
                vWidth = Width;
            }

            if (vHeight != _vHeight || vWidth != _vWidth) {
                _vHeight = vHeight;
                _vWidth = vWidth;

                OnVirtualSizeChanged(EventArgs.Empty);
                CheckScrollValue();
            }
        }

        #region GraphicsDeviceControl Members

        /// <summary>
        /// Initializes control to a usable state, called at time of control creation.
        /// </summary>
        protected override void Initialize ()
        {
            _services = new System.ComponentModel.Design.ServiceContainer();
            _services.AddService(typeof(IGraphicsDeviceService), GraphicsDeviceService);
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _content = new ContentManager(_services);
            _content.RootDirectory = "EditorContent";

            //_effect = _content.Load<Effect>("TransColor");
            //_effectTransColor = _effect.Parameters["transColor"];
            //_effectTransColor.SetValue(Color.White.ToVector4());

            Application.Idle += delegate { Invalidate(); };
        }

        protected override void Draw ()
        {
            GraphicsDevice.Clear(_backColor);

            DrawLayerEventArgs e = new DrawLayerEventArgs(_spriteBatch, _zoom);

            if (_canvasLayer != null)
                _canvasLayer.DrawContent(_spriteBatch);

            foreach (BaseControlLayer layer in _layers) {
                layer.DrawContent(_spriteBatch);
            }
            OnDrawLayerContent(e);

            foreach (BaseControlLayer layer in _layers) {
                layer.DrawGrid(_spriteBatch);
            }
            OnDrawLayerGrid(e);

            foreach (BaseControlLayer layer in _layers) {
                layer.DrawExtra(_spriteBatch);
            }
            OnDrawExtra(e);

            if (_annotLayer != null)
                _annotLayer.DrawContent(_spriteBatch);
        }
        
        #endregion
        
        #region Scrolling

        #region IScrollableControl Members

        public event ScrollEventHandler Scroll;

        public event EventHandler VirtualSizeChanged;

        public event EventHandler ScrollPropertyChanged;

        public Control Control
        {
            get { return this; }
        }

        public System.Drawing.Rectangle VirtualSize
        {
            get { return new System.Drawing.Rectangle(0, 0, _vWidth, _vHeight); }
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

        public int GetScrollSmallChange (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _scrollSmChangeH;
                case ScrollOrientation.VerticalScroll:
                    return _scrollSmChangeV;
            }
            return 0;
        }

        public int GetScrollLargeChange (ScrollOrientation orientation)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    return _scrollLgChangeH;
                case ScrollOrientation.VerticalScroll:
                    return _scrollLgChangeV;
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

        #endregion
        
        #region Scroll Extensions

        public void SetScrollSmallChange (ScrollOrientation orientation, int value)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    _scrollSmChangeH = value;
                    break;
                case ScrollOrientation.VerticalScroll:
                    _scrollSmChangeV = value;
                    break;
            }

            OnScrollPropertyChanged();
        }

        public void SetScrollLargeChange (ScrollOrientation orientation, int value)
        {
            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    _scrollLgChangeH = value;
                    break;
                case ScrollOrientation.VerticalScroll:
                    _scrollLgChangeV = value;
                    break;
            }

            OnScrollPropertyChanged();
        }

        #endregion

        #region Scroll Conditions

        private bool ShouldScroll (Point location)
        {
            return location.X < _autoScrollThreshold
                || location.X > Width - _autoScrollThreshold
                || location.Y < _autoScrollThreshold
                || location.Y > Height - _autoScrollThreshold;
        }

        private Vector2 ScrollGradient (Point location)
        {
            Vector2 scroll = new Vector2();
            if (location.X < _autoScrollThreshold) {
                scroll.X = -((_autoScrollThreshold - location.X) / _autoScrollMult) * _autoScrollBase;
            }
            else if (location.X > Width - _autoScrollThreshold) {
                scroll.X = ((location.X - (Width - _autoScrollThreshold)) / _autoScrollMult) * _autoScrollBase;
            }

            if (location.Y < _autoScrollThreshold) {
                scroll.Y = -((_autoScrollThreshold - location.Y) / _autoScrollMult) * _autoScrollBase;
            }
            else if (location.Y > Height - _autoScrollThreshold) {
                scroll.Y = ((location.Y - (Height - _autoScrollThreshold)) / _autoScrollMult) * _autoScrollBase;
            }

            return scroll;
        }

        #endregion

        #region Scroll Control

        private void StartAutoScroll (Point location)
        {
            _scrollGradient = ScrollGradient(location);

            if (_scrollTimer.Interval != 50) {
                _scrollTimer.Interval = 50;
                _scrollTimer.Tick += new System.EventHandler(ScrollTick);
            }
            _scrollTimer.Enabled = true;
        }

        private void StopAutoScroll ()
        {
            _scrollTimer.Enabled = false;
        }

        private void CheckStartAutoScroll (Point location)
        {
            if (!_canAutoScroll) {
                return;
            }

            if (_scrollTimer.Enabled == false) {
                if (ShouldScroll(location)) {
                    StartAutoScroll(location);
                }
            }
            else if (ShouldScroll(location)) {
                _scrollGradient = ScrollGradient(location);
            }
        }

        private void CheckStopAutoScroll (Point location)
        {
            if (!_canAutoScroll) {
                return;
            }

            if (_scrollTimer.Enabled == true) {
                if (!ShouldScroll(location)) {
                    StopAutoScroll();
                }
            }
        }

        #endregion

        #region Event Dispatchers

        protected virtual void OnScroll (ScrollEventArgs e)
        {
            if (Scroll != null) {
                Scroll(this, e);
            }
        }

        protected virtual void OnScrollPropertyChanged ()
        {
            if (ScrollPropertyChanged != null) {
                ScrollPropertyChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        private void ScrollTick (object sender, EventArgs e)
        {
            if (!_canAutoScroll) {
                return;
            }

            int vHeight = VirtualSize.Height;
            int vWidth = VirtualSize.Width;

            int maxH = Math.Max(0, (int)(((vWidth * _zoom) - Width) / _zoom));
            int maxV = Math.Max(0, (int)(((vHeight * _zoom) - Height) / _zoom));

            if (_scrollGradient.X < 0 && _scrollH > 0 && _canScrollH) {
                ScrollTo(ScrollOrientation.HorizontalScroll, Math.Max(0, _scrollH + (int)_scrollGradient.X));
            }
            else if (_scrollGradient.X > 0 && _scrollH < maxH && _canScrollH) {
                ScrollTo(ScrollOrientation.HorizontalScroll, Math.Min(maxH, _scrollH + (int)_scrollGradient.X));
            }

            if (_scrollGradient.Y < 0 && _scrollV > 0 && _canScrollV) {
                ScrollTo(ScrollOrientation.VerticalScroll, Math.Max(0, _scrollV + (int)_scrollGradient.Y));
            }
            else if (_scrollGradient.Y > 0 && _scrollV < maxV && _canScrollV) {
                ScrollTo(ScrollOrientation.VerticalScroll, Math.Min(maxV, _scrollV + (int)_scrollGradient.Y));
            }
        }

        private void ScrollTo (ScrollOrientation orientation, int value)
        {
            int oldVal = 0;

            switch (orientation) {
                case ScrollOrientation.HorizontalScroll:
                    if (!_canScrollH) {
                        return;
                    }

                    oldVal = _scrollH;
                    _scrollH = value;
                    break;

                case ScrollOrientation.VerticalScroll:
                    if (!_canScrollV) {
                        return;
                    }

                    oldVal = _scrollV;
                    _scrollV = value;
                    break;
            }

            OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, oldVal, value, orientation));
        }

        private void CheckScrollValue ()
        {
            int maxH = Math.Max(0, (int)(((_vWidth * _zoom) - Width) / _zoom));
            int maxV = Math.Max(0, (int)(((_vHeight * _zoom) - Height) / _zoom));

            _scrollH = Math.Min(_scrollH, maxH);
            _scrollV = Math.Min(_scrollV, maxV);
        }
        
        #endregion
        
        public BaseControlLayer FindLayer (string name)
        {
            foreach (BaseControlLayer layer in _layers) {
                if (layer.Layer.Name == name) {
                    return layer;
                }
            }

            return null;
        }

        public Brush CreateSolidColorBrush (Color color)
        {
            return new SolidColorBrush(_spriteBatch.GraphicsDevice, color);
        }

        #region IPointerTarget

        public System.Drawing.Point InteriorOffset
        {
            get
            {
                Vector2 offset = VirtualSurfaceOffset;
                return new System.Drawing.Point((int)offset.X, (int)offset.Y);
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

    public class LayerControlViewport : IViewport
    {
        private LayerControl _control;

        public LayerControlViewport (LayerControl control)
        {
            _control = control;
        }

        public System.Drawing.Point Offset
        {
            get
            {
                return new System.Drawing.Point(
                    _control.GetScrollValue(ScrollOrientation.HorizontalScroll),
                    _control.GetScrollValue(ScrollOrientation.VerticalScroll)
                    );
            }
        }

        public System.Drawing.Size Viewport
        {
            get { return new System.Drawing.Size(_control.Width, _control.Height); }
        }

        public System.Drawing.Size Limit
        {
            get { return new System.Drawing.Size(_control.VirtualWidth, _control.VirtualHeight); }
        }

        public float ZoomFactor
        {
            get { return _control.Zoom; }
        }

        public System.Drawing.Rectangle VisibleRegion
        {
            get
            {
                Rectangle vis = _control.VisibleRegion;
                return new System.Drawing.Rectangle(vis.X, vis.Y, vis.Width, vis.Height);
            }
        }
    }

    public class ObjectTextureService : IDisposable
    {
        IGraphicsDeviceService _deviceService;
        ObjectPoolManager _manager;
        Dictionary<int, Texture2D> _objTextureMap = new Dictionary<int, Texture2D>();

        public ObjectTextureService (ObjectPoolManager manager, IGraphicsDeviceService deviceService)
        {
            _manager = manager;
            _deviceService = deviceService;

            Initialize();
        }

        public void Dispose ()
        {
            if (_objTextureMap != null) {
                foreach (Texture2D tex in _objTextureMap.Values)
                    tex.Dispose();
            }

            if (_manager != null) {
                foreach (ObjectPool pool in _manager.Pools)
                    pool.Objects.CollectionChanged -= ObjectCollectionChanged;

                _manager.Pools.CollectionChanged -= ObjectPoolCollectionChanged;
            }

            _objTextureMap = null;
            _manager = null;
        }

        public Texture2D GetTexture (int objClassId)
        {
            Texture2D tex;
            if (_objTextureMap.TryGetValue(objClassId, out tex))
                return tex;
            else
                return null;
        }

        private void Initialize ()
        {
            foreach (ObjectPool pool in _manager.Pools) {
                ObjectPoolAdded(pool);
            }

            _manager.Pools.CollectionChanged += ObjectPoolCollectionChanged;
        }

        private void ObjectPoolCollectionChanged (object sender, TFCompat.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case TFCompat.NotifyCollectionChangedAction.Add:
                    foreach (ObjectPool pool in e.NewItems)
                        ObjectPoolAdded(pool);
                    break;

                case TFCompat.NotifyCollectionChangedAction.Replace:
                    foreach (ObjectPool pool in e.OldItems)
                        ObjectPoolRemoved(pool);
                    foreach (ObjectPool pool in e.NewItems)
                        ObjectPoolAdded(pool);
                    break;

                case TFCompat.NotifyCollectionChangedAction.Remove:
                    foreach (ObjectPool pool in e.OldItems)
                        ObjectPoolRemoved(pool);
                    break;
            }
        }

        private void ObjectPoolAdded (ObjectPool pool)
        {
            if (pool == null)
                return;

            foreach (ObjectClass objClass in pool) {
                if (_objTextureMap.ContainsKey(objClass.Id))
                    _objTextureMap[objClass.Id].Dispose();

                Texture2D tex = objClass.Image.CreateTexture(_deviceService.GraphicsDevice);
                _objTextureMap[objClass.Id] = tex;
            }

            pool.Objects.CollectionChanged += ObjectCollectionChanged;
        }

        private void ObjectPoolRemoved (ObjectPool pool)
        {
            if (pool == null)
                return;

            foreach (ObjectClass objClass in pool) {
                if (_objTextureMap.ContainsKey(objClass.Id))
                    _objTextureMap[objClass.Id].Dispose();

                _objTextureMap.Remove(objClass.Id);
            }

            pool.Objects.CollectionChanged -= ObjectCollectionChanged;
        }

        private void ObjectCollectionChanged (object sender, TFCompat.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case TFCompat.NotifyCollectionChangedAction.Add:
                    foreach (ObjectClass objClass in e.NewItems)
                        ObjectAdded(objClass);
                    break;

                case TFCompat.NotifyCollectionChangedAction.Replace:
                    foreach (ObjectClass objClass in e.OldItems)
                        ObjectRemoved(objClass);
                    foreach (ObjectClass objClass in e.NewItems)
                        ObjectAdded(objClass);
                    break;

                case TFCompat.NotifyCollectionChangedAction.Remove:
                    foreach (ObjectClass objClass in e.OldItems)
                        ObjectRemoved(objClass);
                    break;
            }
        }

        private void ObjectAdded (ObjectClass objClass)
        {
            if (_objTextureMap.ContainsKey(objClass.Id))
                _objTextureMap[objClass.Id].Dispose();

            Texture2D tex = objClass.Image.CreateTexture(_deviceService.GraphicsDevice);
            _objTextureMap[objClass.Id] = tex;
        }

        private void ObjectRemoved (ObjectClass objClass)
        {
            if (_objTextureMap.ContainsKey(objClass.Id))
                _objTextureMap[objClass.Id].Dispose();

            _objTextureMap.Remove(objClass.Id);
        }

        /*private void ObjectModified (object sender, NamedResourceEventArgs<ObjectClass> e)
        {
            ObjectClass objClass = e.Resource;

            if (_objTextureMap.ContainsKey(objClass.Id))
                _objTextureMap[objClass.Id].Dispose();

            Texture2D tex = objClass.Image.CreateTexture(_deviceService.GraphicsDevice);
            _objTextureMap[objClass.Id] = tex;
        }*/
    }

    public class TilePoolTextureService : IDisposable
    {
        IGraphicsDeviceService _deviceService;
        TilePoolManager _manager;
        Dictionary<string, Texture2D> _poolTextureMap = new Dictionary<string, Texture2D>();

        public TilePoolTextureService (TilePoolManager manager, IGraphicsDeviceService deviceService)
        {
            _manager = manager;
            _deviceService = deviceService;

            _manager.Pools.ResourceAdded += TilePoolAdded;
            _manager.Pools.ResourceModified += TilePoolModified;
            _manager.Pools.ResourceRemapped += TilePoolRemapped;
            _manager.Pools.ResourceRemoved += TilePoolRemoved;

            Initialize();
        }

        public void Dispose ()
        {
            if (_poolTextureMap != null) {
                foreach (Texture2D tex in _poolTextureMap.Values)
                    tex.Dispose();
            }

            if (_manager != null) {
                _manager.Pools.ResourceAdded -= TilePoolAdded;
                _manager.Pools.ResourceModified -= TilePoolModified;
                _manager.Pools.ResourceRemapped -= TilePoolRemapped;
                _manager.Pools.ResourceRemoved -= TilePoolRemoved;
            }

            _poolTextureMap = null;
            _manager = null;
        }

        public Texture2D GetTexture (string poolName)
        {
            Texture2D tex;
            if (_poolTextureMap.TryGetValue(poolName, out tex))
                return tex;
            else
                return null;
        }

        private void Initialize ()
        {
            foreach (TilePool pool in _manager.Pools) {
                Texture2D tex = pool.TileSource.CreateTexture(_deviceService.GraphicsDevice);
                _poolTextureMap.Add(pool.Name, tex);
            }
        }

        private void TilePoolAdded (object sender, NamedResourceEventArgs<TilePool> e)
        {
            TilePool pool = e.Resource;

            Texture2D tex = pool.TileSource.CreateTexture(_deviceService.GraphicsDevice);
            _poolTextureMap.Add(pool.Name, tex);
        }

        private void TilePoolRemoved (object sender, NamedResourceEventArgs<TilePool> e)
        {
            if (_poolTextureMap.ContainsKey(e.Resource.Name))
                _poolTextureMap[e.Resource.Name].Dispose();

            _poolTextureMap.Remove(e.Resource.Name);
        }

        private void TilePoolRemapped (object sender, NamedResourceRemappedEventArgs<TilePool> e)
        {
            Texture2D tex = _poolTextureMap[e.OldName];
            _poolTextureMap.Remove(e.OldName);
            _poolTextureMap.Add(e.NewName, tex);
        }

        private void TilePoolModified (object sender, NamedResourceEventArgs<TilePool> e)
        {
            if (_poolTextureMap.ContainsKey(e.Resource.Name))
                _poolTextureMap[e.Resource.Name].Dispose();

            Texture2D tex = e.Resource.TileSource.CreateTexture(_deviceService.GraphicsDevice);
            _poolTextureMap[e.Resource.Name] = tex;
        }
    }
}
