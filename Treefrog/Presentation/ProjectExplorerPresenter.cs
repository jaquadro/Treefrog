using System;
using System.Collections.Generic;
using Treefrog.Extensibility;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation
{
    public class ProjectExplorerPresenter : Presenter, ICommandSubscriber
    {
        private enum EventBindings
        {
            LibraryAdded,
            LibraryRemoved,
            LibraryModified,
        }

        private EditorPresenter _editor;

        private Project _project;
        private LibraryManager _libraryManager;

        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<Library>>> _libraryEventBindings;

        public ProjectExplorerPresenter ()
        { }

        protected override void InitializeCore ()
        {
            InitializeCommandManager();

            OnAttach<EditorPresenter>(editor => {
                _editor = editor;
                _editor.SyncCurrentProject += EditorSyncCurrentProject;

                InitializeCommandManager(editor);
            });

            OnDetach<EditorPresenter>(editor => {
                BindProject(null);

                _editor.SyncCurrentProject -= EditorSyncCurrentProject;
                _editor = null;
            });

            Components = new InstanceRegistry<ProjectExplorerComponent>();

            _libraryEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<Library>>>() {
                { EventBindings.LibraryAdded, (s, e) => OnLibraryAdded(new ResourceEventArgs<Library>(e.Resource)) },
                { EventBindings.LibraryRemoved, (s, e) => OnLibraryRemoved(new ResourceEventArgs<Library>(e.Resource)) },
                { EventBindings.LibraryModified, (s, e) => OnLibraryModified(new ResourceEventArgs<Library>(e.Resource)) },
            };

            LibraryManagerTag = Guid.NewGuid();
            ProjectManagerTag = Guid.NewGuid();
        }

        public EditorPresenter Editor
        {
            get { return _editor; }
        }

        public InstanceRegistry<ProjectExplorerComponent> Components { get; private set; }

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
                _project.DefaultLibraryChanged -= DefaultLibraryChangedHandler;
            }

            _project = project;

            if (_project != null) {
                _project.DefaultLibraryChanged += DefaultLibraryChangedHandler;

                BindLibraryManager(_project.LibraryManager);
            }
            else {
                BindLibraryManager(null);
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

        public Project Project
        {
            get { return _editor.Project; }
        }

        public Guid LibraryManagerTag { get; private set; }
        public Guid ProjectManagerTag { get; private set; }

        public event EventHandler ProjectReset;

        protected virtual void OnProjectReset (EventArgs e)
        {
            var ev = ProjectReset;
            if (ev != null)
                ev(this, e);
        }

        public event EventHandler DefaultLibraryChanged;

        protected virtual void OnDefaultLibraryChanged (EventArgs e)
        {
            var ev = DefaultLibraryChanged;
            if (ev != null)
                ev(this, e);
        }

        private void DefaultLibraryChangedHandler (object sender, EventArgs e)
        {
            OnDefaultLibraryChanged(EventArgs.Empty);
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

        public void DefaultAction (Guid uid)
        { }

        public CommandMenu Menu (Guid uid)
        {
            if (uid == LibraryManagerTag)
                return LibraryManagerMenu();

            if (_project.LibraryManager.Libraries.Contains(uid))
                return LibraryMenu(uid);

            return new CommandMenu("");
        }

        public CommandMenu LibraryManagerMenu ()
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ProjectAddNewLibrary),
                    new CommandMenuEntry(CommandKey.ProjectAddExistingLibrary),
                },
            });
        }

        public CommandMenu LibraryMenu (Guid uid)
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ProjectSetLibraryDefault, uid),
                },
            });
        }

        #region Commands

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();

            _commandManager.Perform(CommandKey.ViewGrid);
        }

        private void InitializeCommandManager (EditorPresenter editor)
        {
            LibraryCommandActions libraryActions = _editor.CommandActions.LibraryActions;
            _commandManager.Register(CommandKey.ProjectAddNewLibrary, () => { return true; }, libraryActions.CommandCreate);
            _commandManager.Register(CommandKey.ProjectSetLibraryDefault, libraryActions.LibraryExists, libraryActions.CommandSetDefault);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        #endregion
    }
}
