using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Windows.Forms;
using Treefrog.Plugins.Object;
using System.Collections.Generic;
using Treefrog.Extensibility;

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

    public class PanelEventArgs : EventArgs
    {
        public object PanelPresenter { get; private set; }

        public PanelEventArgs (object panelPresenter) 
        {
            PanelPresenter = panelPresenter;
        }
    }

    public class CommandActions
    {
        private PresenterManager _pm;
        private EditorPresenter _editor;

        //private ObjectClassCommandActions _objectClassActions;
        private TilePoolCommandActions _tilePoolActions;
        private LibraryCommandActions _libraryActions;

        public CommandActions (PresenterManager pm, EditorPresenter editor)
        {
            _pm = pm;
            _editor = editor;

            //_objectClassActions = new ObjectClassCommandActions(pm);
            _tilePoolActions = new TilePoolCommandActions(_editor);
            _libraryActions = new LibraryCommandActions(_editor);
        }

        public ObjectClassCommandActions ObjectClassActions
        {
            get
            {
                var presenter = _pm.Lookup<ObjectPoolCollectionPresenter>();
                return (presenter != null) ? presenter.ObjectClassActions : null;
            }
            //get { return _objectClassActions; }
        }

        public TilePoolCommandActions TilePoolActions
        {
            get { return _tilePoolActions; }
        }

        public LibraryCommandActions LibraryActions
        {
            get { return _libraryActions; }
        }
    }

    //public interface IPresenter
    //{
    //    void Initialize (PresenterManager pm);
    //}

    

    public class PresenterManager : InstanceRegistry<Presenter>
    { }

    public class Presentation
    {
        private PresenterManager _pm;
        private EditorPresenter _editor;

        private TilePoolListPresenter _tilePoolList;
        //private ObjectPoolCollectionPresenter _objectPoolCollection;
        private TileBrushManagerPresenter _tileBrushManager;
        private PropertyListPresenter _propertyList;
        //private ProjectExplorerPresenter _projectExplorer;
        private MinimapPresenter _minimap;

        private StandardToolsPresenter _stdTools;
        private DocumentToolsPresenter _docTools;
        private ContentInfoArbitrationPresenter _contentInfo;

        public Presentation (PresenterManager pm, EditorPresenter editor)
        {
            _pm = pm;
            _editor = editor;

            _stdTools = new StandardToolsPresenter(_editor);
            _docTools = new DocumentToolsPresenter(_editor);
            _contentInfo = new ContentInfoArbitrationPresenter(_editor);

            //_tilePoolList = new TilePoolListPresenter(_editor);
            pm.Register(new TilePoolListPresenter(pm));
            //_objectPoolCollection = new ObjectPoolCollectionPresenter(pm);
            pm.Register(new ObjectPoolCollectionPresenter(pm));

            //_tileBrushManager = new TileBrushManagerPresenter(_editor);
            pm.Register(new TileBrushManagerPresenter(pm));

            //_propertyList = new PropertyListPresenter();
            pm.Register(new PropertyListPresenter(pm));
            //_projectExplorer = new ProjectExplorerPresenter(pm);
            pm.Register(new ProjectExplorerPresenter(pm));

            // Temporary until exported by MEF
            ProjectExplorerExt objExplorer = new ProjectExplorerExt();
            objExplorer.Bind(pm.Lookup<ObjectPoolCollectionPresenter>());
            pm.Lookup<ProjectExplorerPresenter>().Components.Register<ProjectExplorerExt>(objExplorer);

            //_minimap = new MinimapPresenter(_editor);
            pm.Register(new MinimapPresenter(pm));
        }

        public IContentInfoPresenter ContentInfo
        {
            get { return _contentInfo; }
        }

        public IDocumentToolsPresenter DocumentTools
        {
            get { return _docTools; }
        }

        public LevelPresenter LayerList
        {
            get { return _editor.CurrentLevel; }
        }

        public LevelPresenter Level
        {
            get { return _editor.CurrentLevel; }
        }

        public PropertyListPresenter PropertyList
        {
            get { return _pm.Lookup<PropertyListPresenter>(); }
            //get { return _propertyList; }
        }

        public ProjectExplorerPresenter ProjectExplorer
        {
            get { return _pm.Lookup<ProjectExplorerPresenter>(); }
            //get { return _projectExplorer; }
        }

        public MinimapPresenter Minimap
        {
            get { return _pm.Lookup<MinimapPresenter>(); }
            //get { return _minimap; }
        }

        public StandardToolsPresenter StandardTools
        {
            get { return _stdTools; }
        }

        public TilePoolListPresenter TilePoolList
        {
            get { return _pm.Lookup<TilePoolListPresenter>(); }
            //get { return _tilePoolList; }
        }

        public ObjectPoolCollectionPresenter ObjectPoolCollection
        {
            get { return _pm.Lookup<ObjectPoolCollectionPresenter>(); }
        //    get { return _objectPoolCollection; }
        }

        public TileBrushManagerPresenter TileBrushes
        {
            get { return _pm.Lookup<TileBrushManagerPresenter>(); }
            //get { return _tileBrushManager; }
        }
    }

    public class EditorPresenter : Presenter, ICommandSubscriber
    {
        private Project _project;

        private ContentWorkspacePresenter _content;
        private LevelContentTypeController _levelContentController;

        private Guid _currentLevel;
        private LevelPresenter _currentLevelRef;

        private Presentation _presentation;
        private CommandActions _commandActions;

        public EditorPresenter (PresenterManager pm)
            : base(pm)
        {
            _commandActions = new CommandActions(pm, this);

            _presentation = new Presentation(pm, this);
            _presentation.TilePoolList.TileSelectionChanged += TilePoolSelectedTileChangedHandler;

            _levelContentController = new LevelContentTypeController(pm, this);

            _content = new ContentWorkspacePresenter(this);
            _content.AddContentController(_levelContentController);

            InitializeCommandManager();
        }

        public void NewDefault ()
        {
            Project prevProject = _project;

            if (_project != null) {
                _project.Modified -= ProjectModifiedHandler;
            }

            _project = EmptyProject();
            _project.ObjectPoolManager.Pools.Add(new ObjectPool("Default"));

            _project.Modified += ProjectModifiedHandler;

            Program.CurrentProject = _project;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            Level level = new Level("Level 1", 0, 0, 800, 480);
            level.Project = _project;
            level.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 16, 16, 50, 30));

            _project.Levels.Add(level);

            SelectLevel(level.Uid);

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;
            propList.Provider = level;

            ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
            info.BindInfoPresenter(CurrentLevel.InfoPresenter);

            Modified = false;
            Project.ResetModified();

            _content.OpenContent(level.Uid);

            RefreshEditor();
        }

        public void New ()
        {
            SelectLevel(Guid.Empty);

            Project project = EmptyProject();
            Program.CurrentProject = project;

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

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            _project.ObjectPoolManager.Pools.Add(new ObjectPool("Default"));

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;

            foreach (Level level in _project.Levels) {
                _content.OpenContent(level.Uid);

                if (_currentLevel == Guid.Empty) {
                    SelectLevel(level.Uid);
                    propList.Provider = level; // Initial Property Provider
                }
            }

            ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
            info.BindInfoPresenter(CurrentLevel.InfoPresenter);

            Modified = false;
            Project.ResetModified();

            

            RefreshEditor();
        }

        public void Open (Project project)
        {
            if (PromptCancelIfModified())
                return;

            Program.CurrentProject = project;

            Project prevProject = _project;

            if (_project != null) {
                _project.Modified -= ProjectModifiedHandler;
            }

            _project = project;
            _project.Modified += ProjectModifiedHandler;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            _currentLevel = Guid.Empty;

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;

            foreach (Level level in _project.Levels) {
                _levelContentController.OpenContent(level.Uid);

                if (_currentLevel == Guid.Empty) {
                    SelectLevel(level.Uid);
                    propList.Provider = level; // Initial Property Provider
                }
            }

            if (CurrentLevel != null) {
                ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
                info.BindInfoPresenter(CurrentLevel.InfoPresenter);
            }

            Modified = false;
            Project.ResetModified();

            RefreshEditor();
        }

        public void Save (Stream stream, ProjectResolver resolver)
        {
            if (_project != null) {
                _project.Save(stream, resolver);
                Modified = false;
            }
        }

        private bool PromptCancelIfModified ()
        {
            return Modified && MessageBox.Show("You currently have unsaved changes.  Close without saving?", "Unsaved Changes", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK;
        }

        private Project EmptyProject ()
        {
            Project project = new Project() {
                Name = "New Project"
            };

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
                return (_currentLevel != null && _levelContentController.ContentIsValid(_currentLevel))
                    ? _levelContentController.GetContent(_currentLevel)
                    : null;
            }
        }

        public ContentWorkspacePresenter ContentWorkspace
        {
            get { return _content; }
        }

        public Presentation Presentation
        {
            get { return _presentation; }
        }

        public CommandActions CommandActions
        {
            get { return _commandActions; }
        }

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

        public bool CanShowTileBrushPanel
        {
            get { return true; }
        }

        public bool ShowGrid
        {
            get { return _commandManager.IsSelected(CommandKey.ViewGrid); }
        }

        public bool Modified
        {
            get { return _modified; }
            private set
            {
                if (_modified != value) {
                    _modified = value;

                    CommandManager.Invalidate(CommandKey.Save);
                    OnSyncModified(EventArgs.Empty);
                }
            }
        }

        public void ActionSelectContent (Guid uid)
        {
            SelectLevel(uid);
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

        public event EventHandler<PanelEventArgs> PanelActivation;

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

        protected virtual void OnPanelActivation (PanelEventArgs e)
        {
            var ev = PanelActivation;
            if (ev != null)
                ev(this, e);
        }

        public void RefreshEditor ()
        {
            OnSyncContentTabs(EventArgs.Empty);
            OnSyncContentView(EventArgs.Empty);
            OnSyncModified(EventArgs.Empty);
        }

        public void ActivatePropertyPanel ()
        {
            OnPanelActivation(new PanelEventArgs(Presentation.PropertyList));
        }

        private string _projectPath;

        public void OpenLevel (Guid uid)
        {
            if (_levelContentController.ContentIsOpen(uid)) {
                SelectLevel(uid);
                return;
            }

            if (_levelContentController.ContentIsValid(uid)) {
                _levelContentController.OpenContent(uid);
                OnSyncContentTabs(EventArgs.Empty);

                SelectLevel(uid);
            }
        }

        #region Command Handling

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();

            _commandManager.AddCommandSubscriber(_presentation.TilePoolList);
            _commandManager.AddCommandSubscriber(_presentation.ObjectPoolCollection);
            _commandManager.AddCommandSubscriber(_presentation.TileBrushes);

            _commandManager.Register(CommandKey.NewProject, CommandCanCreateProject, CommandCreateProject);
            _commandManager.Register(CommandKey.OpenProject, CommandCanOpenProject, CommandOpenProject);
            _commandManager.Register(CommandKey.Save, CommandCanSaveProject, CommandSaveProject);
            _commandManager.Register(CommandKey.SaveAs, CommandCanSaveProjectAs, CommandSaveProjectAs);
            _commandManager.Register(CommandKey.Exit, CommandCanExit, CommandExit);
            _commandManager.Register(CommandKey.ProjectAddLevel, CommandCanAddLevel, CommandAddLevel);
            _commandManager.Register(CommandKey.LevelClose, CommandCanCloseLevel, CommandCloseLevel);
            _commandManager.Register(CommandKey.LevelCloseAllOther, CommandCanCloseAllOtherLevels, CommandCloseAllOtherLevels);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanCreateProject ()
        {
            return true;
        }

        private void CommandCreateProject ()
        {
            if (CommandCanCreateProject()) {
                if (PromptCancelIfModified())
                    return;

                _projectPath = null;
                New();
            }
        }

        private bool CommandCanOpenProject ()
        {
            return true;
        }

        private void CommandOpenProject ()
        {
            if (CommandCanOpenProject()) {
                if (PromptCancelIfModified())
                    return;

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Open Project File";
                ofd.Filter = "Treefrog Project Files|*.tlp;*.tlpx";
                ofd.Multiselect = false;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    if (!File.Exists(ofd.FileName)) {
                        MessageBox.Show("Could not find file: " + ofd.FileName, "Open Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try {
                        using (FileStream fs = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read)) {
                            Project project = new Project(fs, new FileProjectResolver(ofd.FileName));
                            Open(project);

                            _projectPath = ofd.FileName;
                        }
                    }
                    catch (IOException e) {
                        MessageBox.Show("Could not open file '" + ofd.FileName + "' for reading.\n\n" + e.Message, "Open Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private bool CommandCanSaveProject ()
        {
            return Modified;
        }

        private void CommandSaveProject ()
        {
            if (CommandCanSaveProject()) {
                if (_projectPath == null) {
                    CommandSaveProjectAs();
                }
                else {
                    try {
                        using (FileStream fs = File.Open(_projectPath, FileMode.Create, FileAccess.Write)) {
                            Save(fs, new FileProjectResolver(_projectPath));
                        }
                    }
                    catch (IOException e) {
                        MessageBox.Show("Could not save file '" + _projectPath + "'.\n\n" + e.Message, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private bool CommandCanSaveProjectAs ()
        {
            return _project != null;
        }

        private void CommandSaveProjectAs ()
        {
            if (CommandCanSaveProjectAs()) {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.Title = "Save Project File";
                ofd.Filter = "Treefrog Project Files|*.tlpx";
                ofd.OverwritePrompt = true;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    try {
                        using (FileStream fs = File.Open(ofd.FileName, FileMode.Create, FileAccess.Write)) {
                            _project.Name = Path.GetFileNameWithoutExtension(ofd.FileName);
                            _project.FileName = Path.GetFileName(ofd.FileName);
                            Save(fs, new FileProjectResolver(ofd.FileName));
                        }
                    }
                    catch (IOException e) {
                        MessageBox.Show("Could not save file '" + ofd.FileName + "'.\n\n" + e.Message, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private bool CommandCanExit ()
        {
            return true;
        }

        private void CommandExit ()
        {
            if (CommandCanExit()) {
                Application.Exit();
            }
        }

        private bool CommandCanAddLevel ()
        {
            return true;
        }

        private void CommandAddLevel ()
        {
            if (CommandCanAddLevel()) {
                NewLevel form = new NewLevel(_project);
                if (form.ShowDialog() == DialogResult.OK) {
                    SelectLevel(form.Level.Uid);
                    Presentation.PropertyList.Provider = form.Level;

                    Modified = true;
                    RefreshEditor();
                }
            }
        }

        private bool CommandCanCloseLevel ()
        {
            return _currentLevel != null;
        }

        private void CommandCloseLevel ()
        {
            if (CommandCanCloseLevel()) {
                _content.CloseContent(_currentLevel);

                OnSyncContentTabs(EventArgs.Empty);

                if (_levelContentController.OpenContentsCount > 0)
                    SelectLevel(_levelContentController.OpenContents.First().Uid);
                else
                    SelectLevel(Guid.Empty);
            }
        }

        private bool CommandCanCloseAllOtherLevels ()
        {
            return _currentLevel != null && _levelContentController.OpenContentsCount > 1;
        }

        private void CommandCloseAllOtherLevels ()
        {
            if (CommandCanCloseAllOtherLevels()) {
                foreach (LevelPresenter lp in _levelContentController.OpenContents.ToList()) {
                    if (lp.Uid != _currentLevel)
                        _levelContentController.CloseContent(lp.Uid);
                }

                OnSyncContentTabs(EventArgs.Empty);
            }
        }

        private void InvalidateLevelCommands ()
        {
            if (CommandManager != null) {
                CommandManager.Invalidate(CommandKey.LevelClose);
                CommandManager.Invalidate(CommandKey.LevelCloseAllOther);
                CommandManager.Invalidate(CommandKey.LevelRename);
                CommandManager.Invalidate(CommandKey.LevelResize);
                CommandManager.Invalidate(CommandKey.LevelProperties);
            }
        }

        #endregion

        private void TilePoolSelectedTileChangedHandler (object sender, EventArgs e)
        {
            if (CommandManager.IsSelected(CommandKey.TileToolErase))
                CommandManager.Perform(CommandKey.TileToolDraw);
        }

        private void SelectLevel (Guid levelUid)
        {
            Level prev = _project.Levels.Contains(levelUid) ? _project.Levels[levelUid] : null;
            LevelPresenter prevLevel = _currentLevelRef;
            
            if (_currentLevel == levelUid) {
                return;
            }

            // Unbind previously selected layer if necessary
            if (_currentLevelRef != null) {
                _currentLevelRef.Deactivate();
                _commandManager.RemoveCommandSubscriber(_currentLevelRef);
            }

            _currentLevel = Guid.Empty;
            _currentLevelRef = null;

            // Bind new layer
            if (levelUid != null && _levelContentController.ContentIsValid(levelUid)) {
                _currentLevel = levelUid;
                _currentLevelRef = CurrentLevel;

                _commandManager.AddCommandSubscriber(_currentLevelRef);

                if (!_project.Levels.Contains(levelUid)) {
                    throw new InvalidOperationException("Selected a LevelPresenter with no corresponding model Level!  Selected name: " + levelUid);
                }

                ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
                info.BindInfoPresenter(CurrentLevel.InfoPresenter);

                CurrentLevel.InfoPresenter.RefreshContentInfo();

                CurrentLevel.Activate();
            }
            else {
                ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
                info.BindInfoPresenter(null);
            }

            InvalidateLevelCommands();
            CommandManager.Invalidate(CommandKey.ViewGrid);

            OnSyncCurrentLevel(new SyncLevelEventArgs(prev, prevLevel));
            OnSyncContentView(EventArgs.Empty);
        }
    }
}
