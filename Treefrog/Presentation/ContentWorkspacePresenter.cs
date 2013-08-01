using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation
{
    public class ContentPresenterEventArgs : EventArgs
    {
        public ContentPresenter Content { get; private set; }

        public ContentPresenterEventArgs (ContentPresenter content)
        {
            Content = content;
        }
    }

    public abstract class ContentTypeController : IDisposable
    {
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        { }

        public event EventHandler<ContentPresenterEventArgs> ContentOpened;
        public event EventHandler<ContentPresenterEventArgs> ContentClosed;

        protected virtual void OnContentOpened (ContentPresenterEventArgs e)
        {
            var ev = ContentOpened;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnContentClosed (ContentPresenterEventArgs e)
        {
            var ev = ContentClosed;
            if (ev != null)
                ev(this, e);
        }

        public abstract void OpenContent (Guid uid);
        public abstract void CloseContent (Guid uid);

        public IEnumerable<ContentPresenter> OpenContents
        {
            get { return OpenContentsCore; }
        }

        protected virtual IEnumerable<ContentPresenter> OpenContentsCore
        {
            get { yield break; }
        }

        public abstract bool ContentIsValid (Guid uid);

        public ContentPresenter GetContent (Guid uid)
        {
            return GetContentCore(uid);
        }

        protected virtual ContentPresenter GetContentCore (Guid uid)
        {
            return null;
        }
    }

    public class LevelContentTypeController : ContentTypeController
    {
        private EditorPresenter _editor;
        private Project _project;

        private Dictionary<Guid, LevelPresenter> _content;
        private Dictionary<Guid, LevelPresenter> _openContent;

        public LevelContentTypeController (EditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += EditorSyncCurrentProject;

            _content = new Dictionary<Guid, LevelPresenter>();
            _openContent = new Dictionary<Guid, LevelPresenter>();

            BindProject(_editor.Project);
        }

        protected override void Dispose (bool disposing)
        {
            if (_editor != null) {
                if (disposing) {
                    BindProject(null);

                    _editor.SyncCurrentProject -= EditorSyncCurrentProject;
                }

                _editor = null;
            }

            base.Dispose(disposing);
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

            _content.Clear();
            _openContent.Clear();

            if (_project != null) {
                _project.Levels.ResourceAdded -= LevelAddedHandler;
                _project.Levels.ResourceRemoved -= LevelRemovedHandler;
                _project.Levels.ResourceModified -= LevelModifiedHandler;
            }

            _project = project;

            if (_project != null) {
                _project.Levels.ResourceAdded += LevelAddedHandler;
                _project.Levels.ResourceRemoved += LevelRemovedHandler;
                _project.Levels.ResourceModified += LevelModifiedHandler;

                foreach (Level level in _project.Levels)
                    _content[level.Uid] = new LevelPresenter(_editor, level);
            }
        }

        private void LevelAddedHandler (object sender, ResourceEventArgs<Level> e)
        {
            _content[e.Uid] = new LevelPresenter(_editor, e.Resource);
        }

        private void LevelRemovedHandler (object sender, ResourceEventArgs<Level> e)
        {
            _content.Remove(e.Uid);
        }

        private void LevelModifiedHandler (object sender, ResourceEventArgs<Level> e)
        {

        }

        public override void OpenContent (Guid uid)
        {
            if (_content.ContainsKey(uid) && !_openContent.ContainsKey(uid)) {
                _openContent[uid] = _content[uid];
                OnContentOpened(new ContentPresenterEventArgs(_content[uid]));
            }
        }

        public override void CloseContent (Guid uid)
        {
            if (_content.ContainsKey(uid) && _openContent.ContainsKey(uid)) {
                _openContent.Remove(uid);
                OnContentClosed(new ContentPresenterEventArgs(_content[uid]));
            }
        }

        public IEnumerable<LevelPresenter> Contents
        {
            get { return _content.Values; }
        }

        public new IEnumerable<LevelPresenter> OpenContents
        {
            get { return _openContent.Values; }
        }

        protected override IEnumerable<ContentPresenter> OpenContentsCore
        {
            get { return _openContent.Values; }
        }

        public int ContentsCount
        {
            get { return _content.Count; }
        }

        public int OpenContentsCount
        {
            get { return _openContent.Count; }
        }

        public override bool ContentIsValid (Guid uid)
        {
            return _content.ContainsKey(uid);
        }

        public bool ContentIsOpen (Guid uid)
        {
            return _openContent.ContainsKey(uid);
        }

        protected override ContentPresenter GetContentCore (Guid uid)
        {
            return GetContent(uid);
        }

        public new LevelPresenter GetContent (Guid uid)
        {
            if (!_content.ContainsKey(uid))
                return null;
            return _content[uid];
        }
    }

    public class ContentWorkspacePresenter : IDisposable, ICommandSubscriber
    {
        private EditorPresenter _editor;
        private Project _project;

        private List<ContentTypeController> _contentControllers;
        private Dictionary<Guid, ContentPresenter> _openContent;

        public ContentWorkspacePresenter (EditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += EditorSyncCurrentProject;

            _contentControllers = new List<ContentTypeController>();
            _openContent = new Dictionary<Guid, ContentPresenter>();

            InitializeCommandManager();

            BindProject(_editor.Project);
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

                foreach (var controller in _contentControllers)
                    RemoveContentController(controller);

                _editor = null;
            }
        }

        public void AddContentController (ContentTypeController controller)
        {
            controller.ContentOpened += ControllerContentOpenedHandler;
            controller.ContentClosed += ControllerContentClosedHandler;

            _contentControllers.Add(controller);
        }

        public void RemoveContentController (ContentTypeController controller)
        {
            controller.ContentOpened -= ControllerContentOpenedHandler;
            controller.ContentClosed -= ControllerContentClosedHandler;

            _contentControllers.Remove(controller);
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

            }

            _project = project;

            _openContent.Clear();

            if (_project != null) {

            }

            OnProjectReset(EventArgs.Empty);
        }

        private void ControllerContentOpenedHandler (object sender, ContentPresenterEventArgs e)
        {
            if (!_openContent.ContainsKey(e.Content.Uid)) {
                _openContent[e.Content.Uid] = e.Content;
                OnContentOpened(new ContentPresenterEventArgs(e.Content));
            }
        }

        private void ControllerContentClosedHandler (object sender, ContentPresenterEventArgs e)
        {
            if (_openContent.Remove(e.Content.Uid))
                OnContentClosed(new ContentPresenterEventArgs(e.Content));
        }

        public event EventHandler ProjectReset;

        protected virtual void OnProjectReset (EventArgs e)
        {
            var ev = ProjectReset;
            if (ev != null)
                ev(this, e);
        }

        public void OpenContent (Guid uid)
        {
            foreach (var controller in _contentControllers)
                controller.OpenContent(uid);
        }

        public void CloseContent (Guid uid)
        {
            foreach (var controller in _contentControllers)
                controller.CloseContent(uid);
        }

        public bool IsContentOpen (Guid uid)
        {
            return _openContent.ContainsKey(uid);
        }

        public bool IsContentValid (Guid uid)
        {
            foreach (var controller in _contentControllers) {
                if (controller.ContentIsValid(uid))
                    return true;
            }

            return false;
        }

        public ContentPresenter GetContent (Guid uid)
        {
            foreach (var controller in _contentControllers) {
                if (controller.ContentIsValid(uid))
                    return controller.GetContent(uid);
            }

            return null;
        }

        public event EventHandler<ContentPresenterEventArgs> ContentOpened;
        public event EventHandler<ContentPresenterEventArgs> ContentClosed;
        public event EventHandler<ContentPresenterEventArgs> ContentModified;

        protected virtual void OnContentOpened (ContentPresenterEventArgs e)
        {
            var ev = ContentOpened;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnContentClosed (ContentPresenterEventArgs e)
        {
            var ev = ContentClosed;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnContentModified (ContentPresenterEventArgs e)
        {
            var ev = ContentModified;
            if (ev != null)
                ev(this, e);
        }

        public IEnumerable<ContentPresenter> OpenContents
        {
            get { return _openContent.Values; }
        }

        #region Commands

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();
            //_commandManager.CommandInvalidated += HandleCommandInvalidated;

            _commandManager.Register(CommandKey.LevelOpen, CommandCanOpenLevel, CommandOpenLevel);

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
            if (_openContent.ContainsKey(uid))
                return false;

            foreach (var controller in _contentControllers) {
                if (controller.ContentIsValid(uid))
                    return true;
            }

            return false;
        }

        private void CommandOpenLevel (object param)
        {
            if (!CommandCanOpenLevel(param))
                return;

            OpenContent((Guid)param);
        }

        #endregion
    }
}
