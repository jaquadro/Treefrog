using System;
using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Presentation.Commands;
using System.Drawing;
using Treefrog.Aux;
using System.Windows.Forms;
using Treefrog.Framework.Imaging;
using Treefrog.Presentation.Layers;
using Treefrog.Model;
using Treefrog.Presentation.Annotations;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Controllers;
using Treefrog.Framework.Imaging.Drawing;

namespace Treefrog.Presentation
{
    public class SyncTilePoolEventArgs : EventArgs
    {
        public TilePool PreviousTilePool { get; private set; }

        public SyncTilePoolEventArgs (TilePool tilePool)
        {
            PreviousTilePool = tilePool;
        }
    }

    public interface ITilePoolListPresenter : ICommandSubscriber
    {
        //bool CanAddTilePool { get; }
        //bool CanRemoveSelectedTilePool { get; }
        //bool CanShowSelectedTilePoolProperties { get; }

        TilePoolManager TilePoolManager { get; }

        IEnumerable<TilePoolPresenter> TilePoolList { get; }
        TilePoolPresenter SelectedTilePool { get; }
        Tile SelectedTile { get; }                      // Send to ITilePoolPresenter

        event EventHandler SyncTilePoolManager;
        //event EventHandler SyncTilePoolActions;
        event EventHandler SyncTilePoolList;
        event EventHandler SyncTilePoolControl;         // Send to ITilePoolPresenter
        event EventHandler TileSelectionChanged;

        event EventHandler SelectedTilePoolChanged;

        //event EventHandler<SyncTilePoolEventArgs> SyncCurrentTilePool;

        //void ActionImportTilePool ();
        //void ActionRemoveSelectedTilePool ();
        void ActionSelectTilePool (string name);
        //void ActionSelectTile (Tile tile);              // Send to ITilePoolPresenter
        //void ActionShowTilePoolProperties ();

        //void RefreshTilePoolList ();
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

        private void InitializeLayerHierarchy ()
        {
            _tileLayer = new TileSetLayerPresenter(_tileSet);
            _tileLayer.TileSelected += TileSelected;

            _gridLayer = new GridLayerPresenter() {
                GridSpacingX = _tilePool.TileWidth,
                GridSpacingY = _tilePool.TileHeight,
            };
            _annotLayer = new AnnotationLayerPresenter() {
                Annotations = _annotations,
            };

            _rootLayer = new GroupLayerPresenter();
            _rootLayer.Layers.Add(_tileLayer);
            //_rootLayer.Layers.Add(_gridLayer);
            _rootLayer.Layers.Add(_annotLayer);
        }

        private void TileSelected (object sender, TileEventArgs e)
        {
            _selectedTile = e.Tile;

            TileCoord location = _tileLayer.TileToCoord(e.Tile);
            int x = location.X * _tileSet.TileWidth;
            int y = location.Y * _tileSet.TileHeight;

            SelectionAnnot annot = new SelectionAnnot() {
                Start = new Treefrog.Framework.Imaging.Point(x, y),
                End = new Treefrog.Framework.Imaging.Point(x + _tileSet.TileWidth, y + _tileSet.TileHeight),
                Fill = new SolidColorBrush(new Treefrog.Framework.Imaging.Color(192, 0, 0, 128)),
            };

            _annotations.Clear();
            _annotations.Add(annot);

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

    public class TilePoolListPresenter : IDisposable, ITilePoolListPresenter, ICommandSubscriber
    {
        private IEditorPresenter _editor;
        private TilePoolManager _poolManager;

        private Dictionary<string, TilePoolPresenter> _tilePoolPresenters;
        private string _selectedPool;
        private TilePoolPresenter _selectedPoolRef;

        public TilePoolListPresenter (IEditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += EditorSyncCurrentProject;

            _tilePoolPresenters = new Dictionary<string, TilePoolPresenter>();

            InitializeCommandManager();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (_editor != null) {
                if (disposing) {
                    BindTilePoolManager(null);
                    _editor.SyncCurrentProject -= EditorSyncCurrentProject;
                }

                _editor = null;
            }
        }

        private void EditorSyncCurrentProject (object sender, SyncProjectEventArgs e)
        {
            if (_editor.Project != null)
                BindTilePoolManager(_editor.Project.TilePoolManager);
            else
                BindTilePoolManager(null);
        }

        public void BindTilePoolManager (TilePoolManager manager)
        {
            if (_poolManager != null) {
                _poolManager.Pools.ResourceAdded -= TilePoolAdded;
                _poolManager.Pools.ResourceRemoved -= TilePoolRemoved;
                _poolManager.Pools.ResourceRemapped -= TilePoolRemapped;
            }

            _poolManager = manager;
            if (_poolManager != null) {
                _poolManager.Pools.ResourceAdded += TilePoolAdded;
                _poolManager.Pools.ResourceRemoved += TilePoolRemoved;
                _poolManager.Pools.ResourceRemapped += TilePoolRemapped;

                InitializePoolPresenters();
            }
            else {
                ClearPoolPresenters();
            }

            OnSyncTilePoolManager(EventArgs.Empty);
        }

        public TilePoolManager TilePoolManager
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
            _selectedPool = null;
            _selectedPoolRef = null;
        }

        private void InitializePoolPresenters ()
        {
            ClearPoolPresenters();

            foreach (TilePool pool in _poolManager.Pools)
                AddPoolPresenter(pool);

            SelectTilePool();

            OnSyncTilePoolList(EventArgs.Empty);
        }

        private void AddPoolPresenter (TilePool pool)
        {
            TilePoolPresenter presenter = new TilePoolPresenter(pool);
            presenter.SelectedTileChanged += SelectedTileChanged;

            _tilePoolPresenters[pool.Name] = presenter;
        }

        private void RemovePoolPresenter (string name)
        {
            TilePoolPresenter presenter;
            if (_tilePoolPresenters.TryGetValue(name, out presenter))
                presenter.Dispose();

            _tilePoolPresenters.Remove(name);
        }

        private void SelectedTileChanged (object sender, EventArgs e)
        {
            if (sender == _selectedPoolRef)
                OnTileSelectionChanged(EventArgs.Empty);
        }

        private void SelectTilePool ()
        {
            SelectTilePool(null);

            foreach (TilePool pool in _poolManager.Pools) {
                SelectTilePool(pool.Name);
                return;
            }
        }

        private void SelectTilePool (string poolName)
        {
            if (_selectedPool == poolName)
                return;

            if (poolName == null || !_tilePoolPresenters.ContainsKey(poolName)) {
                _selectedPool = null;
                _selectedPoolRef = null;

                return;
            }

            _selectedPool = poolName;
            _selectedPoolRef = _tilePoolPresenters[poolName];

            OnSelectedTilePoolChanged(EventArgs.Empty);
        }

        private void TilePoolAdded (object sender, NamedResourceEventArgs<TilePool> e)
        {
            AddPoolPresenter(e.Resource);
        }

        private void TilePoolRemoved (object sender, NamedResourceEventArgs<TilePool> e)
        {
            RemovePoolPresenter(e.Resource.Name);
        }

        private void TilePoolRemapped (object sender, NamedResourceRemappedEventArgs<TilePool> e)
        {
            if (_tilePoolPresenters.ContainsKey(e.OldName)) {
                _tilePoolPresenters[e.NewName] = _tilePoolPresenters[e.OldName];
                _tilePoolPresenters.Remove(e.OldName);
            }

            if (_selectedPool == e.OldName)
                _selectedPool = e.NewName;
        }

        #region Command Handling

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();

            _commandManager.Register(CommandKey.TilePoolImport, CommandCanImport, CommandImport);
            _commandManager.Register(CommandKey.TilePoolDelete, CommandCanDelete, CommandDelete);
            _commandManager.Register(CommandKey.TilePoolExport, CommandCanExport, CommandExport);
            _commandManager.Register(CommandKey.TilePoolImportOver, CommandCanImportOver, CommandImportOver);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanImport ()
        {
            return true;
        }

        private void CommandImport ()
        {
            if (CommandCanImport()) {
                List<string> currentNames = new List<string>();
                foreach (TilePool pool in _editor.Project.TilePools) {
                    currentNames.Add(pool.Name);
                }

                ImportTilePool form = new ImportTilePool(_editor.Project);
                form.ShowDialog();

                foreach (TilePool pool in _editor.Project.TilePools) {
                    if (!currentNames.Contains(pool.Name)) {
                        SelectTilePool(pool.Name);
                    }
                }

                OnSyncTilePoolList(EventArgs.Empty);
                OnSyncTilePoolControl(EventArgs.Empty);
            }
        }

        private bool CommandCanDelete ()
        {
            return _selectedPoolRef != null;
        }

        private void CommandDelete ()
        {
            if (CommandCanDelete()) {
                if (_selectedPool != null && _editor.Project.TilePools.Contains(_selectedPool)) {
                    _poolManager.Pools.Remove(_selectedPool);
                }

                SelectTilePool();

                OnSyncTilePoolList(EventArgs.Empty);
                OnSyncTilePoolControl(EventArgs.Empty);
            }
        }

        private bool CommandCanExport ()
        {
            return _selectedPool != null;
        }

        private void CommandExport ()
        {
            if (CommandCanExport()) {
                Bitmap export = _selectedPoolRef.TilePool.TileSource.CreateBitmap();

                SaveFileDialog ofd = new SaveFileDialog();
                ofd.Title = "Export Raw Tileset";
                ofd.Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files|*";
                ofd.OverwritePrompt = true;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    export.Save(ofd.FileName);
                }
            }
        }

        private bool CommandCanImportOver ()
        {
            return _selectedPool != null;
        }

        private void CommandImportOver ()
        {
            if (CommandCanImportOver()) {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Import Raw Tileset";
                ofd.Filter = "Images Files|*.bmp;*.gif;*.png|All Files|*";
                ofd.Multiselect = false;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    try {
                        TextureResource import = TextureResourceBitmapExt.CreateTextureResource(ofd.FileName);

                        TextureResource original = _selectedPoolRef.TilePool.TileSource;
                        if (original.Width != import.Width || original.Height != import.Height) {
                            MessageBox.Show("Imported tileset dimensions are incompatible with the selected Tile Pool.", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        _selectedPoolRef.TilePool.ReplaceTexture(import);
                    }
                    catch {
                        MessageBox.Show("Could not read selected image file.", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
        }

        #endregion

        public void ActionSelectTilePool (string name)
        {
            if (_selectedPool != name) {
                SelectTilePool(name);

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
