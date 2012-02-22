using System;
using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.View.Forms;
using Treefrog.Framework;

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

    public interface ITilePoolListPresenter
    {
        bool CanAddTilePool { get; }
        bool CanRemoveSelectedTilePool { get; }
        bool CanShowSelectedTilePoolProperties { get; }

        IEnumerable<TilePool> TilePoolList { get; }
        TilePool SelectedTilePool { get; }
        Tile SelectedTile { get; }                      // Send to ITilePoolPresenter

        event EventHandler SyncTilePoolActions;
        event EventHandler SyncTilePoolList;
        event EventHandler SyncTilePoolControl;         // Send to ITilePoolPresenter
        event EventHandler TileSelectionChanged;

        event EventHandler<SyncTilePoolEventArgs> SyncCurrentTilePool;

        void ActionImportTilePool ();
        void ActionRemoveSelectedTilePool ();
        void ActionSelectTilePool (string name);
        void ActionSelectTile (Tile tile);              // Send to ITilePoolPresenter
        void ActionShowTilePoolProperties ();

        void RefreshTilePoolList ();
    }

    public class TilePoolListPresenter : ITilePoolListPresenter
    {
        #region Fields

        private IEditorPresenter _editor;

        private string _selectedPool;
        private TilePool _selectedPoolRef;

        private Dictionary<string, Tile> _selectedTiles;

        #endregion

        #region Constructors

        public TilePoolListPresenter (IEditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += SyncCurrentProjectHandler;
        }

        #endregion

        private void SyncCurrentProjectHandler (object sender, SyncProjectEventArgs e)
        {
            _selectedTiles = new Dictionary<string, Tile>();

            _editor.Project.TilePools.ResourceRemapped += TilePool_NameChanged;

            SelectTilePool();

            OnSyncTilePoolActions(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        #region Properties

        public bool CanAddTilePool
        {
            get { return true; }
        }

        public bool CanRemoveSelectedTilePool
        {
            get { return SelectedTilePool != null; }
        }

        public bool CanShowSelectedTilePoolProperties
        {
            get { return SelectedTilePool != null; }
        }

        public IEnumerable<TilePool> TilePoolList
        {
            get
            {
                foreach (TilePool pool in _editor.Project.TilePools) {
                    yield return pool;
                }
            }
        }

        public TilePool SelectedTilePool
        {
            get { return _selectedPoolRef; }
        }

        public Tile SelectedTile
        {
            get
            {
                TilePool pool = SelectedTilePool;
                return (pool != null && _selectedTiles.ContainsKey(_selectedPool))
                    ? _selectedTiles[_selectedPool]
                    : null;
            }
        }

        #endregion

        #region Events

        public event EventHandler SyncTilePoolActions;

        public event EventHandler SyncTilePoolList;

        public event EventHandler SyncTilePoolControl;

        public event EventHandler<SyncTilePoolEventArgs> SyncCurrentTilePool;

        public event EventHandler TileSelectionChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnSyncTilePoolActions (EventArgs e)
        {
            if (SyncTilePoolActions != null) {
                SyncTilePoolActions(this, e);
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

        protected virtual void OnSyncCurrentTilePool (SyncTilePoolEventArgs e)
        {
            if (SyncCurrentTilePool != null) {
                SyncCurrentTilePool(this, e);
            }
        }

        protected virtual void OnTileSelectionChanged (EventArgs e)
        {
            if (TileSelectionChanged != null) {
                TileSelectionChanged(this, e);
            }
        }

        #endregion

        private void TilePool_NameChanged (object sender, NamedResourceEventArgs<TilePool> e)
        {
            if (e.Resource != null && e.Resource.Name == _selectedPool) {
                SelectTilePool(e.Resource.Name);
            }
        }

        #region View Action API

        public void ActionImportTilePool ()
        {
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

            OnSyncTilePoolActions(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        public void ActionRemoveSelectedTilePool ()
        {
            if (_selectedPool != null && _editor.Project.TilePools.Contains(_selectedPool)) {
                _editor.Project.TilePools.Remove(_selectedPool);
            }

            SelectTilePool();

            OnSyncTilePoolActions(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        public void ActionSelectTilePool (string name)
        {
            if (_selectedPool != name) {
                SelectTilePool(name);

                OnSyncTilePoolActions(EventArgs.Empty);
                OnSyncTilePoolList(EventArgs.Empty);

                if (SelectedTilePool != null)
                    _editor.Presentation.PropertyList.Provider = SelectedTilePool;
            }
        }

        public void ActionSelectTile (Tile tile)
        {
            if (SelectedTilePool != null) {
                _selectedTiles[_selectedPool] = tile;

                OnSyncTilePoolControl(EventArgs.Empty);
                OnTileSelectionChanged(EventArgs.Empty);

                _editor.Presentation.PropertyList.Provider = tile;
            }
        }

        public void ActionShowTilePoolProperties ()
        {
            if (SelectedTilePool != null)
                _editor.Presentation.PropertyList.Provider = SelectedTilePool;
        }

        public void RefreshTilePoolList ()
        {
            OnSyncTilePoolActions(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        #endregion

        private void SelectTilePool ()
        {
            SelectTilePool(null);

            foreach (TilePool pool in _editor.Project.TilePools) {
                SelectTilePool(pool.Name);
                return;
            }
        }

        private void SelectTilePool (string tilePool)
        {
            TilePool prevPool = _selectedPoolRef;

            if (tilePool == _selectedPool) {
                return;
            }

            _selectedPool = null;
            _selectedPoolRef = null;

            // Bind new pool
            if (tilePool != null && _editor.Project.TilePools.Contains(tilePool)) {
                _selectedPool = tilePool;
                _selectedPoolRef = _editor.Project.TilePools[tilePool];
            }

            OnSyncCurrentTilePool(new SyncTilePoolEventArgs(prevPool));
        }
    }
}
