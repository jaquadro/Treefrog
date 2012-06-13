using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using Treefrog.Controls.Layers;
using System.Windows.Markup;
using Treefrog.Controls;
using Treefrog.Controls.Xna;
using Treefrog.ViewModel;
using GalaSoft.MvvmLight.Command;
using Treefrog.Controls.Hooks;

using XColor = Microsoft.Xna.Framework.Color;

namespace Treefrog.View
{
    /// <summary>
    /// Interaction logic for VirtualXnaCanvas.xaml
    /// </summary>
    [ContentProperty("RootLayer")]
    public partial class VirtualXnaCanvas : UserControl, IScrollInfo
    {
        private const double DefaultLineSize = 16;
        private const double DefaultPageSize = DefaultLineSize * 4;
        private const double DefaultWheelSize = DefaultLineSize * 3;

        private double _lineSize = DefaultLineSize;
        private double _pageSize = DefaultPageSize;
        private double _wheelSize = DefaultWheelSize;

        private bool _canHorizontallyScroll = true;
        private bool _canVerticallyScroll = true;
        private ScrollViewer _scrollOwner;
        //private Vector _offset;
        private Size _extent;
        //private Size _viewport;

        private GraphicsDeviceControl _graphicsControl;
        private XnaCanvasLayer _layer;

        private static readonly Color DefaultClearColor = Color.FromRgb(128, 128, 128);

        public static readonly DependencyProperty ActiveProperty;
        public static readonly DependencyProperty ViewportProperty;
        public static readonly DependencyProperty ClearColorProperty;
        public static readonly DependencyProperty ZoomFactorProperty;

        public static readonly DependencyProperty RootLayerProperty;

        static VirtualXnaCanvas ()
        {
            ActiveProperty = DependencyProperty.Register("IsActive",
                typeof(bool), typeof(VirtualXnaCanvas), new PropertyMetadata(false, HandleIsActiveChanged));
            ClearColorProperty = DependencyProperty.Register("ClearColor",
                typeof(Color), typeof(VirtualXnaCanvas), new PropertyMetadata(DefaultClearColor));
            ViewportProperty = DependencyProperty.Register("Viewport",
                typeof(ViewportVM), typeof(VirtualXnaCanvas), new PropertyMetadata(new ViewportVM()));
            ZoomFactorProperty = DependencyProperty.Register("ZoomFactor",
                typeof(double), typeof(VirtualXnaCanvas), new PropertyMetadata(1.0, HandleZoomFactorChanged));

            RootLayerProperty = DependencyProperty.Register("RootLayer",
                typeof(XnaCanvasLayer), typeof(VirtualXnaCanvas), new PropertyMetadata(null, HandleRootLayerChanged));
        }

        public VirtualXnaCanvas ()
        {
            InitializeComponent();
            DataContextChanged += HandleDataContextChanged;
            Loaded += HandleLoaded;
            Unloaded += HandleUnloaded;
        }

        private void HandleLoaded (object sender, RoutedEventArgs e)
        {
            AttachWindowMouseEvents();
            AttachGlobalMouseEvents();
        }

        private void HandleUnloaded (object sender, RoutedEventArgs e)
        {
            DetachWindowMouseEvents();
            DetachGlobalMouseEvents();

            if (RootLayer != null)
                RootLayer.Dispose();
        }

        public XnaCanvasLayer RootLayer
        {
            get { return (XnaCanvasLayer)this.GetValue(RootLayerProperty); }
            set { this.SetValue(RootLayerProperty, value); }
        }

        private static void HandleRootLayerChanged (object sender, DependencyPropertyChangedEventArgs e)
        {
            VirtualXnaCanvas self = sender as VirtualXnaCanvas;
            if (self != null) {
                self._layer = e.NewValue as XnaCanvasLayer; 
                self._layer.GraphicsDeviceControl = self._graphicsControl;
                self._layer.DataContext = self.FindDataContext();

                self.AddChild(self._layer);
                self.InvalidateMeasure();
            }
        }

        private void HandleDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_layer != null)
                _layer.DataContext = e.NewValue;
            InvalidateMeasure();
            PropagateViewportData();
        }

        private static void HandleIsActiveChanged (object sender, DependencyPropertyChangedEventArgs e)
        {
            VirtualXnaCanvas self = sender as VirtualXnaCanvas;
            if (self != null && self._graphicsControl != null) {
                bool val = (bool)e.NewValue;

                if (val)
                    self._graphicsControl.Activate();
                else
                    self._graphicsControl.Deactivate();
            }
        }

        public bool IsActive
        {
            get { return (bool)this.GetValue(ActiveProperty); }
            set { this.SetValue(ActiveProperty, value); }
        }

        public Color ClearColor
        {
            get { return (Color)this.GetValue(ClearColorProperty); }
            set { this.SetValue(ClearColorProperty, value); }
        }

        public double ZoomFactor
        {
            get { return (double)this.GetValue(ZoomFactorProperty); }
            set { this.SetValue(ZoomFactorProperty, value); }
        }

        private static void HandleZoomFactorChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            VirtualXnaCanvas self = sender as VirtualXnaCanvas;
            if (self != null && self._layer != null) {
                self._layer.ZoomFactor = self.ZoomFactor;
            }
            self.InvalidateMeasure();
        }

        public ViewportVM Viewport
        {
            get { return (ViewportVM)this.GetValue(ViewportProperty); }
            set { this.SetValue(ViewportProperty, value); }
        }

        private void GraphicsDeviceControl_RenderXna (object sender, GraphicsDeviceEventArgs e)
        {
            Color color = ClearColor;
            e.GraphicsDevice.Clear(new XColor(color.R / 255f, color.G / 255f, color.B / 255f));
        }

        #region IScrollInfo Members

        public bool CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set { _canHorizontallyScroll = value; }
        }

        public bool CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set { _canVerticallyScroll = value; }
        }

        public ScrollViewer ScrollOwner
        {
            get { return _scrollOwner; }
            set { _scrollOwner = value; }
        }

        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        public double HorizontalOffset
        {
            get { return Viewport.Offset.X; }
        }

        public double VerticalOffset
        {
            get { return Viewport.Offset.Y; }
        }

        public double ViewportHeight
        {
            get { return Viewport.Viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return Viewport.Viewport.Width; }
        }

        public void LineDown ()
        {
            SetVerticalOffset(VerticalOffset + _lineSize);
        }

        public void LineLeft ()
        {
            SetHorizontalOffset(HorizontalOffset - _lineSize);
        }

        public void LineRight ()
        {
            SetHorizontalOffset(HorizontalOffset + _lineSize);
        }

        public void LineUp ()
        {
            SetVerticalOffset(VerticalOffset - _lineSize);
        }

        public void MouseWheelDown ()
        {
            SetVerticalOffset(VerticalOffset + _wheelSize);
        }

        public void MouseWheelLeft ()
        {
            SetHorizontalOffset(HorizontalOffset - _wheelSize);
        }

        public void MouseWheelRight ()
        {
            SetHorizontalOffset(HorizontalOffset + _wheelSize);
        }

        public void MouseWheelUp ()
        {
            SetVerticalOffset(VerticalOffset - _wheelSize);
        }

        public void PageDown ()
        {
            SetVerticalOffset(VerticalOffset + _pageSize);
        }

        public void PageLeft ()
        {
            SetHorizontalOffset(HorizontalOffset - _pageSize);
        }

        public void PageRight ()
        {
            SetHorizontalOffset(HorizontalOffset + _pageSize);
        }

        public void PageUp ()
        {
            SetVerticalOffset(VerticalOffset - _pageSize);
        }

        public Rect MakeVisible (Visual visual, Rect rectangle)
        {
            return rectangle;
        }

        public void SetHorizontalOffset (double offset)
        {
            offset = Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));
            if (offset != Viewport.Offset.X) {
                Viewport.Offset = new Vector(offset, Viewport.Offset.Y);
                InvalidateArrange();

                if (_layer != null)
                    _layer.HorizontalOffset = Viewport.Offset.X;
            }
        }

        public void SetVerticalOffset (double offset)
        {
            offset = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
            if (offset != Viewport.Offset.Y) {
                Viewport.Offset = new Vector(Viewport.Offset.X, offset);
                InvalidateArrange();

                if (_layer != null)
                    _layer.VerticalOffset = Viewport.Offset.Y;
            }
        }

        #endregion

        protected override Size MeasureOverride (Size constraint)
        {
            Size extent = new Size(0, 0);
            if (_layer != null) {
                extent = new Size(_layer.VirtualWidth, _layer.VirtualHeight);
                extent.Width = Math.Max(0, extent.Width + (ViewportWidth * ZoomFactor - ViewportWidth) / ZoomFactor);
                extent.Height = Math.Max(0, extent.Height + (ViewportHeight * ZoomFactor - ViewportHeight) / ZoomFactor);
            }

            if (extent != _extent) {
                _extent = extent;
                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }

            if (constraint != Viewport.Viewport) {
                Viewport.Viewport = constraint;
                PropagateViewportData();

                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }

            SetHorizontalOffset(HorizontalOffset);
            SetVerticalOffset(VerticalOffset);

            return constraint;
        }

        private void PropagateViewportData ()
        {
            if (_layer != null) {
                _layer.ViewportWidth = 0;
                _layer.ViewportHeight = 0;
                _layer.ViewportWidth = Viewport.Viewport.Width;
                _layer.ViewportHeight = Viewport.Viewport.Height;
            }
        }

        private void graphicsDeviceControl1_Loaded (object sender, RoutedEventArgs e)
        {
            GraphicsDeviceControl control = sender as GraphicsDeviceControl;
            if (control != null) {
                _graphicsControl = control;
                _graphicsControl.RenderXna += GraphicsDeviceControl_RenderXna;

                _layer.GraphicsDeviceControl = control;
            }
        }

        private object FindDataContext ()
        {
            DependencyObject elem = this;
            while (elem != null) {
                FrameworkElement frameElem = elem as FrameworkElement;
                if (frameElem != null && frameElem.DataContext != null)
                    return frameElem.DataContext;

                if (frameElem != null && frameElem.Parent != null)
                    elem = frameElem.Parent;
                else
                    elem = VisualTreeHelper.GetParent(elem);
            }

            return null;
        }

        private static DependencyObject GetParent (DependencyObject obj)
        {
            if (obj == null)
                return null;

            ContentElement ce = obj as ContentElement;
            if (ce != null) {
                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;

                FrameworkContentElement fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            return VisualTreeHelper.GetParent(obj);
        }

        private static T FindAncestorOrSelf<T> (DependencyObject obj)
            where T : DependencyObject
        {
            while (obj != null) {
                T objTest = obj as T;
                if (objTest != null)
                    return objTest;
                obj = GetParent(obj);
            }

            return null;
        }

        private Vector VirtualSurfaceOffset
        {
            get
            {
                double offsetX = 0;
                double offsetY = 0;

                LayerControlAlignment Alignment = LayerControlAlignment.Center;

                if (ViewportWidth > _layer.VirtualWidth * ZoomFactor) {
                    switch (Alignment) {
                        case LayerControlAlignment.Center:
                        case LayerControlAlignment.Upper:
                        case LayerControlAlignment.Lower:
                            offsetX = (ViewportWidth - _layer.VirtualWidth * ZoomFactor) / 2;
                            break;
                        case LayerControlAlignment.Right:
                        case LayerControlAlignment.UpperRight:
                        case LayerControlAlignment.LowerRight:
                            offsetX = (ViewportWidth - _layer.VirtualWidth * ZoomFactor);
                            break;
                    }
                }

                if (ViewportHeight > _layer.VirtualHeight * ZoomFactor) {
                    switch (Alignment) {
                        case LayerControlAlignment.Center:
                        case LayerControlAlignment.Left:
                        case LayerControlAlignment.Right:
                            offsetY = (ViewportHeight - _layer.VirtualHeight * ZoomFactor) / 2;
                            break;
                        case LayerControlAlignment.Lower:
                        case LayerControlAlignment.LowerLeft:
                        case LayerControlAlignment.LowerRight:
                            offsetY = (ViewportHeight - _layer.VirtualHeight * ZoomFactor);
                            break;
                    }
                }

                return new Vector(offsetX, offsetY);
            }
        }

        #region Pointer Device Handling

        #region Global Mouse Handling

        private Point TranslateScreenMousePosition (Point screenPosition)
        {
            Point locationFromScreen = this.PointToScreen(new Point(0, 0));
            Point mouseLocation = new Point(screenPosition.X - locationFromScreen.X, screenPosition.Y - locationFromScreen.Y);

            return mouseLocation;
        }

        private void AttachGlobalMouseEvents ()
        {
            MouseHook.MouseUp += HandleGlobalMouseUp;
        }

        private void DetachGlobalMouseEvents ()
        {
            MouseHook.MouseDown -= HandleGlobalMouseUp;
        }

        private void HandleGlobalMouseUp (object sender, SystemMouseButtonEventArgs e)
        {
            if (!this.IsLoaded || PresentationSource.FromVisual(this) == null)
                return;

            Point mouseLocation = TranslateScreenMousePosition(e.ScreenLocation);

            switch (e.ChangedButton) {
                case MouseButton.Left:
                    HandleButtonUp(mouseLocation, PointerEventType.Primary);
                    break;
                case MouseButton.Right:
                    HandleButtonUp(mouseLocation, PointerEventType.Secondary);
                    break;
            }
        }

        #endregion

        #region Window Mouse Handling

        private void AttachWindowMouseEvents ()
        {
            Window window = Window.GetWindow(this);
            if (window != null) {
                window.MouseUp += HandleWindowMouseUp;
            }
        }

        private void DetachWindowMouseEvents ()
        {
            Window window = Window.GetWindow(this);
            if (window != null) {
                window.MouseUp -= HandleWindowMouseUp;
            }
        }

        private void HandleWindowMouseUp (object sender, MouseEventArgs e)
        {
            
        }

        private void HandleWindowMouseDown (object sender, MouseEventArgs e)
        {

        }

        private void HandleWindowMouseMove (object sender, MouseEventArgs e)
        {

        }

        private void HandleWindowMouseEnter (object sender, MouseEventArgs e)
        {

        }

        private void HandleWindowMouseLeave (object sender, MouseEventArgs e)
        {
            //MessageBox.Show(TranslateMousePosition(e.GetPosition(this)).ToString());
        }

        #endregion

        #region Canvas Mouse Handling

        private void HandleHwndLButtonDown (object sender, HwndMouseEventArgs e)
        {
            HandleButtonDown(e, PointerEventType.Primary);
        }

        private void HandleHwndRButtonDown (object sender, HwndMouseEventArgs e)
        {
            HandleButtonDown(e, PointerEventType.Secondary);
        }

        private void HandleHwndLButtonUp (object sender, HwndMouseEventArgs e)
        {
            HandleButtonUp(e.Position, PointerEventType.Primary);
        }

        private void HandleHwndRButtonUp (object sender, HwndMouseEventArgs e)
        {
            HandleButtonUp(e.Position, PointerEventType.Secondary);
        }

        private void HandleHwndMouseMove (object sender, HwndMouseEventArgs e)
        {
            HandleMouseMove(e.Position);
        }

        private void HandleHwndMouseEnter (object sender, HwndMouseEventArgs e)
        {
            HandleMouseEnter(e.Position);
        }

        private void HandleHwndMouseLeave (object sender, HwndMouseEventArgs e)
        {
            HandleMouseLeave(e.Position);
        }

        #endregion

        #region Mouse Handler Implementation

        private Dictionary<PointerEventType, bool> _sequenceOpen = new Dictionary<PointerEventType, bool>
        {
            { PointerEventType.Primary, false },
            { PointerEventType.Secondary, false },
        };

        private Point TranslateMousePosition (Point position)
        {
            Vector offset = VirtualSurfaceOffset;
            position.X = (position.X - offset.X) / ZoomFactor;
            position.Y = (position.Y - offset.Y) / ZoomFactor;

            position.X += Viewport.Offset.X;
            position.Y += Viewport.Offset.Y;

            return position;
        }

        private void HandleButtonDown (HwndMouseEventArgs e, PointerEventType type)
        {
            if (type == PointerEventType.None)
                return;

            Point position = TranslateMousePosition(e.Position);
            PointerEventInfo info = new PointerEventInfo(type, position.X, position.Y);

            // Ignore event if a sequence is active
            if (_sequenceOpen.Count(kv => { return kv.Value; }) == 0) {
                _sequenceOpen[info.Type] = true;
                if (StartPointerSequence != null)
                    StartPointerSequence.Execute(info);
            }
        }

        private void HandleButtonUp (Point mousePosition, PointerEventType type)
        {
            if (type == PointerEventType.None)
                return;

            Point position = TranslateMousePosition(mousePosition);
            PointerEventInfo info = new PointerEventInfo(type, position.X, position.Y);

            if (_sequenceOpen[info.Type]) {
                _sequenceOpen[info.Type] = false;
                if (EndPointerSequence != null)
                    EndPointerSequence.Execute(info);
            }
        }

        private void HandleMouseMove (Point mousePosition)
        {
            Point position = TranslateMousePosition(mousePosition);
            PointerEventInfo info = new PointerEventInfo(PointerEventType.None, position.X, position.Y);

            if (UpdatePointerSequence != null) {
                if (_sequenceOpen[PointerEventType.Primary])
                    UpdatePointerSequence.Execute(new PointerEventInfo(PointerEventType.Primary, position.X, position.Y));
                if (_sequenceOpen[PointerEventType.Secondary])
                    UpdatePointerSequence.Execute(new PointerEventInfo(PointerEventType.Secondary, position.X, position.Y));
            }

            if (PointerPosition != null)
                PointerPosition.Execute(new PointerEventInfo(PointerEventType.None, position.X, position.Y));
        }

        private void HandleMouseEnter (Point mousePosition)
        {
            Point position = TranslateMousePosition(mousePosition);

            /*if (EndPointerSequence != null) {
                if (_sequenceOpen[PointerEventType.Primary] && e.LeftButton == MouseButtonState.Released) {
                    _sequenceOpen[PointerEventType.Primary] = false;
                    EndPointerSequence.Execute(new PointerEventInfo(PointerEventType.Primary, position.X, position.Y));
                }
                if (_sequenceOpen[PointerEventType.Secondary] && e.RightButton == MouseButtonState.Released) {
                    _sequenceOpen[PointerEventType.Secondary] = false;
                    EndPointerSequence.Execute(new PointerEventInfo(PointerEventType.Secondary, position.X, position.Y));
                }
            }*/
        }

        private void HandleMouseLeave (Point mousePosition)
        {
            if (PointerLeaveField != null)
                PointerLeaveField.Execute(null);
        }

        #endregion

        #region Pointer Event Interface

        public static readonly DependencyProperty StartPointerSequenceProperty = DependencyProperty.Register(
            "StartPointerSequence", typeof(RelayCommand<PointerEventInfo>), typeof(VirtualXnaCanvas),
            new FrameworkPropertyMetadata(default(RelayCommand<PointerEventInfo>)));

        public RelayCommand<PointerEventInfo> StartPointerSequence
        {
            get { return (RelayCommand<PointerEventInfo>)GetValue(StartPointerSequenceProperty); }
            set { SetValue(StartPointerSequenceProperty, value); }
        }

        public static readonly DependencyProperty EndPointerSequenceProperty = DependencyProperty.Register(
            "EndPointerSequence", typeof(RelayCommand<PointerEventInfo>), typeof(VirtualXnaCanvas),
            new FrameworkPropertyMetadata(default(RelayCommand<PointerEventInfo>)));

        public RelayCommand<PointerEventInfo> EndPointerSequence
        {
            get { return (RelayCommand<PointerEventInfo>)GetValue(EndPointerSequenceProperty); }
            set { SetValue(EndPointerSequenceProperty, value); }
        }

        public static readonly DependencyProperty UpdatePointerSequenceProperty = DependencyProperty.Register(
            "UpdatePointerSequence", typeof(RelayCommand<PointerEventInfo>), typeof(VirtualXnaCanvas),
            new FrameworkPropertyMetadata(default(RelayCommand<PointerEventInfo>)));

        public RelayCommand<PointerEventInfo> UpdatePointerSequence
        {
            get { return (RelayCommand<PointerEventInfo>)GetValue(UpdatePointerSequenceProperty); }
            set { SetValue(UpdatePointerSequenceProperty, value); }
        }

        public static readonly DependencyProperty PointerPositionProperty = DependencyProperty.Register(
            "PointerPosition", typeof(RelayCommand<PointerEventInfo>), typeof(VirtualXnaCanvas),
            new FrameworkPropertyMetadata(default(RelayCommand<PointerEventInfo>)));

        public RelayCommand<PointerEventInfo> PointerPosition
        {
            get { return (RelayCommand<PointerEventInfo>)GetValue(PointerPositionProperty); }
            set { SetValue(PointerPositionProperty, value); }
        }

        public static readonly DependencyProperty PointerLeaveFieldProperty = DependencyProperty.Register(
            "PointerLeaveField", typeof(RelayCommand), typeof(VirtualXnaCanvas),
            new FrameworkPropertyMetadata(default(RelayCommand)));

        public RelayCommand PointerLeaveField
        {
            get { return (RelayCommand)GetValue(PointerLeaveFieldProperty); }
            set { SetValue(PointerLeaveFieldProperty, value); }
        }

        #endregion

        #endregion
    }
}
