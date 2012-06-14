using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Treefrog.Controls.Xna;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Controls.Layers
{
    public class XnaCanvasLayer : FrameworkElement, IDisposable // DependencyObject
    {
        public XnaCanvasLayer ()
        {
            ZoomFactor = 1.0;
            this.Unloaded += HandleUnloaded;
        }

        private bool _disposed = false;

        public void Dispose ()
        {
            if (!_disposed) {
                DisposeManaged();
                DisposeUnmanaged();

                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        ~XnaCanvasLayer ()
        {
            DisposeUnmanaged();
        }

        protected virtual void DisposeManaged ()
        {
            UnloadControl();
        }

        protected virtual void DisposeUnmanaged ()
        {
        }

        private void HandleUnloaded (object sender, EventArgs e)
        {
            UnloadControl();
        }

        private void UnloadControl ()
        {
            GraphicsDeviceControl control = this.GraphicsDeviceControl;
            if (control != null) {
                GraphicsDeviceControl = null;
            }
        }

        #region Dependency Properties

        public static readonly DependencyProperty GraphicsDeviceControlProperty;
        public static readonly DependencyProperty IsRenderedProperty;        

        public static readonly DependencyProperty ViewportHeightProperty;
        public static readonly DependencyProperty ViewportWidthProperty;

        static XnaCanvasLayer ()
        {
            GraphicsDeviceControlProperty = DependencyProperty.Register("GraphicsDeviceControl", 
                typeof(GraphicsDeviceControl), typeof(XnaCanvasLayer));
            IsRenderedProperty = DependencyProperty.Register("IsRendered",
                typeof(bool), typeof(XnaCanvasLayer), new PropertyMetadata(true));

            ViewportHeightProperty = DependencyProperty.Register("ViewportHeight",
                typeof(double), typeof(XnaCanvasLayer));
            ViewportWidthProperty = DependencyProperty.Register("ViewportWidth",
                typeof(double), typeof(XnaCanvasLayer));
        }

        #region ZoomFactor Property

        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register("ZoomFactor", 
            typeof(double), typeof(XnaCanvasLayer), new PropertyMetadata(1.0, OnZoomFactorChanged));

        private static void OnZoomFactorChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            XnaCanvasLayer obj = sender as XnaCanvasLayer;
            if (obj != null)
                obj.OnZoomFactorChanged(e);
        }

        protected virtual void OnZoomFactorChanged (DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Horizontal Offset Property

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset",
            typeof(double), typeof(XnaCanvasLayer), new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

        private static void OnHorizontalOffsetChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            XnaCanvasLayer obj = sender as XnaCanvasLayer;
            if (obj != null)
                obj.OnHorizontalOffsetChanged(e);
        }

        protected virtual void OnHorizontalOffsetChanged (DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region Vertical Offset Property

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
            typeof(double), typeof(XnaCanvasLayer), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            XnaCanvasLayer obj = sender as XnaCanvasLayer;
            if (obj != null)
                obj.OnVerticalOffsetChanged(e);
        }

        protected virtual void OnVerticalOffsetChanged (DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #endregion

        #region Dependency Properties

        public GraphicsDeviceControl GraphicsDeviceControl
        {
            get { return this.GetValue(GraphicsDeviceControlProperty) as GraphicsDeviceControl; }
            set
            {
                GraphicsDeviceControl control = this.GraphicsDeviceControl;
                if (control == value)
                    return;

                if (control != null) {
                    control.LoadContent -= GraphicsDeviceControl_LoadContent;
                    control.RenderXna -= GraphicsDeviceControl_RenderXna;
                }

                this.SetValue(GraphicsDeviceControlProperty, value);
                control = value;

                if (control != null) {
                    control.LoadContent += GraphicsDeviceControl_LoadContent;
                    control.RenderXna += GraphicsDeviceControl_RenderXna;
                }
            }
        }

        public bool IsRendered
        {
            get { return (bool)this.GetValue(IsRenderedProperty); }
            set { this.SetValue(IsRenderedProperty, value); }
        }

        public double HorizontalOffset
        {
            get { return (double)this.GetValue(HorizontalOffsetProperty); }
            set { this.SetValue(HorizontalOffsetProperty, value); }
        }

        public double VerticalOffset
        {
            get { return (double)this.GetValue(VerticalOffsetProperty); }
            set { this.SetValue(VerticalOffsetProperty, value); }
        }

        public double ZoomFactor
        {
            get { return (double)this.GetValue(ZoomFactorProperty); }
            set { this.SetValue(ZoomFactorProperty, value); }
        }

        public double ViewportHeight
        {
            get { return (double)this.GetValue(ViewportHeightProperty); }
            set { this.SetValue(ViewportHeightProperty, value); }
        }

        public double ViewportWidth
        {
            get { return (double)this.GetValue(ViewportWidthProperty); }
            set { this.SetValue(ViewportWidthProperty, value); }
        }

        #endregion

        #region Properties

        public virtual double VirtualHeight
        {
            get { return double.IsNaN(Height) ? 0 : Height; }
        }

        public virtual double VirtualWidth
        {
            get { return double.IsNaN(Width) ? 0 : Width; }
        }

        #endregion

        private bool _isLoaded;

        private void GraphicsDeviceControl_LoadContent (object sender, GraphicsDeviceEventArgs e)
        {
            Load(e.GraphicsDevice);
        }

        private void GraphicsDeviceControl_RenderXna (object sender, GraphicsDeviceEventArgs e)
        {
            Render(e.GraphicsDevice);
        }

        public void Load (GraphicsDevice device)
        {
            LoadCore(device);
            _isLoaded = true;
        }

        public void Render (GraphicsDevice device)
        {
            if (!_isLoaded)
                Load(device);

            if (IsRendered)
                RenderCore(device);
        }

        protected virtual void LoadCore (GraphicsDevice device)
        {

        }

        protected virtual void RenderCore (GraphicsDevice device)
        {
            
        }
    }
}
