using System;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Presentation.Layers
{
    public class AnnotationLayerPresenter : RenderLayerPresenter
    {
        private ObservableCollection<Annotation> _annotations;

        public ObservableCollection<Annotation> Annotations
        {
            get { return _annotations; }
            set
            {
                if (_annotations != value) {
                    _annotations = value;
                    OnAnnotationsCollectionChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler AnnotationsCollectionChanged;

        protected virtual void OnAnnotationsCollectionChanged (EventArgs e)
        {
            var ev = AnnotationsCollectionChanged;
            if (ev != null)
                ev(this, e);
        }
    }
}
