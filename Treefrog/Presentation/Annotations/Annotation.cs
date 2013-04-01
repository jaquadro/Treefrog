using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging.Drawing;

namespace Treefrog.Presentation.Annotations
{
    public abstract class Annotation
    {
    }

    public abstract class DrawAnnotation : Annotation
    {
        private Brush _fill;
        private Brush _fillGlow;
        private Pen _outline;
        private Pen _outlineGlow;

        public Brush Fill
        {
            get { return _fill; }
            set
            {
                if (_fill != value) {
                    _fill = value;
                    OnFillInvalidated(EventArgs.Empty);
                }
            }
        }

        public Brush FillGlow
        {
            get { return _fillGlow; }
            set
            {
                if (_fillGlow != value) {
                    _fillGlow = value;
                    OnFillGlowInvalidated(EventArgs.Empty);
                }
            }
        }

        public Pen Outline
        {
            get { return _outline; }
            set
            {
                if (_outline != value) {
                    _outline = value;
                    OnOutlineInvalidated(EventArgs.Empty);
                }
            }
        }

        public Pen OutlineGlow
        {
            get { return _outlineGlow; }
            set
            {
                if (_outlineGlow != value) {
                    _outlineGlow = value;
                    OnOutlineGlowInvalidated(EventArgs.Empty);
                }
            }
        }

        public event EventHandler FillInvalidated;
        public event EventHandler FillGlowInvalidated;
        public event EventHandler OutlineInvalidated;
        public event EventHandler OutlineGlowInvalidated;

        protected virtual void OnFillInvalidated (EventArgs e)
        {
            var ev = FillInvalidated;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnFillGlowInvalidated (EventArgs e)
        {
            var ev = FillGlowInvalidated;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnOutlineInvalidated (EventArgs e)
        {
            var ev = OutlineInvalidated;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnOutlineGlowInvalidated (EventArgs e)
        {
            var ev = OutlineGlowInvalidated;
            if (ev != null)
                ev(this, e);
        }
    }
}
