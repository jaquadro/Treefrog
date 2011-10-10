using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Editor.Model
{
    public abstract class Layer
    {
        #region Fields

        private float _opacity;
        private bool _visible;

        #endregion

        #region Properties

        public bool IsVisible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = MathHelper.Clamp(value, 0f, 1f); }
        }

        #endregion
    }
}
