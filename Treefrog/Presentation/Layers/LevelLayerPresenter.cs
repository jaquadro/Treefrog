using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Controllers;

namespace Treefrog.Presentation.Layers
{
    public class LevelLayerPresenter : RenderLayerPresenter, IPointerResponderProvider
    {
        private ILayerContext _layerContext;
        private Layer _layer;

        public LevelLayerPresenter (ILayerContext layerContext, Layer layer)
        {
            _layerContext = layerContext;
            _layer = layer;
        }

        public virtual void Activate ()
        { }

        public virtual void Deactivate ()
        { }

        protected ILayerContext LayerContext
        {
            get { return _layerContext; }
        }

        public Layer Layer
        {
            get { return _layer; }
        }

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get { yield break; }
        }

        public virtual IPointerResponder PointerEventResponder
        {
            get { return null; }
        }

        public event EventHandler PointerEventResponderChanged;

        protected virtual void OnPointerEventResponderChanged (EventArgs e)
        {
            var ev = PointerEventResponderChanged;
            if (ev != null)
                ev(this, e);
        }

        public string LayerName
        {
            get { return _layer.Name; }
            set { _layer.Name = value; }
        }

        public bool IsVisible
        {
            get { return _layer.IsVisible; }
            set { _layer.IsVisible = value; }
        }

        public float Opacity
        {
            get { return _layer.Opacity; }
            set { _layer.Opacity = value; }
        }
    }
}
