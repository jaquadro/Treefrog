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
        private Pen _outline;

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

        public event EventHandler FillInvalidated;
        public event EventHandler OutlineInvalidated;

        protected virtual void OnFillInvalidated (EventArgs e)
        {
            var ev = FillInvalidated;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnOutlineInvalidated (EventArgs e)
        {
            var ev = OutlineInvalidated;
            if (ev != null)
                ev(this, e);
        }
    }
}
