using System;
using System.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.View.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Presentation.Layers
{
    public abstract class BaseControlLayer : INamedResource
    {
        #region Fields

        private string _name;
        private Layer _layer;

        #endregion

        #region Properties

        public LayerControl Control { get; private set; }

        public bool Selected { get; set; }
        public LayerCondition ShouldDrawContent { get; set; }
        public LayerCondition ShouldDrawGrid { get; set; }
        public LayerCondition ShouldRespondToInput { get; set; }

        #endregion

        #region Constructors

        protected BaseControlLayer (LayerControl control)
        {
            _name = Guid.NewGuid().ToString();

            Control = control;
            Control.AddLayer(this);

            if (Control.Initialized) {
                Initiailize();
            }
            else {
                Control.ControlInitialized += ControlInitializedHandler;
            }
        }

        protected BaseControlLayer (LayerControl control, Layer layer)
            : this(control)
        {
            Layer = layer;
        }

        #endregion

        #region Properties

        public Layer Layer
        {
            get { return _layer; }
            protected set
            {
                _layer = value;
            }
        }

        public abstract int VirtualHeight { get; }

        public abstract int VirtualWidth { get; }

        #endregion

        #region Events

        public event EventHandler VirtualSizeChanged;

        public event EventHandler<DrawLayerEventArgs> DrawExtraCallback;

        #endregion

        #region Event Dispatchers

        protected virtual void OnVirutalSizeChanged (EventArgs e)
        {
            if (VirtualSizeChanged != null) {
                VirtualSizeChanged(this, e);
            }
        }

        protected virtual void OnDrawExtraContent (DrawLayerEventArgs e)
        {
            if (DrawExtraCallback != null) {
                DrawExtraCallback(this, e);
            }
        }

        #endregion

        #region Event Handlers

        private void ControlInitializedHandler (object sender, EventArgs e)
        {
            Initiailize();
        }

        #endregion

        public void DrawContent (SpriteBatch spriteBatch)
        {
            if (CheckLayerCondition(ShouldDrawContent)) {
                DrawContentImpl(spriteBatch);
            }
        }

        public void DrawGrid (SpriteBatch spriteBatch)
        {
            if (CheckLayerCondition(ShouldDrawContent) && CheckLayerCondition(ShouldDrawGrid)) {
                DrawGridImpl(spriteBatch);
            }
        }

        public void DrawExtra (SpriteBatch spriteBatch)
        {
            if (CheckLayerCondition(ShouldDrawContent)) {
                DrawExtraImpl(spriteBatch);
            }
        }

        public virtual void ApplyScrollAttributes ()
        {
            Control.SetScrollLargeChange(ScrollOrientation.HorizontalScroll, 48);
            Control.SetScrollLargeChange(ScrollOrientation.VerticalScroll, 48);
            Control.SetScrollSmallChange(ScrollOrientation.HorizontalScroll, 16);
            Control.SetScrollSmallChange(ScrollOrientation.VerticalScroll, 16);
        }

        protected virtual void Initiailize () { }

        protected virtual void DrawContentImpl (SpriteBatch spriteBatch) { }

        protected virtual void DrawGridImpl (SpriteBatch spriteBatch) { }

        protected virtual void DrawExtraImpl (SpriteBatch spriteBatch) {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            OnDrawExtraContent(new DrawLayerEventArgs(spriteBatch));

            spriteBatch.End();
        }

        protected bool CheckLayerCondition (LayerCondition option)
        {
            return (option == LayerCondition.Always) || (option == LayerCondition.Selected && Selected);
        }

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
            private set
            {
                if (_name != value) {
                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, _name));
                }
            }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged;

        public event EventHandler Modified;

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        #endregion
    }
}
