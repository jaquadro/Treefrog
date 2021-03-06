﻿using System;
using System.Collections.Generic;
using LilyPath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;

namespace Treefrog.Render.Layers
{
    public class RenderLayer : CanvasLayer
    {
        [Flags]
        protected enum RenderMode
        {
            Sprite,
            Drawing,
        }

        protected enum SampleMode
        {
            Point,
            Linear,
        }

        protected enum WrapMode
        {
            Clamp,
            Wrap,
        }

        private SpriteBatch _spriteBatch;
        private DrawBatch _drawBatch;
        private RasterizerState _rasterState;
        private RenderTarget2D _target;

        public RenderLayer ()
            : this(null)
        { }

        public RenderLayer (RenderLayerPresenter model)
            : base(model)
        {
            Mode = RenderMode.Sprite;
            Scissor = true;

            _rasterState = new RasterizerState();
        }

        protected override void DisposeManaged ()
        {
            if (_spriteBatch != null)
                _spriteBatch.Dispose();
            if (_target != null)
                _target.Dispose();
            if (_rasterState != null)
                _rasterState.Dispose();

            base.DisposeManaged();
        }

        protected new RenderLayerPresenter Model
        {
            get { return ModelCore as RenderLayerPresenter; }
        }

        protected RenderMode Mode { get; set; }

        protected bool Scissor { get; set; }

        protected virtual SampleMode LayerSampleMode
        {
            get { return SampleMode.Point; }
        }

        private float LayerOpacity 
        {
            get { return 1f; }
        }

        private SamplerState GetSamplerState (WrapMode wrapMode)
        {
            if (wrapMode == WrapMode.Clamp) {
                if (LayerSampleMode == SampleMode.Point)
                    return SamplerState.PointClamp;
                else
                    return SamplerState.LinearClamp;
            }
            else {
                if (LayerSampleMode == SampleMode.Point)
                    return SamplerState.PointWrap;
                else
                    return SamplerState.LinearWrap;
            }
        }

        protected override void RenderCore (GraphicsDevice device)
        {
            if (Mode.HasFlag(RenderMode.Sprite)) {
                if (_spriteBatch == null || _spriteBatch.GraphicsDevice != device)
                    _spriteBatch = new SpriteBatch(device);

                RenderCore(_spriteBatch);
            }

            if (Mode.HasFlag(RenderMode.Drawing)) {
                if (_drawBatch == null || _drawBatch.GraphicsDevice != device) {
                    _drawBatch = new DrawBatch(device);
                }

                RenderCore(_drawBatch);
            }
        }

        protected virtual void RenderCore (SpriteBatch spriteBatch)
        {
            Vector2 offset = BeginDraw(spriteBatch);
            RenderContent(spriteBatch);
            EndDraw(spriteBatch, offset);
        }

        protected virtual void RenderCore (DrawBatch drawBatch)
        {
            Vector2 offset = BeginDraw(drawBatch);
            RenderContent(drawBatch);
            EndDraw(drawBatch, offset);
        }

        protected virtual void RenderContent (SpriteBatch spriteBatch)
        { }

        protected virtual void RenderContent (DrawBatch drawBatch)
        { }

        protected virtual void RenderCommands (SpriteBatch spriteBatch, TextureCache textureCache, IEnumerable<DrawCommand> drawList)
        {
            foreach (DrawCommand command in drawList) {
                Texture2D texture = textureCache.Resolve(command.Texture);
                if (texture != null)
                    spriteBatch.Draw(texture, command.DestRect.ToXnaRectangle(), command.SourceRect.ToXnaRectangle(), 
                        command.BlendColor.ToXnaColor(), command.Rotation, new Vector2(command.OriginX, command.OriginY), 
                        SpriteEffects.None, 0f);
            }
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch)
        {
            return BeginDraw(spriteBatch, GetSamplerState(WrapMode.Clamp));
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch, SamplerState samplerState)
        {
            return BeginDraw(spriteBatch, samplerState, null);
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch, SamplerState samplerState, Effect effect)
        {
            SetupRenderTarget(spriteBatch.GraphicsDevice);
            return BeginDrawInner(spriteBatch, samplerState, effect);
        }

        protected void EndDraw (SpriteBatch spriteBatch, Vector2 offset)
        {
            EndDrawInner(spriteBatch);

            if (_target != null) {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);

                BeginDrawInner(_spriteBatch, SamplerState.PointClamp, null);
                _spriteBatch.Draw(_target, new Vector2((float)-offset.X, (float)-offset.Y), new Color(1f, 1f, 1f, LayerOpacity));
                EndDrawInner(_spriteBatch);
            }
        }

        private Vector2 BeginDrawInner (SpriteBatch spriteBatch, SamplerState samplerState, Effect effect)
        {
            SetupRasterizerState(spriteBatch.GraphicsDevice);

            Vector2 offset = GetOffset();
            Matrix transform = Matrix.CreateTranslation(offset.X, offset.Y, 0);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, samplerState, null, _rasterState, effect, transform);

            return offset;
        }

        private void EndDrawInner (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        private Vector2 BeginDraw (DrawBatch drawBatch)
        {
            return BeginDraw(drawBatch, GetSamplerState(WrapMode.Wrap));
        }

        private Vector2 BeginDraw (DrawBatch drawBatch, SamplerState samplerState)
        {
            SetupRenderTarget(drawBatch.GraphicsDevice);
            return BeginDrawInner(drawBatch, samplerState);
        }

        private void EndDraw (DrawBatch drawBatch, Vector2 offset)
        {
            EndDrawInner(drawBatch);

            if (_target != null) {
                drawBatch.GraphicsDevice.SetRenderTarget(null);

                BeginDrawInner(_spriteBatch, SamplerState.PointClamp, null);
                _spriteBatch.Draw(_target, new Vector2((float)-offset.X, (float)-offset.Y), new Color(1f, 1f, 1f, LayerOpacity));
                EndDrawInner(_spriteBatch);
            }
        }

        private Vector2 BeginDrawInner (DrawBatch drawBatch, SamplerState samplerState)
        {
            SetupRasterizerState(drawBatch.GraphicsDevice);

            Vector2 offset = GetOffset();
            Matrix transform = Matrix.CreateTranslation(offset.X, offset.Y, 0);

            drawBatch.Begin(DrawSortMode.Deferred, BlendState.NonPremultiplied, samplerState, null, _rasterState, null, transform);

            return offset;
        }

        private void EndDrawInner (DrawBatch drawBatch)
        {
            drawBatch.End();
        }

        private void SetupRenderTarget (GraphicsDevice device)
        {
            if (LayerOpacity < 1f) {
                if (_target == null
                    || _target.GraphicsDevice != device
                    || _target.Width != device.Viewport.Width
                    || _target.Height != device.Viewport.Height) {
                        _target = new RenderTarget2D(device,
                        device.Viewport.Width, device.Viewport.Height,
                        false, SurfaceFormat.Color, DepthFormat.None);
                }

                device.SetRenderTarget(_target);
                device.Clear(Color.Transparent);
            }
        }

        private void SetupRasterizerState (GraphicsDevice device)
        {
            if (Scissor && LevelGeometry != null) {
                device.ScissorRectangle = LevelGeometry.CanvasBounds.ToXnaRectangle();

                bool areaEmpty = device.ScissorRectangle.IsEmpty;
                if (_rasterState == null || _rasterState.ScissorTestEnable == areaEmpty)
                    _rasterState = new RasterizerState() {
                        ScissorTestEnable = !areaEmpty
                    };
            }
            else
                _rasterState = null;
        }

        private Vector2 GetOffset ()
        {
            Vector2 offset = Vector2.Zero;

            if (LevelGeometry != null) {
                offset = LevelGeometry.CanvasBounds.Location.ToXnaVector2();
                offset -= LevelGeometry.VisibleBounds.Location.ToXnaVector2() * LevelGeometry.ZoomFactor;
            }

            return offset;
        }
    }
}
