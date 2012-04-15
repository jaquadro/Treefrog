using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Treefrog.V2.Controls.Xna;
using Treefrog.Framework.Model;
using System.Windows.Input;
using Treefrog.Framework;
using GalaSoft.MvvmLight.Command;
using Treefrog.V2.ViewModel.Layers;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Specialized;
using Treefrog.V2.ViewModel.Utility;

namespace Treefrog.V2.ViewModel
{
    public class LayerCollectionVM : ViewModelBase
    {
        //private LevelGroupLayerVM _layerGroup;
        private OrderedResourceCollection<Layer> _layers;

        private OrderedToObservableAdapter<Layer, LayerItemVM> _adapter;

        public LayerCollectionVM (OrderedResourceCollection<Layer> layers, LevelGroupLayerVM layerGroup)
        {
            //_layerGroup = layerGroup;
            _layers = layers;

            _adapter = new OrderedToObservableAdapter<Layer, LayerItemVM>(layer =>
            {
                return new LayerItemVM(layer, _layers);
            }, layers);

            _collectionView = CollectionViewSource.GetDefaultView(Layers);
            _collectionView.SortDescriptions.Add(new SortDescription("OrderIndex", ListSortDirection.Descending));

            _collectionView.CurrentChanged += (sender, e) => { OnSelectionChanged(); };
        }

        private ICollectionView _collectionView;
        public ObservableCollection<LayerItemVM> Layers
        {
            get
            {
                //if (_layerGroup == null)
                //    return null;
                //return _layerGroup.Layers;
                return _adapter.Dependent;
            }
        }

        private LayerItemVM _selectedLayer;

        public LayerItemVM SelectedLayer
        {
            get { return _collectionView.CurrentItem as LayerItemVM; }
            set
            {
                if (_selectedLayer != value) {
                    _selectedLayer = value;
                    OnSelectionChanged();
                }
            }
        }

        private void OnSelectionChanged ()
        {
            //RaisePropertyChanged("SelectedLayer");
            if (_deleteCommand != null)
                _deleteCommand.RaiseCanExecuteChanged();
            if (_moveUpCommand != null)
                _moveUpCommand.RaiseCanExecuteChanged();
            if (_moveDownCommand != null)
                _moveDownCommand.RaiseCanExecuteChanged();
        }

        #region Commands

        #region Delete Command

        private RelayCommand _deleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(OnExecuteDelete, CanExecuteDelete);
                return _deleteCommand;
            }
        }

        private bool CanExecuteDelete ()
        {
            return SelectedLayer != null;
        }

        private void OnExecuteDelete ()
        {
            if (SelectedLayer != null)
                _layers.Remove(SelectedLayer.LayerName);
        }

        #endregion

        #region Move Up Command

        private RelayCommand _moveUpCommand;

        public ICommand MoveUpCommand
        {
            get
            {
                if (_moveUpCommand == null)
                    _moveUpCommand = new RelayCommand(OnExecuteMoveUp, CanExecuteMoveUp);
                return _moveUpCommand;
            }
        }

        private bool CanExecuteMoveUp ()
        {
            return SelectedLayer != null && SelectedLayer.LayerName != _layers.Last().Name;
        }

        private void OnExecuteMoveUp ()
        {
            if (CanExecuteMoveUp())
                _layers.ChangeIndexRelative(SelectedLayer.LayerName, 1);
            _collectionView.Refresh();
        }

        #endregion

        #region Move Down Command

        private RelayCommand _moveDownCommand;

        public ICommand MoveDownCommand
        {
            get
            {
                if (_moveDownCommand == null)
                    _moveDownCommand = new RelayCommand(OnExecuteMoveDown, CanExecuteMoveDown);
                return _moveDownCommand;
            }
        }

        private bool CanExecuteMoveDown ()
        {
            return SelectedLayer != null && SelectedLayer.LayerName != _layers.First().Name;
        }

        private void OnExecuteMoveDown ()
        {
            if (CanExecuteMoveDown())
                _layers.ChangeIndexRelative(SelectedLayer.LayerName, -1);
            _collectionView.Refresh();
        }

        #endregion

        #endregion
    }
}