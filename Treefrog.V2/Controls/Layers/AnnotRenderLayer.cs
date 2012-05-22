using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using Treefrog.ViewModel.Annotations;
using Treefrog.Controls.Annotations;
using System.Collections.Specialized;

namespace Treefrog.Controls.Layers
{
    public class AnnotRenderLayer : RenderLayer
    {
        protected override void RenderCore (Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            Vector offset = BeginDraw(spriteBatch);
            Rect region = VisibleRegion;

            foreach (AnnotationRenderer renderer in _annotCache.Values)
                renderer.Render(spriteBatch, (float)ZoomFactor);

            EndDraw(spriteBatch, offset);
        }

        #region Annotation Management

        private Dictionary<Annotation, AnnotationRenderer> _annotCache = new Dictionary<Annotation, AnnotationRenderer>();

        public static readonly DependencyProperty AnnotationsProperty = DependencyProperty.Register("Annotations",
            typeof(ObservableCollection<Annotation>), typeof(AnnotRenderLayer), new PropertyMetadata(null, HandleAnnotationsChanged));

        public ObservableCollection<Annotation> Annotations
        {
            get { return (ObservableCollection<Annotation>)GetValue(AnnotationsProperty); }
            set { SetValue(AnnotationsProperty, value); }
        }

        private static void HandleAnnotationsChanged (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AnnotRenderLayer self = sender as AnnotRenderLayer;
            if (self != null) {
                self._annotCache.Clear();

                if (e.OldValue != null) {
                    ObservableCollection<Annotation> annots = e.OldValue as ObservableCollection<Annotation>;
                    annots.CollectionChanged -= self.HandleAnnotCollectionChanged;
                }

                if (e.NewValue != null) {
                    ObservableCollection<Annotation> annots = e.NewValue as ObservableCollection<Annotation>;
                    annots.CollectionChanged += self.HandleAnnotCollectionChanged;

                    foreach (Annotation item in annots)
                        self._annotCache[item] = AnnotationRendererFactory.Create(item);
                }
            }
        }

        private void HandleAnnotCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (Annotation item in e.NewItems)
                        _annotCache[item] = AnnotationRendererFactory.Create(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Annotation item in e.OldItems)
                        _annotCache.Remove(item);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _annotCache.Clear();
                    foreach (Annotation item in sender as ObservableCollection<Annotation>)
                        _annotCache[item] = AnnotationRendererFactory.Create(item);
                    break;
            }
        }

        #endregion
    }
}
