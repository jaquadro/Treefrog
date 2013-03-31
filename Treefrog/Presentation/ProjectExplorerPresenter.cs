using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;

namespace Treefrog.Presentation
{
    public class ProjectExplorerPresenter : IDisposable
    {
        private enum EventBindings
        {
            LibraryAdded,
            LibraryRemoved,
            LibraryModified,

            LevelAdded,
            LevelRemoved,
            LevelModified,
        }

        private IEditorPresenter _editor;

        private Project _project;
        private LibraryManager _libraryManager;

        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<Library>>> _libraryEventBindings;
        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<Level>>> _levelEventBindings;

        public ProjectExplorerPresenter (IEditorPresenter editor)
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
    }
}
