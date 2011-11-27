using System;
using System.IO;
using System.Windows.Forms;
using Treefrog.Framework.Model;

namespace Treefrog.Presentation
{
    public interface IStandardToolsPresenter
    {
        bool CanCreateProject { get; }
        bool CanOpenProject { get; }
        bool CavSaveProject { get; }

        void ActionCreateProject ();
        void ActionOpenProject (string path);
        void ActionSaveProject (string path);

        event EventHandler SyncStandardToolsActions;

        void RefreshStandardTools ();
    }

    public interface IDocumentToolsPresenter
    {
        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }

        bool CanUndo { get; }
        bool CanRedo { get; }

        void ActionCut ();
        void ActionCopy ();
        void ActionPaste ();

        void ActionUndo ();
        void ActionRedo ();

        event EventHandler SyncDocumentToolsActions;

        void RefreshDocumentTools ();
    }

    public class DocumentToolsPresenter : IDocumentToolsPresenter
    {
        private EditorPresenter _editor;

        public DocumentToolsPresenter (EditorPresenter editor)
        {
            _editor = editor;
        }

        #region IDocumentToolsPresenter Members

        public bool CanCut
        {
            get { return false; }
        }

        public bool CanCopy
        {
            get { return false; }
        }

        public bool CanPaste
        {
            get { return false; }
        }

        public bool CanUndo
        {
            get { return _editor.CurrentLevel != null ? _editor.CurrentLevel.History.CanUndo : false; }
        }

        public bool CanRedo
        {
            get { return _editor.CurrentLevel != null ? _editor.CurrentLevel.History.CanRedo : false; }
        }

        public void ActionCut ()
        {
            throw new NotImplementedException();
        }

        public void ActionCopy ()
        {
            throw new NotImplementedException();
        }

        public void ActionPaste ()
        {
            throw new NotImplementedException();
        }

        public void ActionUndo ()
        {
            if (_editor.CurrentLevel != null) {
                _editor.CurrentLevel.History.Undo();
                OnSyncDocumentToolsActions(EventArgs.Empty);
            }
        }

        public void ActionRedo ()
        {
            if (_editor.CurrentLevel != null) {
                _editor.CurrentLevel.History.Redo();
                OnSyncDocumentToolsActions(EventArgs.Empty);
            }
        }

        public event EventHandler SyncDocumentToolsActions;

        protected virtual void OnSyncDocumentToolsActions (EventArgs e)
        {
            if (SyncDocumentToolsActions != null) {
                SyncDocumentToolsActions(this, e);
            }
        }

        public void RefreshDocumentTools ()
        {
            OnSyncDocumentToolsActions(EventArgs.Empty);
        }

        #endregion
    }

    public class StandardToolsPresenter : IStandardToolsPresenter
    {
        private EditorPresenter _editor;

        public StandardToolsPresenter (EditorPresenter editor)
        {
            _editor = editor;
        }

        #region IStandardToolsPresenter Members

        public bool CanCreateProject
        {
            get { return true; }
        }

        public bool CanOpenProject
        {
            get { return true; }
        }

        public bool CavSaveProject
        {
            get { return true; }
        }

        public void ActionCreateProject ()
        {
            _editor.New();
        }

        public void ActionOpenProject (string path)
        {
            Form form = new Form();
            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(form.Handle, 128, 128);

            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read)) {
                Project project = Project.Open(fs, gds.GraphicsDevice);
                _editor.Open(project);
            }
        }

        public void ActionSaveProject (string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write)) {
                _editor.Save(fs);
            }
        }

        public event EventHandler SyncStandardToolsActions;

        protected virtual void OnSyncStandardToolsActions (EventArgs e)
        {
            if (SyncStandardToolsActions != null) {
                SyncStandardToolsActions(this, e);
            }
        }

        public void RefreshStandardTools ()
        {
            OnSyncStandardToolsActions(EventArgs.Empty);
        }

        #endregion
    }
}
