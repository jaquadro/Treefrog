using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Windows.Forms;
using System.Windows.Forms;

namespace Treefrog.Presentation
{
    public class ProjectExplorerPresenter : IDisposable, ICommandSubscriber
    {
        private enum EventBindings
        {
            LibraryAdded,
            LibraryRemoved,
            LibraryModified,

            LevelAdded,
            LevelRemoved,
            LevelModified,

            ObjectAdded,
            ObjectRemoved,
            ObjectModified,

            TilePoolAdded,
            TilePoolRemoved,
            TilePoolModified,
        }

        private EditorPresenter _editor;

        private Project _project;
        private LibraryManager _libraryManager;
        private IObjectPoolManager _objectPoolManager;
        private ITilePoolManager _tilePoolManager;

        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<Library>>> _libraryEventBindings;
        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<Level>>> _levelEventBindings;
        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<ObjectClass>>> _objectEventBindings;
        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<TilePool>>> _tilePoolEventBindings;

        public ProjectExplorerPresenter (EditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += EditorSyncCurrentProject;

            _libraryEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<Library>>>() {
                { EventBindings.LibraryAdded, (s, e) => OnLibraryAdded(new ResourceEventArgs<Library>(e.Resource)) },
                { EventBindings.LibraryRemoved, (s, e) => OnLibraryRemoved(new ResourceEventArgs<Library>(e.Resource)) },
                { EventBindings.LibraryModified, (s, e) => OnLibraryModified(new ResourceEventArgs<Library>(e.Resource)) },
            };

            _levelEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<Level>>>() {
                { EventBindings.LevelAdded, (s, e) => OnLevelAdded(new ResourceEventArgs<Level>(e.Resource)) },
                { EventBindings.LevelRemoved, (s, e) => OnLevelRemoved(new ResourceEventArgs<Level>(e.Resource)) },
                { EventBindings.LevelModified, (s, e) => OnLevelModified(new ResourceEventArgs<Level>(e.Resource)) },
            };

            _objectEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<ObjectClass>>>() {
                { EventBindings.ObjectAdded, (s, e) => OnObjectAdded(new ResourceEventArgs<ObjectClass>(e.Resource)) },
                { EventBindings.ObjectRemoved, (s, e) => OnObjectRemoved(new ResourceEventArgs<ObjectClass>(e.Resource)) },
                { EventBindings.ObjectModified, (s, e) => OnObjectModified(new ResourceEventArgs<ObjectClass>(e.Resource)) },
            };

            _tilePoolEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<TilePool>>>() {
                { EventBindings.TilePoolAdded, (s, e) => OnTilePoolAdded(new ResourceEventArgs<TilePool>(e.Resource)) },
                { EventBindings.TilePoolRemoved, (s, e) => OnTilePoolRemoved(new ResourceEventArgs<TilePool>(e.Resource)) },
                { EventBindings.TilePoolModified, (s, e) => OnTilePoolModified(new ResourceEventArgs<TilePool>(e.Resource)) },
            };

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
                    BindProject(null);

                    _editor.SyncCurrentProject -= EditorSyncCurrentProject;
                }

                _editor = null;
            }
        }

        private void EditorSyncCurrentProject (object sender, SyncProjectEventArgs e)
        {
            if (_editor.Project != null) {
                BindProject(_editor.Project);
            }
            else {
                BindProject(null);
            }
        }

        private void BindProject (Project project)
        {
            if (_project == project)
                return;

            if (_project != null) {
                _project.Levels.ResourceAdded -= _levelEventBindings[EventBindings.LevelAdded];
                _project.Levels.ResourceRemoved -= _levelEventBindings[EventBindings.LevelRemoved];
                _project.Levels.ResourceModified -= _levelEventBindings[EventBindings.LevelModified];
            }

            _project = project;

            if (_project != null) {
                _project.Levels.ResourceAdded += _levelEventBindings[EventBindings.LevelAdded];
                _project.Levels.ResourceRemoved += _levelEventBindings[EventBindings.LevelRemoved];
                _project.Levels.ResourceModified += _levelEventBindings[EventBindings.LevelModified];

                BindLibraryManager(_project.LibraryManager);
                BindObjectManager(_project.ObjectPoolManager);
                BindTilePoolManager(_project.TilePoolManager);
            }
            else {
                BindLibraryManager(null);
                BindObjectManager(null);
                BindTilePoolManager(null);
            }

            OnProjectReset(EventArgs.Empty);
        }

        private void BindLibraryManager (LibraryManager manager)
        {
            if (_libraryManager == manager)
                return;

            if (_libraryManager != null) {
                _libraryManager.Libraries.ResourceAdded -= _libraryEventBindings[EventBindings.LibraryAdded];
                _libraryManager.Libraries.ResourceRemoved -= _libraryEventBindings[EventBindings.LibraryRemoved];
                _libraryManager.Libraries.ResourceModified -= _libraryEventBindings[EventBindings.LibraryModified];
            }

            _libraryManager = manager;

            if (_libraryManager != null) {
                _libraryManager.Libraries.ResourceAdded += _libraryEventBindings[EventBindings.LibraryAdded];
                _libraryManager.Libraries.ResourceRemoved += _libraryEventBindings[EventBindings.LibraryRemoved];
                _libraryManager.Libraries.ResourceModified += _libraryEventBindings[EventBindings.LibraryModified];
            }
        }

        private void BindObjectManager (IObjectPoolManager manager)
        {
            if (_objectPoolManager == manager)
                return;

            if (_objectPoolManager != null) {
                _objectPoolManager.Pools.ResourceAdded -= ObjectPoolAddedHandler;
                _objectPoolManager.Pools.ResourceRemoved -= ObjectPoolRemovedHandler;

                foreach (ObjectPool pool in _objectPoolManager.Pools)
                    UnhookObjectPool(pool);
            }

            _objectPoolManager = manager;

            if (_objectPoolManager != null) {
                _objectPoolManager.Pools.ResourceAdded += ObjectPoolAddedHandler;
                _objectPoolManager.Pools.ResourceRemoved += ObjectPoolRemovedHandler;

                foreach (ObjectPool pool in _objectPoolManager.Pools)
                    HookObjectPool(pool);
            }
        }

        private void UnhookObjectPool (ObjectPool pool)
        {
            if (pool != null) {
                pool.Objects.ResourceAdded -= _objectEventBindings[EventBindings.ObjectAdded];
                pool.Objects.ResourceRemoved -= _objectEventBindings[EventBindings.ObjectRemoved];
                pool.Objects.ResourceModified -= _objectEventBindings[EventBindings.ObjectModified];
            }
        }

        private void HookObjectPool (ObjectPool pool)
        {
            if (pool != null) {
                pool.Objects.ResourceAdded += _objectEventBindings[EventBindings.ObjectAdded];
                pool.Objects.ResourceRemoved += _objectEventBindings[EventBindings.ObjectRemoved];
                pool.Objects.ResourceModified += _objectEventBindings[EventBindings.ObjectModified];
            }
        }

        private void BindTilePoolManager (ITilePoolManager manager)
        {
            if (_tilePoolManager == manager)
                return;

            if (_tilePoolManager != null) {
                _tilePoolManager.Pools.ResourceAdded -= _tilePoolEventBindings[EventBindings.TilePoolAdded];
                _tilePoolManager.Pools.ResourceRemoved -= _tilePoolEventBindings[EventBindings.TilePoolRemoved];
                _tilePoolManager.Pools.ResourceModified -= _tilePoolEventBindings[EventBindings.TilePoolModified];
            }

            _tilePoolManager = manager;

            if (_tilePoolManager != null) {
                _tilePoolManager.Pools.ResourceAdded += _tilePoolEventBindings[EventBindings.TilePoolAdded];
                _tilePoolManager.Pools.ResourceRemoved += _tilePoolEventBindings[EventBindings.TilePoolRemoved];
                _tilePoolManager.Pools.ResourceModified += _tilePoolEventBindings[EventBindings.TilePoolModified];
            }
        }

        public Project Project
        {
            get { return _editor.Project; }
        }

        private void ObjectPoolAddedHandler (object sender, ResourceEventArgs<ObjectPool> e)
        {
            HookObjectPool(e.Resource);
        }

        private void ObjectPoolRemovedHandler (object sender, ResourceEventArgs<ObjectPool> e)
        {
            UnhookObjectPool(e.Resource);
        }

        public event EventHandler ProjectReset;

        protected virtual void OnProjectReset (EventArgs e)
        {
            var ev = ProjectReset;
            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<ResourceEventArgs<Library>> LibraryAdded;
        public event EventHandler<ResourceEventArgs<Library>> LibraryRemoved;
        public event EventHandler<ResourceEventArgs<Library>> LibraryModified;

        protected virtual void OnLibraryAdded (ResourceEventArgs<Library> e)
        {
            var ev = LibraryAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnLibraryRemoved (ResourceEventArgs<Library> e)
        {
            var ev = LibraryRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnLibraryModified (ResourceEventArgs<Library> e)
        {
            var ev = LibraryModified;
            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<ResourceEventArgs<Level>> LevelAdded;
        public event EventHandler<ResourceEventArgs<Level>> LevelRemoved;
        public event EventHandler<ResourceEventArgs<Level>> LevelModified;

        protected virtual void OnLevelAdded (ResourceEventArgs<Level> e)
        {
            var ev = LevelAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnLevelRemoved (ResourceEventArgs<Level> e)
        {
            var ev = LevelRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnLevelModified (ResourceEventArgs<Level> e)
        {
            var ev = LevelModified;
            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<ResourceEventArgs<ObjectClass>> ObjectAdded;
        public event EventHandler<ResourceEventArgs<ObjectClass>> ObjectRemoved;
        public event EventHandler<ResourceEventArgs<ObjectClass>> ObjectModified;

        protected virtual void OnObjectAdded (ResourceEventArgs<ObjectClass> e)
        {
            var ev = ObjectAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnObjectRemoved (ResourceEventArgs<ObjectClass> e)
        {
            var ev = ObjectRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnObjectModified (ResourceEventArgs<ObjectClass> e)
        {
            var ev = ObjectModified;
            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<ResourceEventArgs<TilePool>> TilePoolAdded;
        public event EventHandler<ResourceEventArgs<TilePool>> TilePoolRemoved;
        public event EventHandler<ResourceEventArgs<TilePool>> TilePoolModified;

        protected virtual void OnTilePoolAdded (ResourceEventArgs<TilePool> e)
        {
            var ev = TilePoolAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTilePoolRemoved (ResourceEventArgs<TilePool> e)
        {
            var ev = TilePoolRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTilePoolModified (ResourceEventArgs<TilePool> e)
        {
            var ev = TilePoolModified;
            if (ev != null)
                ev(this, e);
        }

        public void DefaultAction (Guid uid)
        {
            if (_project.Levels.Contains(uid))
                _editor.OpenLevel(uid);
            else if (_project.ObjectPoolManager.Contains(uid))
                _editor.CommandActions.ObjectClassActions.CommandEdit(uid);
        }

        public CommandMenu Menu (Guid uid)
        {
            if (_project.Levels.Contains(uid))
                return LevelMenu(uid);
            if (_project.ObjectPoolManager.Contains(uid))
                return ObjectProtoMenu(uid);
            if (_project.TilePoolManager.Pools.Contains(uid))
                return TileSetMenu(uid);

            return new CommandMenu("");
        }

        public CommandMenu LevelMenu (Guid uid)
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.LevelOpen, uid) { Default = true },
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.LevelClone, uid), 
                    new CommandMenuEntry(CommandKey.LevelDelete, uid),
                    new CommandMenuEntry(CommandKey.LevelRename, uid),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.LevelProperties, uid),
                },
            });
        }

        public CommandMenu ObjectProtoMenu (Guid uid)
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ObjectProtoEdit, uid) { Default = true },
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ObjectProtoClone, uid),
                    new CommandMenuEntry(CommandKey.ObjectProtoDelete, uid),
                    new CommandMenuEntry(CommandKey.ObjectProtoRename, uid),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ObjectProtoProperties, uid),
                },
            });
        }

        public CommandMenu TileSetMenu (Guid uid)
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.TilePoolDelete, uid),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.TilePoolProperties, uid),
                },
            });
        }

        #region Commands

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();
            //_commandManager.CommandInvalidated += HandleCommandInvalidated;

            _commandManager.Register(CommandKey.LevelOpen, CommandCanOpenLevel, CommandOpenLevel);
            _commandManager.Register(CommandKey.LevelClone, CommandCanCloneLevel, CommandCloneLevel);
            _commandManager.Register(CommandKey.LevelDelete, CommandCanDeleteLevel, CommandDeleteLevel);
            _commandManager.Register(CommandKey.LevelRename, CommandCanRenameLevel, CommandRenameLevel);
            _commandManager.Register(CommandKey.LevelProperties, CommandCanLevelProperties, CommandLevelProperties);

            ObjectClassCommandActions objClassActions = _editor.CommandActions.ObjectClassActions;
            _commandManager.Register(CommandKey.ObjectProtoEdit, objClassActions.ObjectExists, objClassActions.CommandEdit);
            _commandManager.Register(CommandKey.ObjectProtoClone, objClassActions.ObjectExists, objClassActions.CommandClone);
            _commandManager.Register(CommandKey.ObjectProtoDelete, objClassActions.ObjectExists, objClassActions.CommandDelete);
            _commandManager.Register(CommandKey.ObjectProtoRename, objClassActions.ObjectExists, objClassActions.CommandRename);
            _commandManager.Register(CommandKey.ObjectProtoProperties, objClassActions.ObjectExists, objClassActions.CommandProperties);

            _commandManager.Perform(CommandKey.ViewGrid);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanOpenLevel (object param)
        {
            if (!(param is Guid))
                return false;

            Guid uid = (Guid)param;
            if (_editor.ContentWorkspace.IsContentOpen(uid))
                return false;

            return _editor.ContentWorkspace.IsContentValid(uid);
        }

        private void CommandOpenLevel (object param)
        {
            if (!CommandCanOpenLevel(param))
                return;

            _editor.ContentWorkspace.OpenContent((Guid)param);
        }

        private bool CommandCanDeleteLevel (object param)
        {
            if (!(param is Guid))
                return false;

            return _project.Levels.Contains((Guid)param);
        }

        private void CommandDeleteLevel (object param)
        {
            if (!CommandCanDeleteLevel(param))
                return;

            if (MessageBox.Show("Are you sure you want to delete this level?\nThis operation cannot be undone.",
                "Confirm Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) {
                return;
            }

            Guid uid = (Guid)param;
            if (_editor.ContentWorkspace.IsContentOpen(uid))
                _editor.ContentWorkspace.CloseContent(uid);

            _project.Levels.Remove(uid);
        }

        private bool CommandCanCloneLevel (object param)
        {
            if (!(param is Guid))
                return false;

            return _project.Levels.Contains((Guid)param);
        }

        private void CommandCloneLevel (object param)
        {
            if (!CommandCanCloneLevel(param))
                return;

            Level level = _project.Levels[(Guid)param];

            Level clone = new Level(level, _project);
            clone.TrySetName(_project.Levels.CompatibleName(level.Name));

            _project.Levels.Add(clone);
        }

        private bool CommandCanRenameLevel (object param)
        {
            if (!(param is Guid))
                return false;

            return _project.Levels.Contains((Guid)param);
        }

        private void CommandRenameLevel (object param)
        {
            if (!CommandCanRenameLevel(param))
                return;

            Level level = _project.Levels[(Guid)param];

            using (NameChangeForm form = new NameChangeForm(level.Name)) {
                foreach (Level lev in level.Project.Levels)
                    form.ReservedNames.Add(lev.Name);

                if (form.ShowDialog() == DialogResult.OK)
                    level.TrySetName(form.Name);
            }
        }

        private bool CommandCanLevelProperties (object param)
        {
            if (!(param is Guid))
                return false;

            return _project.Levels.Contains((Guid)param);
        }

        private void CommandLevelProperties (object param)
        {
            if (!CommandCanLevelProperties(param))
                return;

            _editor.Presentation.PropertyList.Provider = _project.Levels[(Guid)param];
            _editor.ActivatePropertyPanel();
        }

        #endregion
    }
}
