using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Treefrog.Framework.Model;

namespace Treefrog.V2.ViewModel.Layers
{
    public class GroupLayerVM : LayerVM
    {
        private string _name;
        private bool _visible;

        private ObservableCollection<LayerVM> _layers;

        public GroupLayerVM ()
        {
            _name = "Group";
            _visible = true;
            _layers = new ObservableCollection<LayerVM>();
        }

        public override string LayerName
        {
            get { return _name; }
            set
            {
                if (_name != value) {
                    _name = value;
                    RaisePropertyChanged("LayerName");
                }
            }
        }

        public override bool IsVisible
        {
            get { return _visible; }
            set
            {
                if (_visible != value) {
                    _visible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        public virtual ObservableCollection<LayerVM> Layers
        {
            get { return _layers; }
        }
    }
}
