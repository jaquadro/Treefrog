using System.Collections.ObjectModel;

namespace Treefrog.Presentation.Layers
{
    public class GroupLayerPresenter : LayerPresenter
    {
        private ObservableCollection<LayerPresenter> _layers;

        public GroupLayerPresenter ()
        {
            _layers = new ObservableCollection<LayerPresenter>();
        }

        public virtual ObservableCollection<LayerPresenter> Layers
        {
            get { return _layers; }
        }
    }
}
