using System;
using System.IO;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;

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
        bool CanDelete { get; }
        bool CanSelectAll { get; }
        bool CanUnselectAll { get; }

        bool CanUndo { get; }
        bool CanRedo { get; }

        void ActionCut ();
        void ActionCopy ();
        void ActionPaste ();
        void ActionDelete ();
        void ActionSelectAll ();
        void ActionUnselectAll ();

        void ActionUndo ();
        void ActionRedo ();

        event EventHandler SyncDocumentToolsActions;

        event EventHandler CutRaised;
        event EventHandler CopyRaised;
        event EventHandler PasteRaised;
        event EventHandler DeleteRaised;
        event EventHandler SelectAllRaised;
        event EventHandler UnselectAllRaised;

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

        private bool CanPerformCommand (CommandKey key)
        {
            if (_editor.CurrentLevel == null || _editor.CurrentLevel.CommandManager == null)
                return false;

            return _editor.CurrentLevel.CommandManager.CanPerform(key);
        }

        private void PerformCommand (CommandKey key)
        {
            if (_editor.CurrentLevel == null || _editor.CurrentLevel.CommandManager == null)
                return;

            _editor.CurrentLevel.CommandManager.Perform(key);
        }

        public bool CanCut
        {
            get { return CanPerformCommand(CommandKey.Cut); }
            //return _editor.CurrentLevel != null ? (_editor.CurrentLevel.Selection != null) : false; }
        }

        public bool CanCopy
        {
            get { return CanPerformCommand(CommandKey.Copy); }
            //get { return _editor.CurrentLevel != null ? (_editor.CurrentLevel.Selection != null) : false; }
        }

        public bool CanPaste
        {
            get { return CanPerformCommand(CommandKey.Paste); }
            //get { return _editor.CurrentLevel != null && _editor.CurrentLevel.Clipboard != null; }
        }

        public bool CanDelete
        {
            get { return CanPerformCommand(CommandKey.Delete); }
            //get { return _editor.CurrentLevel != null ? (_editor.CurrentLevel.Selection != null) : false; }
        }

        public bool CanSelectAll
        {
            get { return CanPerformCommand(CommandKey.SelectAll); }
            //get { return _editor.CurrentLevel != null; }
        }

        public bool CanUnselectAll
        {
            get { return CanPerformCommand(CommandKey.SelectNone); }
            //get { return _editor.CurrentLevel != null ? (_editor.CurrentLevel.Selection != null) : false; }
        }

        public bool CanUndo
        {
            get { return CanPerformCommand(CommandKey.Undo); }
            //get { return _editor.CurrentLevel != null ? _editor.CurrentLevel.History.CanUndo : false; }
        }

        public bool CanRedo
        {
            get { return CanPerformCommand(CommandKey.Redo); }
            //get { return _editor.CurrentLevel != null ? _editor.CurrentLevel.History.CanRedo : false; }
        }

        public void ActionCut ()
        {
            if (CanCut)
                PerformCommand(CommandKey.Cut);
            else
                OnCutRaised(EventArgs.Empty);
        }

        public void ActionCopy ()
        {
            if (CanCopy)
                PerformCommand(CommandKey.Copy);
            else
                OnCopyRaised(EventArgs.Empty);
        }

        public void ActionPaste ()
        {
            if (CanPaste)
                PerformCommand(CommandKey.Paste);
            else {
                if (_editor.Presentation.LevelTools.ActiveTileTool != TileToolMode.Select) {
                    _editor.Presentation.LevelTools.ActionToggleSelect();
                }
                OnPasteRaised(EventArgs.Empty);
            }
        }

        public void ActionDelete ()
        {
            if (CanDelete)
                PerformCommand(CommandKey.Delete);
            else
                OnDeleteRaised(EventArgs.Empty);
        }

        public void ActionSelectAll ()
        {
            if (CanSelectAll)
                PerformCommand(CommandKey.SelectAll);
            else {
                if (_editor.Presentation.LevelTools.ActiveTileTool != TileToolMode.Select) {
                    _editor.Presentation.LevelTools.ActionToggleSelect();
                }
                OnSelectAllRaised(EventArgs.Empty);
            }
        }

        public void ActionUnselectAll ()
        {
            if (CanUnselectAll)
                PerformCommand(CommandKey.SelectNone);
            else
                OnUnselectAllRaised(EventArgs.Empty);
        }

        public void ActionUndo ()
        {
            if (CanUndo)
                PerformCommand(CommandKey.Undo);
            else {
                if (_editor.CurrentLevel != null) {
                    _editor.CurrentLevel.History.Undo();
                    OnSyncDocumentToolsActions(EventArgs.Empty);
                }
            }
        }

        public void ActionRedo ()
        {
            if (CanRedo)
                PerformCommand(CommandKey.Redo);
            else {
                if (_editor.CurrentLevel != null) {
                    _editor.CurrentLevel.History.Redo();
                    OnSyncDocumentToolsActions(EventArgs.Empty);
                }
            }
        }

        public event EventHandler SyncDocumentToolsActions;

        protected virtual void OnSyncDocumentToolsActions (EventArgs e)
        {
            if (SyncDocumentToolsActions != null) {
                SyncDocumentToolsActions(this, e);
            }
        }

        public event EventHandler CutRaised;
        public event EventHandler CopyRaised;
        public event EventHandler PasteRaised;
        public event EventHandler DeleteRaised;
        public event EventHandler SelectAllRaised;
        public event EventHandler UnselectAllRaised;

        protected virtual void OnCutRaised (EventArgs e)
        {
            if (CutRaised != null) {
                CutRaised(this, e);
            }
            OnSyncDocumentToolsActions(EventArgs.Empty);
        }

        protected virtual void OnCopyRaised (EventArgs e)
        {
            if (CopyRaised != null) {
                CopyRaised(this, e);
            }
            OnSyncDocumentToolsActions(EventArgs.Empty);
        }

        protected virtual void OnPasteRaised (EventArgs e)
        {
            if (PasteRaised != null) {
                PasteRaised(this, e);
            }
            OnSyncDocumentToolsActions(EventArgs.Empty);
        }

        protected virtual void OnDeleteRaised (EventArgs e)
        {
            if (DeleteRaised != null) {
                DeleteRaised(this, e);
            }
            OnSyncDocumentToolsActions(EventArgs.Empty);
        }

        protected virtual void OnSelectAllRaised (EventArgs e)
        {
            if (SelectAllRaised != null) {
                SelectAllRaised(this, e);
            }
            OnSyncDocumentToolsActions(EventArgs.Empty);
        }

        protected virtual void OnUnselectAllRaised (EventArgs e)
        {
            if (UnselectAllRaised != null) {
                UnselectAllRaised(this, e);
            }
            OnSyncDocumentToolsActions(EventArgs.Empty);
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
            //Form form = new Form();
            //GraphicsDeviceService gds = GraphicsDeviceService.AddRef(form.Handle, 128, 128);

            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read)) {
                //Project project = Project.Open(fs, gds.GraphicsDevice);
                Project project = Project.Open(fs);
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
