using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Model.Controls
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

        #endregion

        #region Event Dispatchers

        public virtual void OnVirutalSizeChanged (EventArgs e)
        {
            if (VirtualSizeChanged != null) {
                VirtualSizeChanged(this, e);
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

        protected virtual void Initiailize () { }

        protected virtual void DrawContentImpl (SpriteBatch spriteBatch) { }

        protected virtual void DrawGridImpl (SpriteBatch spriteBatch) { }

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

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        #endregion
    }
}
