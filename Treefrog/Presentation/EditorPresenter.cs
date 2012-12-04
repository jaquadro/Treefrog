using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Windows.Forms;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation
{
    public class SyncLevelEventArgs : EventArgs
    {
        public Level PreviousLevel { get; private set; }
        public LevelPresenter PreviousLevelPresenter { get; private set; }

        public SyncLevelEventArgs (Level level, LevelPresenter controller)
        {
            PreviousLevel = level;
            PreviousLevelPresenter = controller;
        }
    }

    public class SyncProjectEventArgs : EventArgs
    {
        public Project PreviousProject { get; private set; }

        public SyncProjectEventArgs (Project project)
        {
            PreviousProject = project;
        }
    }

    public interface IEditorPresenter
    {
        bool CanShowProjectPanel { get; }
        bool CanShowLayerPanel { get; }
        bool CanShowPropertyPanel { get; }
        bool CanShowTilePoolPanel { get; }
        bool CanShowObjectPoolPanel { get; }

        bool Modified { get; }

        Project Project { get; }

        Presentation Presentation { get; }

        IEnumerable<ILevelPresenter> OpenContent { get; }

        event EventHandler SyncContentTabs;
        event EventHandler SyncContentView;
        event EventHandler SyncModified;

        event EventHandler<SyncProjectEventArgs> SyncCurrentProject;

        event EventHandler<SyncLevelEventArgs> SyncCurrentLevel;

        void RefreshEditor ();
    }

    public class Presentation
    {
        private EditorPresenter _editor;

        private TilePoolListPresenter _tilePoolList;
        private ObjectPoolCollectionPresenter _objectPoolCollection;
        private PropertyListPresenter _propertyList;

        private StandardToolsPresenter _stdTools;
        private DocumentToolsPresenter _docTools;
        private ContentInfoArbitrationPresenter _contentInfo;

        public Presentation (EditorPresenter editor)
        {
            _editor = editor;

            _stdTools = new StandardToolsPresenter(_editor);
            _docTools = new DocumentToolsPresenter(_editor);
            _contentInfo = new ContentInfoArbitrationPresenter(_editor);

            _tilePoolList = new TilePoolListPresenter(_editor);
            _objectPoolCollection = new ObjectPoolCollectionPresenter(_editor);
            _propertyList = new PropertyListPresenter();
        }

        public IContentInfoPresenter ContentInfo
        {
            get { return _contentInfo; }
        }

        public IDocumentToolsPresenter DocumentTools
        {
            get { return _docTools; }
        }

        public ILayerListPresenter LayerList
        {
            get { return _editor.CurrentLevel; }
        }

        public ILevelPresenter Level
        {
            get { return _editor.CurrentLevel; }
        }

        /*public ILevelToolsPresenter LevelTools
        {
            get { return _levelTools; }
        }*/

        public IPropertyListPresenter PropertyList
        {
            get { return _propertyList; }
        }

        public IStandardToolsPresenter StandardTools
        {
            get { return _stdTools; }
        }

        public ITilePoolListPresenter TilePoolList
        {
            get { return _tilePoolList; }
        }

        public IObjectPoolCollectionPresenter ObjectPoolCollection
        {
            get { return _objectPoolCollection; }
        }
    }

    public class EditorPresenter : IEditorPresenter, ICommandSubscriber
    {
        private Project _project;

        private Dictionary<string, LevelPresenter> _levels;
        private string _currentLevel;
        private LevelPresenter _currentLevelRef;

        private Presentation _presentation;

        public EditorPresenter ()
        {
            _presentation = new Presentation(this);
            _presentation.TilePoolList.TileSelectionChanged += TilePoolSelectedTileChangedHandler;

            InitializeCommandManager();
        }

        public EditorPresenter (Project project)
            : this()
        {
            Open(project);
        }

        public void NewDefault ()
        {
            Project prevProject = _project;

            if (_project != null) {
                _project.Modified -= ProjectModifiedHandler;
            }

            _project = EmptyProject();
            _project.Modified += ProjectModifiedHandler;

            _openContent = new List<string>();
            _levels = new Dictionary<string, LevelPresenter>();

            Level level = new Level("Level 1", 16, 16, 50, 30);
            level.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 16, 16, 50, 30));

            LevelPresenter pres = new LevelPresenter(this, level);
            _levels[level.Name] = pres;

            _openContent.Add(level.Name);

            _project.Levels.Add(level);

            SelectLevel("Level 1");

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;
            propList.Provider = level;

            ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
            info.BindInfoPresenter(CurrentLevel.InfoPresenter);

            Modified = false;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            RefreshEditor();
        }

        public void New ()
        {
            Project project = EmptyProject();

            NewLevel form = new NewLevel(project);
            if (form.ShowDialog() != DialogResult.OK) {
                return;
            }

            Project prevProject = _project;

            if (_project != null) {
                _project.Modified -= ProjectModifiedHandler;
            }

            _project = project;
            _project.Modified += ProjectModifiedHandler;

            _openContent = new List<string>();
            _levels = new Dictionary<string, LevelPresenter>();

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;

            foreach (Level level in _project.Levels) {
                LevelPresenter pres = new LevelPresenter(this, level);
                _levels[level.Name] = pres;

                _openContent.Add(level.Name);

                if (_currentLevel == null) {
                    SelectLevel(level.Name);
                    propList.Provider = level; // Initial Property Provider
                }
            }

            _project.ObjectPoolManager.CreatePool("Default");

            ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
            info.BindInfoPresenter(CurrentLevel.InfoPresenter);

            Modified = false;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            RefreshEditor();

            if (CurrentLevel != null) {
                CurrentLevel.RefreshLayerList();
            }
        }

        public void Open (Project project)
        {
            Project prevProject = _project;

            if (_project != null) {
                _project.Modified -= ProjectModifiedHandler;
            }

            _project = project;
            _project.Modified += ProjectModifiedHandler;

            _currentLevel = null;

            _openContent = new List<string>();
            _levels = new Dictionary<string, LevelPresenter>();

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;

            foreach (Level level in _project.Levels) {
                LevelPresenter pres = new LevelPresenter(this, level);
                _levels[level.Name] = pres;

                _openContent.Add(level.Name);

                if (_currentLevel == null) {
                    SelectLevel(level.Name);
                    propList.Provider = level; // Initial Property Provider
                }
            }

            ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
            info.BindInfoPresenter(CurrentLevel.InfoPresenter);

            Modified = false;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            RefreshEditor();

            if (CurrentLevel != null) {
                CurrentLevel.RefreshLayerList();
            }
        }

        public void Save (Stream stream)
        {
            if (_project != null) {
                _project.Save(stream);
                Modified = false;
            }
        }

        private Project EmptyProject ()
        {
            //Form form = new Form();
            //GraphicsDeviceService gds = GraphicsDeviceService.AddRef(form.Handle, 128, 128);

            Project project = new Project();
            //project.Initialize(gds.GraphicsDevice);

            return project;
        }

        public Project Project
        {
            get { return _project; }
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

        public Presentation Presentation
        {
            get { return _presentation; }
        }

        #region IEditorPresenter Members

        List<string> _openContent;

        private bool _modified;

        public bool CanShowProjectPanel
        {
            get { return true; }
        }

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

        public bool CanShowObjectPoolPanel
        {
            get { return true; }
        }

        public bool Modified
        {
            get { return _modified; }
            private set
            {
                if (_modified != value) {
                    _modified = value;
                    OnSyncModified(EventArgs.Empty);
                }
            }
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

        private void ProjectModifiedHandler (object sender, EventArgs e)
        {
            Modified = true;
        }

        public event EventHandler SyncContentTabs;

        public event EventHandler SyncContentView;

        public event EventHandler SyncModified;

        public event EventHandler<SyncProjectEventArgs> SyncCurrentProject;

        public event EventHandler<SyncLevelEventArgs> SyncCurrentLevel;

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

        protected virtual void OnSyncCurrentProject (SyncProjectEventArgs e)
        {
            if (SyncCurrentProject != null) {
                SyncCurrentProject(this, e);
            }
        }

        protected virtual void OnSyncCurrentLevel (SyncLevelEventArgs e)
        {
            if (SyncCurrentLevel != null) {
                SyncCurrentLevel(this, e);
            }
        }

        public void RefreshEditor ()
        {
            OnSyncContentTabs(EventArgs.Empty);
            OnSyncContentView(EventArgs.Empty);
            OnSyncModified(EventArgs.Empty);
        }

        #endregion

        #region Command Handling

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();

            _commandManager.AddCommandSubscriber(_presentation.TilePoolList);
            _commandManager.AddCommandSubscriber(_presentation.ObjectPoolCollection);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        #endregion

        private void TilePoolSelectedTileChangedHandler (object sender, EventArgs e)
        {
            if (CommandManager.IsSelected(CommandKey.TileToolErase))
                CommandManager.Perform(CommandKey.TileToolDraw);
        }

        private void SelectLevel (string level)
        {
            Level prev = _project.Levels.Contains(level) ? _project.Levels[level] : null;
            LevelPresenter prevLevel = _currentLevelRef;
            
            if (_currentLevel == level) {
                return;
            }

            // Unbind previously selected layer if necessary
            if (_currentLevelRef != null) {
                _commandManager.RemoveCommandSubscriber(_currentLevelRef);
            }

            _currentLevel = null;
            _currentLevelRef = null;

            // Bind new layer
            if (level != null && _levels.ContainsKey(level)) {
                _currentLevel = level;
                _currentLevelRef = CurrentLevel;

                _commandManager.AddCommandSubscriber(_currentLevelRef);

                if (!_project.Levels.Contains(level)) {
                    throw new InvalidOperationException("Selected a LevelPresenter with no corresponding model Level!  Selected name: " + level);
                }
            }

            OnSyncCurrentLevel(new SyncLevelEventArgs(prev, prevLevel));
        }
    }
}
