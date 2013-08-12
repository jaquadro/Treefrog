using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Treefrog.Extensibility;
using Treefrog.Framework;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.Model;
using Treefrog.Plugins.Tiles.Layers;
using Treefrog.Presentation;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Layers;

namespace Treefrog.Plugins.Tiles
{
    public class SyncTilePoolEventArgs : EventArgs
    {
        public TilePool PreviousTilePool { get; private set; }

        public SyncTilePoolEventArgs (TilePool tilePool)
        {
            PreviousTilePool = tilePool;
        }
    }

    public class TilePoolPresenter : IPointerResponderProvider, IDisposable
    {
        private TilePool _tilePool;
        private TileSetLayer _tileSet;

        private GroupLayerPresenter _rootLayer;
        private TileSetLayerPresenter _tileLayer;
        private GridLayerPresenter _gridLayer;
        private AnnotationLayerPresenter _annotLayer;
        private ILevelGeometry _levelGeometry;

        private Tile _selectedTile;

        private ObservableCollection<Annotation> _annotations;

        public TilePoolPresenter (TilePool tilePool)
        {
            _tilePool = tilePool;
            _tilePool.TileRemoved += TilePoolTileRemoved;

            _tileSet = new TileSetLayer(tilePool.Name, tilePool);

            _annotations = new ObservableCollection<Annotation>();

            InitializeLayerHierarchy();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (_tilePool != null) {
                if (disposing) {
                    _tileSet.Dispose();
                }

                _tilePool.TileRemoved -= TilePoolTileRemoved;
                _tilePool = null;
            }
        }

        public TilePool TilePool
        {
            get { return _tilePool; }
        }

        public GroupLayerPresenter RootLayer
        {
            get { return _rootLayer; }
        }

        public TileSetLayerPresenter TileLayer
        {
            get { return _tileLayer; }
        }

        public Tile SelectedTile
        {
            get { return _selectedTile; }
        }

        public ILevelGeometry LevelGeometry
        {
            get { return _levelGeometry; }
            set
            {
                _levelGeometry = value;
                _tileLayer.LevelGeometry = value;
            }
        }

        private void TilePoolTileRemoved (object sender, TileEventArgs e)
        {
            if (_selectedTile == e.Tile)
                SelectTile(null);
        }

        private void InitializeLayerHierarchy ()
        {
            _tileLayer = new TileSetLayerPresenter(_tileSet);
            _tileLayer.TileSelected += (s, e) => SelectTile(e.Tile);

            _gridLayer = new GridLayerPresenter() {
                GridSpacingX = _tilePool.TileWidth,
                GridSpacingY = _tilePool.TileHeight,
            };
            _annotLayer = new AnnotationLayerPresenter() {
                Annotations = _annotations,
            };

            _rootLayer = new GroupLayerPresenter();
            _rootLayer.Layers.Add(_tileLayer);
            _rootLayer.Layers.Add(_annotLayer);
        }

        public void SelectTile (Tile tile)
        {
            _selectedTile = tile;

            _annotations.Clear();

            if (_selectedTile != null) {
                TileCoord location = _tileLayer.TileToCoord(tile);
                int x = location.X * _tileSet.TileWidth;
                int y = location.Y * _tileSet.TileHeight;

                SelectionAnnot annot = new SelectionAnnot() {
                    Start = new Treefrog.Framework.Imaging.Point(x, y),
                    End = new Treefrog.Framework.Imaging.Point(x + _tileSet.TileWidth, y + _tileSet.TileHeight),
                    Fill = new SolidColorBrush(new Treefrog.Framework.Imaging.Color(192, 0, 0, 128)),
                };

                _annotations.Add(annot);
            }

            OnSelectedTileChanged(EventArgs.Empty);
        }

        public event EventHandler SelectedTileChanged;

        protected virtual void OnSelectedTileChanged (EventArgs e)
        {
            var ev = SelectedTileChanged;
            if (ev != null)
                ev(this, e);
        }

        public IPointerResponder PointerEventResponder
        {
            get { return _tileLayer; }
        }

        public event EventHandler PointerEventResponderChanged;

        protected virtual void OnPointerEventResponderChanged (EventArgs e)
        {
            var ev = PointerEventResponderChanged;
            if (ev != null)
                ev(this, e);
        }
    }

    public class TilePoolListPresenter : Presenter, ICommandSubscriber
    {
        private EditorPresenter _editor;
        private ITilePoolManager _poolManager;

        private TilePoolCommandActions _tilePoolActions;

        private Dictionary<Guid, TilePoolPresenter> _tilePoolPresenters;
        private Guid _selectedPool;
        private TilePoolPresenter _selectedPoolRef;

        public TilePoolListPresenter ()
        {
            _tilePoolPresenters = new Dictionary<Guid, TilePoolPresenter>();
        }

        protected override void InitializeCore ()
        {
            InitializeCommandManager();

            OnAttach<EditorPresenter>(editor => {
                _editor = editor;
                _editor.SyncCurrentProject += EditorSyncCurrentProject;
            });

            OnDetach<EditorPresenter>(editor => {
                BindTilePoolManager(null);

                _editor.SyncCurrentProject -= EditorSyncCurrentProject;
                _editor = null;
            });

            // Temporary until exported by MEF??
            OnAttach<ProjectExplorerPresenter>(p => {
                TileSetExplorerComponent tilesetExplorer = new TileSetExplorerComponent();
                tilesetExplorer.Bind(this);
                p.Components.Register(tilesetExplorer);
            });
        }

        /*public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }*/

        /*protected virtual void Dispose (bool disposing)
        {
            if (_editor != null) {
                if (disposing) {
                    BindTilePoolManager(null);
                    _editor.SyncCurrentProject -= EditorSyncCurrentProject;
                }

                _editor = null;
            }
        }*/

        private void EditorSyncCurrentProject (object sender, SyncProjectEventArgs e)
        {
            if (_editor.Project != null)
                BindTilePoolManager(_editor.Project.TilePoolManager);
            else
                BindTilePoolManager(null);
        }

        public void BindTilePoolManager (ITilePoolManager manager)
        {
            if (_poolManager != null) {
                _poolManager.PoolAdded -= TilePoolAdded;
                _poolManager.PoolRemoved -= TilePoolRemoved;
                _poolManager.PoolModified -= TilePoolModified;
            }

            _poolManager = manager;
            if (_poolManager != null) {
                _poolManager.PoolAdded += TilePoolAdded;
                _poolManager.PoolRemoved += TilePoolRemoved;
                _poolManager.PoolModified += TilePoolModified;

                InitializePoolPresenters();
            }
            else {
                ClearPoolPresenters();
            }

            OnSyncTilePoolManager(EventArgs.Empty);
        }

        public ITilePoolManager TilePoolManager
        {
            get { return _poolManager; }
        }

        public IEnumerable<TilePoolPresenter> TilePoolList
        {
            get { return _tilePoolPresenters.Values; }
        }

        public TilePoolPresenter SelectedTilePool
        {
            get { return _selectedPoolRef; }
        }

        public Tile SelectedTile
        {
            get { return _selectedPoolRef != null ? _selectedPoolRef.SelectedTile : null; }
        }

        private void ClearPoolPresenters ()
        {
            foreach (TilePoolPresenter presenter in _tilePoolPresenters.Values)
                presenter.Dispose();

            _tilePoolPresenters.Clear();
            _selectedPool = Guid.Empty;
            _selectedPoolRef = null;
        }

        private void InitializePoolPresenters ()
        {
            ClearPoolPresenters();

            foreach (TilePool pool in _poolManager.Pools)
                AddPoolPresenter(pool);

            SelectTilePool();

            OnSelectedTilePoolChanged(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
        }

        private void AddPoolPresenter (TilePool pool)
        {
            TilePoolPresenter presenter = new TilePoolPresenter(pool);
            presenter.SelectedTileChanged += SelectedTileChanged;

            _tilePoolPresenters[pool.Uid] = presenter;
        }

        private void RemovePoolPresenter (Guid poolUid)
        {
            TilePoolPresenter presenter;
            if (_tilePoolPresenters.TryGetValue(poolUid, out presenter))
                presenter.Dispose();

            _tilePoolPresenters.Remove(poolUid);
        }

        private void SelectedTileChanged (object sender, EventArgs e)
        {
            if (sender == _selectedPoolRef) {
                OnTileSelectionChanged(EventArgs.Empty);
                _editor.Presentation.PropertyList.Provider = SelectedTile;

                _commandManager.Invalidate(CommandKey.TileProperties);
                _commandManager.Invalidate(CommandKey.TileDelete);
            }
        }

        private void SelectTilePool ()
        {
            SelectTilePool(Guid.Empty);

            foreach (TilePool pool in _poolManager.Pools) {
                SelectTilePool(pool.Uid);
                return;
            }
        }

        private void SelectTilePool (Guid poolUid)
        {
            if (_selectedPool == poolUid)
                return;

            if (poolUid == null || !_tilePoolPresenters.ContainsKey(poolUid)) {
                _selectedPool = Guid.Empty;
                _selectedPoolRef = null;
                _commandManager.Invalidate(CommandKey.TilePoolProperties);

                return;
            }

            _selectedPool = poolUid;
            _selectedPoolRef = _tilePoolPresenters[poolUid];

            _commandManager.Invalidate(CommandKey.TilePoolProperties);
            _commandManager.Invalidate(CommandKey.TilePoolDelete);
            _commandManager.Invalidate(CommandKey.TilePoolImportMerge);
            _commandManager.Invalidate(CommandKey.TilePoolRename);
            _commandManager.Invalidate(CommandKey.TilePoolExport);
            _commandManager.Invalidate(CommandKey.TilePoolImportOver);

            OnSelectedTilePoolChanged(EventArgs.Empty);
        }

        private void TilePoolAdded (object sender, ResourceEventArgs<TilePool> e)
        {
            AddPoolPresenter(e.Resource);

            SelectTilePool(e.Uid);

            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        private void TilePoolRemoved (object sender, ResourceEventArgs<TilePool> e)
        {
            RemovePoolPresenter(e.Resource.Uid);

            if (_selectedPool == e.Uid)
                SelectTilePool();

            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        private void TilePoolModified (object sender, ResourceEventArgs<TilePool> e)
        {
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        #region Command Handling

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();

            _commandManager.Register(CommandKey.TileProperties, CommandCanTileProperties, CommandTileProperties);
            _commandManager.Register(CommandKey.TileDelete, CommandCanDeleteTile, CommandDeleteTile);

            _tilePoolActions = new TilePoolCommandActions(Manager);
            _commandManager.Register(CommandKey.TilePoolImport, () => { return true; }, _tilePoolActions.CommandImport);
            _commandManager.Register(CommandKey.TilePoolImportMerge, CommandCanOperateOnSelected, WrapCommand(_tilePoolActions.CommandImportMerge));
            _commandManager.Register(CommandKey.TilePoolDelete, CommandCanOperateOnSelected, WrapCommand(_tilePoolActions.CommandDelete));
            _commandManager.Register(CommandKey.TilePoolRename, CommandCanOperateOnSelected, WrapCommand(_tilePoolActions.CommandRename));
            _commandManager.Register(CommandKey.TilePoolProperties, CommandCanOperateOnSelected, WrapCommand(_tilePoolActions.CommandProperties));
            _commandManager.Register(CommandKey.TilePoolExport, CommandCanOperateOnSelected, WrapCommand(_tilePoolActions.CommandExport));
            _commandManager.Register(CommandKey.TilePoolImportOver, CommandCanOperateOnSelected, WrapCommand(_tilePoolActions.CommandImportOver));
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanOperateOnSelected ()
        {
            return _tilePoolActions.TilePoolExists(_selectedPool);
        }

        private System.Action WrapCommand (Action<object> action)
        {
            return () => action(_selectedPool);
        }

        private bool CommandCanTileProperties ()
        {
            return _selectedPoolRef != null && _selectedPoolRef.SelectedTile != null;
        }

        private void CommandTileProperties ()
        {
            if (CommandCanTileProperties()) {
                _editor.Presentation.PropertyList.Provider = _selectedPoolRef.SelectedTile;
                _editor.ActivatePropertyPanel();
            }
        }

        private bool CommandCanDeleteTile ()
        {
            return _selectedPoolRef != null && _selectedPoolRef.SelectedTile != null;
        }

        private void CommandDeleteTile ()
        {
            if (CommandCanDeleteTile()) {
                foreach (Level level in _editor.Project.Levels) {
                    foreach (TileGridLayer layer in level.Layers) {
                        layer.RemoveAllMatchingTiles(_selectedPoolRef.SelectedTile);
                    }
                }

                _selectedPoolRef.TilePool.Tiles.Remove(_selectedPoolRef.SelectedTile.Uid);
            }
        }

        #endregion

        public void ActionSelectTilePool (Guid poolUid)
        {
            if (_selectedPool != poolUid) {
                SelectTilePool(poolUid);

                OnSyncTilePoolList(EventArgs.Empty);

                if (SelectedTilePool != null)
                    _editor.Presentation.PropertyList.Provider = SelectedTilePool.TilePool;
            }
        }

        public event EventHandler SyncTilePoolManager;
        public event EventHandler SyncTilePoolList;
        public event EventHandler SyncTilePoolControl;
        public event EventHandler SelectedTilePoolChanged;
        public event EventHandler TileSelectionChanged;

        protected virtual void OnSyncTilePoolManager (EventArgs e)
        {
            if (SyncTilePoolManager != null) {
                SyncTilePoolManager(this, e);
            }
        }

        protected virtual void OnSyncTilePoolList (EventArgs e)
        {
            if (SyncTilePoolList != null) {
                SyncTilePoolList(this, e);
            }
        }

        protected virtual void OnSyncTilePoolControl (EventArgs e)
        {
            if (SyncTilePoolControl != null) {
                SyncTilePoolControl(this, e);
            }
        }

        protected virtual void OnSelectedTilePoolChanged (EventArgs e)
        {
            if (SelectedTilePoolChanged != null) {
                SelectedTilePoolChanged(this, e);
            }
        }

        protected virtual void OnTileSelectionChanged (EventArgs e)
        {
            if (TileSelectionChanged != null) {
                TileSelectionChanged(this, e);
            }
        }
    }
}
