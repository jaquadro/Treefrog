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

using TextureResource = Treefrog.Framework.Imaging.TextureResource;
using Treefrog.V2.ViewModel.Dialogs;
using Treefrog.V2.Messages;
using GalaSoft.MvvmLight.Messaging;
using Treefrog.Aux;
using Treefrog.Framework.Imaging;
using System.Drawing;

namespace Treefrog.V2.ViewModel
{
    public interface TilePoolManagerService
    {
        TilePoolVM ActiveTilePool { get; }
        TilePoolItemVM ActiveTile { get; }
    }

    public class TilePoolCollectionVM : ViewModelBase, TilePoolManagerService, IDisposable
    {
        private TilePoolManager _manager;
        private ObservableCollection<TilePoolVM> _tilePools;

        public TilePoolCollectionVM ()
            : base()
        {
            _tilePools = new ObservableCollection<TilePoolVM>();
        }

        public TilePoolCollectionVM (TilePoolManager manager)
            : base()
        {
            _manager = manager;
            _tilePools = new ObservableCollection<TilePoolVM>();

            foreach (TilePool pool in manager.Pools) {
                _tilePools.Add(new TilePoolVM(pool));
            }

            if (manager.Pools.Count > 0)
                ActiveTilePool = _tilePools.First();

            manager.Pools.ResourceAdded += HandlePoolAdded;
            manager.Pools.ResourceRemoved += HandlePoolRemoved;
        }

        public void Dispose ()
        {
            if (_manager != null) {
                _manager.Pools.ResourceAdded -= HandlePoolAdded;
                _manager.Pools.ResourceRemoved -= HandlePoolRemoved;
                _manager = null;

                _tilePools.Clear();
                ActiveTilePool = null;

                RefreshCommandState();
            }
        }

        private void RefreshCommandState ()
        {
            RaisePropertyChanged("HasTilePools");
            RaisePropertyChanged("IsImportPoolEnabled");
            RaisePropertyChanged("IsRemovePoolEnabled");

            if (_importPoolCommand != null)
                _importPoolCommand.RaiseCanExecuteChanged();
            if (_removePoolCommand != null)
                _removePoolCommand.RaiseCanExecuteChanged();
        }

        private void HandlePoolAdded (object sender, NamedResourceEventArgs<TilePool> e)
        {
            _tilePools.Add(new TilePoolVM(e.Resource));
            ActiveTilePool = _tilePools.Last();

            RefreshCommandState();
        }

        private void HandlePoolRemoved (object sender, NamedResourceEventArgs<TilePool> e)
        {
            foreach (TilePoolVM vm in _tilePools) {
                if (vm.Name == e.Resource.Name) {
                    _tilePools.Remove(vm);

                    if (ActiveTilePool == vm)
                        ActiveTilePool = (_tilePools.Count > 0) ? _tilePools.First() : null;

                    RefreshCommandState();
                    break;
                }
            }
        }

        public ObservableCollection<TilePoolVM> TilePools
        {
            get { return _tilePools; }
        }

        public bool HasTilePools
        {
            get { return _tilePools.Count > 0; }
        }

        private TilePoolVM _selected;

        public TilePoolVM ActiveTilePool
        {
            get { return _selected; }
            set
            {
                if (_selected != value) {
                    _selected = value;
                    RaisePropertyChanged("ActiveTilePool");
                    RaisePropertyChanged("SelectedBitmapSource");
                    RefreshCommandState();

                    if (_selected != null) {
                        PropertyManagerService service = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<PropertyManagerService>();
                        service.ActiveProvider = _manager.Pools[_selected.Name];
                    }
                }
            }
        }

        public TilePoolItemVM ActiveTile
        {
            get 
            {
                TilePoolVM pool = ActiveTilePool;
                if (pool != null)
                    return pool.SelectedTile;
                return null;
            }
        }

        public TextureResource SelectedBitmapSource
        {
            get
            {
                if (_selected == null)
                    return null;
                return _selected.BitmapSource;
            }
        }

        #region Commands

        private RelayCommand _importPoolCommand;

        public ICommand ImportPoolCommand
        {
            get
            {
                if (_importPoolCommand == null)
                    _importPoolCommand = new RelayCommand(OnImportPool, CanImportPool);
                return _importPoolCommand;
            }
        }

        public bool IsImportPoolEnabled
        {
            get { return CanImportPool(); }
        }

        private bool CanImportPool ()
        {
            return _manager != null;
        }

        private void OnImportPool ()
        {
            ImportTilePoolDialogVM vm = new ImportTilePoolDialogVM();
            foreach (TilePoolVM pool in _tilePools)
                vm.ReservedNames.Add(pool.Name);

            BlockingDialogMessage message = new BlockingDialogMessage(this, vm);
            Messenger.Default.Send(message);

            if (message.DialogResult == true) {
                TilePool.TileImportOptions options = new TilePool.TileImportOptions()
                {
                    TileWidth = vm.TileWidth ?? 0,
                    TileHeight = vm.TileHeight ?? 0,
                    SpaceX = vm.TileSpaceX ?? 0,
                    SpaceY = vm.TileSpaceY ?? 0,
                    MarginX = vm.TileMarginX ?? 0,
                    MarginY = vm.TileMarginY ?? 0,
                    ImportPolicty = TileImportPolicy.ImprotAll,
                };

                using (Bitmap source = new Bitmap(vm.SourceFile)) {
                    TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(source);
                    if (vm.UseTransparentColor) {
                        resource.Apply(c =>
                        {
                            if (c.Equals(vm.TransparentColor))
                                return Colors.Transparent;
                            else
                                return c;
                        });
                    }

                    _manager.ImportTilePool(vm.TilePoolName, resource, options);
                }
            }
        }

        private RelayCommand _removePoolCommand;

        public ICommand RemovePoolCommand
        {
            get
            {
                if (_removePoolCommand == null)
                    _removePoolCommand = new RelayCommand(OnRemovePool, CanRemovePool);
                return _removePoolCommand;
            }
        }

        public bool IsRemovePoolEnabled
        {
            get { return CanRemovePool(); }
        }

        private bool CanRemovePool ()
        {
            return ActiveTilePool != null;
        }

        private void OnRemovePool ()
        {
            if (ActiveTilePool != null) {
                _manager.Pools.Remove(ActiveTilePool.Name);
            }
        }

        #endregion
    }
}
