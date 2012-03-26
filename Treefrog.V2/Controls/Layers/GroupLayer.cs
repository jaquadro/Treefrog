using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;

namespace Treefrog.V2.Controls.Layers
{
    [ContentProperty("Layers")]
    public class GroupLayer : XnaCanvasLayer
    {
        public static readonly DependencyProperty LayersProperty;

        private ObservableCollection<XnaCanvasLayer> _layers = new ObservableCollection<XnaCanvasLayer>();

        static GroupLayer ()
        {
            LayersProperty = DependencyProperty.Register("LayersSource",
                typeof(IEnumerable<XnaCanvasLayer>), typeof(GroupLayer));
        }

        [Bindable(true)]
        public IEnumerable<XnaCanvasLayer> LayersSource
        {
            get { return this.GetValue(LayersProperty) as IEnumerable<XnaCanvasLayer>; }
            set { this.SetValue(LayersProperty, value); }
        }

        [Bindable(true)]
        public ObservableCollection<XnaCanvasLayer> Layers
        {
            get { return _layers; }
        }

        protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == HorizontalOffsetProperty)
                CascadeHorizontalOffset();
            else if (e.Property == VerticalOffsetProperty)
                CascadeVerticalOffset();

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

        #region Properties

        public override double VirtualHeight
        {
            get
            {
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
