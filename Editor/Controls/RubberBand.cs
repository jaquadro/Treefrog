using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Amphibian.Drawing;
using Editor.Model;
using Microsoft.Xna.Framework.Graphics;
using Editor.Model.Controls;

namespace Editor.Controls
{
    public class DrawEventArgs : EventArgs {
        public SpriteBatch SpriteBatch { get; private set; }

        public DrawEventArgs (SpriteBatch spriteBatch) {
            SpriteBatch = spriteBatch;
        }
    }

    public class RubberBand : IDisposable
    {
        private const int _drawOrder = 10;

        private LayerControl _control;
        //private ITileSource _source;

        private Brush _fillBrush;
        private Brush _strokeBrush;

        private Point _start;
        private Point _end;

        private int _snapX;
        private int _snapY;

        public RubberBand (LayerControl control, int snapX, int snapY)
        {
            _control = control;
            //_source = control.TileSource;

            _snapX = snapX;
            _snapY = snapY;

            AttachHandlers();
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(_start.X, _start.Y, _end.X - _start.X + 1, _end.Y - _start.Y + 1); }
        }

        protected bool Disposed { get; private set; }

        public int DrawOrder
        {
            get { return _drawOrder; }
        }

        public Brush FillBrush
        {
            get { return _fillBrush; }
            set { _fillBrush = value; }
        }

        public Brush StrokeBrush
        {
            get { return _strokeBrush; }
            set { _strokeBrush = value; }
        }

        protected virtual void AttachHandlers ()
        {
            _control.DrawExtra += DrawHandler;
        }

        protected virtual void DetachHandlers ()
        {
            _control.DrawExtra -= DrawHandler;
        }

        public void Start (Point start)
        {
            _start = start;
        }

        public void End (Point end)
        {
            _end = end;
        }

        protected virtual void DrawHandler (object sender, DrawLayerEventArgs e)
        {
            if (_fillBrush == null) {
                _fillBrush = new SolidColorBrush(e.SpriteBatch, new Color(.2f, .75f, 1f, .3f));
            }

            Rectangle region = _control.VisibleRegion;

            Vector2 offset = _control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * _control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * _control.Zoom);

            e.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            e.SpriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            int startx = Math.Min(_start.X, _end.X);
            int starty = Math.Min(_start.Y, _end.Y);
            int endx = Math.Max(_start.X, _end.X) + 1;
            int endy = Math.Max(_start.Y, _end.Y) + 1;

            Rectangle box = new Rectangle(
                (int)(startx * _snapX * _control.Zoom),
                (int)(starty * _snapY * _control.Zoom),
                (endx - startx) * (int)(_snapX * _control.Zoom),
                (endy - starty) * (int)(_snapY * _control.Zoom));

            if (_fillBrush != null) {
                Primitives2D.FillRectangle(e.SpriteBatch, box, _fillBrush);
            }

            if (_strokeBrush != null) {
                Primitives2D.DrawRectangle(e.SpriteBatch, box, _strokeBrush);
            }

            e.SpriteBatch.End();
        }

        #region IDisposable Members

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!this.Disposed) {
                if (disposing) {
                    DetachHandlers();
                }

                Disposed = true;
            }
        }

        ~RubberBand ()
        {
            Dispose(false);
        }

        #endregion
    }
}
