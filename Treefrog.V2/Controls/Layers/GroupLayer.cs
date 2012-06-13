using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using Treefrog.ViewModel.Layers;
using System.Collections.Specialized;
using System.Collections;
using Treefrog.ViewModel.Utility;

namespace Treefrog.Controls.Layers
{
    [ContentProperty("Layers")]
    public class GroupLayer : XnaCanvasLayer
    {
        public static readonly DependencyProperty LayersProperty;

        //private ObservableCollection<XnaCanvasLayer> _layers = new ObservableCollection<XnaCanvasLayer>();

        private ObservableCollectionAdapter<LayerVM, XnaCanvasLayer> _adapter = new ObservableCollectionAdapter<LayerVM, XnaCanvasLayer>(layer =>
        {
            if (layer is RenderLayerVM)
                return new RenderLayer() { Model = (RenderLayerVM)layer };
            else if (layer is GroupLayerVM)
                return new GroupLayer() { Model = (GroupLayerVM)layer };
            return null;
        });

        public static readonly DependencyProperty ClearColorProperty;

        public static readonly DependencyProperty ModelProperty;

        static GroupLayer ()
        {
            //LayersProperty = DependencyProperty.Register("LayersSource",
            //    typeof(IEnumerable<LayerVM>), typeof(GroupLayer), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(GroupLayer.OnLayersSourceChanged)));
            ClearColorProperty = DependencyProperty.Register("ClearColor",
                typeof(Color), typeof(GroupLayer));
            ModelProperty = DependencyProperty.Register("Model",
                typeof(GroupLayerVM), typeof(GroupLayer), new PropertyMetadata(null, new PropertyChangedCallback(GroupLayer.OnModelChanged)));
        }

        public GroupLayer ()
        {
            _adapter.Dependent.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add) {
                    XnaCanvasLayer layer = e.NewItems[0] as XnaCanvasLayer;
                    AddLogicalChild(layer);

                    layer.HorizontalOffset = HorizontalOffset;
                    layer.VerticalOffset = VerticalOffset;
                    layer.ZoomFactor = ZoomFactor;
                    layer.ViewportWidth = ViewportWidth;
                    layer.ViewportHeight = ViewportHeight;
                }
            };
        }

        protected override void DisposeManaged ()
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.Dispose();
            }

            base.DisposeManaged();
        }

        public Color ClearColor
        {
            get { return (Color)this.GetValue(ClearColorProperty); }
            set { this.SetValue(ClearColorProperty, value); }
        }

        /*[Bindable(true)]
        public IEnumerable<LayerVM> LayersSource
        {
            get { return this.GetValue(LayersProperty) as IEnumerable<LayerVM>; }
            set
            {
                if (value == null) {
                    this.ClearValue(LayersProperty);
                }
                else {
                    this.SetValue(LayersProperty, value);
                }
            }
            //get { return this.GetValue(LayersProperty) as IEnumerable<XnaCanvasLayer>; }
            //set { this.SetValue(LayersProperty, value); }
        }*/

        public GroupLayerVM Model
        {
            get { return (GroupLayerVM)this.GetValue(ModelProperty); }
            set { this.SetValue(ModelProperty, value); }
        }

        private static void OnModelChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GroupLayer control = d as GroupLayer;
            GroupLayerVM oldModel = e.OldValue as GroupLayerVM;
            GroupLayerVM newModel = e.NewValue as GroupLayerVM;

            if (control != null) {
                control._adapter.Primary = (newModel != null) ? newModel.Layers : null;
            }
        }

        [Bindable(true)]
        public ObservableCollection<XnaCanvasLayer> Layers
        {
            get { return _adapter.Dependent; }
        }

        protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == HorizontalOffsetProperty)
                CascadeHorizontalOffset();
            else if (e.Property == VerticalOffsetProperty)
                CascadeVerticalOffset();
            else if (e.Property == ZoomFactorProperty)
                CascadeZoomFactor();
            else if (e.Property == ViewportWidthProperty)
                CascadeViewportWidth();
            else if (e.Property == ViewportHeightProperty)
                CascadeViewportHeight();

            base.OnPropertyChanged(e);
        }

        private void CascadeHorizontalOffset ()
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.HorizontalOffset = HorizontalOffset;
            }
        }

        private void CascadeVerticalOffset ()
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.VerticalOffset = VerticalOffset;
            }
        }

        private void CascadeZoomFactor ()
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.ZoomFactor = ZoomFactor;
            }
        }

        private void CascadeViewportWidth ()
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.ViewportWidth = ViewportWidth;
            }
        }

        private void CascadeViewportHeight ()
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.ViewportHeight = ViewportHeight;
            }
        }

        #region Properties

        public override double VirtualHeight
        {
            get
            {
                if (!double.IsNaN(Height))
                    return Height;

                double maxHeight = 0;
                foreach (XnaCanvasLayer layer in Layers) {
                    maxHeight = Math.Max(maxHeight, layer.VirtualHeight);
                }
                return maxHeight;
            }
        }

        public override double VirtualWidth
        {
            get
            {
                if (!double.IsNaN(Width))
                    return Width;

                double maxWidth = 0;
                foreach (XnaCanvasLayer layer in Layers) {
                    maxWidth = Math.Max(maxWidth, layer.VirtualWidth);
                }
                return maxWidth;
            }
        }

        #endregion

        protected override void LoadCore (GraphicsDevice device)
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.Load(device);
            }
        }

        protected override void RenderCore (GraphicsDevice device)
        {
            foreach (XnaCanvasLayer layer in Layers) {
                layer.Render(device);
            }
        }
    }
}
