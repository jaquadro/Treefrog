using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Aux;

using Drawing = Treefrog.Framework.Imaging.Drawing;
using XnaDrawing = Amphibian.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Treefrog.Framework;
using Treefrog.Presentation.Annotations;
using Amphibian.Drawing;

namespace Treefrog.Windows.Annotations
{
    public abstract class AnnotationRenderer : IDisposable
    {
        public void Render (SpriteBatch spriteBatch)
        {
            Render(spriteBatch, 1f);
        }

        public void Render (DrawBatch drawBatch)
        {
            Render(drawBatch, 1f);
        }

        public virtual void Render (SpriteBatch spriteBatch, float zoomFactor)
        { }

        public virtual void Render (DrawBatch drawBatch, float zoomFactor)
        { }

        #region Dispose

        private bool _disposed = false;

        public void Dispose ()
        {
            if (!_disposed) {
                DisposeManaged();
                DisposeUnamanged();

                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        protected virtual void DisposeManaged ()
        {
        }

        protected virtual void DisposeUnamanged ()
        {
        }

        #endregion
    }

    public abstract class DrawAnnotationRenderer : AnnotationRenderer
    {
        private DrawAnnotation _data;
        private XnaDrawing.Brush _fillBrush;
        private XnaDrawing.Pen _outlinePen;

        protected DrawAnnotationRenderer (DrawAnnotation data)
        {
            _data = data;
            _data.FillInvalidated += HandleFillInvalidated;
            _data.OutlineInvalidated += HandleOutlineInvalidated;
        }

        protected override void DisposeManaged ()
        {
            if (_data != null) {
                _data.FillInvalidated -= HandleFillInvalidated;
                _data.OutlineInvalidated -= HandleOutlineInvalidated;
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

            base.DisposeManaged();
        }

        protected virtual XnaDrawing.Brush Fill
        {
            get { return _fillBrush; }
        }

        protected virtual XnaDrawing.Pen Outline
        {
            get { return _outlinePen; }
        }

        protected virtual void InitializeResources (GraphicsDevice device)
        {
            if (_fillBrush == null && _data.Fill != null)
                _fillBrush = BrushFactory.Create(device, _data.Fill);
            if (_outlinePen == null && _data.Outline != null)
                _outlinePen = PenFactory.Create(device, _data.Outline);
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
    }

    public class SelectionAnnotRenderer : DrawAnnotationRenderer
    {
        private SelectionAnnot _data;

        public SelectionAnnotRenderer (SelectionAnnot data)
            : base(data)
        {
            _data = data;
        }

        public override void Render (DrawBatch drawBatch, float zoomFactor)
        {
            if (IsDisposed)
                return;

            InitializeResources(drawBatch.GraphicsDevice);

            Rectangle rect = new Rectangle(
                (int)(Math.Min(_data.Start.X, _data.End.X) * zoomFactor),
                (int)(Math.Min(_data.Start.Y, _data.End.Y) * zoomFactor),
                (int)(Math.Abs(_data.End.X - _data.Start.X) * zoomFactor),
                (int)(Math.Abs(_data.End.Y - _data.Start.Y) * zoomFactor)
                );

            if (Fill != null)
                drawBatch.FillRectangle(rect, Fill);
            if (Outline != null)
                drawBatch.DrawRectangle(rect, Outline);
        }
    }

    

    public class BrushFactory 
    {
        public static XnaDrawing.Brush Create (GraphicsDevice device, Drawing.Brush brush)
        {
            if (brush is Drawing.SolidColorBrush) {
                Drawing.SolidColorBrush scBrush = brush as Drawing.SolidColorBrush;
                Color color = new Color(scBrush.Color.R / 255f, scBrush.Color.G / 255f, scBrush.Color.B / 255f, scBrush.Color.A / 255f);
                return new XnaDrawing.SolidColorBrush(device, color);
            }
            else if (brush is Drawing.PatternBrush) {
                Drawing.PatternBrush pBrush = brush as Drawing.PatternBrush;
                using (Texture2D pattern = pBrush.Pattern.CreateTexture(device)) {
                    return new XnaDrawing.PatternBrush(device, pattern, (float)pBrush.Opacity);
                }
            }

            return null;
        }
    }

    public class PenFactory
    {
        public static XnaDrawing.Pen Create (GraphicsDevice device, Drawing.Pen pen)
        {
            if (pen.GetType() == typeof(Drawing.Pen)) {
                XnaDrawing.Brush brush = BrushFactory.Create(device, pen.Brush);
                return new XnaDrawing.Pen(brush, (int)pen.Width);
            }

            return null;
        }
    }
}
