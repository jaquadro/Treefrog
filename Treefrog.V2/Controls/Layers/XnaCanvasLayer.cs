using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Treefrog.V2.Controls.Xna;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.V2.Controls.Layers
{
    public class XnaCanvasLayer : DependencyObject
    {
        public XnaCanvasLayer ()
        {
            ZoomFactor = 1.0;
        }

        #region Dependency Property Registry

        public static readonly DependencyProperty GraphicsDeviceControlProperty;
        public static readonly DependencyProperty IsRenderedProperty;
        public static readonly DependencyProperty HorizontalOffsetProperty;
        public static readonly DependencyProperty VerticalOffsetProperty;
        public static readonly DependencyProperty ZoomFactorProperty;

        public static readonly DependencyProperty ViewportHeightProperty;
        public static readonly DependencyProperty ViewportWidthProperty;
        //public static readonly DependencyProperty VirtualHeightProperty;
        //public static readonly DependencyProperty VirtualWidthProperty;

        //internal static readonly DependencyPropertyKey VirtualHeightKey;
        //internal static readonly DependencyPropertyKey VirtualWidthKey;

        static XnaCanvasLayer ()
        {
            GraphicsDeviceControlProperty = DependencyProperty.Register("GraphicsDeviceControl", 
                typeof(GraphicsDeviceControl), typeof(XnaCanvasLayer));
            IsRenderedProperty = DependencyProperty.Register("IsRendered",
                typeof(bool), typeof(XnaCanvasLayer), new PropertyMetadata(true));

            HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset",
                typeof(double), typeof(XnaCanvasLayer));
            VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
                typeof(double), typeof(XnaCanvasLayer));
            ZoomFactorProperty = DependencyProperty.Register("ZoomFactor",
                typeof(double), typeof(XnaCanvasLayer));

            ViewportHeightProperty = DependencyProperty.Register("ViewportHeight",
                typeof(double), typeof(XnaCanvasLayer));
            ViewportWidthProperty = DependencyProperty.Register("ViewportWidth",
                typeof(double), typeof(XnaCanvasLayer));

            //VirtualHeightKey = DependencyProperty.RegisterReadOnly("VirtualHeight",
            //    typeof(double), typeof(XnaCanvasLayer), new PropertyMetadata();

        }

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
            get { return 0; }
        }

        public virtual double VirtualWidth
        {
            get { return 0; }
        }

        #endregion

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
        }

        public void Render (GraphicsDevice device)
        {
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
