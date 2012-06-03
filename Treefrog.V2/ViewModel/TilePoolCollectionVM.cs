using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Treefrog.Controls.Xna;
using Treefrog.Framework.Model;
using System.Windows.Input;
using Treefrog.Framework;
using GalaSoft.MvvmLight.Command;
using Treefrog.ViewModel.Layers;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;
using Treefrog.ViewModel.Dialogs;
using Treefrog.Messages;
using GalaSoft.MvvmLight.Messaging;
using Treefrog.Aux;
using Treefrog.Framework.Imaging;
using System.Drawing;
using System.ComponentModel;
using System.IO;

namespace Treefrog.ViewModel
{
    public interface TilePoolManagerService : INotifyPropertyChanged
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

                ChangeActiveTilePool(null);

                _tilePools.Clear();

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

        public TextureResource SelectedBitmapSource
        {
            get
            {
                if (_activeTilePool == null)
                    return null;
                return _activeTilePool.BitmapSource;
            }
        }

        #region TilePoolManagerService

        private TilePoolVM _activeTilePool;

        public TilePoolVM ActiveTilePool
        {
            get { return _activeTilePool; }
            set
            {
                if (_activeTilePool != value) {
                    ChangeActiveTilePool(value);
                    RefreshCommandState();

                    if (_activeTilePool != null) {
                        PropertyManagerService service = ServiceContainer.Default.GetService<PropertyManagerService>();
                        service.ActiveProvider = _manager.Pools[_activeTilePool.Name];
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

        private void ChangeActiveTilePool (TilePoolVM newPool)
        {
            if (_activeTilePool != null)
                _activeTilePool.PropertyChanged -= HandleActiveTilePropertyChanged;

            _activeTilePool = newPool;
            if (_activeTilePool != null)
                _activeTilePool.PropertyChanged += HandleActiveTilePropertyChanged;

            RaisePropertyChanged("ActiveTilePool");
            RaisePropertyChanged("ActiveTile");
            RaisePropertyChanged("SelectedBitmapSource");
        }

        private void HandleActiveTilePropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTile")
                RaisePropertyChanged("ActiveTile");
        }

        #endregion

        #region Commands

        #region Import Pool

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
                    ImportPolicty = TileImportPolicy.SetUnique,
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

                    if (vm.ImportMode == ImportTilePoolDialogVM.TileImportMode.Create) {
                        _manager.ImportTilePool(vm.TilePoolName, resource, options);
                    }
                    else if (vm.ImportMode == ImportTilePoolDialogVM.TileImportMode.Merge) {
                        TilePoolVM mergeTarget = null;
                        foreach (TilePoolVM pool in _tilePools) {
                            if (pool.Name == vm.MergeTarget) {
                                mergeTarget = pool;
                                break;
                            }
                        }

                        if (mergeTarget != null) {
                            mergeTarget.TilePool.ImportMerge(resource, options);
                        }
                    }
                }
            }
        }

        #endregion

        #region Remove Pool

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

        #region Export Raw Tileset

        private RelayCommand _exportRawTilesetCommand;

        public ICommand ExportRawTilesetCommand
        {
            get
            {
                if (_exportRawTilesetCommand == null)
                    _exportRawTilesetCommand = new RelayCommand(OnExportRawTileset, CanExportRawTileset);
                return _exportRawTilesetCommand;
            }
        }

        public bool IsExportRawTilesetEnabled
        {
            get { return CanExportRawTileset(); }
        }

        private bool CanExportRawTileset ()
        {
            return ActiveTilePool != null;
        }

        private void OnExportRawTileset ()
        {
            if (ActiveTilePool != null) {
                Bitmap export = ActiveTilePool.TilePool.TileSource.CreateBitmap();

                IOService service = ServiceProvider.GetService<IOService>();
                if (service != null) {
                    string path = service.SaveFileDialog(new SaveFileOptions()
                    {
                        Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files|*",
                        FilterIndex = 0,
                    });

                    export.Save(path);
                }
            }
        }

        #endregion

        #region Import Raw Tileset

        private RelayCommand _importRawTilesetCommand;

        public ICommand ImportRawTilesetCommand
        {
            get
            {
                if (_importRawTilesetCommand == null)
                    _importRawTilesetCommand = new RelayCommand(OnImportRawTileset, CanImportRawTileset);
                return _importRawTilesetCommand;
            }
        }

        public bool IsImportRawTilesetEnabled
        {
            get { return CanImportRawTileset(); }
        }

        private bool CanImportRawTileset ()
        {
            return ActiveTilePool != null;
        }

        private void OnImportRawTileset ()
        {
            if (ActiveTilePool != null) {
                Bitmap export = ActiveTilePool.TilePool.TileSource.CreateBitmap();

                IMessageService messageService = ServiceProvider.GetService<IMessageService>();
                IOService service = ServiceProvider.GetService<IOService>();
                if (service != null) {
                    string path = service.OpenFileDialog(new OpenFileOptions()
                    {
                        Filter = "Images Files|*.bmp;*.gif;*.png|All Files|*",
                        FilterIndex = 0,
                    });

                    try {
                        TextureResource import = TextureResourceBitmapExt.CreateTextureResource(path);

                        TextureResource original = ActiveTilePool.TilePool.TileSource;
                        if (original.Width != import.Width || original.Height != import.Height) {
                            if (messageService != null) {
                                messageService.ShowMessage(new MessageInfo()
                                {
                                    Message = "Imported tileset dimensions are incompatible with the selected Tile Pool.",
                                    Type = MessageType.Warning
                                });
                            }
                            return;
                        }

                        ActiveTilePool.TilePool.ReplaceTexture(import);
                    }
                    catch {
                        if (messageService != null) {
                            messageService.ShowMessage(new MessageInfo()
                            {
                                Message = "Could not read selected image file.",
                                Type = MessageType.Warning
                            });
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
