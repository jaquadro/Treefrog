using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.V2.ViewModel
{
    public enum LayerType
    {
        Generic,
        Tile,
        Object,
    }

    public class LayerItemVM : ViewModelBase
    {
        private OrderedResourceCollection<Layer> _layers;
        private Layer _layer;
        private Func<Layer, int> _orderFunc;

        public LayerItemVM (Layer layer, OrderedResourceCollection<Layer> layers)
        {
            _layer = layer;
            _layers = layers;

            _layer.NameChanged += HandleNameChanged;
            _layer.VisibilityChanged += HandleVisibilityChanged;
        }

        public LayerItemVM (Layer layer, Func<Layer, int> orderFunc)
            : this(layer, new OrderedResourceCollection<Layer>())
        {
            _orderFunc = orderFunc;
        }

        public string LayerName
        {
            get { return _layer.Name; }
            set
            {
                if (_layer.Name != value) {
                    _layer.Name = value;
                    RaisePropertyChanged("LayerName");
                }
            }
        }

        public bool IsVisible
        {
            get { return _layer.IsVisible; }
            set
            {
                if (_layer.IsVisible != value) {
                    _layer.IsVisible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        private void HandleVisibilityChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("IsVisible");
        }

        private void HandleNameChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("LayerName");
        }

        public LayerType LayerType
        {
            get
            {
                if (_layer is TileLayer)
                    return LayerType.Tile;
                else if (_layer is ObjectLayer)
                    return LayerType.Object;
                else
                    return LayerType.Generic;
            }
        }

        public int OrderIndex
        {
            get
            {
                return _layers.IndexOf(_layer.Name);
                /*if (_orderFunc == null)
                    return 0;
                return _orderFunc(_layer);*/
            }
        }

        #region Commands

        private RelayCommand _visibilityCommand;

        public ICommand VisibilityCommand
        {
            get
            {
                if (_visibilityCommand == null)
                    _visibilityCommand = new RelayCommand(OnExecuteVisibility, CanExecuteVisibility);
                return _visibilityCommand;
            }
        }

        private bool CanExecuteVisibility ()
        {
            return true;
        }

        private void OnExecuteVisibility ()
        {
            IsVisible = !IsVisible;
        }

        #endregion
    }
}
