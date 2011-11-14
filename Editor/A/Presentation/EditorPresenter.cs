using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Editor.Model.Controls;
using Editor.Forms;
using System.Windows.Forms;

namespace Editor.A.Presentation
{
    public interface IEditorPresenter
    {
        bool CanShowLayerPanel { get; }
        bool CanShowPropertyPanel { get; }
        bool CanShowTilePoolPanel { get; }

        bool Modified { get; }

        ILayerListPresenter CurrentLayerListPresenter { get; }
        IPropertyListPresenter CurrentPropertyListPresenter { get; }
        ITilePoolListPresenter CurrentTilePoolListPresenter { get; }

        IStandardToolsPresenter CurrentStandardToolsPresenter { get; }
        ILevelToolsPresenter CurrentLevelToolsPresenter { get; }

        IEnumerable<ILevelPresenter> OpenContent { get; }

        public void ActionModify ();

        event EventHandler SyncContentTabs;
        event EventHandler SyncContentView;
        event EventHandler SyncModifed;

        void RefreshEditor ();
    }

    public class EditorPresenter : IEditorPresenter, ITilePoolListPresenter
    {
        private Project _project;

        Dictionary<string, LevelPresenter> _levels;
        string _currentLevel;

        private PropertyListPresenter _propertyList;

        private LevelToolsPresenter _levelTools;
        private StandardToolsPresenter _stdTools;

        public EditorPresenter ()
        {
            _propertyList = new PropertyListPresenter();

            _levelTools = new LevelToolsPresenter(this);
            _stdTools = new StandardToolsPresenter(this);
        }

        public EditorPresenter (Project project)
            : this()
        {
            Open(project);
        }

        public void NewDefault ()
        {
            _openContent = new List<string>();
            _levels = new Dictionary<string, LevelPresenter>();
            _selectedTiles = new Dictionary<string, Tile>();

            Level level = new Level("Level 1", 16, 16, 50, 30);
            level.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 16, 16, 50, 30));

            LevelPresenter pres = new LevelPresenter(this, level);
            _levels[level.Name] = pres;

            _openContent.Add(level.Name);

            _project = EmptyProject();
            _project.Levels.Add(level);

            _currentLevel = "Level 1";

            _propertyList.Provider = level;

            RefreshTilePoolList();
            RefreshEditor();
        }

        public void Open (Project project)
        {
            _project = project;

            _currentLevel = null;

            _openContent = new List<string>();
            _levels = new Dictionary<string, LevelPresenter>();
            _selectedTiles = new Dictionary<string, Tile>();

            foreach (TilePool pool in _project.TilePools) {
                _selectedPool = pool.Name;
                break;
            }

            foreach (Level level in _project.Levels) {
                LevelPresenter pres = new LevelPresenter(this, level);
                _levels[level.Name] = pres;

                _openContent.Add(level.Name);

                if (_currentLevel == null) {
                    _currentLevel = level.Name;
                    _propertyList.Provider = level; // Initial Property Provider
                }
            }

            RefreshTilePoolList();
            RefreshEditor();
        }

        private Project EmptyProject ()
        {
            Form form = new Form();
            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(form.Handle, 128, 128);

            Project project = new Project();
            project.Initialize(gds.GraphicsDevice);

            return project;
        }

        public LevelPresenter CurrentLevel
        {
            get
            {
                return (_currentLevel != null && _levels.ContainsKey(_currentLevel))
                    ? _levels[_currentLevel]
                    : null;
            }
        }

        #region IEditorPresenter Members

        List<string> _openContent;

        private bool _modified;

        public bool CanShowLayerPanel
        {
            get { return true; }
        }

        public bool CanShowPropertyPanel
        {
            get { return true; }
        }

        public bool CanShowTilePoolPanel
        {
            get { return true; }
        }

        public bool Modified
        {
            get { return _modified; }
        }

        public ILayerListPresenter CurrentLayerListPresenter
        {
            get { return CurrentLevel; }
        }

        public IPropertyListPresenter CurrentPropertyListPresenter
        {
            get { return _propertyList; }
        }

        public ITilePoolListPresenter CurrentTilePoolListPresenter
        {
            get { return this; }
        }

        public IStandardToolsPresenter CurrentStandardToolsPresenter
        {
            get { return _stdTools; }
        }

        public ILevelToolsPresenter CurrentLevelToolsPresenter
        {
            get { return _levelTools; }
        }

        public IEnumerable<ILevelPresenter> OpenContent
        {
            get 
            {
                foreach (string name in _openContent) {
                    yield return _levels[name];
                }
            }
        }

        public void ActionModify ()
        {
            if (_modified == false) {
                _modified = true;

                OnSyncModified(EventArgs.Empty);
            }
        }

        public event EventHandler SyncContentTabs;

        public event EventHandler SyncContentView;

        public event EventHandler SyncModified;

        protected virtual void OnSyncContentTabs (EventArgs e)
        {
            if (SyncContentTabs != null) {
                SyncContentTabs(this, e);
            }
        }

        protected virtual void OnSyncContentView (EventArgs e)
        {
            if (SyncContentView != null) {
                SyncContentView(this, e);
            }
        }

        protected virtual void OnSyncModified (EventArgs e)
        {
            if (SyncModified != null) {
                SyncModified(this, e);
            }
        }

        public void RefreshEditor ()
        {
            OnSyncContentTabs(EventArgs.Empty);
            OnSyncContentView(EventArgs.Empty);
            OnSyncModified(EventArgs.Empty);
        }

        #endregion

        #region ITilePoolListPresenter Members

        #region Fields

        private string _selectedPool;
        private Dictionary<string, Tile> _selectedTiles;

        #endregion

        #region Properties

        public bool CanAddTilePool
        {
            get { return true; }
        }

        public bool CanRemoveSelectedTilePool
        {
            get { return SelectedTilePool != null; }
        }

        public IEnumerable<TilePool> TilePoolList
        {
            get
            {
                foreach (TilePool pool in _project.TilePools) {
                    yield return pool;
                }
            }
        }

        public TilePool SelectedTilePool
        {
            get
            {
                return (_selectedPool != null && _project.TilePools.Contains(_selectedPool))
                    ? _project.TilePools[_selectedPool]
                    : null;
            }
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

        #endregion

        #region View Action API

        public void ActionImportTilePool ()
        {
            List<string> currentNames = new List<string>();
            foreach (TilePool pool in _project.TilePools) {
                currentNames.Add(pool.Name);
            }

            ImportTilePool form = new ImportTilePool(_project);
            form.ShowDialog();

            foreach (TilePool pool in _project.TilePools) {
                if (!currentNames.Contains(pool.Name)) {
                    _selectedPool = pool.Name;
                }
            }

            OnSyncTilePoolActions(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        public void ActionRemoveSelectedTilePool ()
        {
            if (_selectedPool != null && _project.TilePools.Contains(_selectedPool)) {
                _project.TilePools.Remove(_selectedPool);
                _selectedPool = null;
            }

            foreach (TilePool pool in _project.TilePools) {
                _selectedPool = pool.Name;
                break;
            }

            OnSyncTilePoolActions(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        public void ActionSelectTilePool (string name)
        {
            if (_selectedPool != name && _project.TilePools.Contains(name)) {
                _selectedPool = name;

                OnSyncTilePoolActions(EventArgs.Empty);
                OnSyncTilePoolList(EventArgs.Empty);
            }
        }

        public void ActionSelectTile (Tile tile)
        {
            if (SelectedTilePool != null) {
                _selectedTiles[_selectedPool] = tile;

                OnSyncTilePoolControl(EventArgs.Empty);
            }
        }

        #endregion

        public void RefreshTilePoolList ()
        {
            OnSyncTilePoolActions(EventArgs.Empty);
            OnSyncTilePoolList(EventArgs.Empty);
            OnSyncTilePoolControl(EventArgs.Empty);
        }

        #endregion
    }
}
