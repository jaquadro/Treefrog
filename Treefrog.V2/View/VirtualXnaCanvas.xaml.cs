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
using Treefrog.V2.Controls.Layers;
using System.Windows.Markup;
using Treefrog.V2.Controls;
using Treefrog.V2.Controls.Xna;

using XColor = Microsoft.Xna.Framework.Color;

namespace Treefrog.V2.View
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
        private Vector _offset;
        private Size _extent;
        private Size _viewport;

        private GraphicsDeviceControl _graphicsControl;
        private XnaCanvasLayer _layer;

        private static readonly Color DefaultClearColor = Color.FromRgb(128, 128, 128);

        public static readonly DependencyProperty ClearColorProperty;

        static VirtualXnaCanvas ()
        {
            ClearColorProperty = DependencyProperty.Register("ClearColor",
                typeof(Color), typeof(VirtualXnaCanvas), new PropertyMetadata(DefaultClearColor));
        }

        public VirtualXnaCanvas ()
        {
            InitializeComponent();
        }

        public XnaCanvasLayer RootLayer
        {
            get { return _layer; }
            set { _layer = value; _layer.GraphicsDeviceControl = _graphicsControl;  }
        }

        public Color ClearColor
        {
            get { return (Color)this.GetValue(ClearColorProperty); }
            set { this.SetValue(ClearColorProperty, value); }
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
            get { return _offset.X; }
        }

        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewport.Width; }
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
            if (offset != _offset.X) {
                _offset.X = offset;
                InvalidateArrange();

                if (_layer != null)
                    _layer.HorizontalOffset = _offset.X;
            }
        }

        public void SetVerticalOffset (double offset)
        {
            offset = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
            if (offset != _offset.Y) {
                _offset.Y = offset;
                InvalidateArrange();

                if (_layer != null)
                    _layer.VerticalOffset = _offset.Y;
            }
        }

        #endregion

        protected override Size MeasureOverride (Size constraint)
        {
            Size extent = new Size(0, 0);
            if (_layer != null)
                extent = new Size(_layer.VirtualWidth, _layer.VirtualHeight);

            if (extent != _extent) {
                _extent = extent;
                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }

            if (constraint != _viewport) {
                _viewport = constraint;
                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }

            return constraint;
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
    }
}
