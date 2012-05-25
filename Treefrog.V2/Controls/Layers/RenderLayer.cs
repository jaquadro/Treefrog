using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows;
using Treefrog.ViewModel.Layers;
using System.Collections.Specialized;
using Treefrog.Framework;
using System.Windows.Data;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;
using TRectangle = Treefrog.Framework.Imaging.Rectangle;
using TColor = Treefrog.Framework.Imaging.Color;
using Treefrog.Aux;

namespace Treefrog.Controls.Layers
{
    public enum LayerControlAlignment
    {
        Center,
        Left,
        Right,
        Upper,
        Lower,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }

    public class RenderLayer : XnaCanvasLayer
    {
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _target;

        public static readonly DependencyProperty LayerOpacityProperty;
        public static readonly DependencyProperty ModelProperty;
        public static readonly DependencyProperty AlignmentProperty;

        static RenderLayer ()
        {
            LayerOpacityProperty = DependencyProperty.Register("LayerOpacity",
                typeof(float), typeof(RenderLayer), new PropertyMetadata(1.0f));
            ModelProperty = DependencyProperty.Register("Model",
                typeof(RenderLayerVM), typeof(RenderLayer), new PropertyMetadata(null, HandleModelChanged));
            AlignmentProperty = DependencyProperty.Register("Alignment",
                typeof(LayerControlAlignment), typeof(RenderLayer), new PropertyMetadata(LayerControlAlignment.Center));
        }

        public float LayerOpacity
        {
            get { return (float)this.GetValue(LayerOpacityProperty); }
            set { this.SetValue(LayerOpacityProperty, value); }
        }

        public RenderLayerVM Model
        {
            get { return (RenderLayerVM)this.GetValue(ModelProperty); }
            set { this.SetValue(ModelProperty, value); }
        }

        public LayerControlAlignment Alignment
        {
            get { return (LayerControlAlignment)this.GetValue(AlignmentProperty); }
            set { this.SetValue(AlignmentProperty, value); }
        }

        // TODO: Some of this binding should be broken up better across the layer hierarchy
        private static void HandleModelChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RenderLayer self = sender as RenderLayer;
            self.RebindTextureSource(e.OldValue as RenderLayerVM, e.NewValue as RenderLayerVM);

            self.SetBinding(LayerOpacityProperty, new Binding("Opacity")
            {
                Source = e.NewValue,
            });

            // This doesn't belong here
            self.SetBinding(IsRenderedProperty, new Binding("IsVisible")
            {
                Source = e.NewValue,
            });
        }

        protected override void RenderCore (GraphicsDevice device)
        {
            if (_spriteBatch == null || _spriteBatch.GraphicsDevice != device)
                _spriteBatch = new SpriteBatch(device);

            RenderCore(_spriteBatch);
        }

        private static Random rand = new Random();

        protected virtual void RenderCore (SpriteBatch spriteBatch)
        {
            Vector offset = BeginDraw(spriteBatch);

            RenderLayerVM model = Model;
            if (model != null) {
                RenderCommands(spriteBatch, model.RenderCommands);
            }

            EndDraw(spriteBatch, offset);
        }

        public static Rectangle ToXnaRectangle (TRectangle rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Color ToXnaColor (TColor color)
        {
            return new Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        protected virtual void RenderCommands (SpriteBatch spriteBatch, IEnumerable<DrawCommand> drawList)
        {
            foreach (DrawCommand command in drawList) {
                Texture2D texture;
                if (_xnaTextures.TryGetValue(command.Texture, out texture)) {
                    if (texture == null) {
                        TextureResource texRef = _textures[command.Texture];
                        texture = texRef.CreateTexture(spriteBatch.GraphicsDevice);
                        _xnaTextures[command.Texture] = texture;
                    }
                    spriteBatch.Draw(texture, ToXnaRectangle(command.DestRect), ToXnaRectangle(command.SourceRect), ToXnaColor(command.BlendColor));
                }
            }
        }

        protected Rect VisibleRegion
        {
            get
            {
                return new Rect(HorizontalOffset, VerticalOffset,
                    Math.Ceiling(Math.Min(ViewportWidth / ZoomFactor, VirtualWidth)),
                    Math.Ceiling(Math.Min(ViewportHeight / ZoomFactor, VirtualHeight)));
            }
        }

        protected Vector VirtualSurfaceOffset
        {
            get
            {
                double offsetX = 0;
                double offsetY = 0;

                if (ViewportWidth > VirtualWidth * ZoomFactor) {
                    switch (Alignment) {
                        case LayerControlAlignment.Center:
                        case LayerControlAlignment.Upper:
                        case LayerControlAlignment.Lower:
                            offsetX = (ViewportWidth - VirtualWidth * ZoomFactor) / 2;
                            break;
                        case LayerControlAlignment.Right:
                        case LayerControlAlignment.UpperRight:
                        case LayerControlAlignment.LowerRight:
                            offsetX = (ViewportWidth - VirtualWidth * ZoomFactor);
                            break;
                    }
                }

                if (ViewportHeight > VirtualHeight * ZoomFactor) {
                    switch (Alignment) {
                        case LayerControlAlignment.Center:
                        case LayerControlAlignment.Left:
                        case LayerControlAlignment.Right:
                            offsetY = (ViewportHeight - VirtualHeight * ZoomFactor) / 2;
                            break;
                        case LayerControlAlignment.Lower:
                        case LayerControlAlignment.LowerLeft:
                        case LayerControlAlignment.LowerRight:
                            offsetY = (ViewportHeight - VirtualHeight * ZoomFactor);
                            break;
                    }
                }

                return new Vector(offsetX, offsetY);
            }
        }

        private Vector BeginDrawInner (SpriteBatch spriteBatch, Effect effect)
        {
            Rect region = VisibleRegion;
            Vector offset = VirtualSurfaceOffset;

            offset.X = Math.Ceiling(offset.X - region.X * ZoomFactor);
            offset.Y = Math.Ceiling(offset.Y - region.Y * ZoomFactor);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, effect, Matrix.CreateTranslation((float)offset.X, (float)offset.Y, 0));
            //spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            return offset;
        }

        private void EndDrawInner (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        protected Vector BeginDraw (SpriteBatch spriteBatch)
        {
            return BeginDraw(spriteBatch, null);
        }

        protected Vector BeginDraw (SpriteBatch spriteBatch, Effect effect)
        {
            _target = null;
            if (LayerOpacity < 1f) {
                _target = new RenderTarget2D(spriteBatch.GraphicsDevice,
                    spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height,
                    false, SurfaceFormat.Color, DepthFormat.None);

                spriteBatch.GraphicsDevice.SetRenderTarget(_target);
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            }

            return BeginDrawInner(spriteBatch, effect);
        }

        protected void EndDraw (SpriteBatch spriteBatch, Vector offset)
        {
            EndDrawInner(spriteBatch);

            if (_target != null) {
                spriteBatch.GraphicsDevice.SetRenderTarget(null);

                BeginDrawInner(spriteBatch, null);
                spriteBatch.Draw(_target, new Vector2((float)-offset.X, (float)-offset.Y), new Color(1f, 1f, 1f, LayerOpacity));
                EndDrawInner(spriteBatch);

                _target = null;
            }
        }

        public override double VirtualHeight
        {
            get
            {
                if (double.IsNaN(Height))
                    return (Model != null) ? Model.LayerHeight : 0;
                else
                    return Height;
            }
        }

        public override double VirtualWidth
        {
            get
            {
                if (double.IsNaN(Width))
                    return (Model != null) ? Model.LayerWidth : 0;
                else
                    return Width;
            }
        }

        #region Texture Management

        private Dictionary<string, TextureResource> _textures = new Dictionary<string, TextureResource>();
        private Dictionary<string, Texture2D> _xnaTextures = new Dictionary<string, Texture2D>();

        // TODO: Be smarter about managing TextureSource.  Bind to it instead?
        // If it changes underneath without a model change, we'll still stay attached to it!
        // Almost certainly this should be moved to a texture manager in the root control!
        private void RebindTextureSource (RenderLayerVM oldModel, RenderLayerVM newModel)
        {
            if (oldModel != null && oldModel.TextureSource != null) {
                oldModel.TextureSource.CollectionChanged -= HandleTextureSourceCollectionChanged;
                _textures.Clear();
            }

            if (newModel != null && newModel.TextureSource != null) {
                newModel.TextureSource.CollectionChanged += HandleTextureSourceCollectionChanged;

                foreach (KeyValuePair<string, TextureResource> item in newModel.TextureSource)
                    AddTexture(item.Key, item.Value);
            }
        }

        private void HandleTextureSourceCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (KeyValuePair<string, TextureResource> item in e.NewItems)
                        AddTexture(item.Key, item.Value);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (KeyValuePair<string, TextureResource> item in e.OldItems)
                        RemoveTexture(item.Key, item.Value);
                    break;
            }
        }

        private void AddTexture (string key, TextureResource source)
        {
            if (key == null || source == null)
                return;

            TextureResource tex = source.Crop(source.Bounds);
            _textures[key] = tex;
            _xnaTextures[key] = null;
        }

        private void RemoveTexture (string key, TextureResource source)
        {
            if (key == null || source == null)
                return;

            _textures.Remove(key);
            _xnaTextures.Remove(key);
        }

        #endregion
    }
}
