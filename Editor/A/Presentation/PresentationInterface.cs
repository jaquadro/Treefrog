using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using System.Windows.Forms;
using System.IO;

namespace Editor.A.Presentation
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
            throw new NotImplementedException();
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
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write)) {
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
