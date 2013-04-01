using System;
using LilyPath;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Windows.Annotations
{
    public abstract class DrawAnnotationRenderer : AnnotationRenderer
    {
        private DrawAnnotation _data;
        private Brush _fillBrush;
        private Pen _outlinePen;
        private Pen _outlineGlowPen;

        protected DrawAnnotationRenderer (DrawAnnotation data)
        {
            _data = data;
            _data.FillInvalidated += HandleFillInvalidated;
            _data.OutlineInvalidated += HandleOutlineInvalidated;
            _data.OutlineGlowInvalidated += HandleOutlineGlowInvalidated;
        }

        protected override void DisposeManaged ()
        {
            if (_data != null) {
                _data.FillInvalidated -= HandleFillInvalidated;
                _data.OutlineInvalidated -= HandleOutlineInvalidated;
                _data.OutlineGlowInvalidated -= HandleOutlineGlowInvalidated;
                _data = null;
            }

            if (_fillBrush != null) {
                _fillBrush.Dispose();
                _fillBrush = null;
            }

            if (_outlinePen != null) {
                _outlinePen.Dispose();
                _outlinePen = null;
            }

            if (_outlineGlowPen != null) {
                _outlineGlowPen.Dispose();
                _outlineGlowPen = null;
            }

            base.DisposeManaged();
        }

        protected virtual Brush Fill
        {
            get { return _fillBrush; }
        }

        protected virtual Pen Outline
        {
            get { return _outlinePen; }
        }

        protected virtual Pen OutlineGlow
        {
            get { return _outlineGlowPen; }
        }

        protected virtual void InitializeResources (GraphicsDevice device)
        {
            if (_fillBrush == null && _data.Fill != null)
                _fillBrush = BrushFactory.Create(device, _data.Fill);
            if (_outlinePen == null && _data.Outline != null)
                _outlinePen = PenFactory.Create(device, _data.Outline);
            if (_outlineGlowPen == null && _data.OutlineGlow != null)
                _outlineGlowPen = PenFactory.Create(device, _data.OutlineGlow);
        }

        private void HandleFillInvalidated (object sender, EventArgs e)
        {
            if (_fillBrush != null) {
                _fillBrush.Dispose();
                _fillBrush = null;
            }
        }

        private void HandleOutlineInvalidated (object sender, EventArgs e)
        {
            if (_outlinePen != null) {
                _outlinePen.Dispose();
                _outlinePen = null;
            }
        }

        private void HandleOutlineGlowInvalidated (object sender, EventArgs e)
        {
            if (_outlineGlowPen != null) {
                _outlineGlowPen.Dispose();
                _outlineGlowPen = null;
            }
        }
    }
}
