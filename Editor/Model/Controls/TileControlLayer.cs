﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows.Forms;

namespace Editor.Model.Controls
{
    public abstract class TileControlLayer : BaseControlLayer
    {
        #region Fields

        // Model

        private TileLayer _layer;

        // Graphics and Service

        private Effect _effectTrans;
        private EffectParameter _effectTransColor;

        private Texture2D _tileGridBrush;
        private Texture2D _tileGridBrushRight;
        private Texture2D _tileGridBrushBottom;

        private Color _gridColor = new Color(0, 0, 0, 0.5f);

        #endregion

        #region Events

        public event EventHandler<TileMouseEventArgs> MouseTileClick;
        public event EventHandler<TileMouseEventArgs> MouseTileDown;
        public event EventHandler<TileMouseEventArgs> MouseTileMove;
        public event EventHandler<TileMouseEventArgs> MouseTileUp;

        #endregion

        #region Constructors

        public TileControlLayer (LayerControl control)
            : base(control)
        {
            Control.ZoomChanged += ControlZoomChangedHandler;

            Control.MouseClick += ControlMouseClickHandler;
            Control.MouseUp += ControlMouseUpHandler;
            Control.MouseDown += ControlMouseDownHandler;
            Control.MouseMove += ControlMouseMoveHandler;
        }

        #endregion

        #region Properties

        public new TileLayer Layer
        {
            get { return _layer; }
            protected set
            {
                _layer = value;
                base.Layer = value;

                if (Control.Initialized) {
                    BuildTileBrush();
                }
            }
        }

        public Color GridColor
        {
            get { return _gridColor; }
            set
            {
                _gridColor = value;

                if (Control.Initialized) {
                    BuildTileBrush();
                }
            }
        }

        #endregion

        #region Event Handlers

        private void ControlZoomChangedHandler (object sender, EventArgs e)
        {
            if (Control.Initialized) {
                BuildTileBrush();
            }
        }

        private void ControlMouseClickHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                OnMouseTileClick(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));
            }
        }

        private void ControlMouseUpHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                OnMouseTileUp(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));
            }
        }

        private void ControlMouseDownHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                OnMouseTileDown(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));
            }
        }

        private void ControlMouseMoveHandler (object sender, MouseEventArgs e)
        {
            if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
                OnMouseTileMove(new TileMouseEventArgs(e, MouseToTileCoords(new Point(e.X, e.Y))));
            }
        }

        #endregion

        #region Event Dispatchers

        protected virtual void OnMouseTileClick (TileMouseEventArgs e)
        {
            if (MouseTileClick != null) {
                MouseTileClick(this, e);
            }
        }

        protected virtual void OnMouseTileDown (TileMouseEventArgs e)
        {
            if (MouseTileDown != null) {
                MouseTileDown(this, e);
            }
        }

        protected virtual void OnMouseTileMove (TileMouseEventArgs e)
        {
            if (MouseTileMove != null) {
                MouseTileMove(this, e);
            }
        }

        protected virtual void OnMouseTileUp (TileMouseEventArgs e)
        {
            if (MouseTileUp != null) {
                MouseTileUp(this, e);
            }
        }

        #endregion

        protected override void Initiailize ()
        {
            base.Initiailize();

            _effectTrans = Control.ContentManager.Load<Effect>("TransColor");
            _effectTransColor = _effectTrans.Parameters["transColor"];
            _effectTransColor.SetValue(Color.White.ToVector4());

            BuildTileBrush();
        }

        protected override void DrawContent (SpriteBatch spriteBatch)
        {
            if (_layer == null) {
                return;
            }

            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, _effectTrans, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            Rectangle tileRegion = new Rectangle(
                region.X / _layer.TileWidth,
                region.Y / _layer.TileHeight,
                (int)(region.Width * Control.Zoom + region.X % _layer.TileWidth + _layer.TileWidth - 1) / _layer.TileWidth,
                (int)(region.Height * Control.Zoom + region.Y % _layer.TileHeight + _layer.TileHeight - 1) / _layer.TileHeight
                );

            DrawTiles(spriteBatch, tileRegion);

            spriteBatch.End();
        }

        protected override void DrawGrid (SpriteBatch spriteBatch)
        {
            if (_layer == null) {
                return;
            }

            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            Rectangle tileRegion = new Rectangle(
                region.X / _layer.TileWidth,
                region.Y / _layer.TileHeight,
                (int)(region.Width * Control.Zoom + region.X % _layer.TileWidth + _layer.TileWidth - 1) / _layer.TileWidth,
                (int)(region.Height * Control.Zoom + region.Y % _layer.TileHeight + _layer.TileHeight - 1) / _layer.TileHeight
                );

            Func<int, int, bool> inside = TileInRegionPredicate(tileRegion);

            for (int x = tileRegion.X; x <= tileRegion.X + tileRegion.Width; x++) {
                for (int y = tileRegion.Y; y <= tileRegion.Y + tileRegion.Height; y++) {
                    bool iCenter = inside(x, y);
                    bool iLeft = inside(x - 1, y);
                    bool iUpper = inside(x, y - 1);
                    bool iUpperLeft = inside(x - 1, y - 1);

                    Vector2 pos = new Vector2(x * _layer.TileWidth * Control.Zoom, y * _layer.TileHeight * Control.Zoom);
                    if (iCenter || (iLeft && iUpper)) {
                        spriteBatch.Draw(_tileGridBrush, pos, Color.White);
                    }
                    else if (iLeft) {
                        spriteBatch.Draw(_tileGridBrushRight, pos, Color.White);
                    }
                    else if (iUpper) {
                        spriteBatch.Draw(_tileGridBrushBottom, pos, Color.White);
                    }
                    if (iUpperLeft) {
                        continue;
                    }
                }
            }

            spriteBatch.End();
        }

        protected virtual void DrawTiles (SpriteBatch spriteBatch, Rectangle tileRegion) { }

        protected abstract Func<int, int, bool> TileInRegionPredicate (Rectangle tileRegion);

        private void BuildTileBrush ()
        {
            if (_layer == null) {
                _tileGridBrush = null;
                _tileGridBrushRight = null;
                _tileGridBrushBottom = null;
                return;
            }

            int x = (int)(_layer.TileWidth * Control.Zoom);
            int y = (int)(_layer.TileHeight * Control.Zoom);

            Color[] colors = new Color[x * y];
            Color[] right = new Color[x * y];
            Color[] bottom = new Color[x * y];
            for (int i = 0; i < x; i++) {
                if (i % 4 != 2) {
                    colors[i] = _gridColor;
                    bottom[i] = _gridColor;
                }
            }
            for (int i = 0; i < y; i++) {
                if (i % 4 != 2) {
                    colors[i * x] = _gridColor;
                    right[i * x] = _gridColor;
                }
            }

            _tileGridBrush = new Texture2D(Control.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrush.SetData(colors);

            _tileGridBrushRight = new Texture2D(Control.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrushRight.SetData(right);

            _tileGridBrushBottom = new Texture2D(Control.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileGridBrushBottom.SetData(bottom);
        }

        private TileCoord MouseToTileCoords (Point mouse)
        {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            int tx = (int)Math.Floor(((float)mouse.X - offset.X) / (_layer.TileWidth * Control.Zoom));
            int ty = (int)Math.Floor(((float)mouse.Y - offset.Y) / (_layer.TileHeight * Control.Zoom));

            return new TileCoord(tx, ty);
        }
    }
}