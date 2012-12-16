using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation.Annotations;
using Treefrog.Windows.Annotations;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Windows.Controls;

namespace Treefrog.Presentation.Layers
{
    public class AnnotationLayer : BaseControlLayer
    {
        public AnnotationLayer (LayerControl control)
            : base(control)
        {
        }

        public override int VirtualHeight
        {
            get { return 0; }
        }

        public override int VirtualWidth
        {
            get { return 0; }
        }

        protected Vector2 BeginDraw (SpriteBatch spriteBatch)
        {
            Rectangle region = Control.VisibleRegion;

            Vector2 offset = Control.VirtualSurfaceOffset;
            offset.X = (float)Math.Ceiling(offset.X - region.X * Control.Zoom);
            offset.Y = (float)Math.Ceiling(offset.Y - region.Y * Control.Zoom);

            //offset.X -= Control.OriginX;
            //offset.Y -= Control.OriginY;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(offset.X, offset.Y, 0));

            return offset;
        }

        protected void EndDraw (SpriteBatch spriteBatch)
        {
            spriteBatch.End();
        }

        public new void DrawContent (SpriteBatch spriteBatch)
        {
            if (Visible == false)
                return;

            Rectangle region = Control.VisibleRegion;

            Vector2 offset = BeginDraw(spriteBatch);

            foreach (AnnotationRenderer renderer in _annotCache.Values)
                renderer.Render(spriteBatch, (float)Control.Zoom);

            EndDraw(spriteBatch);
        }

        /*protected override void DisposeManaged ()
        {
            ClearAnnotationCache();

            if (Annotations != null)
                Annotations.CollectionChanged -= HandleAnnotCollectionChanged;

            base.DisposeManaged();
        }*/

        /*protected override void RenderCore (Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Vector offset = BeginDraw(spriteBatch);
            Rect region = VisibleRegion;

            foreach (AnnotationRenderer renderer in _annotCache.Values)
                renderer.Render(spriteBatch, (float)ZoomFactor);

            EndDraw(spriteBatch, offset);
        }*/

        #region Annotation Management

        private ObservableCollection<Annotation> _annotations;
        private Dictionary<Annotation, AnnotationRenderer> _annotCache = new Dictionary<Annotation, AnnotationRenderer>();

        private void AddToAnnotationCache (Annotation key)
        {
            if (_annotCache.ContainsKey(key))
                RemoveFromAnnotationCache(key);

            _annotCache.Add(key, AnnotationRendererFactory.Create(key));
        }

        private void RemoveFromAnnotationCache (Annotation key)
        {
            AnnotationRenderer renderer;
            if (_annotCache.TryGetValue(key, out renderer))
                renderer.Dispose();

            _annotCache.Remove(key);
        }

        private void ClearAnnotationCache ()
        {
            foreach (AnnotationRenderer renderer in _annotCache.Values)
                if (renderer != null)
                    renderer.Dispose();

            _annotCache.Clear();
        }

        public ObservableCollection<Annotation> Annotations
        {
            get { return _annotations; }
            set {
                ClearAnnotationCache();

                if (_annotations != null)
                    _annotations.CollectionChanged -= HandleAnnotCollectionChanged;

                _annotations = value;
                if (_annotations != null) {
                    _annotations.CollectionChanged += HandleAnnotCollectionChanged;

                    foreach (Annotation item in _annotations)
                        AddToAnnotationCache(item);
                }
            }
        }

        private void HandleAnnotCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (Annotation item in e.NewItems)
                        AddToAnnotationCache(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Annotation item in e.OldItems)
                        RemoveFromAnnotationCache(item);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearAnnotationCache();
                    foreach (Annotation item in sender as ObservableCollection<Annotation>)
                        AddToAnnotationCache(item);
                    break;
            }
        }

        #endregion
    }
}
