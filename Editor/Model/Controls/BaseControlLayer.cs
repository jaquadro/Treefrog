using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Model.Controls
{
    public abstract class BaseControlLayer
    {
        #region Fields

        private Layer _layer;

        #endregion

        #region Properties

        protected LayerControl Control { get; private set; }

        public bool Selected { get; set; }
        public LayerCondition ShouldDrawContent { get; set; }
        public LayerCondition ShouldDrawGrid { get; set; }
        public LayerCondition ShouldRespondToInput { get; set; }

        #endregion

        #region Constructors

        protected BaseControlLayer (LayerControl control)
        {
            Control = control;
            Control.AddLayer(this);

            if (Control.Initialized) {
                Initiailize();
            }
            else {
                Control.ControlInitialized += ControlInitializedHandler;
            }

            Control.DrawLayerContent += DrawContentHandler;
            Control.DrawLayerGrid += DrawGridHandler;
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

        private void DrawContentHandler (object sender, DrawLayerEventArgs e)
        {
            if (CheckLayerCondition(ShouldDrawContent)) {
                DrawContent(e.SpriteBatch);
            }
        }

        private void DrawGridHandler (object sender, DrawLayerEventArgs e)
        {
            if (CheckLayerCondition(ShouldDrawContent) && CheckLayerCondition(ShouldDrawGrid)) {
                DrawGrid(e.SpriteBatch);
            }
        }

        #endregion

        protected virtual void Initiailize () { }

        protected virtual void DrawContent (SpriteBatch spriteBatch) { }

        protected virtual void DrawGrid (SpriteBatch spriteBatch) { }

        protected bool CheckLayerCondition (LayerCondition option)
        {
            return (option == LayerCondition.Always) || (option == LayerCondition.Selected && Selected);
        }
    }
}
