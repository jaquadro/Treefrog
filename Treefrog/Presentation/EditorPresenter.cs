using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Windows.Forms;
using Treefrog.Presentation.Commands;
using Treefrog.Framework;
using System.Linq;

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

    public interface IEditorPresenter
    {
        bool CanShowProjectPanel { get; }
        bool CanShowLayerPanel { get; }
        bool CanShowPropertyPanel { get; }
        bool CanShowTilePoolPanel { get; }
        bool CanShowObjectPoolPanel { get; }
        bool CanShowTileBrushPanel { get; }

        bool ShowGrid { get; }

        bool Modified { get; }

        Project Project { get; }

        Presentation Presentation { get; }
        CommandActions CommandActions { get; }

        //IEnumerable<LevelPresenter> OpenContent { get; }

        void OpenLevel (Guid uid);

        void ActionSelectContent (Guid uid);

        event EventHandler SyncContentTabs;
        event EventHandler SyncContentView;
        event EventHandler SyncModified;

        event EventHandler<SyncProjectEventArgs> SyncCurrentProject;

        event EventHandler<SyncLevelEventArgs> SyncCurrentLevel;

        event EventHandler<PanelEventArgs> PanelActivation;

        void RefreshEditor ();

        // TODO: Change this to something more general
        void ActivatePropertyPanel ();
    }

    public class CommandActions
    {
        private EditorPresenter _editor;

        private ObjectClassCommandActions _objectClassActions;
        private TilePoolCommandActions _tilePoolActions;

        public CommandActions (EditorPresenter editor)
        {
            _editor = editor;

            _objectClassActions = new ObjectClassCommandActions(_editor);
            _tilePoolActions = new TilePoolCommandActions(_editor);
        }

        public ObjectClassCommandActions ObjectClassActions
        {
            get { return _objectClassActions; }
        }

        public TilePoolCommandActions TilePoolActions
        {
            get { return _tilePoolActions; }
        }
    }

    public class Presentation
    {
        private EditorPresenter _editor;

        private TilePoolListPresenter _tilePoolList;
        private ObjectPoolCollectionPresenter _objectPoolCollection;
        private TileBrushManagerPresenter _tileBrushManager;
        private PropertyListPresenter _propertyList;
        private ProjectExplorerPresenter _projectExplorer;
        private MinimapPresenter _minimap;

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
            _tileBrushManager = new TileBrushManagerPresenter(_editor);
            _propertyList = new PropertyListPresenter();
            _projectExplorer = new ProjectExplorerPresenter(_editor);
            _minimap = new MinimapPresenter(_editor);
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

        /*public ILevelToolsPresenter LevelTools
        {
            get { return _levelTools; }
        }*/

        public IPropertyListPresenter PropertyList
        {
            get { return _propertyList; }
        }

        public ProjectExplorerPresenter ProjectExplorer
        {
            get { return _projectExplorer; }
        }

        public MinimapPresenter Minimap
        {
            get { return _minimap; }
        }

        public IStandardToolsPresenter StandardTools
        {
            get { return _stdTools; }
        }

        public ITilePoolListPresenter TilePoolList
        {
            get { return _tilePoolList; }
        }

        public ObjectPoolCollectionPresenter ObjectPoolCollection
        {
            get { return _objectPoolCollection; }
        }

        public ITileBrushManagerPresenter TileBrushes
        {
            get { return _tileBrushManager; }
        }
    }

    public class EditorPresenter : IEditorPresenter, ICommandSubscriber
    {
        private Project _project;

        private ContentWorkspacePresenter _content;
        private LevelContentTypeController _levelContentController;

        //private Dictionary<Guid, LevelPresenter> _levels;
        private Guid _currentLevel;
        private LevelPresenter _currentLevelRef;

        private Presentation _presentation;
        private CommandActions _commandActions;

        public EditorPresenter ()
        {
            _commandActions = new CommandActions(this);

            _presentation = new Presentation(this);
            _presentation.TilePoolList.TileSelectionChanged += TilePoolSelectedTileChangedHandler;

            _levelContentController = new LevelContentTypeController(this);

            _content = new ContentWorkspacePresenter(this);
            _content.AddContentController(_levelContentController);

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
            //_project.Levels.ResourceRemapped += LevelNameChangedHandler;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            _project.ObjectPoolManager.Pools.Add(new ObjectPool("Default"));

            //_openContent = new List<Guid>();
            //_levels = new Dictionary<Guid, LevelPresenter>();

            Level level = new Level("Level 1", 0, 0, 800, 480);
            level.Project = _project;
            level.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 16, 16, 50, 30));

            Level level2 = new Level("Level 2", 0, 0, 800, 480);
            level2.Project = _project;
            level2.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 32, 32, 25, 15));

            //LevelPresenter pres = new LevelPresenter(this, level);
            //_levels[level.Uid] = pres;

            //LevelPresenter pres2 = new LevelPresenter(this, level2);
            //_levels[level2.Uid] = pres2;

            //_openContent.Add(level.Uid);
            //_openContent.Add(level2.Uid);

            _project.Levels.Add(level);
            _project.Levels.Add(level2);

            SelectLevel(level.Uid);

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;
            propList.Provider = level;

            ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
            info.BindInfoPresenter(CurrentLevel.InfoPresenter);

            Modified = false;
            Project.ResetModified();

            

            _content.OpenContent(level.Uid);
            _content.OpenContent(level2.Uid);

            RefreshEditor();
        }

        public void New ()
        {
            SelectLevel(Guid.Empty);

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
            //_project.Levels.ResourceRemapped += LevelNameChangedHandler;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            _project.ObjectPoolManager.Pools.Add(new ObjectPool("Default"));

            //_openContent = new List<Guid>();
            //_levels = new Dictionary<Guid, LevelPresenter>();

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;

            foreach (Level level in _project.Levels) {
                //LevelPresenter pres = new LevelPresenter(this, level);
                //_levels[level.Uid] = pres;

                //_openContent.Add(level.Uid);

                _content.OpenContent(level.Uid);

                if (_currentLevel == Guid.Empty) {
                    SelectLevel(level.Uid);
                    propList.Provider = level; // Initial Property Provider
                }
            }

            //_project.ObjectPoolManager.CreatePool("Default");

            ContentInfoArbitrationPresenter info = _presentation.ContentInfo as ContentInfoArbitrationPresenter;
            info.BindInfoPresenter(CurrentLevel.InfoPresenter);

            Modified = false;
            Project.ResetModified();

            

            RefreshEditor();

            if (CurrentLevel != null) {
                //CurrentLevel.RefreshLayerList();
            }
        }

        public void Open (Project project)
        {
            if (PromptCancelIfModified())
                return;

            Project prevProject = _project;

            if (_project != null) {
                _project.Modified -= ProjectModifiedHandler;
            }

            _project = project;
            _project.Modified += ProjectModifiedHandler;
            //_project.Levels.ResourceRemapped += LevelNameChangedHandler;

            OnSyncCurrentProject(new SyncProjectEventArgs(prevProject));

            _currentLevel = Guid.Empty;

            //_openContent = new List<Guid>();
            //_levels = new Dictionary<Guid, LevelPresenter>();

            PropertyListPresenter propList = _presentation.PropertyList as PropertyListPresenter;

            foreach (Level level in _project.Levels) {
                //LevelPresenter pres = new LevelPresenter(this, level);
                //_levels[level.Uid] = pres;

                //_openContent.Add(level.Uid);
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

            if (CurrentLevel != null) {
                //CurrentLevel.RefreshLayerList();
            }
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
            //Form form = new Form();
            //GraphicsDeviceService gds = GraphicsDeviceService.AddRef(form.Handle, 128, 128);

            Project project = new Project() {
                Name = "New Project"
            };
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

        #region IEditorPresenter Members

        //List<Guid> _openContent;

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

        /*public IEnumerable<LevelPresenter> OpenContent
        {
            get 
            {
                foreach (Guid uid in _openContent) {
                    yield return _levels[uid];
                }
            }
        }*/

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

        #endregion

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

            //_commandManager.RegisterToggle(CommandKey.ViewGrid);

            //_commandManager.Perform(CommandKey.ViewGrid);
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
                    //LevelPresenter pres = new LevelPresenter(this, form.Level);
                    //_levels[form.Level.Uid] = pres;

                    //_openContent.Add(form.Level.Uid);
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
                //_openContent.Remove(_currentLevel);
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

        /*private void LevelNameChangedHandler (object sender, NamedResourceRemappedEventArgs<Level> e)
        {
            if (_levels.ContainsKey(e.OldName)) {
                LevelPresenter lp = _levels[e.OldName];
                _levels.Remove(e.OldName);
                _levels.Add(e.NewName, lp);
            }

            if (_openContent.Contains(e.OldName)) {
                int index = _openContent.IndexOf(e.OldName);
                _openContent.Remove(e.OldName);
                _openContent.Insert(index, e.NewName);
            }

            OnSyncContentTabs(EventArgs.Empty);
        }*/
    }
}
